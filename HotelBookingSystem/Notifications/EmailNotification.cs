namespace HotelsManager
{
    public class EmailNotification : INotification
    {
        public NotificationType Type => NotificationType.Email;

        public bool Send(string message, string recipient)
        {
            // без адреса или без @ письмо отправить некуда
            if (string.IsNullOrWhiteSpace(recipient) || !recipient.Contains('@'))
            {
                return false;
            }

            Console.WriteLine($"📧 Email для {recipient}: {message}");
            return true;
        }
    }
}