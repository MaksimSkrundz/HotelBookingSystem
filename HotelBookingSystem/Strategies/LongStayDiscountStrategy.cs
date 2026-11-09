namespace HotelsManager
{
    // Скидка за длительное проживание (это и есть "система скидок" из задания).
    // Работает поверх любого другого тарифа: сначала тот считает цену,
    // потом из неё вычитается скидка.
    //   от 7 ночей:  -10%
    //   от 14 ночей: -15%
    public class LongStayDiscountStrategy : PriceStrategyBase
    {
        private readonly IPriceStrategy _inner;

        // если тариф не указали, берём обычный
        public LongStayDiscountStrategy(IPriceStrategy? inner = null)
        {
            _inner = inner ?? new RegularPriceStrategy();
        }

        public override string Name => $"{_inner.Name} со скидкой за длительное проживание";

        // размер скидки для данного числа ночей (0.10 значит 10%)
        public static decimal GetDiscount(int days)
        {
            if (days >= 14) return 0.15m;
            if (days >= 7) return 0.10m;
            return 0m;
        }

        protected override decimal Calculate(decimal basePrice, int days, DateTime checkInDate)
        {
            var price = _inner.CalculatePrice(basePrice, days, checkInDate);
            return price * (1m - GetDiscount(days));
        }
    }
}