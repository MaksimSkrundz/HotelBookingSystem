namespace HotelsManager
{
    // Улучшенный номер с видом на море
    public class DeluxeRoom : RoomBase
    {
        public DeluxeRoom(string roomNumber) : base(roomNumber)
        {
        }

        public override RoomType Type => RoomType.Deluxe;
        public override decimal BasePrice => 4500m;
        public override int Capacity => 3;

        public override string GetDescription()
        {
            return $"Делюкс номер {RoomNumber} с видом на море";
        }
    }
}