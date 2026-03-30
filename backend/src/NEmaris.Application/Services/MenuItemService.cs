using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;

namespace NEmaris.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;

    public MenuItemService(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();

        return items.Select(m => new MenuItemDto
        {
            Id = m.Id,
            CategoryId = m.CategoryId,
            Name = m.Name,
            Description = m.Description,
            Price = m.Price,
            Status = (int)m.Status,
            IsAvailable = m.IsAvailable,
            Sku = m.Sku,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }

    public async Task<MenuItemDto> GetByIdAsync(long id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Menu item not found");

        return new MenuItemDto
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Status = (int)item.Status,
            IsAvailable = item.IsAvailable,
            Sku = item.Sku,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    public async Task<long> CreateAsync(CreateMenuItemDto dto)
    {
        var item = new MenuItem
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Status = dto.Status,
            IsAvailable = dto.IsAvailable,
            Sku = dto.Sku,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(item);

        return item.Id;
    }

    public async Task UpdateAsync(long id, UpdateMenuItemDto dto)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Menu item not found");

        item.CategoryId = dto.CategoryId;
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.Status = dto.Status;
        item.IsAvailable = dto.IsAvailable;
        item.Sku = dto.Sku;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item);
    }

    public async Task DeleteAsync(long id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Menu item not found");

        await _repository.DeleteAsync(item);
    }
}