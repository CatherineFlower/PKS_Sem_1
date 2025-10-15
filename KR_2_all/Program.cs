using System;
using System.Globalization;

namespace Pks_Practice2_Var20
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            {
                Console.WriteLine("\n=== ПКС — Практика №2 — Вариант 20 ===");
                Console.WriteLine("1) Калькулятор матриц");
                Console.WriteLine("2) Ряды (arcth x), точность e < 0.01");
                Console.WriteLine("3) Счастливый билет");
                Console.WriteLine("4) Сокращение дроби");
                Console.WriteLine("5) Угадай число (0..63)");
                Console.WriteLine("6) Кофейный аппарат");
                Console.WriteLine("7) Лабораторный опыт (бактерии)");
                Console.WriteLine("8) Колонизация Марса");
                Console.WriteLine("0) Выход");
                Console.Write("Выбор: ");
                var c = Console.ReadLine();
                Console.WriteLine();
                switch (c)
                {
                    case "1": MatrixMenu(); break;
                    case "2": SeriesVar20(); break;
                    case "3": LuckyTicket(); break;
                    case "4": ReduceFraction(); break;
                    case "5": GuessNumber(); break;
                    case "6": CoffeeMachine(); break;
                    case "7": LabExperiment(); break;
                    case "8": MarsColonization(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный пункт.\n"); break;
                }
            }
        }

        // ===== 1) Матрицы =====
        class Matrix
        {
            public int R { get; }
            public int C { get; }
            public double[,] A { get; }
            public Matrix(int r, int c) { R = r; C = c; A = new double[r, c]; }

            public static Matrix CreateManual(string name)
            {
                int r = ReadInt($"Введите кол-во строк {name}: ", 1, 50);
                int c = ReadInt($"Введите кол-во столбцов {name}: ", 1, 50);
                var m = new Matrix(r, c);
                Console.WriteLine($"Введите элементы {name} построчно:");
                for (int i = 0; i < r; i++)
                    for (int j = 0; j < c; j++)
                        m.A[i, j] = ReadDouble($"[{i + 1},{j + 1}]=");
                return m;
            }

            public static Matrix CreateRandom(string name)
            {
                int r = ReadInt($"Введите кол-во строк {name}: ", 1, 50);
                int c = ReadInt($"Введите кол-во столбцов {name}: ", 1, 50);
                double a = ReadDouble("Минимум a: ");
                double b = ReadDouble("Максимум b: ");
                if (a > b) (a, b) = (b, a);
                var rnd = new Random();
                var m = new Matrix(r, c);
                for (int i = 0; i < r; i++)
                    for (int j = 0; j < c; j++)
                        m.A[i, j] = a + rnd.NextDouble() * (b - a);
                Console.WriteLine($"Матрица {name} заполнена случайно из [{a};{b}].");
                return m;
            }

            public void Print(string title)
            {
                Console.WriteLine(title);
                for (int i = 0; i < R; i++)
                {
                    for (int j = 0; j < C; j++) Console.Write($"{A[i, j],10:F3} ");
                    Console.WriteLine();
                }
            }

            public static Matrix Add(Matrix x, Matrix y)
            {
                if (x.R != y.R || x.C != y.C) throw new InvalidOperationException("Размерности не совпадают для сложения");
                var m = new Matrix(x.R, x.C);
                for (int i = 0; i < m.R; i++)
                    for (int j = 0; j < m.C; j++)
                        m.A[i, j] = x.A[i, j] + y.A[i, j];
                return m;
            }

            public static Matrix Mul(Matrix x, Matrix y)
            {
                if (x.C != y.R) throw new InvalidOperationException("Матрицы несовместимы для умножения");
                var m = new Matrix(x.R, y.C);
                for (int i = 0; i < x.R; i++)
                    for (int k = 0; k < x.C; k++)
                    {
                        double v = x.A[i, k];
                        for (int j = 0; j < y.C; j++)
                            m.A[i, j] += v * y.A[k, j];
                    }
                return m;
            }

            public Matrix T()
            {
                var m = new Matrix(C, R);
                for (int i = 0; i < R; i++)
                    for (int j = 0; j < C; j++)
                        m.A[j, i] = A[i, j];
                return m;
            }

            public double Det()
            {
                if (R != C) throw new InvalidOperationException("Детерминант определён только для квадратных матриц");
                var a = (double[,])A.Clone();
                int n = R; double det = 1; int sign = 1;
                for (int i = 0; i < n; i++)
                {
                    int piv = i; double best = Math.Abs(a[i, i]);
                    for (int r = i + 1; r < n; r++)
                        if (Math.Abs(a[r, i]) > best) { best = Math.Abs(a[r, i]); piv = r; }
                    if (Math.Abs(best) < 1e-12) return 0;
                    if (piv != i) { SwapRows(a, i, piv); sign *= -1; }
                    double pivVal = a[i, i]; det *= pivVal;
                    for (int j = i + 1; j < n; j++)
                    {
                        double f = a[j, i] / pivVal;
                        if (Math.Abs(f) < 1e-18) continue;
                        for (int k = i; k < n; k++) a[j, k] -= f * a[i, k];
                    }
                }
                return det * sign;
            }

            public Matrix Inverse()
            {
                if (R != C) throw new InvalidOperationException("Обратимая матрица должна быть квадратной");
                int n = R; var aug = new double[n, 2 * n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++) aug[i, j] = A[i, j];
                    aug[i, n + i] = 1;
                }
                for (int i = 0; i < n; i++)
                {
                    int piv = i; double best = Math.Abs(aug[i, i]);
                    for (int r = i + 1; r < n; r++)
                        if (Math.Abs(aug[r, i]) > best) { best = Math.Abs(aug[r, i]); piv = r; }
                    if (Math.Abs(best) < 1e-12) throw new InvalidOperationException("Детерминант равен 0 — обратной нет");
                    if (piv != i) SwapRows(aug, i, piv);

                    double div = aug[i, i];
                    for (int j = 0; j < 2 * n; j++) aug[i, j] /= div;
                    for (int r = 0; r < n; r++) if (r != i)
                    {
                        double f = aug[r, i]; if (Math.Abs(f) < 1e-18) continue;
                        for (int j = 0; j < 2 * n; j++) aug[r, j] -= f * aug[i, j];
                    }
                }
                var inv = new Matrix(n, n);
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        inv.A[i, j] = aug[i, n + j];
                return inv;
            }

            public static double[] Solve(Matrix a, double[] b)
            {
                if (a.R != a.C) throw new InvalidOperationException("Система должна быть квадратной");
                int n = a.R; var aug = new double[n, n + 1];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++) aug[i, j] = a.A[i, j];
                    aug[i, n] = b[i];
                }
                // прямой ход
                for (int i = 0; i < n; i++)
                {
                    int piv = i; double best = Math.Abs(aug[i, i]);
                    for (int r = i + 1; r < n; r++)
                        if (Math.Abs(aug[r, i]) > best) { best = Math.Abs(aug[r, i]); piv = r; }
                    if (Math.Abs(best) < 1e-12) throw new InvalidOperationException("Система не имеет единственного решения");
                    if (piv != i) SwapRows(aug, i, piv);
                    double div = aug[i, i];
                    for (int j = i; j <= n; j++) aug[i, j] /= div;
                    for (int r = i + 1; r < n; r++)
                    {
                        double f = aug[r, i];
                        for (int j = i; j <= n; j++) aug[r, j] -= f * aug[i, j];
                    }
                }
                // обратный ход
                var x = new double[n];
                for (int i = n - 1; i >= 0; i--)
                {
                    double sum = aug[i, n];
                    for (int j = i + 1; j < n; j++) sum -= aug[i, j] * x[j];
                    x[i] = sum;
                }
                return x;
            }

            private static void SwapRows(double[,] m, int r1, int r2)
            {
                int cols = m.GetLength(1);
                for (int j = 0; j < cols; j++)
                {
                    double t = m[r1, j]; m[r1, j] = m[r2, j]; m[r2, j] = t;
                }
            }
        }

        static void MatrixMenu()
        {
            Console.WriteLine("— Калькулятор матриц —\n");
            Matrix A = CreateMatrix("A");
            A.Print("A:");
            Matrix B = CreateMatrix("B");
            B.Print("B:");
            while (true)
            {
                Console.WriteLine("\nОперации: 1) A+B  2) A*B  3) det(A)  4) inv(A)  5) A^T  6) Решить Ax=b  0) Назад");
                Console.Write("Выбор: ");
                var c = Console.ReadLine();
                try
                {
                    switch (c)
                    {
                        case "1": Matrix.Add(A, B).Print("A+B:"); break;
                        case "2": Matrix.Mul(A, B).Print("A*B:"); break;
                        case "3": Console.WriteLine($"det(A) = {A.Det():F6}"); break;
                        case "4": A.Inverse().Print("A^{-1}:"); break;
                        case "5": A.T().Print("A^T:"); break;
                        case "6":
                            if (A.R != A.C) { Console.WriteLine("A не квадратная — систему решить нельзя."); break; }
                            var b = new double[A.R];
                            for (int i = 0; i < A.R; i++) b[i] = ReadDouble($"b[{i + 1}]=");
                            var x = Matrix.Solve(A, b);
                            Console.WriteLine("x (решение Ax=b):");
                            for (int i = 0; i < x.Length; i++) Console.WriteLine($"x{i + 1} = {x[i]:F6}");
                            break;
                        case "0": return;
                        default: Console.WriteLine("Неверно."); break;
                    }
                }
                catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
            }
        }

        static Matrix CreateMatrix(string name)
        {
            Console.Write($"Создать матрицу {name}: 1-ввод вручную, 2-случайно? ");
            var m = Console.ReadLine();
            return (m == "2") ? Matrix.CreateRandom(name) : Matrix.CreateManual(name);
        }

        // ===== 2) Ряды — вариант 20: arcth(x) =====
        static void SeriesVar20()
        {
            Console.WriteLine("Ряд Маклорена для arcth(x) = x + x^3/3 + x^5/5 + ... (|x|<1)");
            double e = ReadDouble("Введите точность e (<0.01): ");
            if (e <= 0 || e >= 0.01) { Console.WriteLine("e должно быть в (0; 0.01)"); return; }
            double x = ReadDouble("Введите x (|x|<1): ");
            if (Math.Abs(x) >= 1) { Console.WriteLine("Ряд расходится при |x|>=1"); return; }

            // term_k = x^{2k+1}/(2k+1)
            double sum = 0;
            int k = 0;
            while (true)
            {
                double term = Math.Pow(x, 2 * k + 1) / (2 * k + 1);
                if (Math.Abs(term) < e) break;
                sum += term;
                k++;
            }
            Console.WriteLine($"f(x) ≈ {sum:F6} (суммировано {k} членов)");

            int n = ReadInt("Введите n для n-го члена ряда (n>=0): ", 0, int.MaxValue);
            double nth = Math.Pow(x, 2 * n + 1) / (2 * n + 1);
            Console.WriteLine($"n-й член: x^(2n+1)/(2n+1) = {nth:F6}");
        }

        // ===== 3) Счастливый билет =====
        static void LuckyTicket()
        {
            int t = ReadInt("Введите шестизначный номер билета: ", 0, 999999);
            int d1 = (t / 100000) % 10, d2 = (t / 10000) % 10, d3 = (t / 1000) % 10;
            int d4 = (t / 100) % 10, d5 = (t / 10) % 10, d6 = t % 10;
            Console.WriteLine((d1 + d2 + d3) == (d4 + d5 + d6) ? "Счастливый (True)" : "Нет (False)");
        }

        // ===== 4) Сокращение дроби =====
        static void ReduceFraction()
        {
            long m = ReadLong("Введите числитель M: ");
            long n = ReadLongNotZero("Введите знаменатель N (≠0): ");
            long g = Gcd(Math.Abs(m), Math.Abs(n));
            m /= g; n /= g;
            if (n < 0) { n = -n; m = -m; } // минус только у числителя
            Console.WriteLine($"Результат: {m} / {n}");
        }

        // ===== 5) Угадай число (0..63) =====
        static void GuessNumber()
        {
            Console.WriteLine("Загадайте число от 0 до 63 (включительно). Отвечайте 1=да, 0=нет.");
            int lo = 0, hi = 63;
            for (int q = 1; q <= 6; q++) // 2^6 = 64
            {
                int mid = (lo + hi) / 2;
                int ans = ReadInt($"Ваше число > {mid}? (1/0): ", 0, 1);
                if (ans == 1) lo = mid + 1; else hi = mid;
            }
            Console.WriteLine($"Ваше число: {lo}");
        }

        // ===== 6) Кофейный аппарат =====
        static void CoffeeMachine()
        {
            int water = ReadInt("Введите количество воды в мл: ", 0, int.MaxValue);
            int milk = ReadInt("Введите количество молока в мл: ", 0, int.MaxValue);
            int cupsA = 0, cupsL = 0, revenue = 0;
            while (true)
            {
                bool canA = water >= 300;
                bool canL = water >= 30 && milk >= 270;
                if (!canA && !canL)
                {
                    PrintCoffeeReport(water, milk, cupsA, cupsL, revenue);
                    return;
                }
                Console.Write("Выберите напиток (1 — американо, 2 — латте, q — завершить): ");
                string? s = Console.ReadLine();
                if (s == "q")
                {
                    Console.WriteLine("Смена завершена.");
                    PrintCoffeeReport(water, milk, cupsA, cupsL, revenue);
                    return;
                }
                if (s == "1")
                {
                    if (!canA) { Console.WriteLine("Не хватает воды"); continue; }
                    water -= 300; cupsA++; revenue += 150; Console.WriteLine("Ваш напиток готов.");
                }
                else if (s == "2")
                {
                    if (!canL) { Console.WriteLine(water < 30 ? "Не хватает воды" : "Не хватает молока"); continue; }
                    water -= 30; milk -= 270; cupsL++; revenue += 170; Console.WriteLine("Ваш напиток готов.");
                }
                else Console.WriteLine("Некорректный выбор.");
            }
        }
        static void PrintCoffeeReport(int water, int milk, int cupsA, int cupsL, int revenue)
        {
            Console.WriteLine("*Отчёт*");
            Console.WriteLine($"Ингредиентов осталось:\n  Вода: {water} мл\n  Молоко: {milk} мл");
            Console.WriteLine($"Кружек американо приготовлено: {cupsA}");
            Console.WriteLine($"Кружек латте приготовлено: {cupsL}");
            Console.WriteLine($"Итого: {revenue} руб.");
        }

        // ===== 7) Лабораторный опыт =====
        static void LabExperiment()
        {
            long N = ReadLong("Введите количество бактерий N: ");
            int X = ReadInt("Введите количество капель антибиотика X: ", 0, int.MaxValue);
            int hour = 1; int power = 10;        // каждая капля убивает power бактерий в текущий час
            while (N > 0 && X > 0 && power > 0)
            {
                N *= 2;                           // рост
                long killed = (long)X * power;    // эффект антибиотика
                N = Math.Max(0, N - killed);
                Console.WriteLine($"После {hour} часа бактерий осталось {N}");
                hour++; power--;                  // активность падает 10,9,8,...
            }
        }

        // ===== 8) Колонизация Марса =====
        static void MarsColonization()
        {
            long n = ReadLong("Введите n (кол-во модулей): ");
            long a = ReadLong("Введите a: ");
            long b = ReadLong("Введите b: ");
            long w = ReadLong("Введите w (ширина поля): ");
            long h = ReadLong("Введите h (высота поля): ");

            long lo = 0, hi = (long)1e9; // оценка сверху
            while (lo < hi)
            {
                long mid = (lo + hi + 1) / 2;
                if (CanPlace(n, a, b, w, h, mid)) lo = mid; else hi = mid - 1;
            }
            Console.WriteLine($"Ответ d = {lo}");
        }
        static bool CanPlace(long n, long a, long b, long w, long h, long d)
        {
            long W1 = (a + 2 * d) <= 0 ? 0 : w / (a + 2 * d);
            long H1 = (b + 2 * d) <= 0 ? 0 : h / (b + 2 * d);
            long W2 = (b + 2 * d) <= 0 ? 0 : w / (b + 2 * d);
            long H2 = (a + 2 * d) <= 0 ? 0 : h / (a + 2 * d);
            bool o1 = (W1 > 0 && H1 > 0 && W1 * H1 >= n);
            bool o2 = (W2 > 0 && H2 > 0 && W2 * H2 >= n);
            return o1 || o2;
        }

        // ===== ЕДИНЫЕ утилиты ввода (без дублей!) =====
        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var v) && v >= min && v <= max) return v;
                Console.WriteLine($"Введите целое в диапазоне [{min};{max}].");
            }
        }

        static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine()?.Trim().Replace(',', '.');
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return v;
                Console.WriteLine("Введите число.");
            }
        }

        static long ReadLong(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (long.TryParse(Console.ReadLine(), out var v)) return v;
                Console.WriteLine("Введите целое число.");
            }
        }

        static long ReadLongNotZero(string prompt)
        {
            while (true)
            {
                long v = ReadLong(prompt);
                if (v != 0) return v;
                Console.WriteLine("Знаменатель не может быть 0.");
            }
        }

        static long Gcd(long a, long b)
        {
            while (b != 0) { long t = a % b; a = b; b = t; }
            return a;
        }
    }
}
