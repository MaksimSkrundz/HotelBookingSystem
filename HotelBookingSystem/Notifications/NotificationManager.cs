namespace HotelsManager
{
    // Список каналов связи, которые есть у отеля (почта, SMS, push...).
    // Из него бронирование берёт каналы, на которые подписывает гостя.
    public class NotificationManager
    {
        private readonly List<INotification> _notifications = new List<INotification>();

        public IReadOnlyList<INotification> Channels => _notifications;

        public void AddNotification(INotification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _notifications.Add(notification);
        }

        // Отправить одно сообщение сразу по всем каналам
        public void Broadcast(string message, string recipient)
        {
            foreach (var notification in _notifications)
            {
                notification.Send(message, recipient);
            }
        }
    }
}