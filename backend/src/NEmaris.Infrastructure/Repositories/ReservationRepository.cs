using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guests?> GetGuestByPhoneAsync(string phone)
    {
        return await _context.Guests.FirstOrDefaultAsync(g => g.Phone == phone);
    }

    public async Task AddGuestAsync(Guests guest)
    {
        await _context.Guests.AddAsync(guest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGuestAsync(Guests guest)
    {
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();
    }

    public async Task<RestaurantTables?> GetTableByIdAsync(long tableId)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.Id == tableId);
    }

    public async Task<RestaurantTables?> GetTableByNumberAsync(string tableNumber)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.TableNumber == tableNumber);
    }

    public async Task<bool> HasOverlappingReservationAsync(long tableId, DateTime startTime, DateTime endTime, long? excludeReservationId = null)
    {
        return await _context.Reservations.AnyAsync(r =>
            r.TableId == tableId &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.NoShow &&
            r.StartTime < endTime &&
            startTime < r.EndTime &&
            (!excludeReservationId.HasValue || r.Id != excludeReservationId.Value));
    }

    public async Task AddReservationAsync(Reservations reservation)
    {
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Reservations>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate)
    {
        var query = _context.Reservations
            .AsNoTracking()
            .Include(r => r.Guest)
            .Include(r => r.Table)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(r => r.ReservationDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.ReservationDate <= toDate.Value);

        return await query
            .OrderBy(r => r.StartTime)
            .ToListAsync();
    }

    private static readonly TimeSpan LiveReservationLeadTime = TimeSpan.FromMinutes(30);

    public async Task<HashSet<long>> GetTableIdsWithLiveReservationAsync(DateTime nowUtc)
    {
        var leadCutoff = nowUtc + LiveReservationLeadTime;
        var ids = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Late) &&
                r.StartTime <= leadCutoff &&
                nowUtc < r.EndTime)
            .Select(r => r.TableId)
            .Distinct()
            .ToListAsync();
        return ids.ToHashSet();
    }

    public async Task<IReadOnlyList<RestaurantTables>> GetAvailableTablesAsync(DateTime startTime, DateTime endTime, int partySize)
    {
        var reservedTableIds = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Status != ReservationStatus.Cancelled &&
                r.Status != ReservationStatus.NoShow &&
                r.StartTime < endTime &&
                startTime < r.EndTime)
            .Select(r => r.TableId)
            .Distinct()
            .ToListAsync();

        var applyLiveStatus = startTime <= DateTime.UtcNow.AddHours(2);

        return await _context.Tables
            .AsNoTracking()
            .Where(t => t.Capacity >= partySize)
            .Where(t => !reservedTableIds.Contains(t.Id))
            .Where(t => !applyLiveStatus || t.Status == TableStatus.Available)
            .OrderBy(t => t.Capacity)
            .ThenBy(t => t.TableNumber)
            .ToListAsync();
    }

    public async Task<Reservations?> GetReservationByIdAsync(long id)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IReadOnlyList<Reservations>> GetReservationsByPhoneAsync(string phone)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Guest)
            .Include(r => r.Table)
            .Where(r => r.Guest.Phone == phone)
            .OrderBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Reservations>> GetUpcomingReservationsByPhoneAndLastNameAsync(
        string phone, string lastName, DateTime asOfUtc)
    {
        var normalizedLastName = lastName.ToLower();

        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Guest)
            .Include(r => r.Table)
            .Where(r =>
                r.Guest.Phone == phone &&
                r.Guest.LastName.ToLower() == normalizedLastName &&
                (r.Status == ReservationStatus.Active ||
                 r.Status == ReservationStatus.Late ||
                 r.Status == ReservationStatus.Seated) &&
                r.EndTime > asOfUtc)
            .OrderBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<Reservations?> GetActiveReservationForTableCoveringAsync(long tableId, DateTime asOfUtc)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Table)
            .Where(r =>
                r.TableId == tableId &&
                (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Late) &&
                r.StartTime <= asOfUtc &&
                asOfUtc < r.EndTime)
            .OrderBy(r => r.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateReservationAsync(Reservations reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
    }
}
