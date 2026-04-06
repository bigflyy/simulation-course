#define DISABLE_WARNINGS
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

#nullable disable

namespace StatsSimulationLab
{
    public partial class Form1 : Form
    {
        // Элементы управления
        private TabControl tabControl;
        private TabPage tab1, tab2;
        private TextBox[] tbProbs;
        private TextBox tbN1;
        private Button btnStart1, btnAuto;
        private Chart chart1;
        private Label lblResults1;

        private TextBox tbMean, tbVariance, tbN2;
        private Button btnStart2;
        private Chart chart2;
        private Label lblResults2;

        private Random rand = new Random();

        public Form1()
        {
            CreateMyGUI();
            this.Text = "Имитационное моделирование (Lab 06)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void CreateMyGUI()
        {
            tabControl = new TabControl() { Dock = DockStyle.Fill };
            this.Controls.Add(tabControl);

            // --- Вкладка 1: Дискретная СВ ---
            tab1 = new TabPage("Lab06-1 (Дискретная)");
            tabControl.TabPages.Add(tab1);

            Panel leftPanel1 = new Panel() { Width = 200, Dock = DockStyle.Left, Padding = new Padding(10) };
            tab1.Controls.Add(leftPanel1);

            leftPanel1.Controls.Add(new Label() { Text = "Вероятности (p1-p5):", Top = 10, Width = 180 });
            
            tbProbs = new TextBox[5];
            for (int i = 0; i < 5; i++)
            {
                tbProbs[i] = new TextBox() { Top = 30 + i * 30, Width = 100, Left = 10, Text = "0.2" };
                leftPanel1.Controls.Add(tbProbs[i]);
            }

            btnAuto = new Button() { Text = "Auto p5", Top = 180, Left = 110, Width = 70 };
            btnAuto.Click += BtnAuto_Click;
            leftPanel1.Controls.Add(btnAuto);

            leftPanel1.Controls.Add(new Label() { Text = "Размер выборки (N):", Top = 220, Width = 180 });
            tbN1 = new TextBox() { Top = 240, Width = 100, Left = 10, Text = "1000" };
            leftPanel1.Controls.Add(tbN1);

            btnStart1 = new Button() { Text = "START", Top = 280, Width = 170, Left = 10, Height = 40, BackColor = Color.LightBlue };
            btnStart1.Click += BtnStart1_Click;
            leftPanel1.Controls.Add(btnStart1);

            // График
            ChartArea area1 = new ChartArea("Main");
            // Включаем красивое отображение
            area1.AxisX.MajorGrid.LineColor = Color.LightGray;
            area1.AxisY.MajorGrid.LineColor = Color.LightGray;
            
            chart1 = new Chart() { Dock = DockStyle.Fill };
            chart1.ChartAreas.Add(area1);
            
            lblResults1 = new Label() { Dock = DockStyle.Bottom, Height = 80, Font = new Font("Consolas", 10), BackColor = Color.White };
            
            tab1.Controls.Add(chart1);
            tab1.Controls.Add(lblResults1);

            // --- Вкладка 2: Нормальная СВ ---
            tab2 = new TabPage("Lab06-2 (Нормальная)");
            tabControl.TabPages.Add(tab2);

            Panel leftPanel2 = new Panel() { Width = 200, Dock = DockStyle.Left, Padding = new Padding(10) };
            tab2.Controls.Add(leftPanel2);

            leftPanel2.Controls.Add(new Label() { Text = "Среднее (Mean):", Top = 10, Width = 180 });
            tbMean = new TextBox() { Top = 30, Width = 100, Left = 10, Text = "0" };
            leftPanel2.Controls.Add(tbMean);

            leftPanel2.Controls.Add(new Label() { Text = "Дисперсия (Variance):", Top = 60, Width = 180 });
            tbVariance = new TextBox() { Top = 80, Width = 100, Left = 10, Text = "1" };
            leftPanel2.Controls.Add(tbVariance);

            leftPanel2.Controls.Add(new Label() { Text = "Размер выборки (N):", Top = 120, Width = 180 });
            tbN2 = new TextBox() { Top = 140, Width = 100, Left = 10, Text = "1000" };
            leftPanel2.Controls.Add(tbN2);

            btnStart2 = new Button() { Text = "START", Top = 200, Width = 170, Left = 10, Height = 40, BackColor = Color.LightGreen };
            btnStart2.Click += BtnStart2_Click;
            leftPanel2.Controls.Add(btnStart2);

            ChartArea area2 = new ChartArea("Main");
            area2.AxisX.MajorGrid.LineColor = Color.LightGray;
            area2.AxisY.MajorGrid.LineColor = Color.LightGray;

            chart2 = new Chart() { Dock = DockStyle.Fill };
            chart2.ChartAreas.Add(area2);
            
            lblResults2 = new Label() { Dock = DockStyle.Bottom, Height = 80, Font = new Font("Consolas", 10), BackColor = Color.White };
            
            tab2.Controls.Add(chart2);
            tab2.Controls.Add(lblResults2);
        }

        // --- Логика Lab06-1 ---

        private void BtnAuto_Click(object sender, EventArgs e)
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (double.TryParse(tbProbs[i].Text, out double p)) sum += p;
            }
            tbProbs[4].Text = (1.0 - sum).ToString("F4");
        }

        private void BtnStart1_Click(object sender, EventArgs e)
        {
            try
            {
                double[] probs = new double[5];
                double sumCheck = 0;
                for (int i = 0; i < 5; i++)
                {
                    probs[i] = double.Parse(tbProbs[i].Text);
                    sumCheck += probs[i];
                }
                if (Math.Abs(sumCheck - 1.0) > 0.001)
                {
                    MessageBox.Show("Сумма вероятностей должна быть равна 1!");
                    return;
                }
                int N = int.Parse(tbN1.Text);
                RunExperiment1(probs, N);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message);
            }
        }

        private void RunExperiment1(double[] probs, int N)
        {
            double[] values = Enumerable.Range(1, 5).Select(x => (double)x).ToArray();

            // Теория
            double theorMean = 0, theorVar = 0;
            for (int i = 0; i < 5; i++) theorMean += values[i] * probs[i];
            for (int i = 0; i < 5; i++) theorVar += Math.Pow(values[i] - theorMean, 2) * probs[i];

            // Генерация
            double[] sample = new double[N];
            int[] freqs = new int[5];

            for (int i = 0; i < N; i++)
            {
                double r = rand.NextDouble();
                double cumProb = 0;
                int selectedIndex = 4;
                
                for (int j = 0; j < 5; j++)
                {
                    cumProb += probs[j];
                    if (r < cumProb)
                    {
                        selectedIndex = j;
                        break;
                    }
                }
                sample[i] = values[selectedIndex];
                freqs[selectedIndex]++;
            }

            // Статистика
            double empMean = sample.Average();
            double empVar = 0;
            foreach (var x in sample) empVar += Math.Pow(x - empMean, 2);
            empVar /= (N - 1);

            double errMean = theorMean == 0 ? 0 : Math.Abs((empMean - theorMean) / theorMean) * 100;
            double errVar = theorVar == 0 ? 0 : Math.Abs((empVar - theorVar) / theorVar) * 100;

            // Хи-квадрат
            double chiSq = 0;
            for (int i = 0; i < 5; i++)
            {
                double expected = N * probs[i];
                if (expected > 0)
                    chiSq += Math.Pow(freqs[i] - expected, 2) / expected;
            }
            
            double chiCrit = 9.488; // df=4, alpha=0.05
            
            // График
            chart1.Series.Clear();
            Series col = new Series("Частоты") { ChartType = SeriesChartType.Column };
            for(int i=0; i<5; i++)
            {
                // Используем числовую точку, а не строку
                col.Points.AddXY(values[i], (double)freqs[i]/N);
            }
            chart1.Series.Add(col);

            lblResults1.Text = $"N = {N}\n" +
                                $"Average: {empMean:F3} (error = {errMean:F1}%)\n" +
                                $"Variance: {empVar:F3} (error = {errVar:F1}%)\n" +
                                $"Chi-squared: {chiSq:F2} > {chiCrit} is {chiSq > chiCrit}";
        }

        // --- Логика Lab06-2 ---

        private void BtnStart2_Click(object sender, EventArgs e)
        {
            try
            {
                double mean = double.Parse(tbMean.Text);
                double variance = double.Parse(tbVariance.Text);
                int N = int.Parse(tbN2.Text);
                RunExperiment2(mean, variance, N);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message);
            }
        }

        private void RunExperiment2(double mu, double sigmaSq, int N)
        {
            double sigma = Math.Sqrt(sigmaSq);
            double[] sample = new double[N];

            // Метод Бокса-Мюллера
            for (int i = 0; i < N; i++)
            {
                double u1 = rand.NextDouble();
                double u2 = rand.NextDouble();
                double z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                sample[i] = mu + sigma * z;
            }

            // Статистика
            double empMean = sample.Average();
            double empVar = 0;
            foreach (var x in sample) empVar += Math.Pow(x - empMean, 2);
            empVar /= (N - 1);

            double errMean = mu == 0 ? Math.Abs(empMean * 100) : Math.Abs((empMean - mu) / mu) * 100;
            double errVar = sigmaSq == 0 ? Math.Abs(empVar * 100) : Math.Abs((empVar - sigmaSq) / sigmaSq) * 100;

            // Подготовка к гистограмме
            // Число интервалов (карманов) по формуле Стерджесса
            int k = (int)Math.Round(1 + 3.322 * Math.Log10(N));
            if (k < 5) k = 5; 
            if (k > 25) k = 25;

            double min = sample.Min();
            double max = sample.Max();
            
            // Если все числа одинаковые (маловероятно для нормального, но вдруг)
            if (Math.Abs(max - min) < 0.000001) 
            { 
                min -= 1; 
                max += 1; 
            }

            double h = (max - min) / k;

            // Массив для частот
            int[] freqs = new int[k];

            // Распределение значений по интервалам (быстрый способ)
            for (int i = 0; i < N; i++)
            {
                double val = sample[i];
                int binIndex = (int)((val - min) / h);
                
                // Корректировка для последнего элемента (чтобы не вылететь за границы массива)
                if (binIndex >= k) binIndex = k - 1;
                if (binIndex < 0) binIndex = 0;
                
                freqs[binIndex]++;
            }

            // Вычисление Хи-квадрат
            double chiSq = 0;
            for (int i = 0; i < k; i++)
            {
                double left = min + i * h;
                double right = left + h;
                
                // Теоретическая вероятность
                double pTheor = NormalCDF(right, mu, sigma) - NormalCDF(left, mu, sigma);
                double nTheor = N * pTheor;
                
                if (nTheor > 0)
                    chiSq += Math.Pow(freqs[i] - nTheor, 2) / nTheor;
            }

            // Рисование графиков
            chart2.Series.Clear();

            // 1. Гистограмма
            Series hist = new Series("Histogram") { ChartType = SeriesChartType.Column };
            hist["PointWidth"] = "1"; // Столбцы вплотную
            
            for (int i = 0; i < k; i++)
            {
                double left = min + i * h;
                // Высота = плотность вероятности (частота / N / шаг h)
                double height = (double)freqs[i] / N / h;
                hist.Points.AddXY(left, height);
            }
            chart2.Series.Add(hist);

            // 2. Теоретическая кривая (зеленая линия)
            Series line = new Series("Theory") { ChartType = SeriesChartType.Line, Color = Color.Green, BorderWidth = 3 };
            
            // Рисуем линию от min до max
            int steps = 100;
            double stepSize = (max - min) / steps;
            for (int i = 0; i <= steps; i++)
            {
                double x = min + i * stepSize;
                double y = TheorDensity(x, mu, sigma);
                line.Points.AddXY(x, y);
            }
            chart2.Series.Add(line);

            // Степени свободы для Хи-квадрат: k интервалов - 3
            int df = k - 3;
            double chiCrit = GetChiCritical(df);

            lblResults2.Text = $"N = {N}, k = {k}\n" +
                               $"Average: {empMean:F3} (error = {errMean:F1}%)\n" +
                               $"Variance: {empVar:F3} (error = {errVar:F1}%)\n" +
                               $"Chi-squared: {chiSq:F2} > {chiCrit:F2} is {chiSq > chiCrit}";
        }
        
        // Вспомогательные функции
        private double NormalCDF(double x, double mu, double sigma)
        {
            return 0.5 * (1 + Erf((x - mu) / (sigma * Math.Sqrt(2))));
        }

        private double Erf(double x)
        {
            // Аппроксимация функции ошибок
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p * x);
            double y = 1 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        private double TheorDensity(double x, double mu, double sigma)
        {
            double exp = Math.Exp(-(Math.Pow(x - mu, 2) / (2 * Math.Pow(sigma, 2))));
            return (1.0 / (sigma * Math.Sqrt(2 * Math.PI))) * exp;
        }

        private double GetChiCritical(int df)
        {
            var table = new Dictionary<int, double>()
            {
                {1, 3.84}, {2, 5.99}, {3, 7.81}, {4, 9.49}, {5, 11.07},
                {6, 12.59}, {7, 14.07}, {8, 15.51}, {9, 16.92}, {10, 18.31},
                {11, 19.68}, {12, 21.03}, {13, 22.36}, {14, 23.68}, {15, 25.00},
                {16, 26.30}, {17, 27.59}, {18, 28.87}, {19, 30.14}, {20, 31.41}
            };
            if (table.ContainsKey(df)) return table[df];
            return 30.0 + df;
        }
    }
}