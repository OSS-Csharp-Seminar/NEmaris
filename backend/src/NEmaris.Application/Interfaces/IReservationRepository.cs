using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface IReservationRepository
{
    Task<Guests?> GetGuestByPhoneAsync(string phone);
    Task AddGuestAsync(Guests guest);
    Task UpdateGuestAsync(Guests guest);
    Task<RestaurantTables?> GetTableByIdAsync(long tableId);
    Task<RestaurantTables?> GetTableByNumberAsync(string tableNumber);
    Task<bool> HasOverlappingReservationAsync(long tableId, DateTime startTime, DateTime endTime, long? excludeReservationId = null);
    Task AddReservationAsync(Reservations reservation);
    Task<IReadOnlyList<Reservations>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<IReadOnlyList<RestaurantTables>> GetAvailableTablesAsync(DateTime startTime, DateTime endTime, int partySize);
    Task<HashSet<long>> GetTableIdsWithLiveReservationAsync(DateTime nowUtc);
    Task<IReadOnlyDictionary<long, DateTime>> GetUpcomingReservationsByTableAsync(DateTime nowUtc, TimeSpan window);
    Task<DateTime?> GetNextActiveReservationStartAsync(long tableId, DateTime nowUtc, TimeSpan window);
    Task<Reservations?> GetReservationByIdAsync(long id);
    Task<IReadOnlyList<Reservations>> GetReservationsByPhoneAsync(string phone);
    Task<IReadOnlyList<Reservations>> GetUpcomingReservationsByPhoneAndLastNameAsync(string phone, string lastName, DateTime asOfUtc);
    Task<Reservations?> GetActiveReservationForTableCoveringAsync(long tableId, DateTime asOfUtc);
    Task UpdateReservationAsync(Reservations reservation);
}
