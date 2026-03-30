using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly AppDbContext _context;

    public MenuItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItem>> GetAllAsync()
    {
        return await _context.MenuItems
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(long id)
    {
        return await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(MenuItem item)
    {
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem item)
    {
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MenuItem item)
    {
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
    }
}