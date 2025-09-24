using System;
using System.Collections.Generic;

namespace Practical1_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("Выберите задание для запуска:");
                Console.WriteLine("1 - Календарь мая");
                Console.WriteLine("2 - Банкомат");
                Console.WriteLine("0 - Выход");
                Console.Write("Ваш выбор: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        TaskCalendar();
                        break;
                    case "2":
                        TaskATM();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Ошибка: введите 1, 2 или 0.\n");
                        break;
                }
            }
        }

        // ===== Задание 1: Календарь мая =====
        static void TaskCalendar()
        {
            const int daysInMay = 31;

            int startDay;
            while (true)
            {
                Console.Write("Введите номер дня недели, с которого начинается май (1-пн,...,7-вс): ");
                if (int.TryParse(Console.ReadLine(), out startDay) && startDay >= 1 && startDay <= 7)
                    break;
                Console.WriteLine("Ошибка: введите число от 1 до 7.");
            }

            int day;
            while (true)
            {
                Console.Write("Введите день месяца (1-31): ");
                if (int.TryParse(Console.ReadLine(), out day) && day >= 1 && day <= daysInMay)
                    break;
                Console.WriteLine("Ошибка: введите число от 1 до 31.");
            }

            // Определяем день недели для введённого числа
            int dayOfWeek = (startDay + day - 1) % 7;
            if (dayOfWeek == 0) dayOfWeek = 7;

            bool isWeekend = (dayOfWeek == 6 || dayOfWeek == 7);
            bool isHoliday = (day >= 1 && day <= 5) || (day >= 8 && day <= 10);

            if (isWeekend || isHoliday)
                Console.WriteLine("Выходной день.\n");
            else
                Console.WriteLine("Рабочий день.\n");
        }

        // ===== Задание 2: Банкомат =====
        static void TaskATM()
        {
            int amount;
            while (true)
            {
                Console.Write("Введите сумму для снятия (до 150000, кратно 100): ");
                if (int.TryParse(Console.ReadLine(), out amount))
                {
                    if (amount % 100 != 0)
                    {
                        Console.WriteLine("Ошибка: сумма должна быть кратна 100.\n");
                        continue;
                    }
                    if (amount <= 0 || amount > 150000)
                    {
                        Console.WriteLine("Ошибка: сумма должна быть >0 и ≤150000.\n");
                        continue;
                    }
                    break;
                }
                Console.WriteLine("Ошибка: введите целое число.\n");
            }

            int[] bills = { 5000, 2000, 1000, 500, 200, 100 };
            Dictionary<int, int> result = new Dictionary<int, int>();

            int remaining = amount;
            foreach (int bill in bills)
            {
                int count = remaining / bill;
                if (count > 0)
                {
                    result[bill] = count;
                    remaining -= count * bill;
                }
            }

            if (remaining == 0)
            {
                Console.WriteLine($"Размена суммы {amount} руб.:");
                foreach (var kvp in result)
                    Console.WriteLine($"{kvp.Value} x {kvp.Key} руб.");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Выдать указанную сумму невозможно.\n");
            }
        }
    }
}
