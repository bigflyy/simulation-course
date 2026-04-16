using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimulationLabs
{
    public class FormLab6_1 : Form
    {
        private TextBox[] txtProbs;
        private TextBox txtN;
        private Button btnStart, btnClose;
        private Chart chart;
        private Label lblResults;

        public FormLab6_1()
        {
            InitializeComponent();
            CreateManualUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Лаб 06-1: Дискретная СВ";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void CreateManualUI()
        {
            Panel pnlInput = new Panel { Location = new Point(10, 10), Size = new Size(220, 280), BorderStyle = BorderStyle.FixedSingle };

            Label lblTitle = new Label { Text = "Ввод данных", Location = new Point(10, 10), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pnlInput.Controls.Add(lblTitle);

            txtProbs = new TextBox[5];
            string[] labels = { "Вер 1", "Вер 2", "Вер 3", "Вер 4", "Вер 5 (авто)" };

            for (int i = 0; i < 5; i++)
            {
                Label lbl = new Label { Text = labels[i], Location = new Point(10, 40 + i * 30), AutoSize = true };
                pnlInput.Controls.Add(lbl);

                txtProbs[i] = new TextBox { Location = new Point(90, 37 + i * 30), Size = new Size(110, 20) };
                if (i == 4)
                {
                    txtProbs[i].Text = "auto";
                    txtProbs[i].ReadOnly = true;
                    txtProbs[i].BackColor = Color.LightGray;
                }
                else
                {
                    txtProbs[i].Text = "0.2";
                    txtProbs[i].KeyPress += Prob_KeyPress;
                }
                pnlInput.Controls.Add(txtProbs[i]);
            }

            Label lblN = new Label { Text = "N (опытов)", Location = new Point(10, 195), AutoSize = true };
            pnlInput.Controls.Add(lblN);
            txtN = new TextBox { Location = new Point(10, 215), Size = new Size(100, 20), Text = "1000" };
            pnlInput.Controls.Add(txtN);

            btnStart = new Button { Text = "Старт", Location = new Point(10, 245), Size = new Size(90, 30), BackColor = Color.LightBlue };
            btnStart.Click += BtnStart_Click;
            pnlInput.Controls.Add(btnStart);

            btnClose = new Button { Text = "Закрыть", Location = new Point(110, 245), Size = new Size(90, 30) };
            btnClose.Click += (s, e) => this.Close();
            pnlInput.Controls.Add(btnClose);

            this.Controls.Add(pnlInput);

            chart = new Chart { Location = new Point(240, 10), Size = new Size(680, 350) };
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.AxisX.Minimum = 0.5;
            ca.AxisX.Maximum = 5.5;
            ca.AxisY.Minimum = 0;
            ca.AxisX.Interval = 1;
            ca.AxisX.LabelStyle.Format = "0";
            chart.ChartAreas.Add(ca);

            Series sHist = new Series("Гистограмма");
            sHist.ChartType = SeriesChartType.Column;
            sHist.Color = Color.LightBlue;
            sHist.BorderColor = Color.Red;
            sHist.BorderWidth = 1;
            chart.Series.Add(sHist);

            Series sTheory = new Series("Теория");
            sTheory.ChartType = SeriesChartType.Line;
            sTheory.Color = Color.Green;
            sTheory.BorderWidth = 2;
            chart.Series.Add(sTheory);

            this.Controls.Add(chart);

            lblResults = new Label { Location = new Point(240, 370), Size = new Size(680, 200), Font = new Font("Arial", 10) };
            this.Controls.Add(lblResults);
        }

        // Универсальный метод парсинга чисел (работает с точкой и запятой)
        private bool TryParseDouble(string text, out double result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim();

            // Пробуем парсить с текущей культурой
            if (double.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
                return true;

            // Пробуем парсить с инвариантной культурой (точка)
            if (double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return true;

            // Заменяем точку на запятую и пробуем снова
            string withComma = text.Replace('.', ',');
            if (double.TryParse(withComma, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
                return true;

            // Заменяем запятую на точку и пробуем снова
            string withDot = text.Replace(',', '.');
            if (double.TryParse(withDot, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return true;

            return false;
        }

        private void Prob_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
                return;
            }

            var tb = (TextBox)sender;
            if ((e.KeyChar == '.' || e.KeyChar == ',') && tb.Text.IndexOfAny(new[] { '.', ',' }) >= 0)
            {
                e.Handled = true;
                return;
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                double[] probs = new double[5];
                double sumP = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (!TryParseDouble(txtProbs[i].Text, out probs[i]))
                        throw new Exception($"Не удалось разобрать Вер {i + 1}: '{txtProbs[i].Text}'");

                    if (probs[i] < 0 || probs[i] > 1)
                        throw new Exception($"Вер {i + 1} = {probs[i]} должна быть от 0 до 1");

                    sumP += probs[i];
                }

                if (sumP > 1.0001) // Небольшой допуск для погрешности
                    throw new Exception($"Сумма вероятностей ({sumP:F3}) больше 1.0! Уменьшите вероятности.");

                probs[4] = Math.Max(0, 1.0 - sumP);

                if (!int.TryParse(txtN.Text, out int N) || N <= 0)
                    throw new Exception("N должно быть положительным целым числом");

                // Теоретические характеристики
                double theoMean = 0;
                for (int i = 0; i < 5; i++) theoMean += (i + 1) * probs[i];

                double theoVar = 0;
                for (int i = 0; i < 5; i++)
                    theoVar += (i + 1) * (i + 1) * probs[i];
                theoVar -= theoMean * theoMean;

                // Генерация
                Random rand = new Random();
                int[] counts = new int[5];
                // N испытаний 
                for (int k = 0; k < N; k++)
                {
                    double a = rand.NextDouble();
                    for (int i = 0; i < 5; i++)
                    {
                        a -= probs[i];
                        if (a <= 0)
                        {
                            counts[i]++;
                            break;
                        }
                    }
                }

                // Эмпирические
                double[] empProbs = new double[5];
                for (int i = 0; i < 5; i++) empProbs[i] = (double) counts[i] / N;

                double empMean = 0;
                for (int i = 0; i < 5; i++) empMean += (i + 1) * empProbs[i];

                double empVar = 0;
                for (int i = 0; i < 5; i++)
                    empVar += (i+1) * (i+1) * empProbs[i];
                empVar -= empMean * empMean;

                // Хи-квадрат
                double chiSq = 0;
                for (int i = 0; i < 5; i++)
                {
                    double expected = N * probs[i];
                    if (expected > 0)
                        // из (n_i - N * probs[i])^2 / (N*probs[i])
                        chiSq += (counts[i] * counts[i]) / (N * probs[i]);
                }
                chiSq -= N;

                // Критическое значение (0.95 квантиль хи квадрат распределения с 4 степенями свободы
                double criticalVal = 9.488;
                // Есди меньше или равно, считаем что нет оснований отвергнуть нулевую гипотезу -
                // о том что распределния не различаются
                bool rejectH0 = chiSq > criticalVal;

                // Относительные ошибки 
                double errMean = (theoMean != 0) ? Math.Abs((empMean - theoMean) / theoMean) * 100 : 0;
                double errVar = (theoVar != 0) ? Math.Abs((empVar - theoVar) / theoVar) * 100 : 0;

                string resText = $"Мат. ожидание: {empMean:F3} (ошибка = {errMean:F1}%)\n";
                resText += $"Дисперсия: {empVar:F3} (ошибка = {errVar:F1}%)\n";
                resText += $"Хи-квадрат: {chiSq:F2} (крит. значение = {criticalVal})\n";
                if (rejectH0)
                    resText += $"Хи-квадрат > крит. значения -> нулевая гипотеза отвергается: данные противоречат теоретическому распределению";
                else
                    resText += $"Хи-квадрат <= крит. значения -> нет оснований отвергнуть нулевую гипотезу: данные не противоречат теоретическому распределению";
                lblResults.Text = resText;

                // Обновление графика
                chart.Series["Гистограмма"].Points.Clear();
                chart.Series["Теория"].Points.Clear();
                chart.ChartAreas["MainArea"].AxisX.CustomLabels.Clear();

                // Рисуем гистограмму
                for (int i = 0; i < 5; i++)
                {
                    chart.Series["Гистограмма"].Points.AddXY(i + 1, empProbs[i]);
                    chart.Series["Теория"].Points.AddXY(i + 1, probs[i]);

                    var cl = new CustomLabel(i + 0.5, i + 1.5, (i + 1).ToString(), 0, LabelMarkStyle.None);
                    chart.ChartAreas["MainArea"].AxisX.CustomLabels.Add(cl);
                }

                chart.ChartAreas["MainArea"].AxisX.IsMarginVisible = false;
                double maxVal = 0;
                for (int i = 0; i < 5; i++)
                {
                    maxVal = Math.Max(maxVal, probs[i]);
                }
                chart.ChartAreas["MainArea"].AxisY.Maximum = Math.Ceiling(maxVal * 10) / 10 + 0.05;
                chart.Series["Гистограмма"].SetCustomProperty("PointWidth", "0.6");
                chart.Series["Теория"].SetCustomProperty("PointWidth", "0.6");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}