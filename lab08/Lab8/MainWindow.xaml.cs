using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SimulationLabs
{
    public partial class MainWindow : Window
    {
        private readonly Random rand = new Random();
        private readonly SeriesCollection chartSeries = new SeriesCollection();
        private bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем пустые серии для графика
            chartSeries.Add(new ColumnSeries
            {
                Title = "Эмпирическое",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180))
            });
            chartSeries.Add(new ColumnSeries
            {
                Title = "Теоретическое",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70))
            });
            chartDist.Series = chartSeries;

            // Форматирование оси Y — 4 знака после запятой для tooltip и оси
            chartDist.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Вероятность",
                MinValue = 0,
                LabelFormatter = val => val.ToString("N4")
            });
        }


        /// Безопасный разбор строки в double.

        private double ParseSafe(string text)
        {
            if (double.TryParse(text, out var val)) return val;
            return 0;
        }

        // ======================== ЗАПУСК СИМУЛЯЦИИ ========================

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            double lambda = ParseSafe(txtLambda.Text);
            double T = ParseSafe(txtT.Text);
            int N = (int)ParseSafe(txtN.Text);

            if (lambda <= 0 || T <= 0 || N <= 0)
            {
                MessageBox.Show("Все параметры должны быть положительными.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            isRunning = true;
            btnRun.IsEnabled = false;
            lblStatus.Text = "Выполняется...";

            // Симуляция в фоновом потоке
            await Task.Run(() => Simulate(lambda, T, N));

            btnRun.IsEnabled = true;
            isRunning = false;
            lblStatus.Text = $"Готово. λ={lambda}, T={T}, N={N}";
        }


        /// Ядро симуляции: N независимых экспериментов, подсчёт запросов за время T.

        private void Simulate(double lambda, double T, int N)
        {
            // Массив для подсчёта частот: counts[k] = сколько экспериментов дали ровно k запросов
            var counts = new Dictionary<int, int>();
            var allCounts = new List<int>(N);

            for (int exp = 0; exp < N; exp++)
            {
                int requestCount = 0;
                double time = 0;

                // Генерируем интервалы между запросами ~ Exp(λ)
                while (time < T)
                {
                    double u = Math.Max(rand.NextDouble(), 1e-10);
                    double interArrival = -Math.Log(u) / lambda;

                    time += interArrival;

                    // Считаем запрос, только если он уложился в интервал [0, T]
                    if (time <= T)
                        requestCount++;
                }

                allCounts.Add(requestCount);
                if (counts.ContainsKey(requestCount))
                    counts[requestCount]++;
                else
                    counts[requestCount] = 1;
            }

            // Эмпирическое среднее и дисперсия
            double empMean = allCounts.Average();
            double empVar = allCounts.Select(x => Math.Pow(x - empMean, 2)).Average();

            // Теоретические значения для Пуассона: среднее = дисперсия = λ·T
            double theoLambdaT = lambda * T;
            double theoMean = theoLambdaT;
            double theoVar = theoLambdaT; // для Пуассона дисперсия = среднее

            // Теоретическое распределение Пуассона: P(k) = (λT)^k · e^(-λT) / k!
            var theoDist = new Dictionary<int, double>();
            int maxK = counts.Keys.Max();
            // P(0) = e^(-λT)
            double pk = Math.Exp(-theoLambdaT);
            theoDist[0] = pk;


            // покрываем на 10 больше, посмотреть теоретические значения 
            // или 3 сигмы, смотря что больше, чтобы покрыть как можно больше значений 
            int maxTheoreticalK = Math.Max(maxK + 10, (int)(theoLambdaT * 3));

            // Рекуррентно: P(k) = P(k-1) * λT / k
            for (int k = 1; k <= maxTheoreticalK; k++)
            {
                pk *= theoLambdaT / k;
                theoDist[k] = pk;
            }

            // Обновляем UI
            Dispatcher.Invoke(() =>
            {
                // Обновляем результаты
                lblEmpMean.Text = $"Среднее: {empMean:F4}";
                lblEmpVar.Text = $"Дисперсия: {empVar:F4}";
                lblTheoMean.Text = $"Среднее: {theoMean:F4}";
                lblTheoVar.Text = $"Дисперсия: {theoVar:F4}";

                // Формируем заключение
                double meanError = Math.Abs(empMean - theoMean) / theoMean * 100;
                double varError = Math.Abs(empVar - theoVar) / theoVar * 100;

                lblConclusion.Text = $"Эмпирическое среднее ({empMean:F4}) отличается от теоретического ({theoMean:F4}) на {meanError:F2}%.\n" +
                                     $"Эмпирическая дисперсия ({empVar:F4}) отличается от теоретической ({theoVar:F4}) на {varError:F2}%.\n\n" +
                                     $"Вывод: {(meanError < 5 && varError < 10 ? "Результаты хорошо согласуются с теорией Пуассона." : "Результаты умеренно согласуются с теорией; увеличьте N для лучшей точности.")}";

                // Таблица частот (топ-10)
                lstFrequencies.Items.Clear();
                var topFreq = counts.OrderByDescending(kv => kv.Value).Take(10);
                foreach (var (k, count) in topFreq)
                {
                    double empP = (double)count / N;
                    double theoP = theoDist.ContainsKey(k) ? theoDist[k] : 0;
                    lstFrequencies.Items.Add(new FreqRow
                    {
                        K = k,
                        Empirical = empP.ToString("F4"),
                        Theoretical = theoP.ToString("F4")
                    });
                }

                // График: эмпирическое vs теоретическое распределение
                UpdateChart(counts, theoDist, N, theoLambdaT);
            });
        }


        /// Обновление графика: гистограмма с фиксированным числом бинов.
        /// Диапазон k адаптируется, бинов всегда 20.
        private const int NumBins = 20;

        private void UpdateChart(Dictionary<int, int> counts, Dictionary<int, double> theoDist, int N, double theoLambdaT)
        {
            int centerK = (int)Math.Round(theoLambdaT);
            int sigma = (int)Math.Ceiling(Math.Sqrt(theoLambdaT));

            // Определяем диапазон: от 0 до хвоста или ±3σ
            int endK;
            if (centerK <= 100)
            {
                endK = Math.Max(centerK + 4 * sigma, 20);
            }
            else
            {
                endK = centerK + 3 * sigma;
            }
            // Для больших λT показываем окно вокруг пика
            int startK = (centerK > 100) ? centerK - 3 * sigma : 0;
            startK = Math.Max(0, startK);

            double binWidth = (endK - startK) / (double)NumBins;

            // Агрегируем по бинам
            var empValues = new List<double>();
            var theoValues = new List<double>();
            var labels = new List<string>();

            for (int b = 0; b < NumBins; b++)
            {
                double bStart = startK + b * binWidth;
                double bEnd = startK + (b + 1) * binWidth;

                // Суммируем целые k, попадающие в бин
                double empSum = 0, theoSum = 0;
                int kMin = (int)Math.Ceiling(bStart);
                int kMax = (int)Math.Floor(bEnd);
                if (kMax < kMin) kMax = kMin;

                for (int k = kMin; k <= kMax; k++)
                {
                    if (counts.ContainsKey(k)) empSum += (double)counts[k] / N;
                    if (theoDist.ContainsKey(k)) theoSum += theoDist[k];
                }

                empValues.Add(empSum);
                theoValues.Add(theoSum);
                labels.Add($"{bStart:F1}–{bEnd:F1}");
            }

            // Обновляем серии
            if (chartSeries.Count >= 2)
            {
                chartSeries[0].Values = new ChartValues<double>(empValues);
                chartSeries[1].Values = new ChartValues<double>(theoValues);
            }

            // Ось X
            if (chartDist.AxisX.Count > 0)
            {
                chartDist.AxisX[0].Labels = labels;
                chartDist.AxisX[0].Separator = new LiveCharts.Wpf.Separator { Step = 2 };
                chartDist.AxisX[0].LabelsRotation = -45;
            }
        }


        /// Вычисляет ln(n!) через сумму логарифмов (без переполнения).
        private static double LogFactorial(int n)
        {
            double sum = 0;
            for (int i = 1; i <= n; i++)
                sum += Math.Log(i);
            return sum;
        }


        // ======================== ДЕМО: ОДИН ЗАПУСК ========================
        private void BtnDemoRun_Click(object sender, RoutedEventArgs e)
        {
            double lambda = ParseSafe(txtLambda.Text);
            double T = ParseSafe(txtDemoT.Text);

            if (lambda <= 0 || T <= 0)
            {
                lblDemoResult.Text = "ошибка";
                lblDemoResult.Foreground = Brushes.Red;
                return;
            }

            // Моделируем один интервал [0, T]
            int count = 0;
            double time = 0;

            while (time < T)
            {
                double u = Math.Max(rand.NextDouble(), 1e-10);
                double interArrival = -Math.Log(u) / lambda;
                time += interArrival;

                // Считаем запрос, только если он уложился в интервал
                if (time <= T)
                    count++;
            }

            // Показываем результат
            lblDemoResult.Text = $"{count} запросов";
            lblDemoResult.Foreground = Brushes.DarkBlue;
        }

        /// Строка для таблицы частот.
        public class FreqRow
        {
            public int K { get; set; }
            public string Empirical { get; set; } = "";
            public string Theoretical { get; set; } = "";
        }
    }
}
