namespace HotelsManager
{
    // Общая основа для всех номеров: здесь то, что одинаково у каждого типа.
    // Отличия (цена, вместимость, описание) задают сами классы-номера.
    public abstract class RoomBase : IRoom
    {
        public string RoomNumber { get; }
        public abstract RoomType Type { get; }
        public abstract decimal BasePrice { get; }
        public abstract int Capacity { get; }
        public bool IsAvailable { get; set; } = true;

        // Проверка номера комнаты сделана один раз здесь,
        // поэтому ни один тип номера не может создаться с пустым номером.
        protected RoomBase(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                throw new ArgumentException("Номер комнаты не может быть пустым.", nameof(roomNumber));
            }

            RoomNumber = roomNumber.Trim();
        }

        public abstract string GetDescription();

        // Простая стоимость проживания без скидок и надбавок: цена за ночь * число ночей.
        // Тарифы со скидками и надбавками считают стратегии (папка Strategies).
        public virtual decimal CalculateTotalPrice(int days)
        {
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), "Количество ночей должно быть больше нуля.");
            }

            return BasePrice * days;
        }
    }
}