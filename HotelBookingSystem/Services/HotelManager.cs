namespace HotelsManager
{
    // Главный класс отеля: хранит номера и бронирования,
    // ищет свободные номера, создаёт брони и считает статистику.
    public class HotelManager
    {
        private readonly List<IRoom> _rooms = new List<IRoom>();
        private readonly List<Booking> _bookings = new List<Booking>();
        private readonly NotificationManager _notificationManager;

        public HotelManager(NotificationManager notificationManager)
        {
            ArgumentNullException.ThrowIfNull(notificationManager);
            _notificationManager = notificationManager;
        }

        // ---------- номера ----------

        public void AddRoom(IRoom room)
        {
            ArgumentNullException.ThrowIfNull(room);

            if (FindRoom(room.RoomNumber) != null)
            {
                throw new ArgumentException($"Номер {room.RoomNumber} уже есть в отеле.");
            }

            _rooms.Add(room);
        }

        public IRoom? FindRoom(string roomNumber)
        {
            return _rooms.FirstOrDefault(room =>
                string.Equals(room.RoomNumber, roomNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Booking> GetBookings()
        {
            return _bookings;
        }

        // ---------- поиск ----------

        // Номера, которые вообще можно бронировать (не закрыты на ремонт).
        // Даты здесь не учитываются, это общий список.
        public List<IRoom> FindAvailableRooms()
        {
            return _rooms.Where(room => room.IsAvailable).ToList();
        }

        public List<IRoom> FindAvailableRooms(RoomType roomType)
        {
            return _rooms.Where(room => room.IsAvailable && room.Type == roomType).ToList();
        }

        // Поиск на конкретные даты и по критериям (тип, вместимость, цена).
        // Самые дешёвые номера идут первыми.
        public List<IRoom> FindAvailableRooms(DateTime checkIn, DateTime checkOut, RoomSearchCriteria? criteria = null)
        {
            EnsureValidPeriod(checkIn, checkOut);

            return _rooms
                .Where(room => room.IsAvailable)
                .Where(room => IsRoomFree(room, checkIn, checkOut))
                .Where(room => criteria == null || criteria.Matches(room))
                .OrderBy(room => room.BasePrice)
                .ToList();
        }

        // ---------- бронирование ----------

        // channels это каналы, по которым гость хочет получать уведомления.
        // Если не указать, подписываем на все каналы отеля. Пустой список значит "без уведомлений".
        public Booking CreateBooking(string roomNumber, string guestName, string guestEmail,
                                     DateTime checkIn, DateTime checkOut, IPriceStrategy priceStrategy,
                                     IEnumerable<NotificationType>? channels = null)
        {
            var room = FindRoom(roomNumber)
                ?? throw new ArgumentException($"Номер {roomNumber} не найден в отеле.");

            if (!room.IsAvailable)
            {
                throw new InvalidOperationException($"Номер {room.RoomNumber} сейчас закрыт для бронирования.");
            }

            EnsureValidPeriod(checkIn, checkOut);

            if (!IsRoomFree(room, checkIn, checkOut))
            {
                throw new InvalidOperationException($"Номер {room.RoomNumber} уже занят на выбранные даты.");
            }

            // выбираем каналы, на которые подпишем гостя
            IEnumerable<INotification> subscribers = _notificationManager.Channels;
            if (channels != null)
            {
                var wanted = channels.ToList();
                subscribers = subscribers.Where(channel => wanted.Contains(channel.Type)).ToList();
            }

            var booking = new Booking(room, guestName, guestEmail, checkIn, checkOut, priceStrategy, subscribers);
            _bookings.Add(booking);
            return booking;
        }

        // Номер свободен, если ни одна действующая бронь не пересекается с нужными датами.
        // Отменённые брони не мешают, поэтому после Cancel() номер освобождается сам.
        // День выезда одного гостя может быть днём заезда другого.
        private bool IsRoomFree(IRoom room, DateTime checkIn, DateTime checkOut)
        {
            return !_bookings.Any(booking =>
                booking.Status != BookingStatus.Cancelled
                && string.Equals(booking.Room.RoomNumber, room.RoomNumber, StringComparison.OrdinalIgnoreCase)
                && booking.CheckIn.Date < checkOut.Date
                && checkIn.Date < booking.CheckOut.Date);
        }

        private static void EnsureValidPeriod(DateTime checkIn, DateTime checkOut)
        {
            if (checkOut.Date <= checkIn.Date)
            {
                throw new ArgumentException("Дата выезда должна быть позже даты заезда.");
            }
        }

        // ---------- статистика ----------

        public HotelStatistics GetHotelStatistics()
        {
            var today = DateTime.Today;

            var occupiedToday = _bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Where(b => b.CheckIn.Date <= today && today < b.CheckOut.Date)
                .Select(b => b.Room.RoomNumber)
                .Distinct()
                .Count();

            return new HotelStatistics(
                TotalRooms: _rooms.Count,
                RoomsInService: _rooms.Count(room => room.IsAvailable),
                OccupiedToday: occupiedToday,
                ConfirmedBookings: _bookings.Count(b => b.Status == BookingStatus.Confirmed),
                PendingBookings: _bookings.Count(b => b.Status == BookingStatus.Pending),
                CancelledBookings: _bookings.Count(b => b.Status == BookingStatus.Cancelled),
                ConfirmedRevenue: _bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalPrice),
                PendingRevenue: _bookings.Where(b => b.Status == BookingStatus.Pending).Sum(b => b.TotalPrice));
        }

        public void ShowHotelStatus()
        {
            var stats = GetHotelStatistics();

            Console.WriteLine($"Состояние отеля на {DateTime.Today:dd.MM.yyyy}:");
            Console.WriteLine($"  Номеров всего: {stats.TotalRooms} (в эксплуатации: {stats.RoomsInService})");
            Console.WriteLine($"  Занято сегодня: {stats.OccupiedToday} ({stats.OccupancyPercent}%)");
            Console.WriteLine($"  Бронирований: подтверждено {stats.ConfirmedBookings}, " +
                              $"ожидает {stats.PendingBookings}, отменено {stats.CancelledBookings}");
            Console.WriteLine($"  Выручка по подтверждённым бронированиям: {stats.ConfirmedRevenue:F2} руб.");
            Console.WriteLine($"  Ожидается по неподтверждённым: {stats.PendingRevenue:F2} руб.");
        }

        // Сколько номеров каждого типа в отеле (типы без номеров показываются с нулём)
        public Dictionary<RoomType, int> GetRoomStatistics()
        {
            return Enum.GetValues<RoomType>()
                .ToDictionary(type => type, type => _rooms.Count(room => room.Type == type));
        }
    }
}