using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IMenuCategoryService
{
    Task<IReadOnlyList<MenuCategoryDto>> GetAllAsync();
    Task<MenuCategoryDto> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateMenuCategoryDto dto);
    Task UpdateAsync(long id, UpdateMenuCategoryDto dto);
    Task DeleteAsync(long id);
}