namespace NEmaris.Application.DTOs;

public class PublicOverviewDto
{
    public int TotalTables { get; set; }
    public int OccupiedTables { get; set; }
    public int ReservedTables { get; set; }
    public int AvailableTables { get; set; }
    public int ReservationsToday { get; set; }
    public int UpcomingReservations { get; set; }
}
