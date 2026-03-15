using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces_s;
using NEmaris.Domain.Entities;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Repositories;

public class TableRepository : ITableRepository
{
    private readonly AppDbContext _context;

    public TableRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RestaurantTables>> GetAllAsync()
    {
        return await _context.Tables
            .AsNoTracking()
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    public async Task AddAsync(RestaurantTables table)
    {
        await _context.Tables.AddAsync(table);
        await _context.SaveChangesAsync();
    }

    public async Task<RestaurantTables?> GetByIdAsync(long id)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsByTableNumberAsync(string tableNumber, long? excludeId = null)
    {
        return await _context.Tables.AnyAsync(t =>
            t.TableNumber == tableNumber &&
            (!excludeId.HasValue || t.Id != excludeId.Value));
    }

    public async Task UpdateAsync(RestaurantTables table)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RestaurantTables table)
    {
        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();
    }
}
