using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IMenuItemService
{
    Task<IReadOnlyList<MenuItemDto>> GetAllAsync();
    Task<IReadOnlyList<PublicMenuItemDto>> GetPublicMenuAsync();
    Task<MenuItemDto> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateMenuItemDto dto);
    Task UpdateAsync(long id, UpdateMenuItemDto dto);
    Task DeleteAsync(long id);
}