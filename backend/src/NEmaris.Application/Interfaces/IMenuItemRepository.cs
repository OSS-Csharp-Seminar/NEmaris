using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByIdAsync(long id);
    Task AddAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(MenuItem item);
}