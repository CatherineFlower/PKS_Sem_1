using System;
using System.Collections.Generic;
using System.Linq;

namespace Pks_Sem1_Practices;

/// <summary>
/// Практика 3.2 — Система бронирования столов.
/// Запуск: Practice3_2.Run();
/// </summary>
public static class Practice3_2_Booking
{
    public static void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var system = new BookingSystem();
        system.MenuLoop();
    }

    // ====== КЛАСС "СТОЛ" ======

    public class Table
    {
        // Время работы: 9–18, 9 часовых слотов (как в примере)
        private const int OpenHour = 9;
        private const int CloseHour = 18;
        private const int Slots = CloseHour - OpenHour; // 9

        public int Id { get; }
        public string Location { get; set; }
        public int Seats { get; set; }

        // расписание — для каждого часа ссылка на бронь или null
        public Booking?[] Schedule { get; } = new Booking?[Slots];

        public Table(int id, string location, int seats)
        {
            Id = id;
            Location = location;
            Seats = seats;
        }

        public void Edit(string newLocation, int newSeats)
        {
            Location = newLocation;
            Seats = newSeats;
        }

        public bool HasActiveBookings()
        {
            return Schedule.Any(b => b is { IsActive: true });
        }

        /// <summary>Проверка, свободен ли стол в интервале [startHour, endHour).</summary>
        public bool IsFree(int startHour, int endHour)
        {
            int from = startHour - OpenHour;
            int to = endHour - OpenHour;
            if (from < 0 || to > Slots || from >= to) return false;

            for (int i = from; i < to; i++)
            {
                if (Schedule[i] is { IsActive: true }) return false;
            }
            return true;
        }

        /// <summary>Занять слоты под бронь.</summary>
        public void Occupy(Booking booking)
        {
            int from = booking.StartHour - OpenHour;
            int to = booking.EndHour - OpenHour;
            for (int i = from; i < to; i++)
                Schedule[i] = booking;
        }

        /// <summary>Освободить слоты, занятые этой бронью.</summary>
        public void Release(Booking booking)
        {
            for (int i = 0; i < Schedule.Length; i++)
                if (ReferenceEquals(Schedule[i], booking))
                    Schedule[i] = null;
        }

        public void PrintInfo()
        {
            Console.WriteLine("***************************************************************");
            Console.WriteLine($"ID: {Id:D2}");
            Console.WriteLine($"Расположение: {Location}");
            Console.WriteLine($"Количество мест: {Seats}");
            Console.WriteLine($"Расписание ({OpenHour}:00–{CloseHour}:00):");

            for (int h = OpenHour; h < CloseHour; h++)
            {
                int idx = h - OpenHour;
                var b = Schedule[idx];
                string line;
                if (b is null || !b.IsActive)
                {
                    line = "свободно";
                }
                else
                {
                    line = $"ID {b.ClientId}, {b.ClientName}, {b.Phone}";
                }

                Console.WriteLine($"{h:00}:00-{h + 1:00}:00 ---- {line}");
            }

            Console.WriteLine("***************************************************************");
        }
    }

    // ====== КЛАСС "БРОНИРОВАНИЕ" ======

    public class Booking
    {
        public int BookingId { get; }
        public int ClientId { get; }
        public string ClientName { get; set; }
        public string Phone { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public string Comment { get; set; }
        public Table Table { get; set; }
        public bool IsActive { get; set; } = true;

        public Booking(
            int bookingId,
            int clientId,
            string clientName,
            string phone,
            int startHour,
            int endHour,
            string comment,
            Table table)
        {
            BookingId = bookingId;
            ClientId  = clientId;
            ClientName = clientName;
            Phone = phone;
            StartHour = startHour;
            EndHour = endHour;
            Comment = comment;
            Table = table;
        }

        public override string ToString()
        {
            string status = IsActive ? "Активна" : "Отменена";
            return $"Бронь #{BookingId}: клиент {ClientName} (ID {ClientId}, тел. {Phone}), " +
                   $"стол {Table.Id}, {StartHour}:00–{EndHour}:00, статус: {status}, комм.: {Comment}";
        }
    }

    // ====== СИСТЕМА БРОНИРОВАНИЯ ======

    public class BookingSystem
    {
        private readonly List<Table> _tables = new();
        private readonly List<Booking> _bookings = new();

        private int _nextTableId = 1;
        private int _nextClientId = 1;
        private int _nextBookingId = 1;

        public void MenuLoop()
        {
            while (true)
            {
                Console.WriteLine("\n=== Система бронирования ===");
                Console.WriteLine("1) Добавить стол");
                Console.WriteLine("2) Редактировать стол (по ID)");
                Console.WriteLine("3) Показать стол (по ID)");
                Console.WriteLine("4) Показать доступные столы по фильтру");
                Console.WriteLine("5) Создать бронь");
                Console.WriteLine("6) Изменить бронь");
                Console.WriteLine("7) Отменить бронь");
                Console.WriteLine("8) Показать все бронирования");
                Console.WriteLine("9) Найти бронь по имени и последним 4 цифрам телефона");
                Console.WriteLine("0) Назад в главное меню");
                Console.Write("Выбор: ");
                var c = Console.ReadLine();
                Console.WriteLine();

                switch (c)
                {
                    case "1": CreateTable(); break;
                    case "2": EditTable(); break;
                    case "3": ShowTable(); break;
                    case "4": ListAvailableTables(); break;
                    case "5": CreateBooking(); break;
                    case "6": EditBooking(); break;
                    case "7": CancelBooking(); break;
                    case "8": ListBookings(); break;
                    case "9": FindBooking(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный пункт."); break;
                }
            }
        }

        // --- Столы ---

        private void CreateTable()
        {
            string location = ReadNonEmpty("Расположение стола (у окна/у прохода/...): ");
            int seats = ReadInt("Количество мест: ", 1, 100);

            var t = new Table(_nextTableId++, location, seats);
            _tables.Add(t);
            Console.WriteLine($"Стол с ID {t.Id} создан.");
        }

        private void EditTable()
        {
            int id = ReadInt("Введите ID стола: ", 1, int.MaxValue);
            Table? t = _tables.FirstOrDefault(x => x.Id == id);
            if (t == null)
            {
                Console.WriteLine("Стол не найден.");
                return;
            }

            if (t.HasActiveBookings())
            {
                Console.WriteLine("Стол участвует в активных бронях — редактирование запрещено.");
                return;
            }

            string location = ReadNonEmpty("Новое расположение: ");
            int seats = ReadInt("Новое количество мест: ", 1, 100);
            t.Edit(location, seats);
            Console.WriteLine("Информация о столе обновлена.");
        }

        private void ShowTable()
        {
            int id = ReadInt("Введите ID стола: ", 1, int.MaxValue);
            Table? t = _tables.FirstOrDefault(x => x.Id == id);
            if (t == null)
            {
                Console.WriteLine("Стол не найден.");
                return;
            }
            t.PrintInfo();
        }

        private void ListAvailableTables()
        {
            int minSeats = ReadInt("Минимальное количество мест: ", 1, 100);
            int startHour = ReadInt("Время начала (час, 9–17): ", 9, 17);
            int endHour = ReadInt("Время конца (час, 10–18): ", startHour + 1, 18);
            Console.Write("Фильтр по расположению (Enter — без фильтра): ");
            string? locFilter = Console.ReadLine();
            locFilter = string.IsNullOrWhiteSpace(locFilter) ? null : locFilter.Trim();

            var suitable = _tables.Where(t =>
                    t.Seats >= minSeats &&
                    t.IsFree(startHour, endHour) &&
                    (locFilter == null || t.Location.Contains(locFilter, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!suitable.Any())
            {
                Console.WriteLine("Нет подходящих свободных столов.");
                return;
            }

            Console.WriteLine("Подходящие столы:");
            foreach (var t in suitable)
                Console.WriteLine($"ID {t.Id}, мест: {t.Seats}, расп.: {t.Location}");
        }

        // --- Бронирования ---

        private void CreateBooking()
        {
            if (_tables.Count == 0)
            {
                Console.WriteLine("Сначала создайте хотя бы один стол.");
                return;
            }

            string name = ReadNonEmpty("Имя клиента: ");
            string phone = ReadNonEmpty("Телефон клиента: ");
            int startHour = ReadInt("Время начала (час, 9–17): ", 9, 17);
            int endHour = ReadInt("Время окончания (час, 10–18): ", startHour + 1, 18);
            int seats = ReadInt("Сколько мест нужно: ", 1, 100);
            string comment = ReadNonEmpty("Комментарий: ");

            // подберём стол
            var freeTables = _tables.Where(t =>
                    t.Seats >= seats &&
                    t.IsFree(startHour, endHour))
                .ToList();

            if (!freeTables.Any())
            {
                Console.WriteLine("Нет свободных столов под такие условия.");
                return;
            }

            Console.WriteLine("Доступные столы:");
            foreach (var t in freeTables)
                Console.WriteLine($"ID {t.Id}, мест: {t.Seats}, расп.: {t.Location}");

            int tableId = ReadInt("Выберите ID стола: ", 1, int.MaxValue);
            var table = freeTables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
            {
                Console.WriteLine("Неверный ID стола.");
                return;
            }

            int clientId = _nextClientId++;
            var booking = new Booking(
                _nextBookingId++,
                clientId,
                name,
                phone,
                startHour,
                endHour,
                comment,
                table);

            _bookings.Add(booking);
            table.Occupy(booking);

            Console.WriteLine($"Бронь #{booking.BookingId} создана.");
        }

        private void EditBooking()
        {
            int id = ReadInt("Введите ID брони: ", 1, int.MaxValue);
            var b = _bookings.FirstOrDefault(x => x.BookingId == id);
            if (b == null)
            {
                Console.WriteLine("Бронь не найдена.");
                return;
            }

            if (!b.IsActive)
            {
                Console.WriteLine("Бронь уже отменена.");
                return;
            }

            Console.WriteLine("Оставьте пустым, если значение менять не нужно.");

            Console.Write($"Имя клиента ({b.ClientName}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) b.ClientName = name.Trim();

            Console.Write($"Телефон ({b.Phone}): ");
            string? ph = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ph)) b.Phone = ph.Trim();

            int startHour = b.StartHour;
            int endHour = b.EndHour;

            Console.Write($"Время начала ({b.StartHour}): ");
            string? sh = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(sh) && int.TryParse(sh, out int shParsed))
                startHour = Math.Clamp(shParsed, 9, 17);

            Console.Write($"Время окончания ({b.EndHour}): ");
            string? eh = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(eh) && int.TryParse(eh, out int ehParsed))
                endHour = Math.Clamp(ehParsed, 10, 18);

            if (endHour <= startHour)
            {
                Console.WriteLine("Интервал времени некорректен.");
                return;
            }

            Console.Write($"Комментарий ({b.Comment}): ");
            string? comm = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(comm)) b.Comment = comm.Trim();

            // проверяем расписание стола
            b.Table.Release(b);
            if (!b.Table.IsFree(startHour, endHour))
            {
                // возвращаем старое состояние
                b.Table.Occupy(b);
                Console.WriteLine("Изменение времени невозможно: стол занят в этот интервал.");
                return;
            }

            b.StartHour = startHour;
            b.EndHour = endHour;
            b.Table.Occupy(b);
            Console.WriteLine("Бронь обновлена.");
        }

        private void CancelBooking()
        {
            int id = ReadInt("Введите ID брони: ", 1, int.MaxValue);
            var b = _bookings.FirstOrDefault(x => x.BookingId == id);
            if (b == null)
            {
                Console.WriteLine("Бронь не найдена.");
                return;
            }

            if (!b.IsActive)
            {
                Console.WriteLine("Бронь уже отменена.");
                return;
            }

            b.IsActive = false;
            b.Table.Release(b);
            Console.WriteLine("Бронь отменена.");
        }

        private void ListBookings()
        {
            if (_bookings.Count == 0)
            {
                Console.WriteLine("Бронирований нет.");
                return;
            }

            foreach (var b in _bookings)
                Console.WriteLine(b);
        }

        private void FindBooking()
        {
            string name = ReadNonEmpty("Имя клиента: ");
            string last4 = ReadNonEmpty("Последние 4 цифры телефона: ");
            if (last4.Length != 4 || !last4.All(char.IsDigit))
            {
                Console.WriteLine("Нужно ввести ровно 4 цифры.");
                return;
            }

            var found = _bookings
                .Where(b => b.ClientName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            b.Phone.EndsWith(last4) &&
                            b.IsActive)
                .ToList();

            if (!found.Any())
            {
                Console.WriteLine("Подходящих активных броней не найдено.");
                return;
            }

            Console.WriteLine("Найденные бронирования:");
            foreach (var b in found) Console.WriteLine(b);
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ВВОДА =====

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"Введите целое число в диапазоне [{min};{max}].");
            }
        }

        private static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("Строка не может быть пустой.");
            }
        }
    }
}
