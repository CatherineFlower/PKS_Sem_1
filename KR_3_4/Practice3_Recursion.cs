using System;

namespace Pks_Sem1_Practices
{
    internal static class Practice3_Recursion
    {
        public static void Run()
        {
            while (true)
            {
                Console.WriteLine("\n=== Практика 3 — Методы. Рекурсия ===");
                Console.WriteLine("1) Задание 1 — Разворот числа (без цифры 0)");
                Console.WriteLine("2) Задание 2 — Функция Аккермана");
                Console.WriteLine("0) Назад в главное меню");
                Console.Write("Выбор: ");

                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        Task1_ReverseNumber();
                        break;
                    case "2":
                        Task2_Ackermann();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный пункт меню.\n");
                        break;
                }
            }
        }

        // ---------- Задание 1: разворот числа (нельзя вводить числа с цифрой 0) ----------

        private static void Task1_ReverseNumber()
        {
            long n = ReadPositiveWithoutZeros();
            long rev = ReverseNumber(n);
            Console.WriteLine($"Развёрнутое число: {rev}\n");
        }

        // Разворот числа с помощью рекурсии и аккумулятора
        private static long ReverseNumber(long n)
        {
            return ReverseInner(n, 0);
        }

        private static long ReverseInner(long remaining, long acc)
        {
            if (remaining == 0)
                return acc;

            long lastDigit = remaining % 10;
            long rest = remaining / 10;

            return ReverseInner(rest, acc * 10 + lastDigit);
        }

        // Проверка: содержит ли число хотя бы одну цифру 0
        // Только рекурсия и целочисленная арифметика (без строк и циклов)
        private static bool HasZeroDigit(long n)
        {
            long lastDigit = n % 10;
            if (lastDigit == 0)
                return true;

            long rest = n / 10;
            if (rest == 0)
                return false;

            return HasZeroDigit(rest);
        }

        // --- ВВОД С ПРОВЕРКОЙ ВСЕХ НУЛЕЙ (включая ведущие) ---
        static int ReadPositiveWithoutZeros()
        {
            while (true)
            {
                Console.Write("Введите целое положительное число БЕЗ цифры 0: ");
                string? s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Пустой ввод. Повторите.\n");
                    continue;
                }

                s = s.Trim();

                // 1) знак минус запрещён
                if (s[0] == '-')
                {
                    Console.WriteLine("Число должно быть положительным.\n");
                    continue;
                }

                // 2) все символы должны быть цифрами 1..9 (ни одной '0')
                bool ok = true;
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    if (ch < '1' || ch > '9')   // именно 1–9, цифра 0 и любые другие символы запрещены
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok)
                {
                    Console.WriteLine("Число должно состоять только из цифр 1–9, без нулей и посторонних символов.\n");
                    continue;
                }

                // дошли сюда — строка вида "25", "123", "9999" и т.п.
                if (!int.TryParse(s, out int value))
                {
                    Console.WriteLine("Число слишком большое или некорректное.\n");
                    continue;
                }

                return value;
            }
        }


        // ---------------------- Задание 2: функция Аккермана ----------------------

       public static void Task2_Ackermann()
        {
            Console.WriteLine("Функция Аккермана A(m, n). m, n ≥ 0.");

            // Чтобы не улететь в бесконечную рекурсию, сразу ограничим входные данные.
            int m = ReadIntInRange("Введите m (0..3): ", 0, 3);
            int n = ReadIntInRange("Введите n (0..10): ", 0, 10);
            
            const int recursionLimit = 10000; // Лимит глубины рекурсии
            int depth = 0;

            try
            {
                int result = AckermannSafe(m, n, ref depth, recursionLimit);
                Console.WriteLine($"A({m}, {n}) = {result}");
            }
            catch (RecursionDepthExceededException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("Слишком большие аргументы для безопасного рекурсивного вычисления.\n");
            }
        }

        // Безопасная версия функции Аккермана
        private static int AckermannSafe(int m, int n, ref int depth, int limit)
        {
            depth++;
            if (depth > limit)
                throw new RecursionDepthExceededException($"превышен лимит глубины рекурсии ({limit})");

            int result;

            if (m == 0)
            {
                result = n + 1;
            }
            else if (n == 0)
            {
                result = AckermannSafe(m - 1, 1, ref depth, limit);
            }
            else
            {
                int inner = AckermannSafe(m, n - 1, ref depth, limit);
                result = AckermannSafe(m - 1, inner, ref depth, limit);
            }

            depth--; // выходим из текущего кадра
            return result;
        }

        // Утилита ввода целого числа из диапазона
        private static int ReadIntInRange(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max)
                    return v;

                Console.WriteLine($"Введите целое число от {min} до {max}.\n");
            }
        }
        
        // Кастомное исключение для переполнения "логической" глубины
        public class RecursionDepthExceededException : Exception
        {
            public RecursionDepthExceededException(string message) : base(message) { }
        }
    }
}
