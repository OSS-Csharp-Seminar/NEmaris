using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;

namespace NEmaris.Application.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly IMenuCategoryRepository _menuCategoryRepository;

    public MenuCategoryService(IMenuCategoryRepository menuCategoryRepository)
    {
        _menuCategoryRepository = menuCategoryRepository;
    }

    public async Task<IReadOnlyList<MenuCategoryDto>> GetAllAsync()
    {
        var categories = await _menuCategoryRepository.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<MenuCategoryDto> GetByIdAsync(long id)
    {
        var category = await _menuCategoryRepository.GetByIdAsync(id);
        if (category is null)
            throw new KeyNotFoundException("Menu category not found.");

        return MapToDto(category);
    }

    public async Task<long> CreateAsync(CreateMenuCategoryDto dto)
    {
        var categoryNameTaken = await _menuCategoryRepository.ExistsByNameAsync(dto.Name);
        if (categoryNameTaken)
            throw new InvalidOperationException("Menu category name already exists.");

        var category = new MenuCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _menuCategoryRepository.AddAsync(category);
        return category.Id;
    }

    public async Task UpdateAsync(long id, UpdateMenuCategoryDto dto)
    {
        var category = await _menuCategoryRepository.GetByIdAsync(id);
        if (category is null)
            throw new KeyNotFoundException("Menu category not found.");

        var categoryNameTaken = await _menuCategoryRepository.ExistsByNameAsync(dto.Name, id);
        if (categoryNameTaken)
            throw new InvalidOperationException("Menu category name already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.DisplayOrder = dto.DisplayOrder;
        category.UpdatedAt = DateTime.UtcNow;

        await _menuCategoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(long id)
    {
        var category = await _menuCategoryRepository.GetByIdAsync(id);
        if (category is null)
            throw new KeyNotFoundException("Menu category not found.");

        await _menuCategoryRepository.DeleteAsync(category);
    }

    private static MenuCategoryDto MapToDto(MenuCategory category)
    {
        return new MenuCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}