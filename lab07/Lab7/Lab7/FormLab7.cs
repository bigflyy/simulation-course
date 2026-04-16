using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimulationLabs
{
    /// <summary>
    /// Форма симуляции марковской модели погоды.
    /// 3 состояния: 1 — ясно, 2 — облачно, 3 — пасмурно.
    /// Моделирование в реальном времени с визуализацией.
    /// </summary>
    public class FormLab7 : Form
    {
        // Элементы управления
        private TextBox txtLambda12, txtLambda13, txtLambda21, txtLambda23, txtLambda31, txtLambda32;
        private TextBox txtSimDays, txtSpeed;
        private Button btnStart, btnStop, btnReset, btnClose, btnExportCSV;
        private Chart chart;
        private Label lblResults, lblCurrentState;
        private System.Windows.Forms.Timer simTimer;

        // Данные модели
        private double[,] Q;          // Матрица интенсивностей (генератор Q)
        private double[] lambda;      // Суммарные интенсивности выхода из состояний
        private int currentState = 1; // Текущее состояние (1, 2, 3)
        private double simTime = 0;   // Текущее время симуляции (в днях)
        private int dayCount = 0;     // Счётчик дней
        private long[] stateDuration; // Накопленное время в каждом состоянии (в тиках таймера)
        private long totalDuration = 0; // Общее накопленное время
        private bool isRunning = false;
        private Random rand = new Random();

        // Списки для графика
        private List<int> dayStates = new List<int>(); // Состояние по дням
        private List<double> theoProbs = new List<double>(); // Теоретические вероятности

        // Названия состояний
        private string[] stateNames = { "Ясно", "Облачно", "Пасмурно" };
        private Color[] stateColors = { Color.Yellow, Color.LightGray, Color.DarkGray };

        public FormLab7()
        {
            InitializeComponent();
            CreateManualUI();
            InitDefaultIntensities();
        }

        private void InitializeComponent()
        {
            this.Text = "Лаб 07: Марковская модель погоды";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        /// <summary>
        /// Инициализация значений интенсивностей по умолчанию (пример из методички)
        /// lambda_12 = 0.3, lambda_13 = 0.1, lambda_21 = 0.2, lambda_23 = 0.4, lambda_31 = 0.1, lambda_32 = 0.2
        /// </summary>
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

        /// <summary>
        /// Создание пользовательского интерфейса
        /// </summary>
        private void CreateManualUI()
        {
            // === Панель ввода интенсивностей ===
            Panel pnlInput = new Panel { Location = new Point(10, 10), Size = new Size(280, 300), BorderStyle = BorderStyle.FixedSingle };

            Label lblTitle = new Label
            {
                Text = "Интенсивности переходов",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlInput.Controls.Add(lblTitle);

            // Матрица интенсивностей: текстовые поля
            string[] labels = {
                "λ(1→2):", "λ(1→3):",
                "λ(2→1):", "λ(2→3):",
                "λ(3→1):", "λ(3→2):"
            };
            txtLambda12 = new TextBox { Location = new Point(100, 40), Size = new Size(70, 20) };
            txtLambda13 = new TextBox { Location = new Point(200, 40), Size = new Size(70, 20) };
            txtLambda21 = new TextBox { Location = new Point(100, 65), Size = new Size(70, 20) };
            txtLambda23 = new TextBox { Location = new Point(200, 65), Size = new Size(70, 20) };
            txtLambda31 = new TextBox { Location = new Point(100, 90), Size = new Size(70, 20) };
            txtLambda32 = new TextBox { Location = new Point(200, 90), Size = new Size(70, 20) };

            TextBox[] allLambda = { txtLambda12, txtLambda13, txtLambda21, txtLambda23, txtLambda31, txtLambda32 };

            for (int i = 0; i < labels.Length; i++)
            {
                Label lbl = new Label { Text = labels[i], Location = new Point((i % 2 == 0) ? 10 : 110, 42 + (i / 2) * 25 - 2), AutoSize = true };
                pnlInput.Controls.Add(lbl);
                pnlInput.Controls.Add(allLambda[i]);
                allLambda[i].KeyPress += Lambda_KeyPress; // Только цифры и точка/запятая
            }

            // Подписи состояний
            Label lblState1 = new Label { Text = "1: Ясно", Location = new Point(10, 120), AutoSize = true, ForeColor = Color.DarkGoldenrod, Font = new Font("Arial", 8, FontStyle.Bold) };
            Label lblState2 = new Label { Text = "2: Облачно", Location = new Point(10, 138), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Arial", 8, FontStyle.Bold) };
            Label lblState3 = new Label { Text = "3: Пасмурно", Location = new Point(10, 156), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Arial", 8, FontStyle.Bold) };
            pnlInput.Controls.Add(lblState1);
            pnlInput.Controls.Add(lblState2);
            pnlInput.Controls.Add(lblState3);

            // Количество дней симуляции
            Label lblDays = new Label { Text = "Дней моделирования:", Location = new Point(10, 185), AutoSize = true };
            pnlInput.Controls.Add(lblDays);
            txtSimDays = new TextBox { Location = new Point(10, 205), Size = new Size(100, 20), Text = "365" };
            pnlInput.Controls.Add(txtSimDays);

            // Скорость симуляции
            Label lblSpeed = new Label { Text = "Скорость (мс):", Location = new Point(120, 185), AutoSize = true };
            pnlInput.Controls.Add(lblSpeed);
            txtSpeed = new TextBox { Location = new Point(120, 205), Size = new Size(80, 20), Text = "100" };
            pnlInput.Controls.Add(txtSpeed);

            // Кнопки управления
            btnStart = new Button { Text = "Старт", Location = new Point(10, 235), Size = new Size(75, 28), BackColor = Color.LightGreen };
            btnStart.Click += BtnStart_Click;
            pnlInput.Controls.Add(btnStart);

            btnStop = new Button { Text = "Стоп", Location = new Point(90, 235), Size = new Size(75, 28), BackColor = Color.LightCoral, Enabled = false };
            btnStop.Click += BtnStop_Click;
            pnlInput.Controls.Add(btnStop);

            btnReset = new Button { Text = "Сброс", Location = new Point(170, 235), Size = new Size(75, 28) };
            btnReset.Click += BtnReset_Click;
            pnlInput.Controls.Add(btnReset);

            btnExportCSV = new Button { Text = "Экспорт CSV", Location = new Point(10, 268), Size = new Size(120, 28), BackColor = Color.LightYellow };
            btnExportCSV.Click += BtnExportCSV_Click;
            pnlInput.Controls.Add(btnExportCSV);

            btnClose = new Button { Text = "Закрыть", Location = new Point(140, 268), Size = new Size(105, 28) };
            btnClose.Click += (s, e) => this.Close();
            pnlInput.Controls.Add(btnClose);

            this.Controls.Add(pnlInput);

            // === Индикатор текущего состояния ===
            pnlCurrentState = new Panel { Location = new Point(300, 10), Size = new Size(770, 60), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightYellow };
            lblCurrentState = new Label
            {
                Text = "Текущее состояние: Ясно (День 0)",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            pnlCurrentState.Controls.Add(lblCurrentState);
            this.Controls.Add(pnlCurrentState);

            // === График состояний по дням ===
            chart = new Chart { Location = new Point(300, 80), Size = new Size(770, 300) };
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.BackColor = Color.White;
            ca.AxisX.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            ca.AxisX.Title = "День";
            ca.AxisY.Title = "Состояние";
            ca.AxisY.Minimum = 0.5;
            ca.AxisY.Maximum = 3.5;
            ca.AxisY.Interval = 1;
            chart.ChartAreas.Add(ca);

            // Серия для отображения состояний по дням
            Series sStates = new Series("Состояния");
            sStates.ChartType = SeriesChartType.Point;
            sStates.MarkerSize = 8;
            sStates.MarkerStyle = MarkerStyle.Circle;
            chart.Series.Add(sStates);

            this.Controls.Add(chart);

            // === Гистограмма распределения ===
            chartHist = new Chart { Location = new Point(300, 390), Size = new Size(380, 280) };
            chartHist.ChartAreas.Clear();
            chartHist.Series.Clear();

            ChartArea caHist = new ChartArea("HistArea");
            caHist.BackColor = Color.White;
            caHist.AxisX.MajorGrid.LineColor = Color.LightGray;
            caHist.AxisY.MajorGrid.LineColor = Color.LightGray;
            caHist.AxisY.Minimum = 0;
            caHist.AxisY.Maximum = 1.0;
            chartHist.ChartAreas.Add(caHist);

            Series sEmp = new Series("Эмпирические");
            sEmp.ChartType = SeriesChartType.Column;
            sEmp.Color = Color.SteelBlue;
            sEmp.BorderColor = Color.Navy;
            sEmp.BorderWidth = 1;
            chartHist.Series.Add(sEmp);

            Series sTheo = new Series("Теоретические");
            sTheo.ChartType = SeriesChartType.Column;
            sTheo.Color = Color.OrangeRed;
            sTheo.BorderColor = Color.DarkRed;
            sTheo.BorderWidth = 1;
            chartHist.Series.Add(sTheo);

            this.Controls.Add(chartHist);

            // === Панель результатов ===
            lblResults = new Label
            {
                Location = new Point(690, 390),
                Size = new Size(380, 280),
                Font = new Font("Arial", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblResults);

            // Таймер симуляции
            simTimer = new System.Windows.Forms.Timer();
            simTimer.Tick += SimTimer_Tick;
        }

        private Panel pnlCurrentState;
        private Chart chartHist;

        /// <summary>
        /// Валидация ввода: только цифры, точка, запятая
        /// </summary>
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

        /// <summary>
        /// Универсальный метод парсинга чисел (точка и запятая)
        /// </summary>
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

        /// <summary>
        /// Кнопка «Старт» — запуск или продолжение симуляции
        /// </summary>
        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                // При первом запуске считываем параметры
                if (stateDuration == null)
                {
                    if (!ReadAndValidateParameters())
                        return;
                    stateDuration = new long[3];
                    totalDuration = 0;
                    dayStates.Clear();
                    theoProbs.Clear();
                }

                // Определяем интервал таймера
                if (!int.TryParse(txtSpeed.Text, out int interval) || interval < 10)
                    interval = 100;
                simTimer.Interval = interval;
                simTimer.Start();
                isRunning = true;

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                // Блокируем поля ввода во время симуляции
                SetInputEnabled(false);
            }
        }

        /// <summary>
        /// Считывание и валидация параметров интенсивностей
        /// </summary>
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
                // Диагональные элементы = -(сумма недиагональных в строке)
                Q = new double[3, 3];
                Q[0, 0] = -(l12 + l13); Q[0, 1] = l12; Q[0, 2] = l13;
                Q[1, 0] = l21; Q[1, 1] = -(l21 + l23); Q[1, 2] = l23;
                Q[2, 0] = l31; Q[2, 1] = l32; Q[2, 2] = -(l31 + l32);

                // Суммарные интенсивности выхода из каждого состояния
                lambda = new double[3];
                lambda[0] = l12 + l13;
                lambda[1] = l21 + l23;
                lambda[2] = l31 + l32;

                // Проверка: если все intensities = 0, модель не работает
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

        /// <summary>
        /// Расчёт стационарного распределения из уравнения pi * Q = 0 при условии sum(pi) = 1
        /// Решаем систему линейных уравнений методом Гаусса
        /// </summary>
        private void CalculateTheoreticalDistribution()
        {
            // Система: pi * Q = 0, sum(pi) = 1
            // Заменяем последнее уравнение на pi_1 + pi_2 + pi_3 = 1
            // Получаем систему 3x3

            double[,] A = new double[3, 3];
            // Уравнения из pi * Q = 0 (транспонируем Q)
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    A[j, i] = Q[i, j]; // Транспонирование

            // Заменяем последнюю строку на 1, 1, 1 (условие нормировки)
            A[2, 0] = 1; A[2, 1] = 1; A[2, 2] = 1;

            // Правая часть: 0, 0, 1
            double[] b = { 0, 0, 1 };

            // Решаем методом Гаусса
            double[] pi = SolveLinearSystem(A, b);

            if (pi != null)
            {
                theoProbs.Add(pi[0]);
                theoProbs.Add(pi[1]);
                theoProbs.Add(pi[2]);
            }
        }

        /// <summary>
        /// Решение системы линейных уравнений Ax = b методом Гаусса
        /// </summary>
        private double[] SolveLinearSystem(double[,] A, double[] b)
        {
            int n = 3;
            double[,] aug = new double[n, n + 1];

            // Формируем расширенную матрицу
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = A[i, j];
                aug[i, n] = b[i];
            }

            // Прямой ход
            for (int col = 0; col < n; col++)
            {
                // Поиск ведущего элемента
                int maxRow = col;
                for (int row = col + 1; row < n; row++)
                    if (Math.Abs(aug[row, col]) > Math.Abs(aug[maxRow, col]))
                        maxRow = row;

                // Обмен строк
                for (int j = 0; j <= n; j++)
                {
                    double tmp = aug[col, j];
                    aug[col, j] = aug[maxRow, j];
                    aug[maxRow, j] = tmp;
                }

                if (Math.Abs(aug[col, col]) < 1e-12)
                    return null; // Сингулярная матрица

                // Нормализация
                for (int j = col; j <= n; j++)
                    aug[col, j] /= aug[col, col];

                // Исключение
                for (int row = 0; row < n; row++)
                {
                    if (row != col)
                    {
                        double factor = aug[row, col];
                        for (int j = col; j <= n; j++)
                            aug[row, j] -= factor * aug[col, j];
                    }
                }
            }

            // Решение
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
                x[i] = aug[i, n];

            return x;
        }

        /// <summary>
        /// Кнопка «Стоп» — остановка симуляции
        /// </summary>
        private void BtnStop_Click(object sender, EventArgs e)
        {
            simTimer.Stop();
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
            simTimer.Stop();
            isRunning = false;
            currentState = 1;
            simTime = 0;
            dayCount = 0;
            stateDuration = null;
            totalDuration = 0;
            dayStates.Clear();
            theoProbs.Clear();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            SetInputEnabled(true);

            lblCurrentState.Text = "Текущее состояние: Ясно (День 0)";
            pnlCurrentState.BackColor = Color.LightYellow;
            lblResults.Text = "";

            chart.Series["Состояния"].Points.Clear();
            chartHist.Series["Эмпирические"].Points.Clear();
            chartHist.Series["Теоретические"].Points.Clear();
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
            btnStart.Enabled = enabled;
        }

        /// <summary>
        /// Один шаг симуляции (вызывается таймером)
        /// Алгоритм непрерывного марковского процесса:
        /// 1. Из текущего состояния i генерируем время до перехода ~ Exp(lambda_i)
        /// 2. Выбираем следующее состояние j с вероятностью q_ij / lambda_i
        /// 3. Обновляем статистику
        /// </summary>
        private void SimTimer_Tick(object sender, EventArgs e)
        {
            if (!int.TryParse(txtSimDays.Text, out int maxDays) || maxDays <= 0)
                maxDays = 365;

            // Проверяем, достигнут ли лимит дней
            if (dayCount >= maxDays)
            {
                simTimer.Stop();
                isRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                SetInputEnabled(true);
                MessageBox.Show("Симуляция завершена: достигнут лимит " + maxDays + " дней.", "Завершено",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateResults();
                return;
            }

            // Текущее состояние (0-based индекс)
            int i = currentState - 1;

            // Если lambda[i] = 0, состояние поглощающее — остаёмся в нём
            if (lambda[i] == 0)
            {
                stateDuration[i]++;
                totalDuration++;
                dayCount++;
                dayStates.Add(currentState);
                AddStatePoint(dayCount, currentState);
                UpdateCurrentStateDisplay();
                return;
            }

            // Генерируем время до следующего перехода ~ Exp(lambda_i)
            // Формула: delta_t = -ln(U) / lambda_i, где U ~ Uniform(0, 1)
            double u = Math.Max(rand.NextDouble(), 1e-10); // Защита от ln(0)
            double deltaT = -Math.Log(u) / lambda[i];

            // Округляем до целого количества «тиков» (1 тик = 1 день для простоты)
            // Для пошаговой визуализации: 1 тик таймера = 1 день
            int daysInState = Math.Max(1, (int)Math.Round(deltaT));

            // Накопление длительности
            stateDuration[i] += daysInState;
            totalDuration += daysInState;

            // Выбираем следующее состояние
            // Вероятность перехода i→j = q_ij / lambda_i (для j != i)
            double r = rand.NextDouble();
            double cumulative = 0;
            int nextState = i + 1; // По умолчанию остаёмся

            for (int j = 0; j < 3; j++)
            {
                if (j == i) continue; // Пропускаем диагональ
                double transProb = Q[i, j] / lambda[i];
                cumulative += transProb;
                if (r < cumulative)
                {
                    nextState = j + 1; // 1-based
                    break;
                }
            }

            // Записываем состояние для каждого прошедшего дня
            for (int d = 0; d < daysInState && dayCount < maxDays; d++)
            {
                dayCount++;
                dayStates.Add(currentState);
                AddStatePoint(dayCount, currentState);
            }

            // Переходим в новое состояние
            currentState = nextState;
            simTime += daysInState;

            // Обновляем интерфейс
            UpdateCurrentStateDisplay();
            UpdateHistogram();

            // Если достигли лимита — останавливаемся
            if (dayCount >= maxDays)
            {
                simTimer.Stop();
                isRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                SetInputEnabled(true);
                MessageBox.Show("Симуляция завершена: " + maxDays + " дней.", "Завершено",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateResults();
        }

        /// <summary>
        /// Добавление точки на график состояний
        /// </summary>
        private void AddStatePoint(int day, int state)
        {
            // Ограничиваем количество точек на графике для производительности
            if (dayStates.Count > 5000) return;

            var point = new DataPoint(day, state);
            // Раскраска точек по состоянию
            point.Color = stateColors[state - 1];
            point.BorderWidth = 1;
            point.BorderColor = Color.Black;
            point.MarkerSize = 6;
            chart.Series["Состояния"].Points.Add(point);
        }

        /// <summary>
        /// Обновление индикатора текущего состояния
        /// </summary>
        private void UpdateCurrentStateDisplay()
        {
            lblCurrentState.Text = $"Текущее состояние: {stateNames[currentState - 1]} (День {dayCount})";
            pnlCurrentState.BackColor = stateColors[currentState - 1];
        }

        /// <summary>
        /// Обновление гистограммы: эмпирическое vs теоретическое распределение
        /// </summary>
        private void UpdateHistogram()
        {
            if (totalDuration == 0) return;

            chartHist.Series["Эмпирические"].Points.Clear();
            chartHist.Series["Теоретические"].Points.Clear();

            for (int i = 0; i < 3; i++)
            {
                double empFreq = (double)stateDuration[i] / totalDuration;
                chartHist.Series["Эмпирические"].Points.AddXY(i + 1, empFreq);

                if (theoProbs.Count == 3)
                {
                    chartHist.Series["Теоретические"].Points.AddXY(i + 1, theoProbs[i]);
                }
            }

            // Подписи на оси X
            chartHist.ChartAreas["HistArea"].AxisX.CustomLabels.Clear();
            for (int i = 0; i < 3; i++)
            {
                var cl = new CustomLabel(i + 0.5, i + 1.5, stateNames[i], 0, LabelMarkStyle.None);
                chartHist.ChartAreas["HistArea"].AxisX.CustomLabels.Add(cl);
            }
            chartHist.ChartAreas["HistArea"].AxisX.Minimum = 0.5;
            chartHist.ChartAreas["HistArea"].AxisX.Maximum = 3.5;
        }

        /// <summary>
        /// Обновление текстовых результатов
        /// </summary>
        private void UpdateResults()
        {
            if (totalDuration == 0)
            {
                lblResults.Text = "Нет данных. Запустите симуляцию.";
                return;
            }

            string res = "=== СТАТИСТИКА ===\n\n";
            res += $"Всего дней: {dayCount}\n\n";

            res += "Распределение времени по состояниям:\n";
            for (int i = 0; i < 3; i++)
            {
                double pct = (double)stateDuration[i] / totalDuration * 100;
                res += $"  {stateNames[i]}: {stateDuration[i]} дн. ({pct:F1}%)\n";
            }

            if (theoProbs.Count == 3)
            {
                res += "\n=== СРАВНЕНИЕ С ТЕОРЕТИЧЕСКИМ ===\n\n";
                res += "Состояние  | Эмпир.  | Теорет. | Отн. погр.\n";
                res += "-----------|---------|---------|----------\n";

                for (int i = 0; i < 3; i++)
                {
                    double empP = (double)stateDuration[i] / totalDuration;
                    double theoP = theoProbs[i];
                    double err = (theoP != 0) ? Math.Abs((empP - theoP) / theoP) * 100 : 0;
                    res += $"  {stateNames[i],-9}| {empP,6:F3}  | {theoP,6:F3}  | {err,5:F1}%\n";
                }
            }

            res += "\n=== МАТРИЦА ИНТЕНСИВНОСТЕЙ Q ===\n";
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
            if (totalDuration == 0)
            {
                MessageBox.Show("Сначала проведите симуляцию.", "Нет данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Диалог сохранения
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
                    // Заголовок
                    writer.WriteLine("Лабораторная работа №7 — Марковская модель погоды");
                    writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    writer.WriteLine($"Дней моделирования: {dayCount}");
                    writer.WriteLine();

                    // Матрица интенсивностей
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
                    for (int i = 0; i < 3; i++)
                        writer.WriteLine($"{stateNames[i]};{theoProbs[i]:F6}");
                    writer.WriteLine();

                    // Эмпирическое распределение
                    writer.WriteLine("Эмпирическое распределение:");
                    writer.WriteLine($"Состояние;Дней;Доля;Теорет. доля;Отн. погрешность (%)");
                    for (int i = 0; i < 3; i++)
                    {
                        double empP = (double)stateDuration[i] / totalDuration;
                        double theoP = theoProbs[i];
                        double err = (theoP != 0) ? Math.Abs((empP - theoP) / theoP) * 100 : 0;
                        writer.WriteLine($"{stateNames[i]};{stateDuration[i]};{empP:F6};{theoP:F6};{err:F2}");
                    }
                    writer.WriteLine();

                    // Последовательность состояний по дням (первые 100 + последние)
                    writer.WriteLine("Последовательность состояний по дням (первые 100):");
                    writer.WriteLine("День;Состояние;Название");
                    int limit = Math.Min(100, dayStates.Count);
                    for (int d = 0; d < limit; d++)
                    {
                        int s = dayStates[d];
                        writer.WriteLine($"{d + 1};{s};{stateNames[s - 1]}");
                    }
                    if (dayStates.Count > 100)
                    {
                        writer.WriteLine("...");
                        writer.WriteLine("День;Состояние;Название");
                        for (int d = dayStates.Count - 10; d < dayStates.Count; d++)
                        {
                            int s = dayStates[d];
                            writer.WriteLine($"{d + 1};{s};{stateNames[s - 1]}");
                        }
                    }
                }

                MessageBox.Show($"Данные сохранены в:\n{sfd.FileName}", "Экспорт завершён",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
