using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface ITableRepository
{
    Task<IReadOnlyList<RestaurantTables>> GetAllAsync();
    Task AddAsync(RestaurantTables table);
    Task<RestaurantTables?> GetByIdAsync(long id);
    Task<bool> ExistsByTableNumberAsync(string tableNumber, long? excludeId = null);
    Task UpdateAsync(RestaurantTables table);
    Task DeleteAsync(RestaurantTables table);
}
