namespace HotelsManager
{
    // Дополнительный тип номера (пункт 5 задания): семейный номер.
    // Значение RoomType.Family в перечислении уже было, не хватало самого класса.
    public class FamilyRoom : RoomBase
    {
        public FamilyRoom(string roomNumber) : base(roomNumber)
        {
        }

        public override RoomType Type => RoomType.Family;
        public override decimal BasePrice => 5500m;
        public override int Capacity => 5;

        public override string GetDescription()
        {
            return $"Семейный номер {RoomNumber} с двумя спальнями и детской зоной";
        }
    }
}