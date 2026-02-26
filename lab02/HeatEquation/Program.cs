// Свойства меди
const double Rho = 8900;    // плотность [кг/м^3]
const double Cp = 385;      // теплоёмкость [Дж/(кг * градусов Цельсия)]
const double Lambda = 401;  // теплопроводность [Вт/(м* градусов Цельсия)]

// Параметры задачи
const double L = 0.1;       // толщина пластины [м]
const double T0 = 0;        // начальная температура всей пластины [градусов Цельсия]
const double Tleft = -200;  // температура на левом краю [градусов Цельсия]
const double Tright = 20;   // температура на правом краю [градусов Цельсия]
const double TotalTime = 15; // общее время моделирования [секунд]

// Таблица, смотрим температуры после 2 секунд 

double[] steps = { 0.1, 0.01, 0.001, 0.0001, 0.00005};

Console.WriteLine($"{"dt \\ dx",-12}{steps[0],16}{steps[1],16}{steps[2],16}{steps[3],16}{steps[4],16}");
Console.WriteLine(new string('-', 92));

foreach (double dt in steps)
{
    Console.Write($"{dt,-12}");
    foreach (double dx in steps)
    {
        double[] t = Solve(dx, dt, TotalTime: 2);
        Console.Write($"{t[t.Length / 2],16:F4}");
    }
    Console.WriteLine();
}

// СОХРАНЕНИЕ СНИМКОВ В CSV для визуализации в Python
// Берём сетку (dx=0.001) и записываем 200 кадров
const int frameCount = 200;
var snapshots = new List<double[]>();
Solve(0.001, 0.01, snapshots, frameCount, TotalTime);

string csvPath = Path.Combine(AppContext.BaseDirectory, "snapshots.csv");
using (var w = new StreamWriter(csvPath))
{
    // Первая строка — метаданные для Python: Tmin, Tmax, длина, время, кол-во кадров
    w.WriteLine($"{Math.Min(Tleft, Tright)},{Math.Max(Tleft, Tright)},{L},{TotalTime},{snapshots.Count}");
    foreach (var s in snapshots)
        // Преобразуем числа в текст в General формат (выбирает наиболее компактное представление) 
        w.WriteLine(string.Join(",", s.Select(v => v.ToString("G"))));
}
Console.WriteLine($"CSV: {csvPath}");

// Запуск Python-визуализации
string pyPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "visualize.py"));
System.Diagnostics.Process.Start("python", $"\"{pyPath}\" \"{csvPath}\"");

//   1) Прямой ход: от левого края к правому вычисляем коэффициенты alpha, beta
//   2) Обратный ход: от правого края к левому находим температуры
double[] Solve(double dx, double dt, List<double[]> snapshots = null,
               int snapshotCount = 200, double TotalTime = 15)
{
    int n = (int)(L / dx) + 1;          // количество точек сетки
    int totalSteps = (int)(TotalTime / dt); // количество шагов по времени
    var T = new double[n];
    Array.Fill(T, T0);                   // вся пластина при начальной температуре
    T[0] = Tleft;                        // левый край зафиксирован
    T[n - 1] = Tright;                   // правый край зафиксирован

    double A = Lambda / (dx * dx);       // коэфф. при соседних точках
    double C = Lambda / (dx * dx);       


    double B = 2 * A + Rho * Cp / dt;   // коэфф. при текущей точке
    double FCoeff = - Rho * Cp / dt;            // коэфф. при F (связь с текущей точкой на предыдущем шаге по времени)

    var alpha = new double[n];           // прогоночные коэффициенты
    var beta = new double[n];

    int interval = snapshots != null ? Math.Max(1, totalSteps / snapshotCount) : 0;
    if (snapshots != null) snapshots.Add((double[])T.Clone());

    for (int step = 0; step < totalSteps; step++)
    {
        // Прямой ход прогонки: вычисляем alpha и beta слева направо
        alpha[1] = 0;
        beta[1] = Tleft;                // граничное условие слева
        for (int i = 2; i < n - 1; i++)
        {
            double denominator = B - C * alpha[i - 1];
            alpha[i] = A / denominator;
            beta[i] = (C * beta[i - 1] - FCoeff * T[i]) / denominator;
        }

        // Обратный ход: находим температуры справа налево
        T[n - 1] = Tright;              // граничное условие справа
        for (int i = n - 2; i >= 1; i--)
            T[i] = alpha[i] * T[i + 1] + beta[i];
        T[0] = Tleft;

        // Сохраняем снимок для визуализации
        if (interval > 0 && step % interval == 0)
            snapshots.Add((double[])T.Clone());
    }
    return T;
}