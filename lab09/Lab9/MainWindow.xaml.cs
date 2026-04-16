using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace SimulationLabs
{

    /// Лабораторная работа 9: СМО M/M/1/0 (система с потерями).
    /// Если сервер занят — новый запрос отказывается (нет очереди).

    public partial class MainWindow : Window
    {
        private readonly Random rand = new Random();
        private readonly SeriesCollection distSeries = new SeriesCollection();
        private bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем серии для графика распределения
            distSeries.Add(new ColumnSeries
            {
                Title = "Эмпирическое",
                Values = new ChartValues<double> { 0, 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180))
            });
            distSeries.Add(new ColumnSeries
            {
                Title = "Теоретическое",
                Values = new ChartValues<double> { 0, 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70))
            });
            chartDist.Series = distSeries;

            // Форматирование оси Y
            chartDist.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Вероятность",
                MinValue = 0,
                MaxValue = 1,
                LabelFormatter = val => val.ToString("N4")
            });

            // Подсчёт ρ при изменении параметров
            txtLambda.TextChanged += (s, e) => UpdateRhoDisplay();
            txtMu.TextChanged += (s, e) => UpdateRhoDisplay();
            UpdateRhoDisplay();
        }

        private double ParseSafe(string text)
        {
            if (double.TryParse(text, out var val)) return val;
            return 0;
        }

        private void UpdateRhoDisplay()
        {
            double lambda = ParseSafe(txtLambda.Text);
            double mu = ParseSafe(txtMu.Text);
            if (mu > 0 && lambda > 0)
            {
                double rho = lambda / mu;
                lblRho.Text = $"ρ = {rho:F3}";
            }
        }

        // ======================== ЗАПУСК СИМУЛЯЦИИ ========================

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            double lambda = ParseSafe(txtLambda.Text);
            double mu = ParseSafe(txtMu.Text);
            int N = (int)ParseSafe(txtN.Text);

            if (lambda <= 0 || mu <= 0 || N <= 0)
            {
                MessageBox.Show("Все параметры должны быть положительными.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            isRunning = true;
            btnRun.IsEnabled = false;
            lblStatus.Text = "Выполняется...";

            await Task.Run(() => Simulate(lambda, mu, N));

            btnRun.IsEnabled = true;
            isRunning = false;
            lblStatus.Text = $"Готово. λ={lambda}, μ={mu}, N={N}";
        }

    
        /// Симуляция M/M/1/0: система с потерями, без очереди.
        /// N независимых заявок — считаем, сколько были обслужены, сколько отказаны.
    
        private void Simulate(double lambda, double mu, int N)
        {
            double rho = lambda / mu;

            // Теоретические значения для M/M/1/0 (формула Эрланга B, n=1)
            double theoP0 = 1.0 / (1.0 + rho);   // вероятность, что сервер свободен
            double theoP1 = rho / (1.0 + rho);    // вероятность, что сервер занят (P_отк)
            double theoRefusal = theoP1;           // вероятность отказа
            double theoThroughput = lambda * theoP0; // пропускная способность

            // Эмуляция: N независимых заявок
            int accepted = 0;
            int refused = 0;
            // Для распределения: доли времени сервера в каждом состоянии
            // Имитируем непрерывное время
            double totalTime = 0;
            double timeFree = 0;
            double timeBusy = 0;

            double currentTime = 0;
            double serverFreeAt = 0; // момент, когда сервер освободится

            // Генерируем N заявок с экспоненциальными интервалами
            var arrivals = new List<double>();
            currentTime = 0;
            for (int i = 0; i < N; i++)
            {
                double interArrival = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda;
                currentTime += interArrival;
                arrivals.Add(currentTime);
            }
            totalTime = currentTime;

            // Обрабатываем заявки
            serverFreeAt = 0;
            for (int i = 0; i < N; i++)
            {
                double arrivalTime = arrivals[i];

                if (arrivalTime >= serverFreeAt)
                {
                    // Сервер свободен — заявка принята
                    accepted++;
                    double serviceTime = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / mu;
                    serverFreeAt = arrivalTime + serviceTime;
                }
                else
                {
                    // Сервер занят — отказ
                    refused++;
                }
            }

            // Эмпирические вероятности
            double empP0 = (double)accepted / N;
            double empP1 = (double)refused / N;
            double empRefusal = empP1;
            double empThroughput = accepted / (totalTime > 0 ? totalTime : 1);

            // Обновляем UI
            Dispatcher.Invoke(() =>
            {
                // График распределения: два состояния (0 — свободен, 1 — занят)
                chartDist.AxisX[0].Labels = new List<string> { "Свободен (P₀)", "Занят (P₁)" };
                chartDist.AxisX[0].Separator = new LiveCharts.Wpf.Separator { Step = 1 };

                if (distSeries.Count >= 2)
                {
                    distSeries[0].Values = new ChartValues<double>
                    {
                        Math.Round(empP0, 4),
                        Math.Round(empP1, 4)
                    };
                    distSeries[1].Values = new ChartValues<double>
                    {
                        Math.Round(theoP0, 4),
                        Math.Round(theoP1, 4)
                    };
                }

                lblStatus.Text = $"Готово. λ={lambda}, μ={mu}, N={N} | " +
                    $"P_отк: теор={Math.Round(theoRefusal, 4)}, эмп={Math.Round(empRefusal, 4)} | " +
                    $"Пропускная сп.: теор={Math.Round(theoThroughput, 4)}, эмп={Math.Round(empThroughput, 4)} | " +
                    $"Отказов: {refused} из {N}";
            });
        }
    }
}
