using System.Windows.Forms.DataVisualization.Charting;

namespace Simulation1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // График
            var area = chart1.ChartAreas[0];
            area.AxisX.Title = "Дальность, м";
            area.AxisY.Title = "Высота, м";
            area.AxisX.Minimum = 0;
            area.AxisY.Minimum = 0;

            // Максимальные значения
            numericHeight.Maximum = 10000;
            numericAngle.Maximum = 90;
            numericSpeed.Maximum = 10000;
            numericSize.Maximum = 10;
            numericWeight.Maximum = 100000;

            // Значения по умолчанию
            numericHeight.Value = 0;
            numericAngle.Value = 45;
            numericSpeed.Value = 100;
            numericSize.Value = 0.10m;
            numericWeight.Value = 1;
            numericStep.Value = 1.0m;

            MinimumSize = new Size(800, 800);
        }

        private void buttonLaunch_Click(object sender, EventArgs e)
        {
            // Получаем значения
            double h0 = (double)numericHeight.Value;       // начальная высота, метры
            double angleDeg = (double)numericAngle.Value;   // угол, градусы
            double v0 = (double)numericSpeed.Value;         // начальная скорость, м/с
            double diameter = (double)numericSize.Value;     // диаметр, м
            double mass = (double)numericWeight.Value;       // масса, кг

            double dt = (double)numericStep.Value;

            // Физические константы
            const double g = 9.81;        // ускорение свободного падения, м/с^2
            const double rho = 1.29;     // плотность воздуха, кг/м^3
            const double Cd = 0.15;       // Коэффициент лобового сопротивления 
            double S = Math.PI * Math.Pow(diameter / 2.0, 2); // площадь поперечного сечения
            double k = Cd * S * rho / (2.0 * mass);              // k = C*S*rho/(2m)

            // Начальные условия
            double angleRad = angleDeg * Math.PI / 180.0;
            double x = 0, y = h0;
            double vx = v0 * Math.Cos(angleRad);
            double vy = v0 * Math.Sin(angleRad);

            double maxHeight = y;

            // Точки для графика
            var plotX = new List<double> { x };
            var plotY = new List<double> { y };
            while (y >= 0)
            {
                double v = Math.Sqrt(vx * vx + vy * vy);

                // Обновление скорости
                vx = vx - k * vx * v * dt;
                vy = vy - (g + k * vy * v) * dt;

                // Обновление координат
                x = x + vx * dt;
                y = y + vy * dt;

                if (y > maxHeight) maxHeight = y;

                plotX.Add(x);
                plotY.Add(y);
            }

            double range = x;
            double finalSpeed = Math.Sqrt(vx * vx + vy * vy);

            // Добавляем траекторию (не убирая предыдущие) 
            string seriesName = $"dt={dt} #{chart1.Series.Count}";
            var series = new Series(seriesName)
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2
            };
            for (int i = 0; i < plotX.Count; i++)
                series.Points.AddXY(plotX[i], plotY[i]);
            chart1.Series.Add(series);

            // Запишем результаты
            textBoxResults.AppendText(
                $"{seriesName,-18}" +
                $"Дальность={range,10:F4} м   " +
                $"Макс.высота={maxHeight,10:F4} м   " +
                $"V конеч.={finalSpeed,10:F4} м/с" +
                Environment.NewLine);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            textBoxResults.Clear();
        }
    }
}
