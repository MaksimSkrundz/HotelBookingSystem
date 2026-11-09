namespace HotelsManager
{
    // Сводка по отелю: просто набор посчитанных чисел
    public record HotelStatistics(
        int TotalRooms,
        int RoomsInService,
        int OccupiedToday,
        int ConfirmedBookings,
        int PendingBookings,
        int CancelledBookings,
        decimal ConfirmedRevenue,
        decimal PendingRevenue)
    {
        public double OccupancyPercent =>
            TotalRooms == 0 ? 0 : Math.Round(100.0 * OccupiedToday / TotalRooms, 1);
    }
}