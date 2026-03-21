using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface IReservationRepository
{
    Task<Guests?> GetGuestByPhoneAsync(string phone);
    Task AddGuestAsync(Guests guest);
    Task UpdateGuestAsync(Guests guest);
    Task<RestaurantTables?> GetTableByIdAsync(long tableId);
    Task<bool> HasOverlappingReservationAsync(long tableId, DateTime startTime, DateTime endTime);
    Task AddReservationAsync(Reservations reservation);
    Task<IReadOnlyList<Reservations>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<IReadOnlyList<RestaurantTables>> GetAvailableTablesAsync(DateTime startTime, DateTime endTime, int partySize);
}
