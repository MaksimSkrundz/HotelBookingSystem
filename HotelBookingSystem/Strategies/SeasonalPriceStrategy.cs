namespace HotelsManager
{
    // Сезонный тариф: цена зависит от времени года.
    //   20 декабря - 10 января (праздники): +50%
    //   июнь - август (высокий сезон):      +30%
    //   ноябрь и март (низкий сезон):       скидка 20%
    //   остальные месяцы:                   обычная цена
    public class SeasonalPriceStrategy : PriceStrategyBase
    {
        private const decimal HolidayMultiplier = 1.5m;
        private const decimal HighSeasonMultiplier = 1.3m;
        private const decimal LowSeasonMultiplier = 0.8m;

        public override string Name => "Сезонный тариф";

        protected override decimal GetNightMultiplier(DateTime night)
        {
            var isHoliday = (night.Month == 12 && night.Day >= 20)
                         || (night.Month == 1 && night.Day <= 10);

            if (isHoliday)
            {
                return HolidayMultiplier;
            }

            if (night.Month >= 6 && night.Month <= 8)
            {
                return HighSeasonMultiplier;
            }

            if (night.Month == 11 || night.Month == 3)
            {
                return LowSeasonMultiplier;
            }

            return 1m;
        }
    }
}