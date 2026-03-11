using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CellularAutomaton
{
    public static class Constants
    {
        // Вероятность того, что заспавнится облако за один такт (если не превышено максимальное количество облаков)
        public const float CLOUD_SPAWN_CHANCE = 0.1f;
        // Минимальный радиус облака
        public const int CLOUD_MIN_RADIUS = 22;
        // Максимальный радиус облака
        public const int CLOUD_MAX_RADIUS = 55;
        // Шанс загореться дереву, у которого горит соседнее дерево 
        public const float BASE_NEIGHBOR_IGNITE_CHANCE = 0.1f;
        // При максимальной силе ветра (1.0), на сколько по X и по Y будет перемещено облако за один тик
        public const float WIND_CLOUD_MOVEMENT = 5.0f;
        // С какого расстояния за пределами экрана считается, что объект ушёл за экран и его можно удалять (облака)
        public const int OUT_OF_SCREEN_DISTANCE = 150;
    }

    // Представляет возможные физические состояния клетки в сетке симуляции.
    public enum CellState { Empty, Tree, Burning, Water, Charred }

    // Представляет дождевое облако, которое движется по карте и тушит пожары.
    public class Cloud
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }

        public Cloud(float x, float y, float radius, Random rnd)
        {
            X = x;
            Y = y;
            Radius = radius;
        }
    }

    // Основной движок логики для симуляции клеточного автомата.
    public class ForestFireSim
    {
        public int Width { get; }
        public int Height { get; }

        // Двойная буферизация сеток для обеспечения одновременного обновления всех клеток
        public CellState[,] Grid;
        private CellState[,] _nextGrid;

        // Отслеживает, сколько шагов клетка уже горит (запас топлива)
        private int[,] _fuel;

        // Параметры симуляции
        // Вероятность что за тик на пустой клетке вырастет дерево
        public float ProbabilityGrowth = 0.005f;
        // Вероятность молнии (что клетка с деревом станет горящей) 
        public float ProbabilityLightning = 0.001f;
        // Сила ветра (это добавляется к базовой вероятности загореться дереву, у которого горит сосед
        // т.е. при максимальной сонаправленности ветра, добавляется WindStrength к вероятности загореться) 
        // Напр. BASE = 0.1 + WIND = 0.4 = 0.5
        public float WindStrength = 0.2f;
        // Максимальное количество одновременно существующих облаков 
        public int MaxClouds = 5;

        // Физика ветра
        public PointF WindVector = new PointF(0, -1); // По умолчанию — на Север
        // Градусы
        public float WindAngle = 0f;
        // Автоматическое случайное изменение направления ветра 
        public bool AutoChangeWind = true;

        public List<Cloud> Clouds = new List<Cloud>();
        private Random _rnd = new Random();

        public ForestFireSim(int width, int height)
        {
            Width = width;
            Height = height;

            Grid = new CellState[width, height];
            _nextGrid = new CellState[width, height];
            _fuel = new int[width, height];

            Reset();
        }

        // Заполняет сетку начальным ландшафтом.
        public void Reset()
        {
            Clouds.Clear();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    double r = _rnd.NextDouble();
                    // 20% шанс появления дерева, 2% — воды, иначе — пустое место
                    if (r < 0.20) Grid[x, y] = CellState.Tree;
                    else if (r < 0.22) Grid[x, y] = CellState.Water;
                    else Grid[x, y] = CellState.Empty;

                    _fuel[x, y] = 3; // Деревья горят в течение 3 тактов
                }
            }
        }

        // Продвигает симуляцию на один такт вперед.
        public void Step()
        {
            UpdateWind();
            UpdateClouds();

            // Вычисление следующих состояний
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CellState current = Grid[x, y];
                    bool isRaining = IsUnderAnyCloud(x, y);

                    // Поведение по умолчанию: сохранение текущего состояния
                    _nextGrid[x, y] = current;

                    // Логика клеточного автомата 
                    if (current == CellState.Empty && _rnd.NextDouble() < ProbabilityGrowth)
                    {
                        _nextGrid[x, y] = CellState.Tree;
                    }
                    else if (current == CellState.Tree && !isRaining && ShouldIgnite(x, y))
                    {
                        _nextGrid[x, y] = CellState.Burning;
                    }
                    else if (current == CellState.Burning)
                    {
                        // Тушение, если идет дождь или если топливо закончилось
                        if (isRaining || --_fuel[x, y] <= 0)
                        {
                            _nextGrid[x, y] = CellState.Charred;
                            _fuel[x, y] = 3; // Сброс топлива для будущих деревьев
                        }
                    }
                    else if (current == CellState.Charred)
                    {
                        _nextGrid[x, y] = CellState.Empty; // Пепел быстро исчезает
                    }
                }
            }

            // Применение нового поколения
            Array.Copy(_nextGrid, Grid, Width * Height);
        }

        private void UpdateWind()
        {
            if (AutoChangeWind)
            {
                // Случайное изменение угла ветра и ограничение в пределах 0-360 градусов
                // +- 6 градусов
                WindAngle += (float)(_rnd.NextDouble() - 0.5) * 12.0f;
                // приводим в диапазон 0-360 градусов
                // т.к. -10 % 360 = -10 Делаем его положительным (+360) и до 360 % 360
                WindAngle = (WindAngle % 360 + 360) % 360;
            }

            // Перевод градусов в радианы и расчет 2D вектора
            double rad = Math.PI * WindAngle / 180.0;

            // Получаем вектор (можно на единичной окружности показать) 
            // минус так как Y наоброт в приложениях от математики
            WindVector = new PointF((float)Math.Cos(rad), - (float)Math.Sin(rad));
        }

        private void UpdateClouds()
        {
            // Итерация в обратном порядке для безопасного (чтобы не пропускать некоторые элементы при их удалении)
            // удаления элементов во время цикла
            for (int i = Clouds.Count - 1; i >= 0; i--)
            {
                var c = Clouds[i];

                // Движение облака на основе силы ветра
                c.X += WindVector.X * WindStrength * Constants.WIND_CLOUD_MOVEMENT;
                c.Y += WindVector.Y * WindStrength * Constants.WIND_CLOUD_MOVEMENT;

                // Удаление облака, если оно ушло слишком далеко за пределы экрана
                if (c.X < -Constants.OUT_OF_SCREEN_DISTANCE || 
                    c.X > Width + Constants.OUT_OF_SCREEN_DISTANCE ||
                    c.Y < -Constants.OUT_OF_SCREEN_DISTANCE ||
                    c.Y > Height + Constants.OUT_OF_SCREEN_DISTANCE)
                {
                    Clouds.RemoveAt(i);
                }
            }

            // Случайное создание новых облаков до достижения лимита
            if (Clouds.Count < MaxClouds && _rnd.NextDouble() < Constants.CLOUD_SPAWN_CHANCE)
            {
                SpawnCloud();
            }
        }

        private void SpawnCloud()
        {
            float x, y;

            // Создание облака с той стороны, ОТКУДА дует ветер
            // Если дует больше горизонтально, чем вертикально
            if (Math.Abs(WindVector.X) > Math.Abs(WindVector.Y))
            {
                // Если дует вправо, то появляется слева. Если дует влево, то появляется справва
                x = WindVector.X > 0 ? -100 : Width + 100;
                // Случайногде то по Y
                y = _rnd.Next(-50, Height + 50);
            }
            else
            {
                // случайно по X
                x = _rnd.Next(-50, Width + 50);
                // Если дует вниз, то появляется сверху, если дует вверх то появляется снизу
                y = WindVector.Y > 0 ? -100 : Height + 100;
            }

            Clouds.Add(new Cloud(x, y, _rnd.Next(Constants.CLOUD_MIN_RADIUS, Constants.CLOUD_MAX_RADIUS), _rnd));
        }

        // Использует квадрат расстояния для определения, находится ли точка под облаком.
        public bool IsUnderAnyCloud(int x, int y)
        {
            // Если расстояние до любого из облака меньше чем его радиус, возвращаем True
            return Clouds.Any(c => (x - c.X) * (x - c.X) + (y - c.Y) * (y - c.Y) < (c.Radius * c.Radius));
        }

        // Определяет, должно ли дерево загореться (от соседей, ветра или молнии).
        private bool ShouldIgnite(int x, int y)
        {
            // Самопроизвольное возгорание (молния)
            if (_rnd.NextDouble() < ProbabilityLightning) return true;

            // Проверка окрестности Мура (8 соседних клеток)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Пропуск самой клетки

                    int neighborX = x + dx;
                    int neighborY = y + dy;

                    // Проверка границ и проверка на горение соседа
                    if (neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height &&
                        Grid[neighborX, neighborY] == CellState.Burning)
                    {
                        float length = (float)Math.Sqrt(dx * dx + dy * dy); // 1 или 1.41
                        // windvector единичный так как длина его sqrt(sin^2(alpha) + cos^2(alpha)) = 1

                        // Насколько направление распространения огня совпадает с ветром? [0,1]; 1 - угол равен нулю.
                        // cos alpha = (A dot B ) / (|A| * |B|)
                        float dotProduct = ((-dx * WindVector.X) + (-dy * WindVector.Y))/length;

                        double prob = Constants.BASE_NEIGHBOR_IGNITE_CHANCE + (dotProduct * WindStrength);

                        if (_rnd.NextDouble() < prob) return true;
                    }
                }
            }
            return false;
        }
    }

    // Главное окно пользовательского интерфейса, отвечающее за отрисовку и ввод.
    public partial class Form1 : Form
    {
        private ForestFireSim _sim = null!;
        private System.Windows.Forms.Timer _timer = null!;
        private FlowLayoutPanel _controlPanel = null!;
        private TrackBar _windTrackBar = null!;

        public Form1()
        {
            this.Text = "Forest Fire Simulation";
            this.DoubleBuffered = true; // Предотвращает мерцание интерфейса
            this.WindowState = FormWindowState.Maximized;

            _sim = new ForestFireSim(180, 100);
            _timer = new System.Windows.Forms.Timer { Interval = 30 };

            _timer.Tick += (s, e) => {
                _sim.Step();
                if (_sim.AutoChangeWind) _windTrackBar.Value = (int)_sim.WindAngle;
                Invalidate(); // Вызывает OnPaint
            };

            CreateUI();
        }

        private void CreateUI()
        {
            _controlPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(10),
                AutoScroll = true
            };

            // Вспомогательная функция для генерации слайдеров 
            void AddSlider(string name, int min, int max, int start, Action<int, Label> onVal, bool isWind = false)
            {
                Panel p = new Panel { Width = 230, Height = 80, Margin = new Padding(5) };
                Label l = new Label { Text = name, Dock = DockStyle.Top, AutoSize = true };
                TrackBar t = new TrackBar { Minimum = min, Maximum = max, Value = start, Dock = DockStyle.Bottom, TickStyle = TickStyle.None };

                t.ValueChanged += (s, e) => onVal(t.Value, l);
                if (isWind) _windTrackBar = t;

                p.Controls.Add(l);
                p.Controls.Add(t);
                _controlPanel.Controls.Add(p);
                onVal(start, l); // Инициализация текста метки
            }

            AddSlider("Шанс роста", 0, 100, 5, (v, l) => {
                _sim.ProbabilityGrowth = v / 100f;
                l.Text = $"Шанс роста: {v}%";
            });

            AddSlider("Риск огня", 0, 100, 1, (v, l) => {
                _sim.ProbabilityLightning = v / 1000f;
                l.Text = $"Риск огня: {v / 10f}%";
            });

            AddSlider("Задержка (мс)", 1, 200, 50, (v, l) => {
                _timer.Interval = v;
                l.Text = $"Задержка: {v} мс";
            });

            AddSlider("Максимум облаков", 0, 40, 5, (v, l) => {
                _sim.MaxClouds = v;
                l.Text = $"Максимум облаков: {v}";
            });

            AddSlider("Сила ветра", 0, 100, 20, (v, l) => {
                _sim.WindStrength = v / 100f;
                l.Text = $"Сила ветра: {v / 100f:F2}";
            });

            AddSlider("Угол ветра", 0, 360, 0, (v, l) => {
                _sim.WindAngle = v;
                l.Text = $"Угол ветра: {v}°";
            }, true);

            Button btn = new Button { Text = "Старт/Стоп", Size = new Size(100, 50), Font = new Font("Arial", 9, FontStyle.Bold), Margin = new Padding(5, 20, 5, 5) };
            btn.Click += (s, e) => _timer.Enabled = !_timer.Enabled;

            CheckBox chk = new CheckBox { Text = "Авто-ветер", Checked = true, AutoSize = true, Margin = new Padding(5, 35, 0, 0) };
            chk.CheckedChanged += (s, e) => _sim.AutoChangeWind = chk.Checked;

            _controlPanel.Controls.Add(btn);
            _controlPanel.Controls.Add(chk);
            this.Controls.Add(_controlPanel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_sim == null) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cellW = (float)ClientSize.Width / _sim.Width;
            float cellH = (float)(ClientSize.Height - _controlPanel.Height) / _sim.Height;

            // Отрисовка сетки карты
            for (int x = 0; x < _sim.Width; x++)
            {
                for (int y = 0; y < _sim.Height; y++)
                {
                    // Использование системных кистей 
                    Brush b = _sim.Grid[x, y] switch
                    {
                        CellState.Tree => Brushes.ForestGreen,
                        CellState.Burning => Brushes.OrangeRed,
                        CellState.Water => Brushes.DodgerBlue,
                        CellState.Charred => Brushes.DimGray,
                        _ => Brushes.Wheat
                    };
                    // Добавка +0.4f предотвращает появление щелей между плитками из-за округления float
                    g.FillRectangle(b, x * cellW, y * cellH, cellW + 0.4f, cellH + 0.4f);
                }
            }

            // Отрисовка облаков
            foreach (var c in _sim.Clouds)
            {
                float cx = c.X * cellW, cy = c.Y * cellH, cr = c.Radius * cellW;
                using (var rain = new SolidBrush(Color.FromArgb(60, 30, 60, 150)))
                {
                    g.FillEllipse(rain, cx - cr, cy - cr, cr * 2, cr * 2);
                }
            }

            // Отрисовка фона информационной панели (использование 'using' для очистки ресурсов кисти)
            using (SolidBrush infoBg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {   
                g.FillRectangle(infoBg, 10, 10, 250, 40);
            }
            g.DrawString($"Облаков ~на~ экране: {_sim.Clouds.Count}", Font, Brushes.White, 20, 22);

            // Отрисовка фона компаса
            int ax = ClientSize.Width - 80, ay = 60;
            g.FillEllipse(Brushes.White, ax - 45, ay - 45, 90, 90);
            // Рисуем круг компаса
            g.DrawEllipse(Pens.Black, ax - 45, ay - 45, 90, 90);

            // Север (North) - сверху
            g.DrawString("N", Font, Brushes.Blue, ax - 6, ay - 42);

            // Юг (South) - снизу
            g.DrawString("S", Font, Brushes.Red, ax - 6, ay + 28);

            // Запад (West) - слева
            // Смещаем по X влево на ~40 пикселей, по Y центрируем
            g.DrawString("W", Font, Brushes.Black, ax - 42, ay - 7);

            // Восток (East) - справа
            // Смещаем по X вправо на ~30 пикселей, по Y центрируем
            g.DrawString("E", Font, Brushes.Black, ax + 28, ay - 7);

            // Отрисовка стрелки компаса 
            using (Pen p = new Pen(Color.OrangeRed, 4) { CustomEndCap = new AdjustableArrowCap(3, 3) })
            {
                g.DrawLine(p, ax, ay, ax + _sim.WindVector.X * 30, ay + _sim.WindVector.Y * 30);
            }
        }
    }
}