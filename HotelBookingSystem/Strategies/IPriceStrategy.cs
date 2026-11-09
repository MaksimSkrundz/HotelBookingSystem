namespace HotelsManager
{
    // Общий вид любого тарифа (паттерн Strategy)
    public interface IPriceStrategy
    {
        string Name { get; }

        decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate);
    }
}