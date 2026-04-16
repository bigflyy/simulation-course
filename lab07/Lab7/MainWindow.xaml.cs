using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using LiveCharts;
using LiveCharts.Wpf;

namespace SimulationLabs
{
    /// <summary>
    /// Запись о переходе: время и состояние.
    /// </summary>
    public class TransitionRecord
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

    /// <summary>
    /// Главное окно: Марковская модель погоды с непрерывным временем.
    /// 3 состояния: 1 — ясно, 2 — облачно, 3 — пасмурно.
    /// Моделирование в реальном времени с визуализацией и статистикой.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ======================== ПОЛЯ МОДЕЛИ ========================

        private double[,] Q = new double[3, 3];    // Матрица интенсивностей (генератор Q)
        private double[] lambda = new double[3];    // Суммарные интенсивности выхода: λ[i] = -Q[i,i]

        private int currentState = 1;               // Текущее состояние (1, 2, 3)
        private double currentTime = 0;             // Текущее непрерывное время
        private double simulationEndTime = 365.0;   // Длительность симуляции (дни)

        private readonly Random rand = new Random();

        // История переходов
        private readonly List<TransitionRecord> transitionHistory = new List<TransitionRecord>();

        // Накопленное время в каждом состоянии (Algorithm 2 из лекции)
        private readonly double[] totalTimeInState = new double[3];

        // Флаги
        private bool isRunning = false;
        private bool _isInitialized = false;

        // Результаты алгоритма 1
        private int[] alg1Counts = new int[3];
        private int alg1N = 0;
        private double alg1T = 0;
        private bool alg1Complete = false;
        private bool alg2Complete = false;
        private double alg2TotalSimulationTime;
        private static double[] alg2Share = new double[3];

        // Теоретическое стационарное распределение
        private readonly List<double> theoProbs = new List<double>();

        // LiveCharts серии
        private readonly SeriesCollection alg1Series = new SeriesCollection();  // Алгоритм 1: частоты конечных состояний
        private readonly SeriesCollection alg2Series = new SeriesCollection();  // Алгоритм 2: доля времени

        // Словарь имён состояний
        private static readonly string[] StateNames = { "Ясно", "Облачно", "Пасмурно" };
        private static readonly SolidColorBrush[] StateColors = {
            new SolidColorBrush(Color.FromRgb(255, 200, 0)),  // Жёлтый — ясно
            new SolidColorBrush(Color.FromRgb(180, 180, 180)), // Серый — облачно
            new SolidColorBrush(Color.FromRgb(100, 100, 110))  // Тёмный — пасмурно
        };

        // ======================== КОНСТРУКТОР ========================

        public MainWindow()
        {
            InitializeComponent();
            InitializeCharts();
            _isInitialized = true;
            UpdateDiagonal();
            UpdateCurrentStateDisplay();
        }


        /// Обработчик изменения любой ячейки матрицы — автопересчёт диагонали.
        private void MatrixCell_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitialized)
                UpdateDiagonal();
        }


        /// Разрешаем ввод цифр, точки, запятой и минуса в поля матрицы.
        private void MatrixCell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем: цифры, точка, запятая, минус
            string text = e.Text;
            bool isAllowed = text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-');
            e.Handled = !isAllowed;
        }

        /// Обновление финальных долей времени (после завершения симуляции)
        private void UpdateFinalTimeShares()
        {
            alg2TotalSimulationTime = totalTimeInState.Sum();
            if (alg2TotalSimulationTime <= 0) return;

            alg2Share[0] = totalTimeInState[0] / alg2TotalSimulationTime;
            alg2Share[1] = totalTimeInState[1] / alg2TotalSimulationTime;
            alg2Share[2] = totalTimeInState[2] / alg2TotalSimulationTime;

            Dispatcher.Invoke(() =>
            {
                lblAlg2Pct1.Text = $"Ясно: {alg2Share[0]:F4}";
                lblAlg2Pct2.Text = $"Облачно: {alg2Share[1]:F4}";
                lblAlg2Pct3.Text = $"Пасмурно: {alg2Share[2]:F4}";
            });
        }

        /// Инициализация LiveCharts серий.
        private void InitializeCharts()
        {
            // Алгоритм 1: эмпирические частоты конечных состояний + теоретическое π
            alg1Series.Add(new ColumnSeries
            {
                Title = "Эмпирическая частота",
                Values = new ChartValues<double> { 0, 0, 0 },
                Fill = new SolidColorBrush(Color.FromRgb(100, 180, 100))
            });
            alg1Series.Add(new ColumnSeries
            {
                Title = "Теоретическое π",
                Values = new ChartValues<double> { 0, 0, 0 },
                Fill = new SolidColorBrush(Color.FromRgb(70, 130, 180))
            });
            chartAlg1.Series = alg1Series;
            chartAlg1.AxisY.Add(new Axis
            {
                Title = "Частота",
                MinValue = 0,
                MaxValue = 1,
                LabelFormatter = val => val.ToString("N1"),
                Separator = new LiveCharts.Wpf.Separator { Step = 0.1 }
            });

            // Алгоритм 2: доля времени в каждом состоянии
            alg2Series.Clear();
            var areaFills = new Brush[] {
                new SolidColorBrush(Color.FromArgb(60, 255, 215, 0)),  // Желтый (Ясно)
                new SolidColorBrush(Color.FromArgb(60, 169, 169, 169)), // Серый (Облачно)
                new SolidColorBrush(Color.FromArgb(60, 47, 79, 79))    // Темный (Пасмурно)
            };

            for (int i = 0; i < 3; i++)
            {
                alg2Series.Add(new LineSeries
                {
                    Title = StateNames[i],
                    Values = new ChartValues<LiveCharts.Defaults.ObservablePoint>(),
                    PointGeometry = null,
                    Fill = areaFills[i],
                    Stroke = StateColors[i],
                    StrokeThickness = 1.5,
                    AreaLimit = 0,
                });
            }
            chartAlg2.Series = alg2Series;

            // 3. Настройка Осей и СТАТИЧНЫХ линий теории (через Sections)
            chartAlg2.AxisY.Clear();
            var yAxis = new Axis
            {
                MinValue = 0,
                MaxValue = 1,
                LabelFormatter = val => val.ToString("F2")
            };

            // Добавляем пунктирные линии только если теория уже рассчитана
            if (theoProbs != null && theoProbs.Count == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    yAxis.Sections.Add(new AxisSection
                    {
                        Value = theoProbs[i], // Константное значение по Y
                        Stroke = StateColors[i],
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 5, 3 }, // Пунктир
                    });
                }
            }
            chartAlg2.AxisY.Add(yAxis);

            chartAlg2.AxisX.Clear();
            chartAlg2.AxisX.Add(new Axis
            {
                Title = "Дни (Время)",
                LabelFormatter = val => val.ToString("N0")
            });
        }

        // ======================== ОБРАБОТКА МАТРИЦЫ Q ========================
        /// Привязка событий к полям ввода матрицы для автопересчёта диагонали.
        private void UpdateDiagonal()
        {
            // Сумма не-диагональных элементов в каждой строке с обратным знаком
            // q_ii = -sum(q_ij, j != i)
            double q01 = ParseSafe(txtQ01?.Text);
            double q02 = ParseSafe(txtQ02?.Text);
            double q10 = ParseSafe(txtQ10?.Text);
            double q12 = ParseSafe(txtQ12?.Text);
            double q20 = ParseSafe(txtQ20?.Text);
            double q21 = ParseSafe(txtQ21?.Text);

            if (txtQ00 != null) txtQ00.Text = $"-{q01 + q02:F3}";
            if (txtQ11 != null) txtQ11.Text = $"-{q10 + q12:F3}";
            if (txtQ22 != null) txtQ22.Text = $"-{q20 + q21:F3}";
        }


        /// Безопасный парсинг double из строки (null-safe).
        private static double ParseSafe(string? text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val;
            return 0;
        }


        /// Чтение и валидация параметров из UI. Строит матрицу Q и массив λ.
        private bool ReadAndValidateParameters()
        {
            try
            {
                // Считываем off-diagonal элементы
                Q[0, 1] = ParseSafe(txtQ01.Text);
                Q[0, 2] = ParseSafe(txtQ02.Text);
                Q[1, 0] = ParseSafe(txtQ10.Text);
                Q[1, 2] = ParseSafe(txtQ12.Text);
                Q[2, 0] = ParseSafe(txtQ20.Text);
                Q[2, 1] = ParseSafe(txtQ21.Text);

                // Диагональ: q_ii = -sum(q_ij, j != i)
                Q[0, 0] = -(Q[0, 1] + Q[0, 2]);
                Q[1, 1] = -(Q[1, 0] + Q[1, 2]);
                Q[2, 2] = -(Q[2, 0] + Q[2, 1]);

                // Интенсивности выхода из состояний
                lambda[0] = -Q[0, 0];
                lambda[1] = -Q[1, 1];
                lambda[2] = -Q[2, 2];

                // Валидация: все off-diagonal >= 0, все λ > 0
                for (int i = 0; i < 3; i++)
                {
                    if (lambda[i] <= 0)
                    {
                        MessageBox.Show($"Интенсивность выхода из состояния {i + 1} равна 0. " +
                                        "Укажите хотя бы одну ненулевую интенсивность перехода.",
                                        "Ошибка параметров", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        if (i != j && Q[i, j] < 0)
                        {
                            MessageBox.Show($"Элемент Q[{i + 1},{j + 1}] не может быть отрицательным.",
                                            "Ошибка параметров", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                }

                // Длительность симуляции
                simulationEndTime = ParseSafe(txtSimDays.Text);
                if (simulationEndTime <= 0)
                {
                    MessageBox.Show("Длительность симуляции должна быть положительной.",
                                    "Ошибка параметров", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Начальное состояние
                currentState = cmbStartState.SelectedIndex + 1;

                return true;
            }
            catch
            {
                MessageBox.Show("Ошибка разбора параметров. Проверьте корректность ввода.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Выражается из уравнений π·Q = 0, Σπᵢ = 1
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

        // ======================== КНОПКИ УПРАВЛЕНИЯ ========================
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            if (!ReadAndValidateParameters())
                return;

            ResetSimulation();
            CalculateTheoreticalDistribution();

            UpdateAlg2TheoreticalLines(); // после вычисления теоретических характеристик 

            UpdateTheoreticalDisplay();

            // Скрыть надпись "нет истории"
            if (lblNoHistory != null)
                lblNoHistory.Visibility = Visibility.Collapsed;

            // Записываем начальное состояние (день 0.0)
            transitionHistory.Add(new TransitionRecord { Time = 0, State = currentState });
            // Добавляем на график с алгоритмом 2
            int startIdx = currentState - 1;
            for (int i = 0; i < 3; i++)
            {
                double share = (i == startIdx) ? 1.0 : 0.0;
                alg2Series[i].Values.Add(new LiveCharts.Defaults.ObservablePoint(0, share));
            }
            UpdateTransitionList();


            isRunning = true;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            DisableMatrixInputs(true);

            // Запуск асинхронной симуляции
            _ = RunAlg2SimulationAsync();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            DisableMatrixInputs(false);
        }


        /// Полный сброс всех данных симуляции.
        private void ResetSimulation()
        {
            isRunning = false;
            currentTime = 0;
            currentState = cmbStartState.SelectedIndex + 1;
            transitionHistory.Clear();
            totalTimeInState[0] = 0;
            totalTimeInState[1] = 0;
            totalTimeInState[2] = 0;

            // Очистить графики 
            foreach (var series in alg2Series)
            {
                series.Values.Clear();
            }

            // Очистить историю UI
            pnlHistory.Children.Clear();
            if (lblNoHistory != null)
            {
                lblNoHistory.Visibility = Visibility.Visible;
                pnlHistory.Children.Add(lblNoHistory);
            }

            UpdateCurrentStateDisplay();

            chartAlg2.Update(true, true);
        }


        /// Блокировка/разблокировка полей ввода матрицы.
        private void DisableMatrixInputs(bool disabled)
        {
            txtQ01.IsReadOnly = disabled;
            txtQ02.IsReadOnly = disabled;
            txtQ10.IsReadOnly = disabled;
            txtQ12.IsReadOnly = disabled;
            txtQ20.IsReadOnly = disabled;
            txtQ21.IsReadOnly = disabled;
            txtSimDays.IsReadOnly = disabled;
            cmbStartState.IsEnabled = !disabled;
        }

        /// Обновляет только теоретические линии на графике Алгоритма 2
        private void UpdateAlg2TheoreticalLines()
        {
            if (chartAlg2.AxisY.Count == 0 || theoProbs.Count != 3) return;

            var yAxis = chartAlg2.AxisY[0];
            yAxis.Sections.Clear(); // Удаляем старые линии

            for (int i = 0; i < 3; i++)
            {
                yAxis.Sections.Add(new AxisSection
                {
                    Value = theoProbs[i],
                    Stroke = StateColors[i],
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 5, 3 }
                });
            }

            chartAlg2.Update(true, true); // Принудительно обновляем график
        }
        // Запуск алгоритма 2 симуляции
        private async Task RunAlg2SimulationAsync()
        {
            while (currentTime < simulationEndTime && isRunning)
            {
                int currentStateIndex = currentState - 1;

                // 1. Генерируем время пребывания ~ Exp(λ)
                // Формула: τ = -ln(U) / λ, где U ~ Uniform(0,1)
                double holdingTime = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda[currentStateIndex];

                // 2. Algorithm 2: накапливаем точное время в состоянии
                // Если holdingTime выходит за simulationEndTime — обрезаем
                double actualHoldingTime = Math.Min(holdingTime, simulationEndTime - currentTime);
                totalTimeInState[currentStateIndex] += actualHoldingTime;

                // 3. Продвигаем непрерывное время
                currentTime += holdingTime;

                if (currentTime > simulationEndTime) break;

                // 4. Выбираем следующее состояние по вероятностям перехода
                // P(i→j) = Q[i,j] / λ[i] для j ≠ i, P(i→i) = 0

                // Подготовить массив вероятностей перехода (индекс с 0) 
                double[] transProbs = new double[3];
                for (int j = 0; j < 3; j++)
                {
                    // Самопереход запрещён во вложенной цепи
                    transProbs[j] = (j == currentStateIndex) ? 0 : Q[currentStateIndex, j] / lambda[currentStateIndex];
                }
                double r = rand.NextDouble();
                int nextIndex = SelectEvent(r, transProbs); // индекс с 0 
                int nextState = nextIndex + 1; // индекс с 1

                // 5. Записываем событие перехода
                currentState = nextState;
                transitionHistory.Add(new TransitionRecord { Time = currentTime, State = currentState });

                // 6. Обновляем UI
                UpdateCurrentStateDisplay();
                UpdateTransitionList();
                UpdateAlg2Chart();
                UpdateFinalTimeShares();

                // 7. Пауза для визуализации (не влияет на результат вычислений)
                await Task.Delay(int.Parse(txtSpeed.Text));
            }

            // Симуляция завершена
            isRunning = false;
            alg2Complete = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            DisableMatrixInputs(false);
        }

        // ======================== ОБНОВЛЕНИЕ UI ========================


        /// Обновление отображения текущего состояния (иконка + подпись).
        private void UpdateCurrentStateDisplay()
        {
            int idx = currentState - 1;
            string stateName = StateNames[idx];

            Dispatcher.Invoke(() =>
            {
                string imageName = idx switch
                {
                    0 => "sunny.png",
                    1 => "cloudy.png",
                    2 => "overcast.png",
                    _ => "sunny.png"
                };

                try
                {
                    var uri = new Uri($"pack://application:,,,/images/{imageName}", UriKind.Absolute);
                    imgCurrentState.Source = BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch { }

                lblCurrentState.Text = $"{currentState} — {stateName}";
                lblCurrentDay.Text = $"День: {currentTime:F2}";
            });
        }


        /// Добавление нового элемента в ленту истории состояний.
        private void UpdateTransitionList()
        {
            if (transitionHistory.Count == 0) return;

            var record = transitionHistory[^1];
            int idx = record.State - 1;

            Dispatcher.Invoke(() =>
            {
                if (lblNoHistory != null && pnlHistory.Children.Contains(lblNoHistory))
                {
                    pnlHistory.Children.Remove(lblNoHistory);
                }

                var border = new Border
                {
                    Style = (Style)FindResource("HistoryItemStyle"),
                    ToolTip = $"День: {record.Time:F2}\nСостояние: {record.State} — {record.StateName}"
                };

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                var image = new Image
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(3)
                };

                string imageName = idx switch
                {
                    0 => "sunny.png",
                    1 => "cloudy.png",
                    2 => "overcast.png",
                    _ => "sunny.png"
                };

                try
                {
                    var uri = new Uri($"pack://application:,,,/images/{imageName}", UriKind.Absolute);
                    image.Source = BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch { }

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                textBlock.Inlines.Add(new Run { Text = $"День {record.Time:F1}", FontWeight = FontWeights.Bold });
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run { Text = $"{record.StateName}" });

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(textBlock);
                border.Child = stackPanel;

                pnlHistory.Children.Insert(0, border);

                const int maxHistoryItems = 500;
                while (pnlHistory.Children.Count > maxHistoryItems)
                {
                    pnlHistory.Children.RemoveAt(pnlHistory.Children.Count - 1);
                }
            });
        }


        /// Обновление графика алгоритмы 2: доля времени в каждом состоянии.
        private void UpdateAlg2Chart()
        {
            double totalExact = totalTimeInState.Sum();
            if (totalExact <= 0) return;
            Dispatcher.Invoke(() =>
            {
                // 1. Добавляем новую точку
                for (int i = 0; i < 3; i++)
                {
                    double share = totalTimeInState[i] / totalExact;
                    alg2Series[i].Values.Add(new LiveCharts.Defaults.ObservablePoint(currentTime, share));
                }

                // 2. Удаляем старые, если превысили предел
                if (alg2Series[0].Values.Count > 365)
                {
                    foreach (var s in alg2Series)
                    {
                        if (s.Values.Count > 0) s.Values.RemoveAt(0);
                    }
                }

                // 3. САМОЕ ВАЖНОЕ: Двигаем видимую область оси X
                // Чтобы ось всегда показывала последние 365 дней от текущего момента
                var xAxis = chartAlg2.AxisX[0];
                xAxis.MaxValue = currentTime;
                xAxis.MinValue = currentTime - 365 > 0 ? currentTime - 365 : 0;
            });
        }


        /// Обновление отображения теоретических значений (общее для обоих алгоритмов).
        private void UpdateTheoreticalDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                if (theoProbs.Count == 3)
                {
                    lblTheo1.Text = $"π₁ = {theoProbs[0]:F4}";
                    lblTheo2.Text = $"π₂ = {theoProbs[1]:F4}";
                    lblTheo3.Text = $"π₃ = {theoProbs[2]:F4}";
                }
            });
        }

        // ======================== АЛГОРИТМ 1: Batch-симуляция ========================


        /// Кнопка запуска алгоритма 1: N независимых запусков, каждый длительностью T.
        /// Считаем частоту конечных состояний.
        private async void BtnAlg1Run_Click(object sender, RoutedEventArgs e)
        {
            double T = ParseSafe(txtAlg1T.Text);
            int N = (int)ParseSafe(txtAlg1N.Text);

            if (T <= 0 || N <= 0)
            {
                MessageBox.Show("T и N должны быть положительными.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ReadAndValidateParameters())
                return;

            CalculateTheoreticalDistribution();
            UpdateTheoreticalDisplay();

            btnAlg1Run.IsEnabled = false;
            lblAlg1Status.Text = "Выполняется...";

            int startState = cmbStartState.SelectedIndex + 1; // читаем на UI-потоке

            // Запуск batch-симуляции в фоне
            await Task.Run(() =>
            {
                int[] finalCounts = new int[3];

                for (int run = 0; run < N; run++)
                {
                    int state = startState; // начальное состояние
                    double time = 0;

                    while (time < T)
                    {
                        int stateIdx = state - 1;
                        // Время удержания в состоянии (экспоненциальное распределение)
                        double hold = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda[stateIdx];
                        time += hold;
                        if (time > T) break;

                        // Выбираем следующее состояние
                        // Подготавливаем массив вероятностей перехода
                        double[] transProbs = new double[3];
                        for (int j = 0; j < 3; j++)
                        {
                            // P(i→j) = Q[i,j] / λ[i] для j≠i, P(i→i) = 0
                            transProbs[j] = (j == stateIdx) ? 0 : Q[stateIdx, j] / lambda[stateIdx];
                        }

                        // Использовать универсальный алгоритм выбора
                        double r = rand.NextDouble();
                        int nextIdx = SelectEvent(r, transProbs);  // индекс с нуля

                        // Конвертировать в индекс с 1
                        state = nextIdx + 1;
                    }

                    finalCounts[state - 1]++;
                }

                // Сохраняем результаты для экспорта
                alg1Counts = finalCounts;
                alg1N = N;
                alg1T = T;
                alg1Complete = true;

                // Обновляем UI из UI-потока
                Dispatcher.Invoke(() =>
                {
                    double freq0 = Math.Round((double)finalCounts[0] / N, 4);
                    double freq1 = Math.Round((double)finalCounts[1] / N, 4);
                    double freq2 = Math.Round((double)finalCounts[2] / N, 4);

                    if (alg1Series.Count >= 2)
                    {
                        alg1Series[0].Values = new ChartValues<double> { freq0, freq1, freq2 };

                        if (theoProbs.Count == 3)
                        {
                            alg1Series[1].Values = new ChartValues<double>
                            {
                                Math.Round(theoProbs[0], 4),
                                Math.Round(theoProbs[1], 4),
                                Math.Round(theoProbs[2], 4)
                            };
                        }
                    }

                    lblAlg1Emp1.Text = $"Ясно: {freq0:F4}";
                    lblAlg1Emp2.Text = $"Облачно: {freq1:F4}";
                    lblAlg1Emp3.Text = $"Пасмурно: {freq2:F4}";

                    lblAlg1Status.Text = $"Готово. N={N}, T={T:F0} дн.";
                    chartAlg1.Update(true);
                    btnAlg1Run.IsEnabled = true;
                });
            });
        }

        // ======================== ЭКСПОРТ CSV ========================


        /// Экспорт результатов в два CSV файла: summary + history.
        /// Доступен только после запуска обоих алгоритмов.
        private void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (!alg1Complete)
            {
                MessageBox.Show("Запустите алгоритм 1 перед экспортом.",
                                "Экспорт CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!alg2Complete)
            {
                MessageBox.Show("Запустите алгоритм 2 перед экспортом.",
                                "Экспорт CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"lab7_summary_T{alg1T:F0}_N{alg1N}.csv",
                Title = "Сохранить файлы в выбранной папке"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string dir = Path.GetDirectoryName(saveDialog.FileName);
                if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;

                string summaryPath = Path.Combine(dir, $"lab7_summary_T{alg1T:F0}_N{alg1N}.csv");
                string historyPath = Path.Combine(dir, $"lab7_history_T{alg1T:F0}_N{alg1N}.csv");

                ExportSummary(summaryPath);
                ExportHistory(historyPath);

                MessageBox.Show($"Файлы сохранены:\n{summaryPath}\n{historyPath}",
                                "Экспорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        /// Экспорт сводной таблицы: оба алгоритма + теория.
        private void ExportSummary(string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);

            writer.WriteLine("Состояние,Alg1_Частота,Alg2_Доля,Alg2_Время,Теоретическое_π,Alg1_Ошибка,Alg2_Ошибка");

            for (int i = 0; i < 3; i++)
            {
                double alg1Freq = alg1N > 0 ? (double)alg1Counts[i] / alg1N : 0;
                double alg2ShareExport = alg2TotalSimulationTime > 0 ? alg2Share[i] : 0;
                double theoretical = theoProbs.Count > i ? theoProbs[i] : 0;

                writer.WriteLine($"{StateNames[i]},{alg1Freq:F4},{alg2ShareExport:F4},{totalTimeInState[i]:F2},{theoretical:F4},{Math.Abs(alg1Freq - theoretical):F4},{Math.Abs(alg2ShareExport - theoretical):F4}");
            }
        }


        /// Экспорт истории переходов (алгоритм 2).
        private void ExportHistory(string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);

            writer.WriteLine("№,Время,Состояние,Название");
            for (int i = 0; i < transitionHistory.Count; i++)
            {
                var rec = transitionHistory[i];
                writer.WriteLine($"{i},{rec.Time:F4},{rec.State},{rec.StateName}");
            }
        }

        /// Алгоритм выбора события из полной группы:
        /// A := alpha; k := 1;
        /// A := A - p_k; если A <= 0 — произошло событие k, иначе k++
        private static int SelectEvent(double alpha, double[] probs)
        {
            double a = alpha;
            for (int k = 0; k < probs.Length - 1; k++)
            {
                a -= probs[k]; // вычитаем вероятность k-го события
                if (a <= 0)
                    return k;  // событие k произошло
            }
            return probs.Length - 1; // последнее событие — всё, что осталось
        }
    }
}

