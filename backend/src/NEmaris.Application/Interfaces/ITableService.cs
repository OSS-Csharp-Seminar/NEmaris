using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface ITableService
{
    Task<IReadOnlyList<TableDto>> GetAllAsync();
    Task<TableDto> GetByIdAsync(long id);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long id, UpdateTableDto dto);
    Task<TableDto> ChangeGuestCountAsync(long id, int change);
    Task<TableDto> MarkOccupiedAsync(long id, string waiterUserId);
    Task DeleteTableAsync(long id);
}
