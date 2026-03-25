using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteRV
{
    public partial class Form1 : Form
    {
        // Критическое значение хи-квадрат при alpha=0.05, df=4 (5 значений - 1)
        private static readonly double ChiSquaredCritical = 9.488;
        private readonly Random rng = new(); // базовый датчик

        // Значения дискретной случайной величины
        private static readonly int[] X = { 1, 2, 3, 4, 5 };

        public Form1()
        {
            InitializeComponent();
            UpdateP5Label();
        }

        // Считываем вероятности из полей ввода, p5 = 1 - сумма остальных
        private double[] GetProbabilities()
        {
            double p1 = (double)nudP1.Value;
            double p2 = (double)nudP2.Value;
            double p3 = (double)nudP3.Value;
            double p4 = (double)nudP4.Value;
            double p5 = 1.0 - p1 - p2 - p3 - p4;
            return new[] { p1, p2, p3, p4, p5 };
        }

        // Обновляем отображение p5 при изменении вероятностей
        private void UpdateP5Label()
        {
            double p5 = 1.0 - (double)(nudP1.Value + nudP2.Value + nudP3.Value + nudP4.Value);
            lblP5Value.Text = p5.ToString("0.00");
            lblP5Value.ForeColor = p5 < 0 ? Color.Red : Color.Black;
        }

        private void NudP_ValueChanged(object? sender, EventArgs e)
        {
            UpdateP5Label();
        }

        /// <summary>
        /// Генерация одной ДСВ по алгоритму из лекции:
        /// A := alpha (из базового датчика); k := 1;
        /// A := A - p_k; если A &lt;= 0, то x := x_k; иначе k++
        /// </summary>
        /// Генерация одной ДСВ по алгоритму из лекции:
        /// A := alpha (из базового датчика); k := 1;
        /// A := A - p_k; если A <= 0, то x := x_k; иначе k++
        private int GenerateDRV(double[] p)
        {
            double a = rng.NextDouble(); // alpha из базового датчика
            for (int k = 0; k < p.Length - 1; k++)
            {
                a -= p[k]; // вычитаем вероятность
                if (a <= 0)
                    return X[k]; // значение ДСВ определено
            }
            return X[p.Length - 1]; // последнее значение — всё, что осталось
        }

        /// <summary>
        /// Проводим N экспериментов, возвращаем массив счётчиков n_i
        /// </summary>
        private int[] RunExperiment(double[] p, int n)
        {
            int[] counts = new int[p.Length];
            for (int i = 0; i < n; i++)
            {
                int val = GenerateDRV(p);
                counts[val - 1]++;
            }
            return counts;
        }

        // Теоретическое мат. ожидание: E = sum(p_i * x_i)
        private static double TheoreticalE(double[] p)
        {
            double e = 0;
            for (int i = 0; i < p.Length; i++)
                e += p[i] * X[i];
            return e;
        }

        // Теоретическая дисперсия: D = sum(p_i * x_i^2) - E^2
        private static double TheoreticalD(double[] p)
        {
            double e = TheoreticalE(p);
            double ex2 = 0;
            for (int i = 0; i < p.Length; i++)
                ex2 += p[i] * X[i] * X[i];
            return ex2 - e * e;
        }

        // Статистика хи-квадрат: X^2 = sum(n_i^2 / (N * p_i)) - N
        private static double ChiSquared(int[] counts, int n, double[] p)
        {
            double chi2 = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                chi2 += (double)counts[i] * counts[i] / (n * p[i]);
            }
            return chi2 - n;
        }

        // Формирование текста результатов для одного эксперимента
        private string FormatResults(double[] p, int n, int[] counts)
        {
            var sb = new StringBuilder();

            // Эмпирические вероятности: p^_i = n_i / N
            double[] pHat = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
                pHat[i] = (double)counts[i] / n;

            // Таблица распределения
            sb.AppendLine($"N = {n}");
            sb.AppendLine(new string('-', 36));
            sb.AppendLine($"{"x_i",-6} {"p_i",-8} {"p^_i",-8} {"n_i",-8}");
            sb.AppendLine(new string('-', 36));
            for (int i = 0; i < p.Length; i++)
                sb.AppendLine($"{X[i],-6} {p[i],-8:0.0000} {pHat[i],-8:0.0000} {counts[i],-8}");
            sb.AppendLine(new string('-', 36));

            // Теоретические E и D
            double eTheor = TheoreticalE(p);
            double dTheor = TheoreticalD(p);

            // Эмпирические E и D: E^ = sum(p^_i * x_i), D^ = sum(p^_i * x_i^2) - E^2
            double eHat = 0;
            for (int i = 0; i < p.Length; i++)
                eHat += pHat[i] * X[i];
            double eHatX2 = 0;
            for (int i = 0; i < p.Length; i++)
                eHatX2 += pHat[i] * X[i] * X[i];
            double dHat = eHatX2 - eHat * eHat;

            // Относительные погрешности
            double deltaE = Math.Abs(eTheor) > 1e-12 ? Math.Abs(eHat - eTheor) / Math.Abs(eTheor) : 0;
            double deltaD = Math.Abs(dTheor) > 1e-12 ? Math.Abs(dHat - dTheor) / Math.Abs(dTheor) : 0;

            sb.AppendLine($"E (теор.) = {eTheor:0.4f}");
            sb.AppendLine($"E (эмп.)  = {eHat:0.4f}");
            sb.AppendLine($"delta_E   = {deltaE:0.4f} ({deltaE * 100:0.2f}%)");
            sb.AppendLine();
            sb.AppendLine($"D (теор.) = {dTheor:0.4f}");
            sb.AppendLine($"D (эмп.)  = {dHat:0.4f}");
            sb.AppendLine($"delta_D   = {deltaD:0.4f} ({deltaD * 100:0.2f}%)");
            sb.AppendLine();

            // Критерий хи-квадрат
            double chi2 = ChiSquared(counts, n, p);
            int df = p.Length - 1;
            string verdict = chi2 <= ChiSquaredCritical ? "H0 ПРИНЯТА" : "H0 ОТВЕРГНУТА";

            sb.AppendLine($"Хи-квадрат = {chi2:0.4f}");
            sb.AppendLine($"Критич.    = {ChiSquaredCritical:0.4f} (a=0.05, df={df})");
            sb.AppendLine($"Результат  : {verdict}");
            sb.AppendLine();

            return sb.ToString();
        }

        // Обновление гистограммы эмпирических частот
        private void UpdateChart(double[] p, int[] counts, int n)
        {
            var series = chart1.Series["Frequency"];
            series.Points.Clear();
            for (int i = 0; i < p.Length; i++)
            {
                double pHat = (double)counts[i] / n;
                var pt = series.Points.Add(pHat);
                pt.AxisLabel = $"x={X[i]}";
                pt.Label = pHat.ToString("0.####");
            }
            chart1.ChartAreas[0].AxisY.Title = "Эмпирическая вероятность";
            chart1.ChartAreas[0].AxisX.Title = "Значение";
        }

        private void BtnRun_Click(object? sender, EventArgs e)
        {
            double[] p = GetProbabilities();
            if (p[4] < -0.001)
            {
                MessageBox.Show("Сумма p1..p4 превышает 1. Скорректируйте вероятности.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (p[4] < 0) p[4] = 0;

            int n = (int)nudN.Value;
            int[] counts = RunExperiment(p, n);

            UpdateChart(p, counts, n);
            txtResults.Text = FormatResults(p, n, counts);
        }

        private void BtnRunAll_Click(object? sender, EventArgs e)
        {
            double[] p = GetProbabilities();
            if (p[4] < -0.001)
            {
                MessageBox.Show("Сумма p1..p4 превышает 1. Скорректируйте вероятности.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (p[4] < 0) p[4] = 0;

            int[] sizes = { 10, 100, 1000, 10000 };
            var sb = new StringBuilder();
            sb.AppendLine("=== Серия экспериментов (N=10, 100, 1000, 10000) ===");
            sb.AppendLine();

            int[]? lastCounts = null;
            int lastN = 0;

            foreach (int n in sizes)
            {
                int[] counts = RunExperiment(p, n);
                sb.Append(FormatResults(p, n, counts));
                sb.AppendLine("====================================");
                sb.AppendLine();
                lastCounts = counts;
                lastN = n;
            }

            // Гистограмма для последнего (наибольшего) N
            if (lastCounts != null)
                UpdateChart(p, lastCounts, lastN);

            txtResults.Text = sb.ToString();
        }
    }
}
