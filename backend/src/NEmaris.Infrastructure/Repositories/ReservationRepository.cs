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

    public async Task<bool> HasOverlappingReservationAsync(long tableId, DateTime startTime, DateTime endTime)
    {
        return await _context.Reservations.AnyAsync(r =>
            r.TableId == tableId &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.NoShow &&
            r.StartTime < endTime &&
            startTime < r.EndTime);
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

        return await _context.Tables
            .AsNoTracking()
            .Where(t => t.Capacity >= partySize)
            .Where(t => !reservedTableIds.Contains(t.Id))
            .OrderBy(t => t.Capacity)
            .ThenBy(t => t.TableNumber)
            .ToListAsync();
    }
}
