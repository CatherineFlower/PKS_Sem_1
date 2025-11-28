using System;

namespace Pks_Sem1_Practices
{
    internal static class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("\n=== ПКС — Практики 3, 3.2 и 4 ===");
                Console.WriteLine("1) Практика 3 — Методы. Рекурсия");
                Console.WriteLine("2) Практика 3.2 — Система бронирования");
                Console.WriteLine("3) Практика 4 — Система заказов");
                Console.WriteLine("0) Выход");
                Console.Write("Выбор: ");

                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        Practice3_Recursion.Run();
                        break;
                    case "2":
                        Practice3_2_Booking.Run();
                        break;
                    case "3":
                        Practice4_Orders.Run();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный пункт меню.\n");
                        break;
                }
            }
        }
    }
}