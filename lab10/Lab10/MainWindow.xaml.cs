using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;

namespace SimulationLabs
{
    public partial class MainWindow : Window
    {
        private readonly Random rand = new Random();
        private readonly SeriesCollection queueSeries = new SeriesCollection();
        private readonly SeriesCollection resultsSeries = new SeriesCollection();
        private bool isRunning = false;
        private bool simulationDone = false;

        // Результаты симуляции для экспорта
        private QueueSystem? lastSystem;
        private double lastT;

        // Временные ряды для графика
        private List<double> timePoints = new();
        private List<double> busySeries = new();
        private List<double> queueSeriesData = new();

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем серии графика очереди
            queueSeries.Add(new LineSeries
            {
                Title = "Занятые серверы",
                Values = new ChartValues<double> { 0 },
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180)),
                Fill = System.Windows.Media.Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometrySize = 3,
                LineSmoothness = 0
            });
            queueSeries.Add(new LineSeries
            {
                Title = "Длина очереди",
                Values = new ChartValues<double> { 0 },
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70)),
                Fill = System.Windows.Media.Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometrySize = 3,
                LineSmoothness = 0
            });
            chartQueue.Series = queueSeries;

            // Ось Y очереди — только целые числа
            chartQueue.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Число",
                MinValue = 0,
                LabelFormatter = val => val.ToString("F0"),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 }
            });

            // Инициализируем серии графика результатов
            resultsSeries.Add(new ColumnSeries
            {
                Title = "Обслужено",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180))
            });
            resultsSeries.Add(new ColumnSeries
            {
                Title = "Отказы",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70))
            });
            resultsSeries.Add(new ColumnSeries
            {
                Title = "Нетерпеливые",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 160, 50))
            });
            chartResults.Series = resultsSeries;

            // Оси графика результатов (с форматированием Y)
            chartResults.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Labels = new List<string> { "Доли заявок" }
            });
            chartResults.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Доля",
                MinValue = 0,
                MaxValue = 1,
                LabelFormatter = val => val.ToString("N4")
            });
        }

        private double ParseSafe(string text)
        {
            if (double.TryParse(text, out var val)) return val;
            return 0;
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            double lambda = ParseSafe(txtLambda.Text);
            double mu = ParseSafe(txtMu.Text);
            int n = (int)ParseSafe(txtN.Text);
            int maxQueue = (int)ParseSafe(txtQueueSize.Text);
            double patience = ParseSafe(txtPatience.Text);
            double T = ParseSafe(txtT.Text);

            if (lambda <= 0 || mu <= 0 || n <= 0 || maxQueue < 0 || patience <= 0 || T <= 0)
            {
                MessageBox.Show("Все параметры должны быть положительными.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            isRunning = true;
            btnRun.IsEnabled = false;
            lblStatus.Text = "Выполняется...";

            await Task.Run(() => Simulate(lambda, mu, n, maxQueue, patience, T));

            btnRun.IsEnabled = true;
            btnExport.IsEnabled = true;
            isRunning = false;
            simulationDone = true;
            lblStatus.Text = $"Готово. λ={lambda}, μ={mu}, n={n}, T={T}";
        }

        private void Simulate(double lambda, double mu, int n, int maxQueue, double patience, double T)
        {
            var qs = new QueueSystem(n, maxQueue, lambda, mu, patience);

            // Временные ряды для графика
            var timePoints = new List<double>();
            var busyData = new List<double>();
            var queueData = new List<double>();
            double snapshotInterval = T / 500;
            double nextSnapshot = 0;

            // Генерируем все заявки заранее
            double currentTime = 0;
            var arrivals = new List<double>();
            while (currentTime < T)
            {
                double interArrival = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda;
                currentTime += interArrival;
                if (currentTime < T)
                    arrivals.Add(currentTime);
            }

            // Очередь событий: прибытия и завершения обслуживания
            var events = new SortedSet<(double time, int type, int requestId)>();
            // type: 0 = прибытие, 1 = завершение обслуживания

            var allRequests = new Dictionary<int, Request>();
            int reqId = 0;

            // Добавляем все прибытия
            foreach (var arrTime in arrivals)
            {
                reqId++;
                double serviceTime = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / mu;
                double maxWait = patience; // фиксированное терпение из GUI
                var req = new Request(reqId, arrTime, serviceTime, maxWait);
                allRequests[reqId] = req;
                events.Add((arrTime, 0, reqId));
            }

            // Обработка событий
            while (events.Count > 0)
            {
                var evt = events.Min;
                events.Remove(evt);
                currentTime = evt.time;

                if (evt.type == 0)
                {
                    // Прибытие
                    var req = allRequests[evt.requestId];
                    qs.Arrival(req, currentTime);

                    // Если заявка встала в очередь, планируем проверку нетерпеливости
                    if (qs.WaitingQueue.Any(wq => wq.Id == req.Id))
                    {
                        double leaveTime = req.ArrivalTime + req.MaxWaitTime;
                        events.Add((leaveTime, 2, req.Id)); // type 2 = проверка нетерпеливости
                    }
                }
                else if (evt.type == 1)
                {
                    // Завершение обслуживания сервером
                    qs.TryServeFromQueue(currentTime);

                    // Если кто-то начал обслуживание, запланируем завершение
                    foreach (var server in qs.Servers)
                    {
                        if (server.FreeAt > currentTime && server.FreeAt < T)
                        {
                            // Уже запланировано
                        }
                    }
                }
                else if (evt.type == 2)
                {
                    // Проверка нетерпеливости
                    qs.CheckImpatient(currentTime);
                }

                // После каждого события: планируем завершения обслуживания
                foreach (var server in qs.Servers)
                {
                    if (server.FreeAt > currentTime && server.FreeAt <= T)
                    {
                        // Проверяем, нет ли уже такого события
                        bool exists = events.Any(ev => ev.time == server.FreeAt && ev.type == 1);
                        if (!exists)
                            events.Add((server.FreeAt, 1, 0));
                    }
                }

                // Снимки для графика
                while (nextSnapshot <= currentTime && nextSnapshot <= T)
                {
                    timePoints.Add(nextSnapshot);
                    busyData.Add(qs.BusyServers(nextSnapshot));
                    queueData.Add(qs.WaitingQueue.Count);
                    nextSnapshot += snapshotInterval;
                }
            }

            // Сохраняем для экспорта
            lastSystem = qs;
            lastT = T;
            this.timePoints = timePoints;
            this.busySeries = busyData;
            this.queueSeriesData = queueData;

            // Обновляем UI
            Dispatcher.Invoke(() =>
            {
                // График очереди
                if (queueSeries.Count >= 2)
                {
                    queueSeries[0].Values = new ChartValues<double>(busyData);
                    queueSeries[1].Values = new ChartValues<double>(queueData);
                }
                // Ось X: реальные значения времени
                if (chartQueue.AxisX.Count > 0)
                {
                    var labels = new List<string>();
                    for (int i = 0; i < timePoints.Count; i++)
                        labels.Add(timePoints[i].ToString("F0"));
                    chartQueue.AxisX[0].Labels = labels;
                    int step = timePoints.Count > 200 ? 50 : (timePoints.Count > 50 ? 10 : 1);
                    chartQueue.AxisX[0].Separator = new LiveCharts.Wpf.Separator { Step = step };
                    chartQueue.AxisX[0].LabelsRotation = -45;
                }

                // График результатов
                double total = qs.TotalArrivals > 0 ? qs.TotalArrivals : 1;
                double servedP = Math.Round((double)qs.TotalServed / total, 4);
                double refusedP = Math.Round((double)qs.TotalRefused / total, 4);
                double impatientP = Math.Round((double)qs.TotalImpatient / total, 4);

                if (resultsSeries.Count >= 3)
                {
                    resultsSeries[0].Values = new ChartValues<double> { servedP };
                    resultsSeries[1].Values = new ChartValues<double> { refusedP };
                    resultsSeries[2].Values = new ChartValues<double> { impatientP };
                }

                // Лог
                txtLog.Text = string.Join("\n", qs.EventLog);
            });
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (lastSystem == null || !simulationDone)
            {
                MessageBox.Show("Сначала запустите симуляцию.", "Экспорт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"lab10_report_n{lastSystem.NumServers}_T{lastT:F0}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using var writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8);

                var qs = lastSystem;
                writer.WriteLine("=== Лабораторная работа 10: СМО M/M/n с усложнениями ===");
                writer.WriteLine();
                writer.WriteLine("Параметры:");
                writer.WriteLine($"  λ (прибытие) = {qs.Lambda}");
                writer.WriteLine($"  μ (обслуживание) = {qs.Mu}");
                writer.WriteLine($"  n (серверы) = {qs.NumServers}");
                writer.WriteLine($"  Макс. размер очереди = {qs.MaxQueueSize}");
                writer.WriteLine($"  Макс. терпение = {qs.MaxPatience}");
                writer.WriteLine($"  Длительность T = {lastT}");
                writer.WriteLine();
                writer.WriteLine("Результаты:");
                writer.WriteLine($"  Всего заявок: {qs.TotalArrivals}");
                writer.WriteLine($"  Обслужено: {qs.TotalServed}");
                writer.WriteLine($"  Отказы (очередь полна): {qs.TotalRefused}");
                writer.WriteLine($"  Нетерпеливые: {qs.TotalImpatient}");
                writer.WriteLine($"  Ср. время ожидания: {(qs.TotalServed > 0 ? qs.TotalWaitTime / qs.TotalServed : 0):F4}");
                writer.WriteLine($"  Ср. время обслуживания: {(qs.TotalServed > 0 ? qs.TotalServiceTime / qs.TotalServed : 0):F4}");
                writer.WriteLine();
                writer.WriteLine("Доли:");
                double total = qs.TotalArrivals > 0 ? qs.TotalArrivals : 1;
                writer.WriteLine($"  Обслужено: {(double)qs.TotalServed / total:F4}");
                writer.WriteLine($"  Отказы: {(double)qs.TotalRefused / total:F4}");
                writer.WriteLine($"  Нетерпеливые: {(double)qs.TotalImpatient / total:F4}");
                writer.WriteLine();
                writer.WriteLine("Лог событий:");
                foreach (var line in qs.EventLog)
                    writer.WriteLine(line);
            }
        }
    }
}
