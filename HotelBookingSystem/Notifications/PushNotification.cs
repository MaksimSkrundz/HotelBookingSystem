namespace HotelsManager
{
    public class PushNotification : INotification
    {
        public NotificationType Type => NotificationType.Push;

        public bool Send(string message, string recipient)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                return false;
            }

            Console.WriteLine($"🔔 Push для {recipient}: {message}");
            return true;
        }
    }
}