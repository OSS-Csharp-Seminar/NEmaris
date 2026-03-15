using NEmaris.Application.DTO_s;
using NEmaris.Application.Interace_s;
using NEmaris.Application.Interfaces_s;
using NEmaris.Domain.Entities;

namespace NEmaris.Application.Service_s;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;

    public TableService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<IReadOnlyList<TableDto>> GetAllAsync()
    {
        var tables = await _tableRepository.GetAllAsync();
        return tables.Select(MapToDto).ToList();
    }

    public async Task<TableDto> GetByIdAsync(long id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        return MapToDto(table);
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var tableNumberTaken = await _tableRepository.ExistsByTableNumberAsync(dto.TableNumber);
        if (tableNumberTaken)
            throw new InvalidOperationException("Table number already exists.");

        var table = new RestaurantTables
        {
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            Zone = dto.Zone,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tableRepository.AddAsync(table);
        return table.Id;
    }

    public async Task UpdateTableAsync(long id, UpdateTableDto dto)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        var tableNumberTaken = await _tableRepository.ExistsByTableNumberAsync(dto.TableNumber, id);
        if (tableNumberTaken)
            throw new InvalidOperationException("Table number already exists.");

        table.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Zone = dto.Zone;
        table.Status = dto.Status;
        table.UpdatedAt = DateTime.UtcNow;

        await _tableRepository.UpdateAsync(table);
    }

    public async Task DeleteTableAsync(long id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        await _tableRepository.DeleteAsync(table);
    }

    private static TableDto MapToDto(RestaurantTables table)
    {
        return new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            Zone = table.Zone,
            Status = table.Status,
            CreatedAt = table.CreatedAt,
            UpdatedAt = table.UpdatedAt
        };
    }
}
