namespace HotelsManager
{
    public class SMSNotification : INotification
    {
        public NotificationType Type => NotificationType.SMS;

        public bool Send(string message, string recipient)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                return false;
            }

            Console.WriteLine($"📱 SMS для {recipient}: {message}");
            return true;
        }
    }
}