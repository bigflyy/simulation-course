namespace BasicRng
{
    internal static class Program
    {
        static void Main()
        {
            int sampleSize = 100000;

            // Мультипликативный конгруэнтный генератор
            // Формула: x*_i = (beta * x*_{i-1}) mod M
            //          x_i  = x*_i / M  — нормировка в [0, 1)
            ulong M = 9223372036854775808;           // модуль (2^63)
            ulong beta = 4294967299;                 // множитель (2^32 + 3)
            ulong x0 = 4294967299;               // зерно (начальное значение = beta)

            double sumCustom = 0.0;   // сумма сгенерированных значений (для среднего)
            double sumSquaredCustom = 0.0; // сумма квадратов (для дисперсии)

            for (int i = 0; i < sampleSize; i++)
            {
                // Генерируем следующее целое число по формуле
                x0 = (beta * x0) % M;

                // Нормируем в [0, 1) делением на модуль
                double xi = (double)x0 / M;

                sumCustom += xi;
                sumSquaredCustom += xi * xi;
            }

            // Выборочное среднее: x_ = (1/N) * sum(xi)
            double meanCustom = sumCustom / sampleSize;
            // Несмещённая выборочная дисперсия: S^2 = (1/(N-1)) * sum((xi - x_)^2)
            //   = (1/(N-1)) * (sum(xi^2) - N * x_^2)
            double varCustom = (sumSquaredCustom - sampleSize * meanCustom * meanCustom) / (sampleSize - 1);

            // --- Встроенный датчик System.Random ---
            var rng = new Random();
            double sumBuiltin = 0.0;
            double sumSquaredBuiltin = 0.0;

            for (int i = 0; i < sampleSize; i++)
            {
                double xi = rng.NextDouble();
                sumBuiltin += xi;
                sumSquaredBuiltin += xi * xi;
            }

            double meanBuiltin = sumBuiltin / sampleSize;
            double varBuiltin = (sumSquaredBuiltin - sampleSize * meanBuiltin * meanBuiltin) / (sampleSize - 1);

            // Теоретические значения для равномерного распределения U(0, 1)
            double theorMean = 0.5;
            double theorVar = 1.0 / 12.0;

            // --- Вывод результатов ---
            Console.WriteLine($"Размер выборки: {sampleSize}");
            Console.WriteLine();
            Console.WriteLine("=== Свой ГПСЧ (мультипликативный конгруэнтный) ===");
            Console.WriteLine($"  Среднее:    {meanCustom:F6}   (ошибка: {Math.Abs(meanCustom - theorMean):E3})");
            Console.WriteLine($"  Дисперсия:  {varCustom:F6}   (ошибка: {Math.Abs(varCustom - theorVar):E3})");
            Console.WriteLine();
            Console.WriteLine("=== Встроенный System.Random ===");
            Console.WriteLine($"  Среднее:    {meanBuiltin:F6}   (ошибка: {Math.Abs(meanBuiltin - theorMean):E3})");
            Console.WriteLine($"  Дисперсия:  {varBuiltin:F6}   (ошибка: {Math.Abs(varBuiltin - theorVar):E3})");
            Console.WriteLine();
            Console.WriteLine("=== Теоретические значения U(0, 1) ===");
            Console.WriteLine($"  Среднее:    {theorMean:F6}");
            Console.WriteLine($"  Дисперсия:  {theorVar:F6}");
        }
    }
}
