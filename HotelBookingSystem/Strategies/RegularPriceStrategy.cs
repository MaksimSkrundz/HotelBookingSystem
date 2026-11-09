namespace HotelsManager
{
    // Обычный тариф: каждая ночь по базовой цене, без надбавок и скидок
    public class RegularPriceStrategy : PriceStrategyBase
    {
        public override string Name => "Обычный тариф";
    }
}