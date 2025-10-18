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
        
        public enum SolutionType { Unique, None, Infinite }

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
            
            public int Rank(double eps = 1e-10) => RankOf((double[,])A.Clone(), eps);

            public static int RankOf(double[,] m, double eps = 1e-10)
            {
                int rows = m.GetLength(0), cols = m.GetLength(1);
                int r = 0;

                for (int c = 0; c < cols && r < rows; c++)
                {
                    // поиск опорной строки
                    int piv = r; double best = Math.Abs(m[r, c]);
                    for (int i = r + 1; i < rows; i++)
                    {
                        double v = Math.Abs(m[i, c]);
                        if (v > best) { best = v; piv = i; }
                    }
                    if (best < eps) continue;

                    // перестановка строк
                    if (piv != r)
                        for (int j = c; j < cols; j++)
                            (m[r, j], m[piv, j]) = (m[piv, j], m[r, j]);

                    // приведение к ступенчатому виду
                    double div = m[r, c];
                    for (int j = c; j < cols; j++) m[r, j] /= div;

                    for (int i = 0; i < rows; i++)
                    {
                        if (i == r) continue;
                        double f = m[i, c];
                        if (Math.Abs(f) < eps) continue;
                        for (int j = c; j < cols; j++) m[i, j] -= f * m[r, j];
                    }
                    r++;
                }
                return r;
            }
            
            public (SolutionType kind, double[]? x) SolveClassify(double[] b, double eps = 1e-10)
            {
                if (R != C) return (SolutionType.Infinite, null); // неквадратная — точно не единственное

                int n = R;

                // rank(A)
                int rankA = this.Rank(eps);

                // rank([A|b])
                var aug = new double[n, n + 1];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++) aug[i, j] = A[i, j];
                    aug[i, n] = b[i];
                }
                int rankAug = RankOf(aug, eps);

                if (rankA < rankAug) return (SolutionType.None, null);
                if (rankA == n)      return (SolutionType.Unique, Matrix.Solve(this, b));
                return (SolutionType.Infinite, null);
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
                        {
                            if (A.R != A.C) { Console.WriteLine("A не квадратная — у системы нет единственного решения."); break; }

                            var b = new double[A.R];
                            for (int i = 0; i < A.R; i++) b[i] = ReadDouble($"b[{i + 1}]=");

                            try
                            {
                                var (kind, x) = A.SolveClassify(b);
                                switch (kind)
                                {
                                    case SolutionType.Unique:
                                        Console.WriteLine("Единственное решение:");
                                        for (int i = 0; i < x!.Length; i++)
                                            Console.WriteLine($"x{i + 1} = {x[i]:F6}");
                                        break;
                                    case SolutionType.None:
                                        Console.WriteLine("Система несовместна: решений нет.");
                                        break;
                                    case SolutionType.Infinite:
                                        Console.WriteLine("Система совместна, но имеет бесконечно много решений.");
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка при решении: {ex.Message}");
                            }
                            break;
                        }
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

        // ===== 3) Счастливый билет (не обрезать ведущие нули, без int.Parse) =====
        static void LuckyTicket()
        {
            while (true)
            {
                Console.Write("Введите шестизначный номер билета: ");
                string? s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Ошибка: пустой ввод.\n");
                    continue;
                }

                s = s.Trim();

                // Ровно 6 символов, только цифры 0..9 — никаких пробелов/знаков.
                if (s.Length != 6)
                {
                    Console.WriteLine("Ошибка: номер билета должен содержать ровно 6 цифр.\n");
                    continue;
                }
                bool digitsOnly = true;
                foreach (char ch in s)
                {
                    if (ch < '0' || ch > '9') { digitsOnly = false; break; }
                }
                if (!digitsOnly)
                {
                    Console.WriteLine("Ошибка: номер билета должен состоять только из цифр 0–9.\n");
                    continue;
                }

                // Суммы считаем по символам, без преобразования строки в число.
                int sum1 = (s[0] - '0') + (s[1] - '0') + (s[2] - '0');
                int sum2 = (s[3] - '0') + (s[4] - '0') + (s[5] - '0');

                Console.WriteLine(sum1 == sum2 ? "Счастливый (True)" : "Нет (False)");
                break;
            }
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
            int milk  = ReadInt("Введите количество молока в мл: ", 0, int.MaxValue);
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

                // Печатаем только доступные варианты
                Console.Write("Выберите напиток: ");
                if (canA) Console.Write("[1 — американо] ");
                if (canL) Console.Write("[2 — латте] ");
                Console.Write("[q — завершить]: ");

                string? s = Console.ReadLine();
                if (s == "q")
                {
                    Console.WriteLine("Смена завершена.");
                    PrintCoffeeReport(water, milk, cupsA, cupsL, revenue);
                    return;
                }

                if (s == "1")
                {
                    if (!canA) { Console.WriteLine("Американо сейчас недоступен (не хватает воды)."); continue; }
                    water -= 300; cupsA++; revenue += 150; Console.WriteLine("Ваш напиток готов.");
                }
                else if (s == "2")
                {
                    if (!canL)
                    {
                        Console.WriteLine(water < 30 ? "Латте недоступен (не хватает воды)." : "Латте недоступен (не хватает молока).");
                        continue;
                    }
                    water -= 30; milk -= 270; cupsL++; revenue += 170; Console.WriteLine("Ваш напиток готов.");
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Выберите доступный вариант.");
                }
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

        // ===== 8) Колонизация Марса (смешанные ориентации, горизонт/вертик разрез) =====
        static void MarsColonization()
        {
            long n = ReadLong("Введите n (кол-во модулей): ");
            long a = ReadLong("Введите a: ");
            long b = ReadLong("Введите b: ");
            long w = ReadLong("Введите w (ширина поля): ");
            long h = ReadLong("Введите h (высота поля): ");

            // Предупреждение: что максимум возможно при d = 0 (самые маленькие модули)
            long maxAtZero = MaxPlaceMixed(n, a, b, w, h, 0);
            if (maxAtZero < n)
            {
                Console.WriteLine($"Предупреждение: даже без защиты (d = 0) можно разместить максимум {maxAtZero} модулей. Требуется {n}.");
                Console.WriteLine("Ответ d = 0");
                return;
            }

            long lo = 0, hi = (long)1e9;
            while (lo < hi)
            {
                long mid = (lo + hi + 1) / 2;
                if (CanPlaceMixed(n, a, b, w, h, mid)) lo = mid; else hi = mid - 1;
            }
            Console.WriteLine($"Ответ d = {lo}");
        }

        // Проверяем, можно ли разместить >= n модулей при данном d,
        // разрешая СМЕШАННЫЕ ориентации и горизонтальный/вертикальный «разрез» поля.
        static bool CanPlaceMixed(long n, long a, long b, long W, long H, long d)
        {
            long wA = a + 2 * d, hA = b + 2 * d; // ориентация А: a×b
            long wB = b + 2 * d, hB = a + 2 * d; // ориентация B: b×a (поворот)

            if (wA <= 0 || hA <= 0 || wB <= 0 || hB <= 0) return false;
            // если модуль больше поля по обеим ориентациям — сразу нет
            if ((wA > W || hA > H) && (wB > W || hB > H)) return false;

            // 1) Горизонтальный разрез: сверху rA рядов ориентации A, снизу — ориентация B
            long rowsAmax = H / hA;
            for (long rA = 0; rA <= rowsAmax; rA++)
            {
                long cntA = rA * (W / wA);
                long remH = H - rA * hA;
                long rowsB = remH / hB;
                long cntB = rowsB * (W / wB);
                if (cntA + cntB >= n) return true;
            }

            // 2) Горизонтальный разрез, но верх/низ меняем местами (сначала B, потом A)
            long rowsBmax = H / hB;
            for (long rB = 0; rB <= rowsBmax; rB++)
            {
                long cntB = rB * (W / wB);
                long remH = H - rB * hB;
                long rowsA = remH / hA;
                long cntA = rowsA * (W / wA);
                if (cntA + cntB >= n) return true;
            }

            // 3) Вертикальный разрез: слева cA столбцов ориентации A, справа — B
            long colsAmax = W / wA;
            for (long cA = 0; cA <= colsAmax; cA++)
            {
                long cntA = cA * (H / hA);
                long remW = W - cA * wA;
                long colsB = remW / wB;
                long cntB = colsB * (H / hB);
                if (cntA + cntB >= n) return true;
            }

            // 4) Вертикальный разрез, но меняем местами (сначала B, потом A)
            long colsBmax = W / wB;
            for (long cB = 0; cB <= colsBmax; cB++)
            {
                long cntB = cB * (H / hB);
                long remW = W - cB * wB;
                long colsA = remW / wA;
                long cntA = colsA * (H / hA);
                if (cntA + cntB >= n) return true;
            }

            // 5) Быстрая проверка «чистой» ориентации (на всякий случай)
            long pureA = (W / wA) * (H / hA);
            long pureB = (W / wB) * (H / hB);
            return Math.Max(pureA, pureB) >= n;
        }

        // Сколько максимум модулей можно уместить при данном d,
        // учитывая смешанные раскладки (для предупреждения при d=0).
        static long MaxPlaceMixed(long n, long a, long b, long W, long H, long d)
        {
            long best = 0;
            long wA = a + 2 * d, hA = b + 2 * d;
            long wB = b + 2 * d, hB = a + 2 * d;

            if (wA <= 0 || hA <= 0 || wB <= 0 || hB <= 0) return 0;

            // Горизонтальные разрезы
            long rowsAmax = H / hA;
            for (long rA = 0; rA <= rowsAmax; rA++)
            {
                long cntA = rA * (W / wA);
                long remH = H - rA * hA;
                long cntB = (remH / hB) * (W / wB);
                best = Math.Max(best, cntA + cntB);
            }
            long rowsBmax = H / hB;
            for (long rB = 0; rB <= rowsBmax; rB++)
            {
                long cntB = rB * (W / wB);
                long remH = H - rB * hB;
                long cntA = (remH / hA) * (W / wA);
                best = Math.Max(best, cntA + cntB);
            }

            // Вертикальные разрезы
            long colsAmax = W / wA;
            for (long cA = 0; cA <= colsAmax; cA++)
            {
                long cntA = cA * (H / hA);
                long remW = W - cA * wA;
                long cntB = (remW / wB) * (H / hB);
                best = Math.Max(best, cntA + cntB);
            }
            long colsBmax = W / wB;
            for (long cB = 0; cB <= colsBmax; cB++)
            {
                long cntB = cB * (H / hB);
                long remW = W - cB * wB;
                long cntA = (remW / wA) * (H / hA);
                best = Math.Max(best, cntA + cntB);
            }

            // Чистые ориентации
            long pureA = (W / wA) * (H / hA);
            long pureB = (W / wB) * (H / hB);
            best = Math.Max(best, Math.Max(pureA, pureB));
            return best;
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
