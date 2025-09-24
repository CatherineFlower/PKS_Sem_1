using System;
using System.Globalization;

namespace IfElseTasks
{
    internal class Program
    {
        // Календарные системы
        private static readonly GregorianCalendar Gcal = new GregorianCalendar();
        private static readonly JulianCalendar Jcal = new JulianCalendar();

        private enum RusStyle { Old, New, Skipped } // 1–13.02.1918 — «пропущенные» гражданские даты

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1) Ввод даты
            Console.WriteLine("Введите дату (форматы: dd MM yyyy | dd.MM.yyyy | yyyy-MM-dd):");
            string? inputDate = Console.ReadLine();

            if (!TryParseDate(inputDate, out int d, out int m, out int y))
            {
                Console.WriteLine("Ошибка: не удалось распознать дату. Используйте dd MM yyyy, dd.MM.yyyy или yyyy-MM-dd.");
                return;
            }

            RusStyle style = DetectRussianStyle(y, m, d);

            if (style == RusStyle.Skipped)
            {
                Console.WriteLine("Внимание: в российском гражданском календаре дат 01–13.02.1918 не существовало.");
                Console.WriteLine("Эта дата будет интерпретирована как ГРИГОРИАНСКАЯ (новый стиль).\n");
                style = RusStyle.New;
            }

            DateTime currentDate;
            try
            {
                currentDate = style == RusStyle.Old
                    ? Jcal.ToDateTime(y, m, d, 0, 0, 0, 0) // интерпретация
                                                           // компонентов в юлианском календаре
                    : new DateTime(y, m, d); // григорианский
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Ошибка: такая дата невозможна в выбранной системе календаря.");
                return;
            }

            DateTime tomorrow = currentDate.AddDays(1);

            if (style == RusStyle.Old)
            {
                Console.WriteLine($"Ввод (старый стиль): {FormatAs(d, m, y)}");
                Console.WriteLine($"По новому стилю (эквивалент): {currentDate:dd.MM.yyyy}");
                Console.WriteLine($"Завтра (по новому стилю): {tomorrow:dd.MM.yyyy}\n");
            }
            else
            {
                Console.WriteLine($"Сегодня: {currentDate:dd.MM.yyyy}");
                Console.WriteLine($"Завтра:  {tomorrow:dd.MM.yyyy}\n");
            }

            // 2) Високосность 
            bool leap;
            try
            {
                leap = (style == RusStyle.Old) ? Jcal.IsLeapYear(y) : Gcal.IsLeapYear(y);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Невозможно проверить високосность для указанного года в этом календаре.");
                leap = false;
            }

            string styleLabel = (style == RusStyle.Old) ? " (старый стиль)" : "";
            Console.WriteLine($"{y} год{styleLabel} {(leap ? "" : "НЕ ")}високосный.\n");

            // 3) Рабочий / выходной
            int dayOfWeekNum = ReadIntInRange(
                prompt: "Введите номер дня недели (1 - понедельник, ..., 7 - воскресенье): ",
                min: 1, max: 7);

            if (dayOfWeekNum <= 5)
                Console.WriteLine("Это рабочий день.\n");
            else
                Console.WriteLine("Это выходной день.\n");

            // 4) Оптимальная масса тела с валидацией и «без минусов»
            int height = ReadIntInRange("Введите ваш рост (см): ", 50, 300); // разумные границы
            int weight = ReadIntInRange("Введите ваш вес (кг): ", 10, 500); // разумные границы

            int optimal = height - 100;
            int delta = Math.Abs(weight - optimal); // никогда не уйдем в минус при выводе

            if (weight > optimal)
                Console.WriteLine($"Вам необходимо похудеть на {delta} кг.");
            else if (weight < optimal)
                Console.WriteLine($"Вам необходимо поправиться на {delta} кг.");
            else
                Console.WriteLine("Ваш вес оптимален!");
        }

        // Вспомогательные функции
        // Автоопределение гражданского стиля для России (реформа 1918 г.)
        private static RusStyle DetectRussianStyle(int y, int m, int d)
        {
            if (y < 1918) return RusStyle.Old;
            if (y > 1918) return RusStyle.New;

            // y == 1918
            if (m < 2) return RusStyle.Old; // январь — старый стиль
            if (m > 2) return RusStyle.New; // март и далее — новый

            // февраль 1918
            if (d <= 13) return RusStyle.Skipped; // 01–13.02.1918 не существовали
            return RusStyle.New; // 14.02.1918 и далее — новый стиль
        }

        // Универсальный парсинг строки в компоненты даты 
        private static bool TryParseDate(string? s, out int day, out int month, out int year)
        {
            day = month = year = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();
            string[] formats = { "dd MM yyyy", "dd.MM.yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd"};

            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            {
                day = dt.Day;
                month = dt.Month;
                year = dt.Year;
                return true;
            }

            // fallback — ручной разбор
            char[] seps = { ' ', '.', '-', '/'};
            var parts = s.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return false;

            if (parts[0].Length == 4)
            {
                if (!int.TryParse(parts[0], out year)) return false;
                if (!int.TryParse(parts[1], out month)) return false;
                if (!int.TryParse(parts[2], out day)) return false;
            }
            else
            {
                if (!int.TryParse(parts[0], out day)) return false;
                if (!int.TryParse(parts[1], out month)) return false;
                if (!int.TryParse(parts[2], out year)) return false;
            }

            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;
            return true;
        }

        // Безопасный ввод целого в диапазоне [min; max]
        private static int ReadIntInRange(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();

                if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    Console.WriteLine("Неверный ввод. Введите целое число.");
                    continue;
                }

                if (v < min || v > max)
                {
                    Console.WriteLine($"Число вне допустимого диапазона [{min}; {max}]. Повторите.");
                    continue;
                }

                return v;
            }
        }

        private static string FormatAs(int d, int m, int y) => $"{d:00}.{m:00}.{y:0000}";
    }
}
