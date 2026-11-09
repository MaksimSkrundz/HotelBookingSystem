namespace HotelsManager
{
    // Фабрика номеров (паттерн Factory)
    public class RoomFactory
    {
        public IRoom CreateRoom(RoomType type, string roomNumber)
        {
            return type switch
            {
                RoomType.Standard => CreateStandardRoom(roomNumber),
                RoomType.Deluxe => CreateDeluxeRoom(roomNumber),
                RoomType.Suite => CreateSuite(roomNumber),
                RoomType.Family => CreateFamilyRoom(roomNumber),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Неизвестный тип номера.")
            };
        }

        public IRoom CreateStandardRoom(string roomNumber)
        {
            return new StandardRoom(roomNumber);
        }

        public IRoom CreateDeluxeRoom(string roomNumber)
        {
            return new DeluxeRoom(roomNumber);
        }

        public IRoom CreateSuite(string roomNumber)
        {
            return new Suite(roomNumber);
        }

        public IRoom CreateFamilyRoom(string roomNumber)
        {
            return new FamilyRoom(roomNumber);
        }
    }
}