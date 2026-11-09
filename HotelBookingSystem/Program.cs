namespace HotelsManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== СИСТЕМА БРОНИРОВАНИЯ ОТЕЛЯ ===\n");

            // Инициализация менеджера уведомлений
            var notificationManager = new NotificationManager();
            notificationManager.AddNotification(new EmailNotification());
            notificationManager.AddNotification(new SMSNotification());
            notificationManager.AddNotification(new PushNotification());

            // Инициализация менеджера отеля
            var hotelManager = new HotelManager(notificationManager);

            // Запуск тестов
            RunTests(hotelManager, notificationManager);

            Console.WriteLine("\n=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
        }

        static void RunTests(HotelManager hotelManager, NotificationManager notificationManager)
        {
            Test1_BasicRoomOperations(hotelManager);
            Test2_PriceStrategies();
            Test3_NotificationSystem(notificationManager);
            Test4_BookingSystem(hotelManager);
            Test5_HotelStatistics(hotelManager);
        }

        static void Test1_BasicRoomOperations(HotelManager hotelManager)
        {
            Console.WriteLine("--- ТЕСТ 1: Базовые операции с номерами ---");

            try
            {
                // Номера создаём через фабрику: нужный класс она выбирает сама
                var factory = new RoomFactory();

                var rooms = new List<IRoom>
                {
                    factory.CreateRoom(RoomType.Standard, "101"),
                    factory.CreateRoom(RoomType.Deluxe, "201"),
                    factory.CreateRoom(RoomType.Suite, "301"),
                    factory.CreateRoom(RoomType.Family, "401")
                };

                foreach (var room in rooms)
                {
                    Console.WriteLine($"Создан номер: {room.GetDescription()} ({room.BasePrice} руб./ночь, мест: {room.Capacity})");
                    hotelManager.AddRoom(room);
                }

                // Проверяем, что фабрика не создаёт некорректные номера
                try
                {
                    factory.CreateRoom((RoomType)99, "999");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("Фабрика отклонила неизвестный тип номера");
                }

                try
                {
                    factory.CreateRoom(RoomType.Standard, "  ");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Фабрика отклонила пустой номер комнаты");
                }

                Console.WriteLine("✅ Базовые операции с номерами выполнены успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }

        static void Test2_PriceStrategies()
        {
            Console.WriteLine("\n--- ТЕСТ 2: Стратегии ценообразования ---");

            try
            {
                var basePrice = 3000m;
                var days = 3;
                var checkInDate = new DateTime(2024, 12, 25); // Праздничные даты

                Console.WriteLine($"Базовая цена: {basePrice} руб./ночь");
                Console.WriteLine($"Период: {days} ночей");
                Console.WriteLine($"Заезд: {checkInDate:dd.MM.yyyy} ({checkInDate:dddd})");

                IPriceStrategy[] strategies =
                {
                    new RegularPriceStrategy(),
                    new WeekendPriceStrategy(),
                    new SeasonalPriceStrategy()
                };

                foreach (var strategy in strategies)
                {
                    Console.WriteLine($"{strategy.Name}: {strategy.CalculatePrice(basePrice, days, checkInDate):F2} руб.");
                }

                // Скидка за длительное проживание считается поверх любого тарифа
                var longStayDays = 10;
                var regular = new RegularPriceStrategy();
                var withDiscount = new LongStayDiscountStrategy(regular);

                Console.WriteLine($"\nПроживание {longStayDays} ночей:");
                Console.WriteLine($"  {regular.Name}: {regular.CalculatePrice(basePrice, longStayDays, checkInDate):F2} руб.");
                Console.WriteLine($"  {withDiscount.Name}: {withDiscount.CalculatePrice(basePrice, longStayDays, checkInDate):F2} руб.");

                Console.WriteLine("✅ Стратегии ценообразования работают");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }

        static void Test3_NotificationSystem(NotificationManager notificationManager)
        {
            Console.WriteLine("\n--- ТЕСТ 3: Система уведомлений ---");

            try
            {
                var testMessage = "Тестовое уведомление от системы бронирования";
                var testRecipient = "client@example.com";

                Console.WriteLine("Отправка тестовых уведомлений...");
                notificationManager.Broadcast(testMessage, testRecipient);

                // Observer: бронирование само рассылает сообщения своим подписчикам
                Console.WriteLine("\nУведомления о бронировании (Observer):");

                var room = new RoomFactory().CreateRoom(RoomType.Deluxe, "201");
                var booking = new Booking(
                    room,
                    "Иван Петров",
                    "ivan.petrov@mail.ru",
                    DateTime.Today.AddDays(7),
                    DateTime.Today.AddDays(10),
                    new RegularPriceStrategy(),
                    notificationManager.Channels);

                Console.WriteLine($"Подписчиков у бронирования: {booking.Subscribers.Count}");

                Console.WriteLine("\nПодтверждаем бронирование:");
                booking.Confirm();

                Console.WriteLine("\nГость отписался от SMS и отменил бронирование:");
                booking.Unsubscribe(NotificationType.SMS);
                booking.Cancel();

                Console.WriteLine("\nПовторное подтверждение отменённой брони:");
                try
                {
                    booking.Confirm();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Отказ: {ex.Message}");
                }

                Console.WriteLine("✅ Система уведомлений работает");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }

        static void Test4_BookingSystem(HotelManager hotelManager)
        {
            Console.WriteLine("\n--- ТЕСТ 4: Система бронирования ---");

            try
            {
                var checkIn = DateTime.Now.AddDays(7);
                var checkOut = DateTime.Now.AddDays(10);

                // Поиск доступных номеров
                var availableRooms = hotelManager.FindAvailableRooms();
                Console.WriteLine($"Найдено доступных номеров: {availableRooms.Count}");

                if (availableRooms.Count > 0)
                {
                    var booking = hotelManager.CreateBooking(
                        availableRooms[0].RoomNumber,
                        "Иван Петров",
                        "ivan.petrov@mail.ru",
                        checkIn,
                        checkOut,
                        new RegularPriceStrategy()
                    );

                    Console.WriteLine($"Создано бронирование #{booking.BookingId}");
                    Console.WriteLine($"Гость: {booking.GuestName}");
                    Console.WriteLine($"Стоимость: {booking.TotalPrice:F2} руб.");

                    // Подтверждение бронирования
                    booking.Confirm();

                    // Поиск на даты: занятый номер в результат не попадает
                    Console.WriteLine($"\nСвободные номера на {checkIn:dd.MM} - {checkOut:dd.MM}:");
                    foreach (var room in hotelManager.FindAvailableRooms(checkIn, checkOut))
                    {
                        Console.WriteLine($"  {room.GetDescription()}, {room.BasePrice} руб./ночь");
                    }

                    // Поиск по критериям: от 3 мест и не дороже 6000 руб.
                    var criteria = new RoomSearchCriteria { MinCapacity = 3, MaxPrice = 6000m };
                    Console.WriteLine("\nНомера от 3 мест и не дороже 6000 руб. на те же даты:");
                    foreach (var room in hotelManager.FindAvailableRooms(checkIn, checkOut, criteria))
                    {
                        Console.WriteLine($"  {room.GetDescription()}, {room.BasePrice} руб./ночь");
                    }

                    // Занятый номер нельзя забронировать, даже если даты пересекаются частично
                    Console.WriteLine("\nПопытка забронировать занятый номер:");
                    try
                    {
                        hotelManager.CreateBooking(booking.Room.RoomNumber, "Анна Смирнова", "anna@mail.ru",
                            checkIn.AddDays(1), checkOut.AddDays(1), new RegularPriceStrategy());
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Отказ: {ex.Message}");
                    }

                    // Несуществующий номер
                    try
                    {
                        hotelManager.CreateBooking("999", "Анна Смирнова", "anna@mail.ru",
                            checkIn, checkOut, new RegularPriceStrategy());
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Отказ: {ex.Message}");
                    }

                    // В день выезда одного гостя другой может заехать
                    Console.WriteLine("\nЗаезд нового гостя в день выезда предыдущего (уведомления только по e-mail):");
                    var next = hotelManager.CreateBooking(booking.Room.RoomNumber, "Мария Смирнова", "maria@mail.ru",
                        checkOut, checkOut.AddDays(2), new WeekendPriceStrategy(), new[] { NotificationType.Email });
                    Console.WriteLine($"Создано бронирование #{next.BookingId}, " +
                                      $"стоимость {next.TotalPrice:F2} руб. ({next.PriceStrategy.Name})");

                    // Отмена освобождает номер
                    Console.WriteLine("\nОтмена второго бронирования:");
                    next.Cancel();
                    var freeAgain = hotelManager.FindAvailableRooms(checkOut, checkOut.AddDays(2));
                    Console.WriteLine($"Свободных номеров на {checkOut:dd.MM} - {checkOut.AddDays(2):dd.MM}: {freeAgain.Count}");

                    Console.WriteLine("✅ Система бронирования работает");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }

        static void Test5_HotelStatistics(HotelManager hotelManager)
        {
            Console.WriteLine("\n--- ТЕСТ 5: Статистика отеля ---");

            try
            {
                // Заселим гостя сегодня и оформим ещё одну бронь на будущее без уведомлений
                var today = DateTime.Today;

                var inHouse = hotelManager.CreateBooking("201", "Олег Крылов", "oleg@mail.ru",
                    today, today.AddDays(2), new RegularPriceStrategy(), new[] { NotificationType.Email });
                inHouse.Confirm();

                hotelManager.CreateBooking("301", "Пётр Сидоров", "petr@mail.ru",
                    today.AddDays(30), today.AddDays(34), new SeasonalPriceStrategy(),
                    Array.Empty<NotificationType>());

                Console.WriteLine();
                hotelManager.ShowHotelStatus();

                var statistics = hotelManager.GetRoomStatistics();
                Console.WriteLine("\nСтатистика по типам номеров:");
                foreach (var stat in statistics)
                {
                    Console.WriteLine($"  {stat.Key}: {stat.Value} номеров");
                }

                Console.WriteLine("\nВсе бронирования:");
                foreach (var booking in hotelManager.GetBookings())
                {
                    Console.WriteLine($"  {booking}");
                }

                Console.WriteLine("✅ Статистика отеля работает");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }
    }
}