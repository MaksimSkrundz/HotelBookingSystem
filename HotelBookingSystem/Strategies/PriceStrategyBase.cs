namespace HotelsManager
{
    // Общая часть всех тарифов: проверка данных и подсчёт по ночам.
    // Каждый тариф только говорит, во сколько раз конкретная ночь дороже
    // или дешевле обычной (метод GetNightMultiplier), остальное делается здесь.
    public abstract class PriceStrategyBase : IPriceStrategy
    {
        public abstract string Name { get; }

        public decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate)
        {
            if (basePrice <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Цена за ночь должна быть больше нуля.");
            }

            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), "Количество ночей должно быть больше нуля.");
            }

            var total = Calculate(basePrice, days, checkInDate);

            // деньги округляем до копеек
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        // Считаем каждую ночь отдельно по её дате и складываем.
        // Так надбавка за выходные или сезон действует только на те ночи, которые к ним относятся.
        protected virtual decimal Calculate(decimal basePrice, int days, DateTime checkInDate)
        {
            decimal total = 0;

            for (int i = 0; i < days; i++)
            {
                var night = checkInDate.Date.AddDays(i);
                total += basePrice * GetNightMultiplier(night);
            }

            return total;
        }

        // По умолчанию ночь стоит обычную цену
        protected virtual decimal GetNightMultiplier(DateTime night)
        {
            return 1m;
        }
    }
}