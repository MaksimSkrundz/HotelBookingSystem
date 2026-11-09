namespace HotelsManager
{
    // Условия поиска номера. Заполняется только то, что нужно:
    // всё, что оставлено пустым, в поиске не учитывается.
    //   new RoomSearchCriteria { MinCapacity = 3, MaxPrice = 6000m }
    public class RoomSearchCriteria
    {
        public RoomType? Type { get; init; }
        public int? MinCapacity { get; init; }
        public decimal? MaxPrice { get; init; }

        public bool Matches(IRoom room)
        {
            if (Type.HasValue && room.Type != Type.Value)
            {
                return false;
            }

            if (MinCapacity.HasValue && room.Capacity < MinCapacity.Value)
            {
                return false;
            }

            if (MaxPrice.HasValue && room.BasePrice > MaxPrice.Value)
            {
                return false;
            }

            return true;
        }
    }
}