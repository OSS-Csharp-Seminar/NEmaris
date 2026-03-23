using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface IMenuCategoryRepository
{
    Task<IReadOnlyList<MenuCategory>> GetAllAsync();
    Task<MenuCategory?> GetByIdAsync(long id);
    Task AddAsync(MenuCategory category);
    Task UpdateAsync(MenuCategory category);
    Task DeleteAsync(MenuCategory category);
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
}