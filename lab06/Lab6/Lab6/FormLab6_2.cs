using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimulationLabs
{
    public class FormLab6_2 : Form
    {
        private TextBox txtMean, txtVar, txtN;
        private Button btnStart, btnClose;
        private Chart chart;
        private Label lblResults;

        public FormLab6_2()
        {
            InitializeComponent();
            CreateManualUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Lab 06-2: Нормальная СВ";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void CreateManualUI()
        {
            // --- Панель ввода ---
            Panel pnlInput = new Panel { Location = new Point(10, 10), Size = new Size(250, 200), BorderStyle = BorderStyle.FixedSingle };
            AddInput(pnlInput, "Мат. ожидание", 10, 10, out txtMean, "1");
            AddInput(pnlInput, "Дисперсия", 10, 40, out txtVar, "1");
            AddInput(pnlInput, "Объём выборки", 10, 70, out txtN, "1000");

            btnStart = new Button { Text = "Старт", Location = new Point(10, 110), Size = new Size(90, 30), BackColor = Color.LightGreen };
            btnStart.Click += BtnStart_Click;
            pnlInput.Controls.Add(btnStart);

            btnClose = new Button { Text = "Закрыть", Location = new Point(110, 110), Size = new Size(90, 30) };
            btnClose.Click += (s, e) => this.Close();
            pnlInput.Controls.Add(btnClose);
            this.Controls.Add(pnlInput);

            // --- График ---
            chart = new Chart { Location = new Point(270, 10), Size = new Size(650, 350) };
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.BackColor = Color.White;
            ca.AxisX.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            ca.AxisX.LabelStyle.Format = "F2"; // Будет переопределено динамически
            ca.AxisY.LabelStyle.Format = "F3";
            chart.ChartAreas.Add(ca);

            Series sHist = new Series("Hist");
            sHist.ChartType = SeriesChartType.Column;
            sHist.Color = Color.LightBlue;
            sHist.BorderColor = Color.Red;
            sHist.BorderWidth = 1;
            chart.Series.Add(sHist);

            Series sCurve = new Series("Curve");
            sCurve.ChartType = SeriesChartType.Line;
            sCurve.Color = Color.Green;
            sCurve.BorderWidth = 2;
            chart.Series.Add(sCurve);

            this.Controls.Add(chart);

            // --- Результаты ---
            lblResults = new Label { Location = new Point(240, 370), Size = new Size(680, 200), Font = new Font("Arial", 10) };
            this.Controls.Add(lblResults);
        }

        private void AddInput(Panel p, string text, int x, int y, out TextBox tb, string defVal)
        {
            Label lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true };
            p.Controls.Add(lbl);
            tb = new TextBox { Location = new Point(x + 120, y - 3), Size = new Size(110, 20), Text = defVal };
            p.Controls.Add(tb);
        }
        private double GetChiSquareCriticalValue(int df)
        {
            // Критические значения для Хи квадрат распределение при уровне значимости = 0.05
            switch (df)
            {
                case 1: return 3.841;
                case 2: return 5.991;
                case 3: return 7.815;
                case 4: return 9.488;
                case 5: return 11.070;
                case 6: return 12.592;
                case 7: return 14.067;
                case 8: return 15.507;
                case 9: return 16.919;
                case 10: return 18.307;
                case 11: return 19.675;
                case 12: return 21.026;
                case 13: return 22.362;
                case 14: return 23.685;
                case 15: return 24.996;
                case 16: return 26.296;
                case 17: return 27.587;
                case 18: return 28.869;
                case 19: return 30.144;
                case 20: return 31.410;
                case 21: return 32.671;
                case 22: return 33.924;
                case 23: return 35.172;
                case 24: return 36.415;
                case 25: return 37.652;
                case 26: return 38.885;
                case 27: return 40.113;
                case 28: return 41.337;
                case 29: return 42.557;
                case 30: return 43.773;
                default:
                    return -1;
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                double mean = double.Parse(txtMean.Text.Replace('.', ','));
                double variance = double.Parse(txtVar.Text.Replace('.', ','));
                if (variance <= 0) throw new Exception("Дисперсия должна быть положительной");
                double sigma = Math.Sqrt(variance);
                if (!int.TryParse(txtN.Text, out int N) || N <= 0) throw new Exception("N должно быть положительным");

                // 1. Генерация случайных величин распределенных в соответсвии с нормальным законом
                // (Бокс-Мюллер)
                Random rand = new Random();
                List<double> data = new List<double>(N);
                for (int i = 0; i < N; i += 2)
                {
                    double u1 = Math.Max(rand.NextDouble(), 1e-6);
                    double u2 = rand.NextDouble();
                    double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                    data.Add(mean + sigma * z);
                    if (i + 1 < N)
                    {
                        double z2 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                        data.Add(mean + sigma * z2);
                        // причем z1 и z2 независимы!
                        // чтобы не генерировать лишний раз u1 u2 сразу считаем по двум формулам
                    }
                }

                // 2. Эмпирические статистики
                double empMean = 0, empVar = 0;
                foreach (var v in data) empMean += v;
                empMean /= N;
                foreach (var v in data) empVar += (v - empMean) * (v - empMean);
                empVar /= (N - 1);

                // 3. Границы графика
                double xMin = data.Min() + 0.01;
                double xMax = data.Max() + 0.01;
                // Выбор числа интервалов - Sturges' formula
                // Хорош если данные ~ нормально распределенны 
                // может быть плох для n < 30
                int numBins = (int)Math.Ceiling(Math.Log2(N)) + 1;

                double binWidth = (xMax - xMin) / numBins;

                // 4. Распределение по бинам
                int[] counts = new int[numBins];
                foreach (var v in data)
                {
                    if (v >= xMin && v < xMax)
                    {
                        int idx = (int)((v - xMin) / binWidth);
                        counts[idx]++;
                    }
                }

                // 5. Хи-квадрат
                double chiSq = 0;
                for (int i = 0; i < numBins; i++)
                {
                    double lower = xMin + i * binWidth;
                    double upper = lower + binWidth;
                    // NormalCDF (Normal Cumulative Distribution Function)
                    // — это статистическая функция, вычисляющая вероятность того,
                    // что случайная величина, имеющая нормальное распределение, попадет в заданный интервал.
                    double pBin = NormalCDF(upper, mean, sigma) - NormalCDF(lower, mean, sigma);
                    double expected = N * pBin;
                    chiSq += (counts[i] * counts[i]) / expected;
                }
                chiSq -= N;

                // -1, последний столбец (bin) автоматически свое значение получает чтобы в сумме было N 
                double criticalVal = GetChiSquareCriticalValue(numBins - 1);

                bool rejectH0 = chiSq > criticalVal;

                double errMean = (mean != 0) ? Math.Abs((empMean - mean) / mean) * 100 : (sigma != 0 ? Math.Abs(empMean - mean) / sigma * 100 : 0);
                double errVar = (variance != 0) ? Math.Abs((empVar - variance) / variance) * 100 : 0;

                string resText = $"Мат. ожидание: {empMean:F3} (ошибка = {errMean:F1}%)\n";
                resText += $"Дисперсия: {empVar:F3} (ошибка = {errVar:F1}%)\n";
                resText += $"Хи-квадрат: {chiSq:F2} (крит. значение = {criticalVal})\n";
                if (rejectH0)
                    resText += $"Хи-квадрат > крит. значения -> нулевая гипотеза отвергается: данные противоречат теоретическому распределению";
                else
                    resText += $"Хи-квадрат ≤ крит. значения -> нет оснований отвергнуть нулевую гипотезу: данные не противоречат теоретическому распределению";
                lblResults.Text = resText;

                // 6. Настройка графика
                chart.Series["Hist"].Points.Clear();
                chart.Series["Curve"].Points.Clear();

                // Ось X: фиксированные лимиты + умные метки
                chart.ChartAreas["MainArea"].AxisX.Minimum = xMin;
                chart.ChartAreas["MainArea"].AxisX.Maximum = xMax;

                double xInterval = Math.Max(0.5, Math.Ceiling(sigma));
                // Защита от слишком частых меток при малой σ
                if (xInterval < (xMax - xMin) / 15) xInterval = (xMax - xMin) / 8;

                chart.ChartAreas["MainArea"].AxisX.Interval = xInterval;
                chart.ChartAreas["MainArea"].AxisX.MajorGrid.Interval = xInterval;

                // Адаптивное количество знаков после запятой
                int decimals = sigma < 0.1 ? 3 : (sigma < 10 ? 2 : 1);
                chart.ChartAreas["MainArea"].AxisX.LabelStyle.Format = "F" + decimals;

                // Ось Y: масштаб под теоретический пик PDF
                double maxPDF = 1.0 / (sigma * Math.Sqrt(2.0 * Math.PI));
                double yMax = Math.Max(0.05, 1.3 * maxPDF * binWidth);
                chart.ChartAreas["MainArea"].AxisY.Minimum = 0;
                chart.ChartAreas["MainArea"].AxisY.Maximum = yMax;
                chart.ChartAreas["MainArea"].AxisY.LabelStyle.Format = "F3";

                // Гистограмма (относительные частоты)
                chart.ChartAreas["MainArea"].AxisX.CustomLabels.Clear();
                for (int i = 0; i < numBins; i++)
                {
                    double freq = (double)counts[i] / N;
                    double xCenter = xMin + i * binWidth + binWidth / 2.0;
                    chart.Series["Hist"].Points.AddXY(xCenter, freq);

                    double lower = xMin + i * binWidth;
                    double upper = lower + binWidth;
                    string fmt = "F" + decimals;
                    string intervalLabel = $"({lower.ToString(fmt)}, {upper.ToString(fmt)}]";
                    var cl = new CustomLabel(lower, upper, intervalLabel, 0, LabelMarkStyle.None);
                    chart.ChartAreas["MainArea"].AxisX.CustomLabels.Add(cl);
                }
                chart.ChartAreas["MainArea"].AxisX.IsMarginVisible = false;

                // Теоретическая кривая (PDF * binWidth для совпадения масштаба с частотами
                double step = binWidth / 5.0;
                for (double x = xMin; x <= xMax; x += step)
                {
                    // Если в эмпиричеком варианте мы использовали эмпирическую оценку, то здесь 
                    // теоретическую 
                    double pdfVal = NormalPDF(x, mean, sigma);
                    chart.Series["Curve"].Points.AddXY(x, pdfVal * binWidth);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        // Численное вычисление площади под кривой нормального распределения на интервале (~ -infty, x]
        // Cumulitive distribution function - *вероятность* что случайная величина <= x
        private double NormalCDF(double x, double mean, double sigma)
        {
            // 1. Пределы интегрирования 
            // Нормальное распределение практически 0 за +/- 6 сигм
            double lowerLimit = mean - 6 * sigma;
            double upperLimit = x;

            if (upperLimit <= lowerLimit) return 0.0;

            // 2. Делим на N прямоугольников
            int numRectangles = 10000;
            double width = (upperLimit - lowerLimit) / numRectangles;

            double area = 0.0;

            // 3. Складываем площади
            // Используем среднюю точку 
            for (int i = 0; i < numRectangles; i++)
            {
                // Найти среднюю точку текущего прямоугольника 
                double midPoint = lowerLimit + (i + 0.5) * width;

                // Получить высоту прямоугольника в этой точке 
                double height = NormalPDF(midPoint, mean, sigma);

                // Площадь = Высота * ширина 
                area += height * width;
            }

            return area;
        }
        // Вычислить значение нормального распределение в точке x с параметрами mean и sigma 
        // Probability density function - плотность в конкретной точке 
        private double NormalPDF(double x, double mean, double sigma)
        {
            return (1.0 / (sigma * Math.Sqrt(2.0 * Math.PI))) * Math.Exp(-0.5 * Math.Pow((x - mean) / sigma, 2));
        }
    }
}