namespace HotelsManager
{
    // Выходной тариф: ночи с пятницы на субботу и с субботы на воскресенье дороже на 20%
    public class WeekendPriceStrategy : PriceStrategyBase
    {
        private const decimal WeekendSurcharge = 0.20m;

        public override string Name => "Выходной тариф";

        protected override decimal GetNightMultiplier(DateTime night)
        {
            var isWeekendNight = night.DayOfWeek == DayOfWeek.Friday
                              || night.DayOfWeek == DayOfWeek.Saturday;

            return isWeekendNight ? 1m + WeekendSurcharge : 1m;
        }
    }
}