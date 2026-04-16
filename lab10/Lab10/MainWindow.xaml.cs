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
        private readonly Random randomNumberGenerator = new Random();
        private readonly SeriesCollection queueLengthChartSeries = new SeriesCollection();
        private readonly SeriesCollection resultsChartSeries = new SeriesCollection();
        private bool isSimulationRunning = false;
        private bool hasSimulationCompleted = false;

        // Результаты симуляции для экспорта
        private QueueSystem? mostRecentQueueSystem;
        private double mostRecentSimulationDuration;

        // Временные ряды для графика
        private List<double> simulationTimePoints = new();
        private List<double> busyServersTimeSeries = new();
        private List<double> queueLengthTimeSeries = new();

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем серии графика очереди
            queueLengthChartSeries.Add(new LineSeries
            {
                Title = "Занятые серверы",
                Values = new ChartValues<double> { 0 },
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180)),
                Fill = System.Windows.Media.Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometrySize = 3,
                LineSmoothness = 0
            });
            queueLengthChartSeries.Add(new LineSeries
            {
                Title = "Длина очереди",
                Values = new ChartValues<double> { 0 },
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70)),
                Fill = System.Windows.Media.Brushes.Transparent,
                StrokeThickness = 2,
                PointGeometrySize = 3,
                LineSmoothness = 0
            });
            chartQueue.Series = queueLengthChartSeries;

            // Ось Y очереди — только целые числа
            chartQueue.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Число",
                MinValue = 0,
                LabelFormatter = val => val.ToString("F0"),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 }
            });

            // Инициализируем серии графика результатов
            resultsChartSeries.Add(new ColumnSeries
            {
                Title = "Обслужено",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180))
            });
            resultsChartSeries.Add(new ColumnSeries
            {
                Title = "Отказы",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 70, 70))
            });
            resultsChartSeries.Add(new ColumnSeries
            {
                Title = "Нетерпеливые",
                Values = new ChartValues<double> { 0 },
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 160, 50))
            });
            chartResults.Series = resultsChartSeries;

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
            if (double.TryParse(text, out var parsedValue)) return parsedValue;
            return 0;
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulationRunning) return;

            double arrivalRate = ParseSafe(txtLambda.Text);
            double serviceRate = ParseSafe(txtMu.Text);
            int numberOfServers = (int)ParseSafe(txtN.Text);
            int maximumQueueSize = (int)ParseSafe(txtQueueSize.Text);
            double maximumPatienceTime = ParseSafe(txtPatience.Text);
            double simulationDuration = ParseSafe(txtT.Text);

            if (arrivalRate <= 0 || serviceRate <= 0 || numberOfServers <= 0 || maximumQueueSize < 0 || maximumPatienceTime <= 0 || simulationDuration <= 0)
            {
                MessageBox.Show("Все параметры должны быть положительными.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            isSimulationRunning = true;
            btnRun.IsEnabled = false;
            lblStatus.Text = "Выполняется...";

            await Task.Run(() => Simulate(arrivalRate, serviceRate, numberOfServers, maximumQueueSize, maximumPatienceTime, simulationDuration));

            btnRun.IsEnabled = true;
            btnExport.IsEnabled = true;
            isSimulationRunning = false;
            hasSimulationCompleted = true;
            lblStatus.Text = $"Готово. λ={arrivalRate}, μ={serviceRate}, n={numberOfServers}, T={simulationDuration}";
        }

        private void Simulate(double arrivalRate, double serviceRate,
            int numberOfServers, int maximumQueueSize,
            double maximumPatienceTime, double simulationDuration)
        {
            var queueSystem = new QueueSystem(numberOfServers, maximumQueueSize,
                                              arrivalRate, serviceRate, maximumPatienceTime);

            // Временные ряды для графика
            var simulationTimePointsLocal = new List<double>();
            var busyServersDataLocal = new List<double>();
            var queueLengthDataLocal = new List<double>();
            double snapshotInterval = simulationDuration / 500;
            double nextSnapshotTime = 0;

            // Генерируем все заявки заранее
            double currentSimulationTime = 0;
            var arrivalTimesList = new List<double>();
            while (currentSimulationTime < simulationDuration)
            {
                double interArrivalTime = -Math.Log(Math.Max(randomNumberGenerator.NextDouble(), 1e-10)) / arrivalRate;
                currentSimulationTime += interArrivalTime;
                if (currentSimulationTime < simulationDuration)
                    arrivalTimesList.Add(currentSimulationTime);
            }

            // Очередь событий: прибытия и завершения обслуживания
            var scheduledEvents = new SortedSet<(double eventTime, int eventType, int requestIdentifier)>();
            // type: 0 = прибытие, 1 = завершение обслуживания

            var requestsById = new Dictionary<int, Request>();
            int requestIdentifierCounter = 0;

            // Добавляем все прибытия
            foreach (var arrivalTimeValue in arrivalTimesList)
            {
                requestIdentifierCounter++;
                double serviceTimeValue = -Math.Log(Math.Max(randomNumberGenerator.NextDouble(), 1e-10)) / serviceRate;
                double maximumWaitTimeValue = maximumPatienceTime; // фиксированное терпение из GUI
                var currentRequest = new Request(requestIdentifierCounter, arrivalTimeValue, serviceTimeValue, maximumWaitTimeValue);
                requestsById[requestIdentifierCounter] = currentRequest;
                scheduledEvents.Add((arrivalTimeValue, 0, requestIdentifierCounter));
            }

            // Обработка событий
            while (scheduledEvents.Count > 0)
            {
                // Получаем event с минимальным временем
                var currentEvent = scheduledEvents.Min;
                scheduledEvents.Remove(currentEvent);
                currentSimulationTime = currentEvent.eventTime;

                if (currentEvent.eventType == 0)
                {
                    // Прибытие
                    var incomingRequest = requestsById[currentEvent.requestIdentifier];
                    queueSystem.Arrival(incomingRequest, currentSimulationTime);

                    // Если заявка встала в очередь, планируем проверку нетерпеливости
                    if (queueSystem.WaitingQueue.Any(waitingRequest => waitingRequest.Id == incomingRequest.Id))
                    {
                        double impatientLeaveTime = incomingRequest.ArrivalTime + incomingRequest.MaxWaitTime;
                        scheduledEvents.Add((impatientLeaveTime, 2, incomingRequest.Id)); // type 2 = проверка нетерпеливости
                    }
                }
                else if (currentEvent.eventType == 1)
                {
                    // Завершение обслуживания сервером
                    queueSystem.TryServeFromQueue(currentSimulationTime);
                }
                else if (currentEvent.eventType == 2)
                {
                    // Проверка нетерпеливости
                    queueSystem.CheckImpatient(currentSimulationTime);
                }

                // После каждого события: планируем завершения обслуживания
                foreach (var currentServer in queueSystem.Servers)
                {
                    if (currentServer.FreeAt > currentSimulationTime && currentServer.FreeAt <= simulationDuration)
                    {
                        // Проверяем, нет ли уже такого события
                        bool eventAlreadyExists = scheduledEvents.Any(existingEvent => existingEvent.eventTime == currentServer.FreeAt && existingEvent.eventType == 1);
                        if (!eventAlreadyExists)
                            scheduledEvents.Add((currentServer.FreeAt, 1, 0));
                    }
                }

                // Снимки для графика
                while (nextSnapshotTime <= currentSimulationTime && nextSnapshotTime <= simulationDuration)
                {
                    simulationTimePointsLocal.Add(nextSnapshotTime);
                    busyServersDataLocal.Add(queueSystem.BusyServers(nextSnapshotTime));
                    queueLengthDataLocal.Add(queueSystem.WaitingQueue.Count);
                    nextSnapshotTime += snapshotInterval;
                }
            }

            // Сохраняем для экспорта
            mostRecentQueueSystem = queueSystem;
            mostRecentSimulationDuration = simulationDuration;
            this.simulationTimePoints = simulationTimePointsLocal;
            this.busyServersTimeSeries = busyServersDataLocal;
            this.queueLengthTimeSeries = queueLengthDataLocal;

            // Обновляем UI
            Dispatcher.Invoke(() =>
            {
                // График очереди
                if (queueLengthChartSeries.Count >= 2)
                {
                    queueLengthChartSeries[0].Values = new ChartValues<double>(busyServersDataLocal);
                    queueLengthChartSeries[1].Values = new ChartValues<double>(queueLengthDataLocal);
                }
                // Ось X: реальные значения времени
                if (chartQueue.AxisX.Count > 0)
                {
                    var timeLabels = new List<string>();
                    for (int labelIndex = 0; labelIndex < simulationTimePointsLocal.Count; labelIndex++)
                        timeLabels.Add(simulationTimePointsLocal[labelIndex].ToString("F0"));
                    chartQueue.AxisX[0].Labels = timeLabels;
                    int labelStep = simulationTimePointsLocal.Count > 200 ? 50 : (simulationTimePointsLocal.Count > 50 ? 10 : 1);
                    chartQueue.AxisX[0].Separator = new LiveCharts.Wpf.Separator { Step = labelStep };
                    chartQueue.AxisX[0].LabelsRotation = -45;
                }

                // График результатов
                double totalArrivalsCount = queueSystem.TotalArrivals > 0 ? queueSystem.TotalArrivals : 1;
                double servedProportion = Math.Round((double)queueSystem.TotalServed / totalArrivalsCount, 4);
                double refusedProportion = Math.Round((double)queueSystem.TotalRefused / totalArrivalsCount, 4);
                double impatientProportion = Math.Round((double)queueSystem.TotalImpatient / totalArrivalsCount, 4);

                if (resultsChartSeries.Count >= 3)
                {
                    resultsChartSeries[0].Values = new ChartValues<double> { servedProportion };
                    resultsChartSeries[1].Values = new ChartValues<double> { refusedProportion };
                    resultsChartSeries[2].Values = new ChartValues<double> { impatientProportion };
                }

                // Лог
                txtLog.Text = string.Join("\n", queueSystem.EventLog);
            });
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (mostRecentQueueSystem == null || !hasSimulationCompleted)
            {
                MessageBox.Show("Сначала запустите симуляцию.", "Экспорт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"lab10_report_n{mostRecentQueueSystem.NumServers}_T{mostRecentSimulationDuration:F0}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var fileWriter = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8);

                var exportedQueueSystem = mostRecentQueueSystem;
                fileWriter.WriteLine("=== Лабораторная работа 10: СМО M/M/n с усложнениями ===");
                fileWriter.WriteLine();
                fileWriter.WriteLine("Параметры:");
                fileWriter.WriteLine($"  λ (прибытие) = {exportedQueueSystem.Lambda}");
                fileWriter.WriteLine($"  μ (обслуживание) = {exportedQueueSystem.Mu}");
                fileWriter.WriteLine($"  n (серверы) = {exportedQueueSystem.NumServers}");
                fileWriter.WriteLine($"  Макс. размер очереди = {exportedQueueSystem.MaxQueueSize}");
                fileWriter.WriteLine($"  Макс. терпение = {exportedQueueSystem.MaxPatience}");
                fileWriter.WriteLine($"  Длительность T = {mostRecentSimulationDuration}");
                fileWriter.WriteLine();
                fileWriter.WriteLine("Результаты:");
                fileWriter.WriteLine($"  Всего заявок: {exportedQueueSystem.TotalArrivals}");
                fileWriter.WriteLine($"  Обслужено: {exportedQueueSystem.TotalServed}");
                fileWriter.WriteLine($"  Отказы (очередь полна): {exportedQueueSystem.TotalRefused}");
                fileWriter.WriteLine($"  Нетерпеливые: {exportedQueueSystem.TotalImpatient}");
                fileWriter.WriteLine($"  Ср. время ожидания: {(exportedQueueSystem.TotalServed > 0 ? exportedQueueSystem.TotalWaitTime / exportedQueueSystem.TotalServed : 0):F4}");
                fileWriter.WriteLine($"  Ср. время обслуживания: {(exportedQueueSystem.TotalServed > 0 ? exportedQueueSystem.TotalServiceTime / exportedQueueSystem.TotalServed : 0):F4}");
                fileWriter.WriteLine();
                fileWriter.WriteLine("Доли:");
                double totalArrivalsForExport = exportedQueueSystem.TotalArrivals > 0 ? exportedQueueSystem.TotalArrivals : 1;
                fileWriter.WriteLine($"  Обслужено: {(double)exportedQueueSystem.TotalServed / totalArrivalsForExport:F4}");
                fileWriter.WriteLine($"  Отказы: {(double)exportedQueueSystem.TotalRefused / totalArrivalsForExport:F4}");
                fileWriter.WriteLine($"  Нетерпеливые: {(double)exportedQueueSystem.TotalImpatient / totalArrivalsForExport:F4}");
                fileWriter.WriteLine();
                fileWriter.WriteLine("Лог событий:");
                foreach (var logEntry in exportedQueueSystem.EventLog)
                    fileWriter.WriteLine(logEntry);
            }
        }
    }
}