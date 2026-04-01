namespace RandomEvents
{
    public partial class Form1 : Form
    {
        private readonly Random rng = new(); // базовый датчик

        // Варианты ответов шара предсказаний
        private readonly string[] Answers =
        {
            "Определённо да",
            "Вероятно да",
            "Возможно",
            "Вероятно нет",
            "Определённо нет"
        };

        public Form1()
        {
            InitializeComponent();

            btnAnswer.Click += BtnAnswer_Click;
            btnRun.Click += BtnRun_Click;

            nudP1.ValueChanged += ProbabilityChanged;
            nudP2.ValueChanged += ProbabilityChanged;
            nudP3.ValueChanged += ProbabilityChanged;
            nudP4.ValueChanged += ProbabilityChanged;
        }

        // ===== Задание 5.1: Да или Нет =====

        private void BtnAnswer_Click(object? sender, EventArgs e)
        {
            double p = (double)nudProbability.Value;

            // Да/Нет — частный случай полной группы из 2 событий: {p, 1-p}
            double alpha = rng.NextDouble();
            //                           inline создание массива 
            int k = SelectEvent(alpha, new[] { p, 1.0 - p });

            if (k == 0)
            {
                lblResult.Text = "ДА!";
                lblResult.ForeColor = Color.Green;
            }
            else
            {
                lblResult.Text = "НЕТ!";
                lblResult.ForeColor = Color.Red;
            }
        }

        // ===== Задание 5.2: Шар предсказаний =====

        // Пересчитываем p5 при изменении любой вероятности
        private void ProbabilityChanged(object? sender, EventArgs e)
        {
            double sum = (double)(nudP1.Value + nudP2.Value + nudP3.Value + nudP4.Value);
            double p5 = Math.Max(0, 1.0 - sum);
            lblP5Value.Text = p5.ToString("F2");

            // Красный цвет если сумма превышает 1
            lblP5Value.ForeColor = sum > 1.0 ? Color.Red : Color.Black;
        }

        private void BtnRun_Click(object? sender, EventArgs e)
        {
            // Собираем вероятности, p5 = 1 - сумма остальных
            double[] probs = new double[5];
            probs[0] = (double)nudP1.Value;
            probs[1] = (double)nudP2.Value;
            probs[2] = (double)nudP3.Value;
            probs[3] = (double)nudP4.Value;

            // Проверка: сумма вероятностей не должна превышать 1
            double sum = probs[0] + probs[1] + probs[2] + probs[3];
            if (sum > 1.0)
            {
                txtResults.Text = "Ошибка: sum(p_i) > 1";
                return;
            }

            probs[4] = Math.Max(0, 1.0 - probs[0] - probs[1] - probs[2] - probs[3]);

            // Генерируем одно предсказание
            double alpha = rng.NextDouble();
            int k = SelectEvent(alpha, probs);

            txtResults.Text = Answers[k];
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