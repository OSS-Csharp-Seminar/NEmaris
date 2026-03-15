using NEmaris.Application.DTO_s;

namespace NEmaris.Application.Interace_s;

public interface ITableService
{
    Task<IReadOnlyList<TableDto>> GetAllAsync();
    Task<TableDto> GetByIdAsync(long id);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long id, UpdateTableDto dto);
    Task DeleteTableAsync(long id);
}
