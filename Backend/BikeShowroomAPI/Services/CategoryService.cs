using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly BikeShowroomContext _context;

    public CategoryService(BikeShowroomContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDTO>> GetCategoriesAsync(int? companyId = null)
    {
        var query = _context.Categories.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);

        return await query
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CategoryDTO?> GetCategoryAsync(int id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => ToDto(c))
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO dto)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.IsActive);
        if (!companyExists)
            return ServiceResult<CategoryDTO>.Fail("Company does not exist or is inactive. Please select a valid company.");

        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.ParentCategoryId.Value && c.CompanyId == dto.CompanyId && c.IsActive);
            if (!parentExists)
                return ServiceResult<CategoryDTO>.Fail("Parent category does not exist in this company.");
        }

        var category = new Category
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return ServiceResult<CategoryDTO>.Ok(ToDto(category));
    }

    public async Task<ServiceResult> UpdateCategoryAsync(int id, CreateCategoryDTO dto)
    {
        var existingCategory = await _context.Categories.FindAsync(id);
        if (existingCategory == null)
            return ServiceResult.Fail("Category not found", ServiceResultStatus.NotFound);

        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.ParentCategoryId.Value && c.CompanyId == existingCategory.CompanyId && c.IsActive);
            if (!parentExists)
                return ServiceResult.Fail("Parent category does not exist in this company.");
        }

        existingCategory.Name = dto.Name;
        existingCategory.Description = dto.Description;
        existingCategory.ParentCategoryId = dto.ParentCategoryId;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return ServiceResult.Fail("Category not found", ServiceResultStatus.NotFound);

        category.IsActive = false;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private static CategoryDTO ToDto(Category c) => new()
    {
        Id = c.Id,
        CompanyId = c.CompanyId,
        Name = c.Name,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        IsActive = c.IsActive
    };
}
