using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimulationLabs
{
    /// Форма симуляции марковской модели погоды.
    /// 3 состояния: 1 — ясно, 2 — облачно, 3 — пасмурно.
    /// Моделирование в реальном времени с визуализацией через изображения погоды.
    public class FormLab7 : Form
    {
        // Элементы управления
        private TextBox txtLambda12, txtLambda13, txtLambda21, txtLambda23, txtLambda31, txtLambda32;
        private TextBox txtSimDays, txtSpeed;
        private Button btnStart, btnStop, btnReset, btnClose, btnExportCSV;
        private Chart chart;
        private Label lblResults, lblCurrentState;

        // Изображения погоды
        private PictureBox picWeather;           // Текущее состояние (большая иконка)
        private Image[] weatherImages;           // Массив изображений для каждого состояния
        private FlowLayoutPanel flowWeatherHistory; // Лента иконок погоды

        // Данные модели
        private double[,] Q;          // Матрица интенсивностей (генератор Q)
        private double[] lambda;      // Суммарные интенсивности выхода из состояний
        private int currentState = 1; // Текущее состояние (1, 2, 3)

        private int daysInLastState = 0;      // Сколько дней пробыли в предыдущем состоянии

        private long totalDuration = 0; // Общее накопленное время
        private double[] exactStateDuration; // Точное время в каждом состоянии (непрерывное)
        private double exactTotalDuration = 0; // Общее точное время (непрерывное)
        private double simulationEndTime = 365.0;  

        private bool isRunning = false;
        private Random rand = new Random();

        private class TransitionRecord
        {
            public double Time { get; set; }
            public int State { get; set; }
            public string StateName => State switch
            {
                1 => "Ясно",
                2 => "Облачно",
                3 => "Пасмурно",
                _ => "Неизвестно"
            };
        }

        private double currentTime = 0.0;           // Непрерывное время (дни)
        private double[] totalTimeInState;          // Algorithm 2: точное время в каждом состоянии
        private List<TransitionRecord> transitionHistory; // История событий (время → состояние)
        private System.Windows.Forms.Timer animTimer; // Только для задержки анимации, не для логики

        // Списки для графика
        private List<int> dayStates = new List<int>(); // Состояние по дням
        private List<double> theoProbs = new List<double>(); // Теоретические вероятности

        // Названия состояний
        private string[] stateNames = { "Ясно", "Облачно", "Пасмурно" };
        private Color[] stateColors = { Color.FromArgb(255, 235, 59), Color.FromArgb(189, 189, 189), Color.FromArgb(117, 117, 117) };

        public FormLab7()
        {
            InitializeComponent();
            LoadWeatherImages();
            CreateManualUI();
            InitDefaultIntensities();
        }

        private void InitializeComponent()
        {
            this.Text = "Лаб 07: Марковская модель погоды";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        /// Загрузка изображений погоды из папки images.
        /// Если файл не найден — создаётся заглушка программно.
        /// Ожидаемые файлы: sunny.png, cloudy.png, overcast.png
        private void LoadWeatherImages()
        {
            weatherImages = new Image[3];

            string[] fileNames = { "sunny.png", "cloudy.png", "overcast.png" };

            for (int i = 0; i < 3; i++)
            {
                // Путь к файлу: ищем в папке images рядом с exe и в исходниках
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string imgPath = Path.Combine(exeDir, "images", fileNames[i]);

                if (!File.Exists(imgPath))
                {
                    // Пробуем из директории проекта
                    string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "images", fileNames[i]);
                    if (File.Exists(projectDir))
                        imgPath = projectDir;
                    else
                        imgPath = null;
                }

                if (imgPath != null && File.Exists(imgPath))
                    weatherImages[i] = Image.FromFile(imgPath);
                else
                    throw new Exception("No images for weather states");
            }
        }

        /// Инициализация значений интенсивностей по умолчанию
        private void InitDefaultIntensities()
        {
            txtLambda12.Text = "0.3";
            txtLambda13.Text = "0.1";
            txtLambda21.Text = "0.2";
            txtLambda23.Text = "0.4";
            txtLambda31.Text = "0.1";
            txtLambda32.Text = "0.2";
            txtSimDays.Text = "365";
            txtSpeed.Text = "100"; // мс между шагами
        }

        /// Создание пользовательского интерфейса
        private void CreateManualUI()
        {

            // === Панель ввода: матрица 3×3 + управление ===
            Panel pnlInput = new Panel { Location = new Point(10, 10), Size = new Size(310, 400), BorderStyle = BorderStyle.FixedSingle };

            Label lblTitle = new Label
            {
                Text = "Матрица интенсивностей Q",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            pnlInput.Controls.Add(lblTitle);

            // --- Легенда: "из \ в" ---
            Label lblFromTo = new Label
            {
                Text = "из \\ в →",
                Location = new Point(10, 42),
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            pnlInput.Controls.Add(lblFromTo);

            // --- Заголовки столбцов ---
            int colStartX = 55;
            int colWidth = 80;
            int colHeaderY = 62;
            string[] colHeaders = { "1. Ясно", "2. Облачно", "3. Пасмурно" };
            for (int j = 0; j < 3; j++)
            {
                Label lbl = new Label
                {
                    Text = colHeaders[j],
                    Location = new Point(colStartX + j * colWidth, colHeaderY),
                    Size = new Size(colWidth - 5, 28),
                    Font = new Font("Arial", 7, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = stateColors[j],
                    BorderStyle = BorderStyle.FixedSingle
                };
                pnlInput.Controls.Add(lbl);
            }

            // --- Строки матрицы ---
            string[] rowLabels = { "1.Ясно", "2.Облачно", "3.Пасмурно" };
            int rowStartY = 95;
            int rowHeight = 30;

            // Текстовые поля для off-diagonal элементов
            txtLambda12 = new TextBox();
            txtLambda13 = new TextBox();
            txtLambda21 = new TextBox();
            txtLambda23 = new TextBox();
            txtLambda31 = new TextBox();
            txtLambda32 = new TextBox();
            TextBox[] allLambda = { txtLambda12, txtLambda13, txtLambda21, txtLambda23, txtLambda31, txtLambda32 };

            TextBox[,] Qcells = new TextBox[3, 3];

            for (int i = 0; i < 3; i++)
            {
                // Подпись строки (укороченные, чтобы помещались)
                Label rowLbl = new Label
                {
                    Text = rowLabels[i],
                    Location = new Point(2, rowStartY + i * rowHeight),
                    Size = new Size(52, rowHeight),
                    Font = new Font("Arial", 7, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = stateColors[i]
                };
                pnlInput.Controls.Add(rowLbl);

                for (int j = 0; j < 3; j++)
                {
                    if (i == j)
                    {
                        // Диагональ — авто-расчёт, readonly
                        TextBox diagCell = new TextBox
                        {
                            Location = new Point(colStartX + j * colWidth, rowStartY + i * rowHeight),
                            Size = new Size(colWidth - 5, rowHeight - 6),
                            TextAlign = HorizontalAlignment.Center,
                            ReadOnly = true,
                            BackColor = Color.LightGray,
                            ForeColor = Color.DarkRed,
                            Font = new Font("Consolas", 9, FontStyle.Bold),
                            Text = "auto"
                        };
                        Qcells[i, j] = diagCell;
                        pnlInput.Controls.Add(diagCell);
                    }
                    else
                    {
                        // Off-diagonal — ввод
                        int idx;
                        if (i == 0 && j == 1) idx = 0;
                        else if (i == 0 && j == 2) idx = 1;
                        else if (i == 1 && j == 0) idx = 2;
                        else if (i == 1 && j == 2) idx = 3;
                        else if (i == 2 && j == 0) idx = 4;
                        else idx = 5; // i=2, j=1

                        allLambda[idx].Location = new Point(colStartX + j * colWidth, rowStartY + i * rowHeight);
                        allLambda[idx].Size = new Size(colWidth - 5, rowHeight - 6);
                        allLambda[idx].TextAlign = HorizontalAlignment.Center;
                        allLambda[idx].Font = new Font("Consolas", 9);
                        allLambda[idx].KeyPress += Lambda_KeyPress;
                        Qcells[i, j] = allLambda[idx];
                        pnlInput.Controls.Add(allLambda[idx]);
                    }
                }
            }

            // --- Подпись под матрицей ---
            Label lblNote = new Label
            {
                Text = "Диагональ qᵢᵢ = -Σqᵢⱼ (расчитывается автоматически)",
                Location = new Point(10, rowStartY + 3 * rowHeight + 5),
                AutoSize = true,
                Font = new Font("Arial", 7, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            pnlInput.Controls.Add(lblNote);

            // --- Параметры моделирования ---
            int paramY = rowStartY + 3 * rowHeight + 35;

            Label lblDays = new Label { Text = "Дней моделирования:", Location = new Point(10, paramY), AutoSize = true, Font = new Font("Arial", 9) };
            pnlInput.Controls.Add(lblDays);
            txtSimDays = new TextBox { Location = new Point(10, paramY + 18), Size = new Size(120, 20), Text = "365", Font = new Font("Arial", 9) };
            pnlInput.Controls.Add(txtSimDays);

            Label lblSpeed = new Label { Text = "Шаг таймера (мс):", Location = new Point(145, paramY), AutoSize = true, Font = new Font("Arial", 9) };
            pnlInput.Controls.Add(lblSpeed);
            txtSpeed = new TextBox { Location = new Point(145, paramY + 18), Size = new Size(90, 20), Text = "100", Font = new Font("Arial", 9) };
            pnlInput.Controls.Add(txtSpeed);

            // --- Кнопки ---
            int btnY = paramY + 48;
            btnStart = new Button { Text = "Старт", Location = new Point(10, btnY), Size = new Size(90, 30), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnStart.Click += BtnStart_Click;
            pnlInput.Controls.Add(btnStart);

            btnStop = new Button { Text = "Стоп", Location = new Point(105, btnY), Size = new Size(90, 30), BackColor = Color.LightCoral, Enabled = false, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnStop.Click += BtnStop_Click;
            pnlInput.Controls.Add(btnStop);

            btnReset = new Button { Text = "Сброс", Location = new Point(200, btnY), Size = new Size(80, 30), Font = new Font("Arial", 9) };
            btnReset.Click += BtnReset_Click;
            pnlInput.Controls.Add(btnReset);

            int btnY2 = btnY + 35;
            btnExportCSV = new Button { Text = "Экспорт CSV", Location = new Point(10, btnY2), Size = new Size(140, 28), BackColor = Color.LightYellow, Font = new Font("Arial", 9) };
            btnExportCSV.Click += BtnExportCSV_Click;
            pnlInput.Controls.Add(btnExportCSV);

            btnClose = new Button { Text = "Закрыть", Location = new Point(155, btnY2), Size = new Size(125, 28), Font = new Font("Arial", 9) };
            btnClose.Click += (s, e) => this.Close();
            pnlInput.Controls.Add(btnClose);

            this.Controls.Add(pnlInput);

            // === Блок текущего состояния с изображением ===
            pnlWeatherCurrent = new Panel { Location = new Point(330, 10), Size = new Size(1040, 160), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.WhiteSmoke };

            // Большая иконка погоды
            picWeather = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(140, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Image = weatherImages[0]
            };
            pnlWeatherCurrent.Controls.Add(picWeather);

            // Текстовая подпись (высота 80 чтобы не обрезалось)
            lblCurrentState = new Label
            {
                Text = "День 0 — Ясно",
                Location = new Point(165, 25),
                AutoSize = false,
                Size = new Size(400, 80),
                Font = new Font("Arial", 20, FontStyle.Bold)
            };
            pnlWeatherCurrent.Controls.Add(lblCurrentState);

            // Подсказка
            Label lblHint = new Label
            {
                Text = "Запустите симуляцию для начала моделирования",
                Location = new Point(165, 115),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            pnlWeatherCurrent.Controls.Add(lblHint);

            this.Controls.Add(pnlWeatherCurrent);

            // === Лента погоды по дням ===
            // Метка над лентой
            Label lblHistoryLabel = new Label
            {
                Text = "Последние 30 дней →",
                Location = new Point(330, 175),
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblHistoryLabel);

            // Сама лента (иконки 32x32 + бордер = 34, высота 42 = скролл-бар + запас)
            flowWeatherHistory = new BufferedFlowLayoutPanel
            {
                Location = new Point(330, 193),
                Size = new Size(1040, 42),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0),
                Padding = new Padding(1)
            };
            this.Controls.Add(flowWeatherHistory);

            // === График: гистограмма распределения ===
            chart = new Chart { Location = new Point(330, 255), Size = new Size(520, 280) };
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.BackColor = Color.White;
            ca.AxisX.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.Minimum = 0;
            ca.AxisY.Maximum = 1.0;
            chart.ChartAreas.Add(ca);

            Series sEmp = new Series("Эмпирические");
            sEmp.ChartType = SeriesChartType.Column;
            sEmp.Color = Color.SteelBlue;
            sEmp.BorderColor = Color.Navy;
            sEmp.BorderWidth = 1;
            chart.Series.Add(sEmp);

            Series sTheo = new Series("Теоретические");
            sTheo.ChartType = SeriesChartType.Column;
            sTheo.Color = Color.OrangeRed;
            sTheo.BorderColor = Color.DarkRed;
            sTheo.BorderWidth = 1;
            chart.Series.Add(sTheo);

            this.Controls.Add(chart);

            // === Панель результатов ===
            lblResults = new Label
            {
                Location = new Point(860, 255),
                Size = new Size(510, 280),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(lblResults);

            // Сохраняем ссылку на Q-ячейки для обновления диагонали
            qDiagCells = new TextBox[3];
            for (int i = 0; i < 3; i++)
                qDiagCells[i] = Qcells[i, i];
        }

        private Panel pnlWeatherCurrent;
        private TextBox[] qDiagCells; // Ячейки диагонали (для отображения авто-значений)

        /// Валидация ввода: только цифры, точка, запятая
        private void Lambda_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
                return;
            }
            var tb = (TextBox)sender;
            if ((e.KeyChar == '.' || e.KeyChar == ',') && tb.Text.IndexOfAny(new[] { '.', ',' }) >= 0)
                e.Handled = true;
        }

        /// Универсальный метод парсинга чисел (точка и запятая)
        private bool TryParseDouble(string text, out double result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();

            if (double.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out result)) return true;
            if (double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result)) return true;
            if (double.TryParse(text.Replace('.', ','), NumberStyles.Number, CultureInfo.CurrentCulture, out result)) return true;
            if (double.TryParse(text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result)) return true;
            return false;
        }

        /// Кнопка «Старт» — запуск или продолжение симуляции
        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                if (currentTime == 0.0)
                {
                    if (!ReadAndValidateParameters())
                        return;
                    totalDuration = 0;
                    exactStateDuration = new double[3];
                    exactTotalDuration = 0;
                    dayStates.Clear();
                    flowWeatherHistory.Controls.Clear();
                }
                else
                {
                    // Повторный запуск: перечитываем параметры и обновляем диагональ
                    UpdateQFromInputs();
                }

                if (!int.TryParse(txtSpeed.Text, out int interval) || interval < 10)
                    interval = 10;
                isRunning = true;

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                SetInputEnabled(false);

                await RunSimulationAsync();
            }
        }

        /// Перечитать интенсивности из текстовых полей и обновить матрицу Q + диагональ в UI.
        /// Вызывается при повторном запуске (после Стоп), чтобы изменения вступили в силу.
        private void UpdateQFromInputs()
        {
            if (!TryParseDouble(txtLambda12.Text, out double l12)) l12 = 0;
            if (!TryParseDouble(txtLambda13.Text, out double l13)) l13 = 0;
            if (!TryParseDouble(txtLambda21.Text, out double l21)) l21 = 0;
            if (!TryParseDouble(txtLambda23.Text, out double l23)) l23 = 0;
            if (!TryParseDouble(txtLambda31.Text, out double l31)) l31 = 0;
            if (!TryParseDouble(txtLambda32.Text, out double l32)) l32 = 0;

            // Матрица Q
            Q = new double[3, 3];
            Q[0, 0] = -(l12 + l13); Q[0, 1] = l12; Q[0, 2] = l13;
            Q[1, 0] = l21; Q[1, 1] = -(l21 + l23); Q[1, 2] = l23;
            Q[2, 0] = l31; Q[2, 1] = l32; Q[2, 2] = -(l31 + l32);

            lambda = new double[3];
            lambda[0] = l12 + l13;
            lambda[1] = l21 + l23;
            lambda[2] = l31 + l32;

            // Пересчитываем стационарное распределение
            CalculateTheoreticalDistribution();

            // Обновляем диагональ в UI
            if (qDiagCells != null)
            {
                qDiagCells[0].Text = Q[0, 0].ToString("F2");
                qDiagCells[1].Text = Q[1, 1].ToString("F2");
                qDiagCells[2].Text = Q[2, 2].ToString("F2");
            }
        }

        /// Считывание и валидация параметров интенсивностей
        private bool ReadAndValidateParameters()
        {
            try
            {
                double l12, l13, l21, l23, l31, l32;
                if (!TryParseDouble(txtLambda12.Text, out l12) || l12 < 0) throw new Exception("λ(1→2) должна быть ≥ 0");
                if (!TryParseDouble(txtLambda13.Text, out l13) || l13 < 0) throw new Exception("λ(1→3) должна быть ≥ 0");
                if (!TryParseDouble(txtLambda21.Text, out l21) || l21 < 0) throw new Exception("λ(2→1) должна быть ≥ 0");
                if (!TryParseDouble(txtLambda23.Text, out l23) || l23 < 0) throw new Exception("λ(2→3) должна быть ≥ 0");
                if (!TryParseDouble(txtLambda31.Text, out l31) || l31 < 0) throw new Exception("λ(3→1) должна быть ≥ 0");
                if (!TryParseDouble(txtLambda32.Text, out l32) || l32 < 0) throw new Exception("λ(3→2) должна быть ≥ 0");

                // Матрица интенсивностей Q
                Q = new double[3, 3];
                Q[0, 0] = -(l12 + l13); Q[0, 1] = l12; Q[0, 2] = l13;
                Q[1, 0] = l21; Q[1, 1] = -(l21 + l23); Q[1, 2] = l23;
                Q[2, 0] = l31; Q[2, 1] = l32; Q[2, 2] = -(l31 + l32);

                // Обновляем диагональные ячейки в UI
                if (qDiagCells != null)
                {
                    qDiagCells[0].Text = Q[0, 0].ToString("F2");
                    qDiagCells[1].Text = Q[1, 1].ToString("F2");
                    qDiagCells[2].Text = Q[2, 2].ToString("F2");
                }

                // Суммарные интенсивности выхода из каждого состояния
                lambda = new double[3];
                lambda[0] = l12 + l13;
                lambda[1] = l21 + l23;
                lambda[2] = l31 + l32;

                if (lambda[0] == 0 && lambda[1] == 0 && lambda[2] == 0)
                    throw new Exception("Все интенсивности равны 0 — переходы невозможны");

                // Расчёт теоретического стационарного распределения
                CalculateTheoreticalDistribution();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка параметров: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// Расчёт стационарного распределения: π·Q = 0, Σπᵢ = 1
        /// для 3-х состояний
        private void CalculateTheoreticalDistribution()
        {
            double l12 = Q[0, 1], l13 = Q[0, 2];
            double l21 = Q[1, 0], l23 = Q[1, 2];
            double l31 = Q[2, 0], l32 = Q[2, 1];

            double A = (l12 + l13);
            double B = (l21 + l23);

            // Выражается из уравнений 
            double k = (A * B - l12 * l21) / (l31 * B + l21 * l32);
            double m = (A - k * l31) / l21;
            double sum = 1 + m + k;

            // Из условия нормировки 
            double pi1 = 1 / sum;
            double pi2 = m * pi1;
            double pi3 = k * pi1;

            theoProbs.Clear();
            theoProbs.Add(pi1);
            theoProbs.Add(pi2);
            theoProbs.Add(pi3);
        }

        /// <summary>
        /// Кнопка «Стоп» — остановка симуляции
        /// </summary>
        private void BtnStop_Click(object sender, EventArgs e)
        {

            isRunning = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            SetInputEnabled(true);
        }

        /// <summary>
        /// Кнопка «Сброс» — полный сброс
        /// </summary>
        private void BtnReset_Click(object sender, EventArgs e)
        {

            isRunning = false;
            currentState = 1;
            currentTime = 0;
            totalTimeInState = null;
            transitionHistory.Clear();

            totalDuration = 0;
            exactStateDuration = null;
            exactTotalDuration = 0;
            dayStates.Clear();
            theoProbs.Clear();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            SetInputEnabled(true);

            lblCurrentState.Text = "День 0 — Ясно";
            picWeather.Image = weatherImages[0];
            pnlWeatherCurrent.BackColor = Color.WhiteSmoke;
            lblResults.Text = "";
            flowWeatherHistory.Controls.Clear();

            // Сброс диагональных ячеек
            if (qDiagCells != null)
            {
                qDiagCells[0].Text = "auto";
                qDiagCells[1].Text = "auto";
                qDiagCells[2].Text = "auto";
            }

            chart.Series["Эмпирические"].Points.Clear();
            chart.Series["Теоретические"].Points.Clear();
        }

        /// <summary>
        /// Включение/отключение элементов ввода
        /// </summary>
        private void SetInputEnabled(bool enabled)
        {
            txtLambda12.Enabled = enabled;
            txtLambda13.Enabled = enabled;
            txtLambda21.Enabled = enabled;
            txtLambda23.Enabled = enabled;
            txtLambda31.Enabled = enabled;
            txtLambda32.Enabled = enabled;
            txtSimDays.Enabled = enabled;
            txtSpeed.Enabled = enabled;
            btnStart.Enabled = enabled;
        }

        /// <summary>
        /// Добавление маленькой иконки погоды в ленту истории
        /// </summary>
        private void AddWeatherIconToHistory(int state)
        {
            // День = целая часть времени + 1 (дни нумеруются с 1)
            int dayCount = (int)Math.Floor(currentTime) + 1;
            // Маленькая иконка 32x32
            PictureBox icon = new PictureBox
            {
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = weatherImages[state - 1],
                Margin = new Padding(1),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Тултип с номером дня
            icon.SetToolTipText($"День {dayCount}: {stateNames[state - 1]}");

            flowWeatherHistory.Controls.Add(icon);

            // Автопрокрутка вправо к последней иконке
            flowWeatherHistory.ScrollControlIntoView(icon);

            // Ограничиваем: последние 30 иконок (без мерцания)
            if (flowWeatherHistory.Controls.Count > 30)
            {
                flowWeatherHistory.SuspendLayout();
                while (flowWeatherHistory.Controls.Count > 30)
                    flowWeatherHistory.Controls.RemoveAt(0);
                flowWeatherHistory.ResumeLayout();
            }
        }


        private async Task RunSimulationAsync()
        {
            while (currentTime < simulationEndTime && isRunning)
            {
                int currentStateIndex = currentState - 1;

                // 1. Генерируем время пребывания ~ Exp(λ)
                // Формула: τ = -ln(U) / λ, где U ~ Uniform(0,1)
                double holdingTime = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda[currentStateIndex];

                // 2. Algorithm 2: накапливаем точное время в состоянии
                totalTimeInState[currentStateIndex] += holdingTime;

                // 3. Продвигаем непрерывное время
                currentTime += holdingTime;

                if (currentTime > simulationEndTime) break;

                // 4. Выбираем следующее состояние по вероятностям перехода
                // P(i→j) = Q[i,j] / λ[i] для j ≠ i
                double r = rand.NextDouble();
                double cumulative = 0;
                int nextState = currentState;

                for (int j = 0; j < 3; j++)
                {
                    if (j == currentStateIndex) continue;
                    double transProb = Q[currentStateIndex, j] / lambda[currentStateIndex];
                    cumulative += transProb;
                    if (r < cumulative)
                    {
                        nextState = j + 1;
                        break;
                    }
                }

                // 5. Записываем событие перехода
                currentState = nextState;
                transitionHistory.Add(new TransitionRecord { Time = currentTime, State = currentState });

                // 6. Обновляем UI
                UpdateCurrentStateDisplay();
                UpdateTransitionList();
                UpdateStatistics();
                UpdateHistogram();

                // 7. Пауза для визуализации (не влияет на математику!)
                await Task.Delay(int.Parse(txtSpeed.Text));
            }
        }
        private void UpdateStatistics()
        {
            double total = totalTimeInState[0] + totalTimeInState[1] + totalTimeInState[2];
            if (total == 0) return;

            string stats = "=== АЛГОРИТМ 2 ===\n\n";
            stats += $"Всего времени: {total:F2} дней\n";
            stats += $"Переходов: {transitionHistory.Count}\n\n";
            stats += "Распределение времени:\n";

            for (int i = 0; i < 3; i++)
            {
                // Эмпирическая вероятность = время в состоянии / общее время
                double pct = totalTimeInState[i] / total * 100;
                stats += $"  {stateNames[i],-8}: {totalTimeInState[i],6:F2} дн ({pct,5:F1}%)\n";
            }

            // Сравнение с теоретическим стационарным распределением
            if (theoProbs.Count == 3)
            {
                stats += "\n=== СРАВНЕНИЕ ===\n";
                stats += "Сост.   | Эмпир. | Теор.  | Ошибка\n";
                stats += "--------|--------|--------|-------\n";

                for (int i = 0; i < 3; i++)
                {
                    double empP = totalTimeInState[i] / total;
                    double theoP = theoProbs[i];
                    double err = Math.Abs(empP - theoP) * 100;
                    stats += $"{stateNames[i],-7}| {empP,6:F3}  | {theoP,6:F3}  | {err,5:F2}%\n";
                }
            }

            lblResults.Text = stats;
        }

        private void UpdateTransitionList()
        {
            // Очищаем ленту и добавляем последние 30 переходов как иконки
            flowWeatherHistory.Controls.Clear();

            int startIndex = Math.Max(0, transitionHistory.Count - 30);

            for (int i = startIndex; i < transitionHistory.Count; i++)
            {
                var record = transitionHistory[i];
                int day = (int)Math.Floor(record.Time) + 1;

                // Создаём иконку с тултипом
                PictureBox icon = new PictureBox
                {
                    Size = new Size(32, 32),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = weatherImages[record.State - 1],
                    Margin = new Padding(1),
                    BorderStyle = BorderStyle.FixedSingle
                };
                icon.SetToolTipText($"t={record.Time:F2} | День {day} | {record.StateName}");

                flowWeatherHistory.Controls.Add(icon);
            }

            // Автопрокрутка вправо
            if (flowWeatherHistory.Controls.Count > 0)
                flowWeatherHistory.ScrollControlIntoView(flowWeatherHistory.Controls[^1]);
        }

        /// <summary>
        /// Обновление индикатора текущего состояния (изображение + текст)
        /// </summary>
        private void UpdateCurrentStateDisplay()
        {
            // День = целая часть времени + 1 (дни нумеруются с 1)
            int currentDay = (int)Math.Floor(currentTime) + 1;

            // Обновляем большую иконку погоды
            picWeather.Image = weatherImages[currentState - 1];

            lblCurrentState.Text = $"День {currentDay}\n{stateNames[currentState - 1]}";

            // Меняем фон панели в цвет состояния
            pnlWeatherCurrent.BackColor = stateColors[currentState - 1];
        }

        /// <summary>
        /// Обновление гистограммы: эмпирическое vs теоретическое распределение
        /// </summary>
        private void UpdateHistogram()
        {
            if (exactTotalDuration == 0) return;

            chart.Series["Эмпирические"].Points.Clear();
            chart.Series["Теоретические"].Points.Clear();

            for (int i = 0; i < 3; i++)
            {
                double empFreq = exactStateDuration[i] / exactTotalDuration;
                chart.Series["Эмпирические"].Points.AddXY(i + 1, empFreq);

                if (theoProbs.Count == 3)
                {
                    chart.Series["Теоретические"].Points.AddXY(i + 1, theoProbs[i]);
                }
            }

            // Подписи на оси X с изображениями-ключами
            chart.ChartAreas["MainArea"].AxisX.CustomLabels.Clear();
            for (int i = 0; i < 3; i++)
            {
                var cl = new CustomLabel(i + 0.5, i + 1.5, stateNames[i], 0, LabelMarkStyle.None);
                chart.ChartAreas["MainArea"].AxisX.CustomLabels.Add(cl);
            }
            chart.ChartAreas["MainArea"].AxisX.Minimum = 0.5;
            chart.ChartAreas["MainArea"].AxisX.Maximum = 3.5;
        }

        /// <summary>
        /// Обновление текстовых результатов
        /// </summary>
        private void UpdateResults()
        {
            // День = целая часть времени + 1 (дни нумеруются с 1)
            int dayCount = (int)Math.Floor(currentTime) + 1;

            if (exactTotalDuration == 0)
            {
                lblResults.Text = "Нет данных. Запустите симуляцию.";
                return;
            }

            string res = "=== СТАТИСТИКА ===\n\n";
            res += $"Всего дней: {dayCount}\n\n";

            res += "Распределение (по точному времени):\n";
            for (int i = 0; i < 3; i++)
            {
                double pct = exactStateDuration[i] / exactTotalDuration * 100;
                res += $"  {stateNames[i]}: {exactStateDuration[i]:F2} дн. ({pct:F1}%)\n";
            }

            if (theoProbs.Count == 3)
            {
                res += "\n=== СРАВНЕНИЕ ===\n\n";
                res += "Сост.   | Эмп.  | Теор. | Погр.\n";
                res += "--------|-------|-------|-------\n";
                for (int i = 0; i < 3; i++)
                {
                    double empP = exactStateDuration[i] / exactTotalDuration;
                    double theoP = theoProbs[i];
                    double err = Math.Abs(empP - theoP);
                    res += $"  {stateNames[i],-6}| {empP,4:F3}  | {theoP,4:F3}  | {err,4:F3}\n";
                }
            }

            res += "\n=== МАТРИЦА Q ===\n";
            for (int i = 0; i < 3; i++)
            {
                res += $"  [{Q[i, 0],6:F2}] [{Q[i, 1],6:F2}] [{Q[i, 2],6:F2}]\n";
            }

            lblResults.Text = res;
        }

        /// <summary>
        /// Экспорт результатов в CSV файл
        /// </summary>
        private void BtnExportCSV_Click(object sender, EventArgs e)
        {
            // День = целая часть времени + 1 (дни нумеруются с 1)
            int dayCount = (int)Math.Floor(currentTime) + 1;

            if (exactTotalDuration == 0)
            {
                MessageBox.Show("Сначала проведите симуляцию.", "Нет данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt",
                    FileName = $"lab7_weather_{dayCount}days.csv",
                    DefaultExt = "csv"
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                using (var writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("Лабораторная работа №7 — Марковская модель погоды");
                    writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    writer.WriteLine($"Дней моделирования: {dayCount}");
                    writer.WriteLine();

                    // Матрица Q
                    writer.WriteLine("Матрица интенсивностей Q:");
                    writer.WriteLine($";{stateNames[0]};{stateNames[1]};{stateNames[2]}");
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write($"{stateNames[i]}");
                        for (int j = 0; j < 3; j++)
                            writer.Write($";{Q[i, j]:F4}");
                        writer.WriteLine();
                    }
                    writer.WriteLine();

                    // Стационарное распределение
                    writer.WriteLine("Стационарное распределение:");
                    if (theoProbs.Count == 3)
                    {
                        for (int i = 0; i < 3; i++)
                            writer.WriteLine($"{stateNames[i]};{theoProbs[i]:F6}");
                    }
                    else
                    {
                        writer.WriteLine("Не рассчитано");
                    }
                    writer.WriteLine();

                    // Эмпирическое распределение
                    writer.WriteLine("Эмпирическое распределение:");
                    writer.WriteLine($"Состояние;Точн. время;Доля;Теорет. доля;Абс. погрешность");
                    for (int i = 0; i < 3; i++)
                    {
                        double empP = exactStateDuration[i] / exactTotalDuration;
                        double theoP = (theoProbs.Count == 3) ? theoProbs[i] : 0;
                        double err = Math.Abs(empP - theoP);
                        writer.WriteLine($"{stateNames[i]};{exactStateDuration[i]:F4};{empP:F6};{theoP:F6};{err:F6}");
                    }
                    writer.WriteLine();

                    // Последовательность состояний
                    writer.WriteLine("День;Состояние;Название");
                    int exportLimit = Math.Min(dayStates.Count, 1000);
                    for (int d = 0; d < exportLimit; d++)
                    {
                        int s = dayStates[d];
                        if (s >= 1 && s <= 3)
                            writer.WriteLine($"{d + 1};{s};{stateNames[s - 1]}");
                    }
                }

                MessageBox.Show($"Данные сохранены в:\n{sfd.FileName}", "Экспорт завершён",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта:\n{ex.GetType().Name}: {ex.Message}\n\nStack:\n{ex.StackTrace}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// FlowLayoutPanel с двойной буферизацией — убирает мерцание при добавлении/удалении контролов
    /// </summary>
    public class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public BufferedFlowLayoutPanel()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }

    /// Extension-метод для тултипа на PictureBox
    /// </summary>
    public static class ControlExtensions
    {
        private static ToolTip sharedToolTip;

        public static void SetToolTipText(this Control control, string text)
        {
            if (sharedToolTip == null)
                sharedToolTip = new ToolTip();
            sharedToolTip.SetToolTip(control, text);
        }
    }
}
