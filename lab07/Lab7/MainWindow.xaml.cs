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

        /// <summary>
        /// Обработчик изменения любой ячейки матрицы — автопересчёт диагонали.
        /// </summary>
        private void MatrixCell_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitialized)
                UpdateDiagonal();
        }

        /// <summary>
        /// Разрешаем ввод цифр, точки, запятой и минуса в поля матрицы.
        /// </summary>
        private void MatrixCell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем: цифры, точка, запятая, минус
            string text = e.Text;
            bool isAllowed = text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-');
            e.Handled = !isAllowed;
        }

        /// <summary>
        /// Инициализация LiveCharts серий.
        /// </summary>
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
            alg2Series.Add(new ColumnSeries
            {
                Title = "Доля времени",
                Values = new ChartValues<double> { 0, 0, 0 },
                Fill = new SolidColorBrush(Color.FromRgb(100, 180, 100))
            });
            alg2Series.Add(new ColumnSeries
            {
                Title = "Теоретическое π",
                Values = new ChartValues<double> { 0, 0, 0 },
                Fill = new SolidColorBrush(Color.FromRgb(70, 130, 180))
            });
            chartAlg2.Series = alg2Series;
            chartAlg2.AxisY.Add(new Axis
            {
                Title = "Доля времени",
                MinValue = 0,
                MaxValue = 1,
                LabelFormatter = val => val.ToString("N1"),
                Separator = new LiveCharts.Wpf.Separator { Step = 0.1 }
            });
        }

        // ======================== ОБРАБОТКА МАТРИЦЫ Q ========================

        /// <summary>
        /// Привязка событий к полям ввода матрицы для автопересчёта диагонали.
        /// </summary>
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

        /// <summary>
        /// Безопасный парсинг double из строки (null-safe).
        /// </summary>
        private static double ParseSafe(string? text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val;
            return 0;
        }

        /// <summary>
        /// Чтение и валидация параметров из UI. Строит матрицу Q и массив λ.
        /// </summary>
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

        /// <summary>
        /// Расчёт стационарного распределения: π·Q = 0, Σπᵢ = 1
        /// для 3-х состояний
        /// </summary>
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

        // ======================== КНОПКИ УПРАВЛЕНИЯ ========================

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            if (!ReadAndValidateParameters())
                return;

            ResetSimulation();
            CalculateTheoreticalDistribution();
            UpdateTheoreticalDisplay();

            // Скрыть надпись "нет истории"
            if (lblNoHistory != null)
                lblNoHistory.Visibility = Visibility.Collapsed;

            // Записываем начальное состояние (день 0.0)
            transitionHistory.Add(new TransitionRecord { Time = 0, State = currentState });
            UpdateTransitionList();

            isRunning = true;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            DisableMatrixInputs(true);

            // Запуск асинхронной симуляции
            _ = RunSimulationAsync();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            DisableMatrixInputs(false);
        }

        /// <summary>
        /// Полный сброс всех данных симуляции.
        /// </summary>
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
            if (alg2Series.Count >= 2)
            {
                alg2Series[0].Values = new ChartValues<double> { 0, 0, 0 };
                alg2Series[1].Values = new ChartValues<double> { 0, 0, 0 };
            }

            // Очистить историю UI
            pnlHistory.Children.Clear();
            if (lblNoHistory != null)
            {
                lblNoHistory.Visibility = Visibility.Visible;
                pnlHistory.Children.Add(lblNoHistory);
            }

            UpdateCurrentStateDisplay();
            UpdateAlg2Chart();
            chartAlg2.Update(true);
        }

        /// <summary>
        /// Блокировка/разблокировка полей ввода матрицы.
        /// </summary>
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

        // ======================== ЯДРО СИМУЛЯЦИИ ========================

        private async Task RunSimulationAsync()
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
                UpdateAlg2Chart();

                // 7. Пауза для визуализации (не влияет на математику!)
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

        /// <summary>
        /// Обновление отображения текущего состояния (иконка + подпись).
        /// </summary>
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

        /// <summary>
        /// Добавление нового элемента в ленту истории состояний.
        /// </summary>
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

        /// <summary>
        /// Обновление графика алгоритмы 2: доля времени в каждом состоянии.
        /// Троттлинг: обновление не чаще чем раз в 300мс.
        /// </summary>
        private long _lastAlg2UpdateMs = 0;
        private const long Alg2ThrottleMs = 300;

        private void UpdateAlg2Chart()
        {
            double totalExact = totalTimeInState[0] + totalTimeInState[1] + totalTimeInState[2];

            double pct0 = totalExact > 0 ? totalTimeInState[0] / totalExact : 0;
            double pct1 = totalExact > 0 ? totalTimeInState[1] / totalExact : 0;
            double pct2 = totalExact > 0 ? totalTimeInState[2] / totalExact : 0;

            long nowMs = Environment.TickCount64;
            if (nowMs - _lastAlg2UpdateMs < Alg2ThrottleMs) return;
            _lastAlg2UpdateMs = nowMs;

            lblAlg2Pct1.Text = $"Ясно: {pct0:F4}";
            lblAlg2Pct2.Text = $"Облачно: {pct1:F4}";
            lblAlg2Pct3.Text = $"Пасмурно: {pct2:F4}";

            Dispatcher.Invoke(() =>
            {
                if (alg2Series.Count >= 2)
                {
                    alg2Series[0].Values = new ChartValues<double>
                    {
                        Math.Round(pct0, 4),
                        Math.Round(pct1, 4),
                        Math.Round(pct2, 4)
                    };

                    if (theoProbs.Count == 3)
                    {
                        alg2Series[1].Values = new ChartValues<double>
                        {
                            Math.Round(theoProbs[0], 4),
                            Math.Round(theoProbs[1], 4),
                            Math.Round(theoProbs[2], 4)
                        };
                    }
                }

                chartAlg2.Update(true);
            });
        }

        /// <summary>
        /// Обновление отображения теоретических значений (общее для обоих алгоритмов).
        /// </summary>
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

        /// <summary>
        /// Кнопка запуска алгоритма 1: N независимых запусков, каждый длительностью T.
        /// Считаем частоту конечных состояний.
        /// </summary>
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
                        double hold = -Math.Log(Math.Max(rand.NextDouble(), 1e-10)) / lambda[stateIdx];
                        time += hold;
                        if (time > T) break;

                        // Выбираем следующее состояние
                        double r = rand.NextDouble();
                        double cum = 0;
                        int next = state;

                        for (int j = 0; j < 3; j++)
                        {
                            if (j == stateIdx) continue;
                            double tp = Q[stateIdx, j] / lambda[stateIdx];
                            cum += tp;
                            if (r < cum)
                            {
                                next = j + 1;
                                break;
                            }
                        }
                        state = next;
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

        /// <summary>
        /// Экспорт результатов в два CSV файла: summary + history.
        /// Доступен только после запуска обоих алгоритмов.
        /// </summary>
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

        /// <summary>
        /// Экспорт сводной таблицы: оба алгоритма + теория.
        /// </summary>
        private void ExportSummary(string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);

            double totalExact = totalTimeInState.Sum();

            writer.WriteLine("Состояние,Alg1_Частота,Alg2_Доля,Alg2_Время,Теоретическое_π,Alg1_Ошибка,Alg2_Ошибка");

            for (int i = 0; i < 3; i++)
            {
                double alg1Freq = alg1N > 0 ? (double)alg1Counts[i] / alg1N : 0;
                double alg2Share = totalExact > 0 ? totalTimeInState[i] / totalExact : 0;
                double theoretical = theoProbs.Count > i ? theoProbs[i] : 0;

                writer.WriteLine($"{StateNames[i]},{alg1Freq:F4},{alg2Share:F4},{totalTimeInState[i]:F2},{theoretical:F4},{Math.Abs(alg1Freq - theoretical):F4},{Math.Abs(alg2Share - theoretical):F4}");
            }
        }

        /// <summary>
        /// Экспорт истории переходов (алгоритм 2).
        /// </summary>
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

        /// <summary>
        /// Подсчёт количества посещений состояния (по истории переходов).
        /// </summary>
        private int CountVisits(int state)
        {
            return transitionHistory.Count(r => r.State == state);
        }

        // ======================== ВСПОМОГАТЕЛЬНЫЕ ========================
    }
}
