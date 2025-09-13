using System;
using System.Globalization;

namespace Calculator_PR1_PKS;

internal static class Program
{
    // Память и последний результат
    private static double _memory = 0.0;
    private static double _last = 0.0;

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Классический калькулятор (консоль, C#)";

        PrintHeader();

        while (true)
        {
            PrintMenu();
            Console.Write("Выберите пункт меню: ");
            var cmd = Console.ReadLine()?.Trim().ToLowerInvariant();
            Console.WriteLine();

            switch (cmd)
            {
                case "1": DoBinary("+"); break;
                case "2": DoBinary("-"); break;
                case "3": DoBinary("*"); break;
                case "4": DoBinary("/"); break;
                case "5": DoBinary("%"); break;

                case "6": DoUnary("1/x"); break;
                case "7": DoUnary("x^2"); break;
                case "8": DoUnary("sqrt"); break;

                case "m+": MemoryPlus(); break;
                case "m-": MemoryMinus(); break;
                case "mr": MemoryRecall(); break;

                case "c": ClearLast(); break;

                case "q":
                case "exit":
                    Console.WriteLine("Выход. До свидания!");
                    return;

                default:
                    Console.WriteLine("Неизвестная команда. Повторите ввод.\n");
                    break;
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("==============================");
        Console.WriteLine("  КЛАССИЧЕСКИЙ КАЛЬКУЛЯТОР");
        Console.WriteLine("  Операции: +  -  *  /  %  1/x  x^2  √x  M+  M-  MR");
        Console.WriteLine("==============================\n");
    }

    private static void PrintMenu()
    {
        Console.WriteLine("Меню:");
        Console.WriteLine(" 1) a + b       2) a - b       3) a * b");
        Console.WriteLine(" 4) a / b       5) a % b");
        Console.WriteLine(" 6) 1/x (унарно)  7) x^2 (унарно)  8) √x (унарно)");
        Console.WriteLine(" m+, m-, mr — операции памяти");
        Console.WriteLine(" c — сбросить последний результат, q/exit — выход");
        Console.WriteLine($" Память: {_memory.ToString(CultureInfo.InvariantCulture)} | Last: {_last.ToString(CultureInfo.InvariantCulture)}\n");
    }

    // === Память ===
    private static void MemoryPlus()
    {
        _memory += _last;
        Console.WriteLine($"M+: память = {_memory.ToString(CultureInfo.InvariantCulture)}\n");
    }

    private static void MemoryMinus()
    {
        _memory -= _last;
        Console.WriteLine($"M-: память = {_memory.ToString(CultureInfo.InvariantCulture)}\n");
    }

    private static void MemoryRecall()
    {
        _last = _memory;
        Console.WriteLine($"MR: память = {_memory.ToString(CultureInfo.InvariantCulture)}, Last = {_last.ToString(CultureInfo.InvariantCulture)}\n");
    }

    private static void ClearLast()
    {
        _last = 0.0;
        Console.WriteLine("Last сброшен в 0\n");
    }

    // === Бинарные операции ===
    private static void DoBinary(string op)
    {
        var a = ReadDouble("Введите a: ");
        var b = ReadDouble("Введите b: ");

        try
        {
            double result = op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => Divide(a, b),
                "%" => Mod(a, b),
                _ => throw new InvalidOperationException("Неизвестная операция")
            };

            _last = result;
            Console.WriteLine($"Результат: {_last.ToString(CultureInfo.InvariantCulture)}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}\n");
        }
    }

    // === Унарные операции ===
    private static void DoUnary(string op)
    {
        var x = ReadDouble("Введите x: ");

        try
        {
            double result = op switch
            {
                "1/x" => Reciprocal(x),
                "x^2" => x * x,
                "sqrt" => SqrtChecked(x),
                _ => throw new InvalidOperationException("Неизвестная операция")
            };

            _last = result;
            Console.WriteLine($"Результат: {_last.ToString(CultureInfo.InvariantCulture)}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}\n");
        }
    }

    // === Низкоуровневые проверки ===
    private static double Divide(double a, double b)
    {
        if (b == 0.0)
            throw new DivideByZeroException("Деление на ноль запрещено");
        return a / b;
    }

    private static double Mod(double a, double b)
    {
        if (b == 0.0)
            throw new DivideByZeroException("Остаток по модулю на 0 не определён");
        return a % b;
    }

    private static double Reciprocal(double x)
    {
        if (x == 0.0)
            throw new DivideByZeroException("1/0 не определён");
        return 1.0 / x;
    }

    private static double SqrtChecked(double x)
    {
        if (x < 0)
            throw new ArgumentOutOfRangeException(nameof(x), "Квадратный корень от отрицательного числа не определён в R");
        return Math.Sqrt(x);
    }

    // Надёжный ввод double: поддержка точки и запятой
    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (s is null)
            {
                Console.WriteLine("Пустой ввод. Повторите.");
                continue;
            }

            s = s.Trim().Replace(',', '.');
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            Console.WriteLine("Неверный формат числа. Пример: 12.34 или 12,34");
        }
    }
}
