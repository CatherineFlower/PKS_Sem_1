using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pks_Sem1_Practices;

/// <summary>
/// Практика 4 — Система заказов.
/// Запуск: Practice4_Orders.Run();
/// </summary>
public static class Practice4_Orders
{
    public static void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var system = new OrderSystem();
        system.MenuLoop();
    }

    // ===== КАТЕГОРИИ БЛЮД =====

    public enum DishCategory
    {
        Drinks,
        Salads,
        ColdSnacks,
        HotSnacks,
        Soups,
        MainCourses,
        Desserts
    }

    // ===== КЛАСС "БЛЮДО" =====

    public class Dish
    {
        public int Id { get; }
        public string Name { get; set; }
        public string Composition { get; set; }
        public string Weight { get; set; } // "100/20/50"
        public decimal Price { get; set; }
        public DishCategory Category { get; set; }
        public int CookTimeMinutes { get; set; }
        public string[] Types { get; set; } // острое, веганское и т.п.

        public Dish(
            int id,
            string name,
            string composition,
            string weight,
            decimal price,
            DishCategory category,
            int cookTimeMinutes,
            string[] types)
        {
            Id = id;
            Name = name;
            Composition = composition;
            Weight = weight;
            Price = price;
            Category = category;
            CookTimeMinutes = cookTimeMinutes;
            Types = types;
        }

        public override string ToString()
        {
            string types = Types.Length > 0 ? string.Join(", ", Types) : "—";
            return $"[{Id}] {Name} ({Category}), {Price:0.00} руб., вес {Weight}, готовка {CookTimeMinutes} мин., типы: {types}";
        }
    }

    // Одна позиция в заказе (блюдо + количество)
    public class OrderItem
    {
        public Dish Dish { get; }
        public int Count { get; private set; }

        public OrderItem(Dish dish, int count)
        {
            Dish = dish;
            Count = count;
        }

        public void ChangeCount(int newCount) => Count = newCount;

        public decimal LineTotal => Dish.Price * Count;
    }

    // ===== КЛАСС "ЗАКАЗ" =====

    public class Order
    {
        public int Id { get; }
        public int TableId { get; set; }
        public List<OrderItem> Items { get; } = new();
        public string Comment { get; set; }
        public DateTime OpenTime { get; }
        public int WaiterId { get; set; }
        public DateTime? CloseTime { get; private set; }
        public decimal Total { get; private set; }

        public bool IsClosed => CloseTime.HasValue;

        public Order(int id, int tableId, int waiterId, string comment)
        {
            Id = id;
            TableId = tableId;
            WaiterId = waiterId;
            Comment = comment;
            OpenTime = DateTime.Now;
        }

        public void AddDish(Dish dish, int count)
        {
            var existing = Items.FirstOrDefault(i => i.Dish.Id == dish.Id);
            if (existing != null)
                existing.ChangeCount(existing.Count + count);
            else
                Items.Add(new OrderItem(dish, count));

            RecalculateTotal();
        }

        public void ChangeDishCount(int dishId, int newCount)
        {
            var item = Items.FirstOrDefault(i => i.Dish.Id == dishId);
            if (item == null) return;

            if (newCount <= 0) Items.Remove(item);
            else item.ChangeCount(newCount);

            RecalculateTotal();
        }

        public void Close()
        {
            if (IsClosed) return;
            CloseTime = DateTime.Now;
            RecalculateTotal();
        }

        public void RecalculateTotal()
        {
            decimal sum = 0;
            foreach (var i in Items) sum += i.LineTotal;
            Total = sum;
        }

        public override string ToString()
        {
            string status = IsClosed ? $"Закрыт ({CloseTime})" : "Открыт";
            return $"Заказ #{Id}, стол {TableId}, официант {WaiterId}, " +
                   $"{status}, позиций: {Items.Count}, сумма: {Total:0.00} руб., комментарий: {Comment}";
        }

        // Печать чека (только для закрытого заказа)
        public void PrintReceipt()
        {
            if (!IsClosed)
            {
                Console.WriteLine("Нельзя печатать чек: заказ ещё не закрыт.");
                return;
            }

            Console.WriteLine("*************************************************");
            Console.WriteLine($"Столик: {TableId}");
            Console.WriteLine($"Официант: {WaiterId}");
            Console.WriteLine($"Период обслуживания: с {OpenTime} по {CloseTime}");
            Console.WriteLine();

            var byCategory = Items
                .GroupBy(i => i.Dish.Category)
                .OrderBy(g => g.Key);

            foreach (var catGroup in byCategory)
            {
                Console.WriteLine($"Категория: {catGroup.Key}");
                decimal subTotal = 0;
                foreach (var item in catGroup)
                {
                    decimal lineTotal = item.LineTotal;
                    subTotal += lineTotal;
                    Console.WriteLine(
                        $"{item.Dish.Name}: {item.Count} * {item.Dish.Price:0.00} = {lineTotal:0.00} руб.");
                }
                Console.WriteLine($"Подытог категории: {subTotal:0.00} руб.\n");
            }

            Console.WriteLine($"Итог счета: {Total:0.00} руб.");
            Console.WriteLine("*************************************************");
        }
    }

    // ===== СТАТИСТИКА (in / out / params) =====

    public static class OrderStatistics
    {
        // Подсчёт суммы всех закрытых заказов (используем in и out)
        public static void CalcClosedTotal(in List<Order> orders, out decimal total)
        {
            decimal sum = 0;
            foreach (var o in orders)
                if (o.IsClosed)
                    sum += o.Total;
            total = sum;
        }

        // Количество и сумма закрытых заказов конкретного официанта
        public static int CountClosedByWaiter(
            in List<Order> orders,
            int waiterId,
            out decimal total)
        {
            int count = 0;
            decimal sum = 0;
            foreach (var o in orders)
            {
                if (o.IsClosed && o.WaiterId == waiterId)
                {
                    count++;
                    sum += o.Total;
                }
            }

            total = sum;
            return count;
        }

        // Статистика по количеству заказанных блюд (используем params)
        public static Dictionary<int, int> CollectDishStats(params Order[] orders)
        {
            var dict = new Dictionary<int, int>(); // DishId -> count

            foreach (var o in orders)
            {
                foreach (var item in o.Items)
                {
                    int id = item.Dish.Id;
                    if (!dict.ContainsKey(id)) dict[id] = 0;
                    dict[id] += item.Count;
                }
            }

            return dict;
        }
    }

    // ===== СИСТЕМА ЗАКАЗОВ =====

    public class OrderSystem
    {
        private readonly List<Dish> _dishes = new();
        private readonly List<Order> _orders = new();

        private int _nextDishId = 1;
        private int _nextOrderId = 1;

        public void MenuLoop()
        {
            while (true)
            {
                Console.WriteLine("\n=== Система заказов ===");
                Console.WriteLine("1) Добавить блюдо");
                Console.WriteLine("2) Редактировать блюдо");
                Console.WriteLine("3) Удалить блюдо");
                Console.WriteLine("4) Показать все блюда (меню)");
                Console.WriteLine("5) Создать заказ");
                Console.WriteLine("6) Изменить заказ");
                Console.WriteLine("7) Закрыть заказ");
                Console.WriteLine("8) Показать заказ");
                Console.WriteLine("9) Печать чека");
                Console.WriteLine("10) Стоимость всех закрытых заказов");
                Console.WriteLine("11) Статистика по официанту");
                Console.WriteLine("12) Статистика по блюдам");
                Console.WriteLine("0) Назад в главное меню");
                Console.Write("Выбор: ");
                var c = Console.ReadLine();
                Console.WriteLine();

                switch (c)
                {
                    case "1": CreateDish(); break;
                    case "2": EditDish(); break;
                    case "3": DeleteDish(); break;
                    case "4": PrintMenu(); break;
                    case "5": CreateOrder(); break;
                    case "6": EditOrder(); break;
                    case "7": CloseOrder(); break;
                    case "8": ShowOrder(); break;
                    case "9": PrintReceipt(); break;
                    case "10": ShowClosedTotal(); break;
                    case "11": ShowWaiterStats(); break;
                    case "12": ShowDishStats(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный пункт."); break;
                }
            }
        }

        // ---------- БЛЮДА ----------

        private void CreateDish()
        {
            // Название и состав — обязательно хоть одна буква
            string name = ReadTextWithLetters("Название блюда: ");
            string comp = ReadTextWithLetters("Состав: ");
            // Вес строго в формате a/b/c
            string weight = ReadWeight("Вес (формат 300/50/20): ");

            decimal price = ReadDecimal("Цена: ", 0.01m, 100000m);
            int cook = ReadInt("Время готовки (мин): ", 1, 600);

            Console.WriteLine("Категория:");
            foreach (DishCategory cat in Enum.GetValues(typeof(DishCategory)))
                Console.WriteLine($"{(int)cat} - {cat}");
            int catInt = ReadInt("Выбор категории (число): ",
                0, Enum.GetValues(typeof(DishCategory)).Length - 1);
            var category = (DishCategory)catInt;

            Console.Write("Типы (через запятую, например: острое, веганское). Можно оставить пустым: ");
            string? typesInput = Console.ReadLine();
            string[] types = string.IsNullOrWhiteSpace(typesInput)
                ? Array.Empty<string>()
                : typesInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var d = new Dish(_nextDishId++, name, comp, weight, price, category, cook, types);
            _dishes.Add(d);
            Console.WriteLine($"Блюдо добавлено с Id = {d.Id}");
        }

        private void EditDish()
        {
            if (_dishes.Count == 0)
            {
                Console.WriteLine("Сначала добавьте хотя бы одно блюдо.");
                return;
            }

            int id = ReadInt("Id блюда: ", 1, int.MaxValue);
            var d = _dishes.FirstOrDefault(x => x.Id == id);
            if (d == null)
            {
                Console.WriteLine("Блюдо не найдено.");
                return;
            }

            Console.WriteLine("Оставьте поле пустым, если не хотите его менять.");

            // Название
            Console.Write($"Название ({d.Name}): ");
            string? s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
            {
                s = s.Trim();
                if (s.Any(char.IsLetter))
                    d.Name = s;
                else
                    Console.WriteLine("Название не изменено: нужна хотя бы одна буква.");
            }

            // Состав
            Console.Write($"Состав ({d.Composition}): ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
            {
                s = s.Trim();
                if (s.Any(char.IsLetter))
                    d.Composition = s;
                else
                    Console.WriteLine("Состав не изменён: нужна хотя бы одна буква.");
            }

            // Вес
            Console.Write($"Вес ({d.Weight}, формат 300/50/20): ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
            {
                while (true)
                {
                    s = s.Trim();
                    if (IsValidWeight(s))
                    {
                        d.Weight = s;
                        break;
                    }

                    Console.WriteLine("Неверный формат веса. Нужен формат 300/50/20 (три положительных целых).");
                    Console.Write("Вес (формат 300/50/20, Enter — оставить старый): ");
                    s = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(s)) break;
                }
            }

            // Цена
            Console.Write($"Цена ({d.Price:0.00}): ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
            {
                s = s.Trim().Replace(',', '.');
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var pr) &&
                    pr >= 0.01m && pr <= 100000m)
                {
                    d.Price = pr;
                }
                else
                {
                    Console.WriteLine("Цена не изменена: нужно число от 0,01 до 100000.");
                }
            }

            // Время готовки
            Console.Write($"Время готовки ({d.CookTimeMinutes}): ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var ct) && ct >= 1 && ct <= 600)
                d.CookTimeMinutes = ct;

            // Категория
            Console.WriteLine("Категория (Enter — без изменения):");
            foreach (DishCategory cat in Enum.GetValues(typeof(DishCategory)))
                Console.WriteLine($"{(int)cat} - {cat}");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var ci) &&
                Enum.IsDefined(typeof(DishCategory), ci))
                d.Category = (DishCategory)ci;

            // Типы
            Console.Write("Типы (через запятую, Enter — без изменения): ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
            {
                d.Types = s.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }

            Console.WriteLine("Блюдо изменено.");
        }

        private void DeleteDish()
        {
            int id = ReadInt("Id блюда для удаления: ", 1, int.MaxValue);
            var d = _dishes.FirstOrDefault(x => x.Id == id);
            if (d == null)
            {
                Console.WriteLine("Блюдо не найдено.");
                return;
            }

            _dishes.Remove(d);
            Console.WriteLine("Блюдо удалено.");
        }

        private void PrintMenu()
        {
            if (_dishes.Count == 0)
            {
                Console.WriteLine("Меню пусто.");
                return;
            }

            PrintMenuByCategories(in _dishes);
        }

        private static void PrintMenuByCategories(in List<Dish> dishes)
        {
            Console.WriteLine("=== МЕНЮ ===");

            var grouped = dishes
                .GroupBy(d => d.Category)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                Console.WriteLine($"\n[{group.Key}]");
                foreach (var d in group)
                    Console.WriteLine(d);
            }
        }

        // ---------- ЗАКАЗЫ ----------

        private void CreateOrder()
        {
            if (_dishes.Count == 0)
            {
                Console.WriteLine("Нельзя создать заказ: меню пусто.");
                return;
            }

            int tableId = ReadInt("Id стола: ", 1, int.MaxValue);
            int waiterId = ReadInt("Id официанта: ", 1, int.MaxValue);
            string comment = ReadNonEmpty("Комментарий: ");

            var order = new Order(_nextOrderId++, tableId, waiterId, comment);
            _orders.Add(order);

            Console.WriteLine("Теперь добавим блюда в заказ.");
            AddDishesToOrder(order);
        }

        private void AddDishesToOrder(Order order)
        {
            while (true)
            {
                PrintMenu();
                Console.WriteLine("Добавление блюд в заказ. Для выхода введите 0.");
                int dishId = ReadInt("Id блюда (0 — закончить): ", 0, int.MaxValue);
                if (dishId == 0) break;

                var d = _dishes.FirstOrDefault(x => x.Id == dishId);
                if (d == null)
                {
                    Console.WriteLine("Такого блюда нет.");
                    continue;
                }

                int count = ReadInt("Количество: ", 1, 1000);
                order.AddDish(d, count);
                Console.WriteLine("Блюдо добавлено в заказ.");
            }
        }

        private Order? FindOrderById()
        {
            int id = ReadInt("Id заказа: ", 1, int.MaxValue);
            var o = _orders.FirstOrDefault(x => x.Id == id);
            if (o == null)
                Console.WriteLine("Заказ не найден.");
            return o;
        }

        private void EditOrder()
        {
            var order = FindOrderById();
            if (order == null) return;

            if (order.IsClosed)
            {
                Console.WriteLine("Нельзя изменять закрытый заказ.");
                return;
            }

            Console.WriteLine("1) Добавить блюда\n2) Изменить количество блюда\n0) Назад");
            int ch = ReadInt("Выбор: ", 0, 2);
            switch (ch)
            {
                case 1:
                    AddDishesToOrder(order);
                    break;
                case 2:
                    if (order.Items.Count == 0)
                    {
                        Console.WriteLine("В заказе ещё нет блюд.");
                        return;
                    }

                    Console.WriteLine("Текущие позиции:");
                    foreach (var it in order.Items)
                        Console.WriteLine($"{it.Dish.Id}: {it.Dish.Name}, кол-во {it.Count}");

                    int dishId = ReadInt("Id блюда для изменения: ", 1, int.MaxValue);
                    int newCount = ReadInt("Новое количество (0 — удалить позицию): ", 0, 1000);
                    order.ChangeDishCount(dishId, newCount);
                    Console.WriteLine("Заказ обновлён.");
                    break;
            }
        }

        private void CloseOrder()
        {
            var order = FindOrderById();
            if (order == null) return;

            if (order.IsClosed)
            {
                Console.WriteLine("Заказ уже закрыт.");
                return;
            }

            order.Close();
            Console.WriteLine("Заказ закрыт.");
        }

        private void ShowOrder()
        {
            var order = FindOrderById();
            if (order == null) return;

            Console.WriteLine(order);
            if (order.Items.Count > 0)
            {
                Console.WriteLine("Позиции:");
                foreach (var i in order.Items)
                    Console.WriteLine(
                        $"{i.Dish.Name}: {i.Count} * {i.Dish.Price:0.00} = {i.LineTotal:0.00}");
            }
        }

        private void PrintReceipt()
        {
            var order = FindOrderById();
            if (order == null) return;
            order.PrintReceipt();
        }

        // ---------- СТАТИСТИКА ----------

        private void ShowClosedTotal()
        {
            OrderStatistics.CalcClosedTotal(in _orders, out var total);
            Console.WriteLine($"Сумма всех закрытых заказов: {total:0.00} руб.");
        }

        private void ShowWaiterStats()
        {
            int waiterId = ReadInt("Id официанта: ", 1, int.MaxValue);
            int count = OrderStatistics.CountClosedByWaiter(in _orders, waiterId, out var total);
            Console.WriteLine(
                $"Закрытых заказов официанта {waiterId}: {count}, суммарная выручка: {total:0.00} руб.");
        }

        private void ShowDishStats()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Заказов ещё нет.");
                return;
            }

            var stats = OrderStatistics.CollectDishStats(_orders.ToArray());

            if (stats.Count == 0)
            {
                Console.WriteLine("Блюда ещё не заказывали.");
                return;
            }

            Console.WriteLine("Статистика по блюдам (DishId -> количество):");
            foreach (var kvp in stats.OrderByDescending(k => k.Value))
            {
                var dish = _dishes.FirstOrDefault(d => d.Id == kvp.Key);
                string name = dish?.Name ?? $"Id {kvp.Key}";
                Console.WriteLine($"{name}: {kvp.Value}");
            }
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ ВВОДЫ =====

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

        /// <summary>Строка с хотя бы одной буквой (для названия, состава).</summary>
        private static string ReadTextWithLetters(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Строка не может быть пустой.");
                    continue;
                }

                s = s.Trim();
                if (!s.Any(char.IsLetter))
                {
                    Console.WriteLine("Должна быть хотя бы одна буква, а не только цифры.");
                    continue;
                }

                return s;
            }
        }

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"Введите целое число от {min} до {max}.");
            }
        }

        /// <summary>Проверка формата веса a/b/c.</summary>
        private static bool IsValidWeight(string s)
        {
            string[] parts = s.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return false;

            foreach (var p in parts)
            {
                if (!int.TryParse(p, out int v) || v <= 0 || v > 100000)
                    return false;
            }

            return true;
        }

        /// <summary>Чтение веса формата 300/50/20.</summary>
        private static string ReadWeight(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Вес не может быть пустым. Формат: 300/50/20");
                    continue;
                }

                s = s.Trim();
                if (IsValidWeight(s)) return s;

                Console.WriteLine("Неверный формат веса. Нужен формат 300/50/20 (три положительных целых).");
            }
        }

        private static decimal ReadDecimal(string prompt, decimal min, decimal max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (s is null)
                {
                    Console.WriteLine($"Введите число от {min} до {max}.");
                    continue;
                }

                s = s.Trim().Replace(',', '.');

                if (decimal.TryParse(s, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var v) &&
                    v >= min && v <= max)
                {
                    return v;
                }

                Console.WriteLine($"Введите число от {min} до {max}.");
            }
        }
    }
}
