using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Repositories;

public class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly AppDbContext _context;

    public MenuCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuCategory>> GetAllAsync()
    {
        return await _context.MenuCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<MenuCategory?> GetByIdAsync(long id)
    {
        return await _context.MenuCategories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(MenuCategory category)
    {
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuCategory category)
    {
        _context.MenuCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MenuCategory category)
    {
        _context.MenuCategories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
    {
        return await _context.MenuCategories.AnyAsync(c =>
            c.Name == name &&
            (!excludeId.HasValue || c.Id != excludeId.Value));
    }
}