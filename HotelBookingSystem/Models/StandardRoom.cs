namespace HotelsManager
{
    // Самый простой и дешёвый номер
    public class StandardRoom : RoomBase
    {
        public StandardRoom(string roomNumber) : base(roomNumber)
        {
        }

        public override RoomType Type => RoomType.Standard;
        public override decimal BasePrice => 2500m;
        public override int Capacity => 2;

        public override string GetDescription()
        {
            return $"Стандартный номер {RoomNumber} для двух гостей";
        }
    }
}