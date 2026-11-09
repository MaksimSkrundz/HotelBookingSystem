namespace HotelsManager
{
    // Бронирование номера. Здесь работает паттерн Observer:
    // бронирование это "издатель", а каналы связи (INotification) это "подписчики".
    // Когда с бронированием что-то происходит (создали, подтвердили, отменили),
    // оно само рассылает сообщение всем подписчикам. Бронь не знает,
    // что это за каналы: ей достаточно, что у каждого есть метод Send.
    public class Booking
    {
        private static int _nextBookingId = 1;

        private readonly List<INotification> _notifications = new List<INotification>();

        public int BookingId { get; }
        public IRoom Room { get; }
        public string GuestName { get; }
        public string GuestContact { get; }
        public DateTime CheckIn { get; }
        public DateTime CheckOut { get; }
        public IPriceStrategy PriceStrategy { get; }
        public decimal TotalPrice { get; }
        public BookingStatus Status { get; private set; } = BookingStatus.Pending;

        public int Nights => (CheckOut.Date - CheckIn.Date).Days;

        public IReadOnlyList<INotification> Subscribers => _notifications;

        public string StatusText => Status switch
        {
            BookingStatus.Pending => "ожидает подтверждения",
            BookingStatus.Confirmed => "подтверждено",
            _ => "отменено"
        };

        // guestContact это адрес, на который уходят уведомления (например, e-mail)
        // subscribers это каналы, на которые гость подписан с самого начала
        public Booking(IRoom room, string guestName, string guestContact,
                       DateTime checkIn, DateTime checkOut, IPriceStrategy priceStrategy,
                       IEnumerable<INotification>? subscribers = null)
        {
            ArgumentNullException.ThrowIfNull(room);
            ArgumentNullException.ThrowIfNull(priceStrategy);

            if (string.IsNullOrWhiteSpace(guestName))
            {
                throw new ArgumentException("Укажите имя гостя.", nameof(guestName));
            }

            if (string.IsNullOrWhiteSpace(guestContact))
            {
                throw new ArgumentException("Укажите контакт гостя для уведомлений.", nameof(guestContact));
            }

            if (checkIn.Date < DateTime.Today)
            {
                throw new ArgumentException("Дата заезда не может быть в прошлом.", nameof(checkIn));
            }

            if (checkOut.Date <= checkIn.Date)
            {
                throw new ArgumentException("Дата выезда должна быть позже даты заезда.", nameof(checkOut));
            }

            Room = room;
            GuestName = guestName.Trim();
            GuestContact = guestContact.Trim();
            CheckIn = checkIn;
            CheckOut = checkOut;
            PriceStrategy = priceStrategy;
            BookingId = _nextBookingId++;

            // стоимость считает выбранная стратегия (паттерн Strategy)
            TotalPrice = priceStrategy.CalculatePrice(room.BasePrice, Nights, checkIn);

            if (subscribers != null)
            {
                foreach (var subscriber in subscribers)
                {
                    Subscribe(subscriber);
                }
            }

            Notify($"Бронирование №{BookingId} создано: {Room.GetDescription()}, " +
                   $"{CheckIn:dd.MM.yyyy} - {CheckOut:dd.MM.yyyy} ({Nights} ноч.), " +
                   $"{PriceStrategy.Name}, стоимость {TotalPrice:F2} руб. Ожидает подтверждения.");
        }

        // подписать канал на уведомления (один и тот же канал дважды не добавляем)
        public void Subscribe(INotification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            if (!_notifications.Contains(notification))
            {
                _notifications.Add(notification);
            }
        }

        public void Unsubscribe(INotification notification)
        {
            _notifications.Remove(notification);
        }

        // отписаться сразу от всех каналов одного вида, например от SMS
        public void Unsubscribe(NotificationType type)
        {
            _notifications.RemoveAll(n => n.Type == type);
        }

        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Подтвердить можно только бронирование, которое ожидает подтверждения (сейчас: {StatusText}).");
            }

            Status = BookingStatus.Confirmed;
            Notify($"Бронирование №{BookingId} подтверждено. Ждём вас {CheckIn:dd.MM.yyyy}, {GuestName}!");
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
            {
                throw new InvalidOperationException("Бронирование уже отменено.");
            }

            Status = BookingStatus.Cancelled;
            Notify($"Бронирование №{BookingId} отменено.");
        }

        // Разослать сообщение всем подписчикам.
        // Если один канал сломался, остальные всё равно получат сообщение.
        private void Notify(string message)
        {
            foreach (var notification in _notifications.ToArray())
            {
                try
                {
                    if (!notification.Send(message, GuestContact))
                    {
                        Console.WriteLine($"⚠️  Не удалось отправить уведомление ({notification.Type}) для {GuestContact}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Канал {notification.Type} недоступен: {ex.Message}");
                }
            }
        }

        public override string ToString()
        {
            return $"Бронь №{BookingId}: {GuestName}, {Room.GetDescription()}, " +
                   $"{CheckIn:dd.MM.yyyy} - {CheckOut:dd.MM.yyyy}, {TotalPrice:F2} руб., {StatusText}";
        }
    }
}