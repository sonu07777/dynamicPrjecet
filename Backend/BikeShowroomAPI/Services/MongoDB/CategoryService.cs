using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class CategoryService : ICategoryService
{
    private readonly IMongoCollection<Category> _categories;
    private readonly IMongoCollection<Company> _companies;

    public CategoryService(MongoDbContext context)
    {
        _categories = context.Categories;
        _companies = context.Companies;
    }

    public async Task<List<CategoryDTO>> GetCategoriesAsync(string? companyId = null)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.IsActive, true);

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Category>.Filter.Eq(c => c.CompanyId, companyId);
        }

        var categories = await _categories.Find(filter).SortBy(c => c.Name).ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDTO?> GetCategoryAsync(string id)
    {
        var category = await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        return category == null ? null : ToDto(category);
    }

    public async Task<ServiceResult<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO dto)
    {
        var companyExists = await _companies.Find(c => c.Id == dto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<CategoryDTO>.Fail("Company does not exist or is inactive. Please select a valid company.");

        if (!string.IsNullOrEmpty(dto.ParentCategoryId))
        {
            var parentExists = await _categories.Find(c =>
                c.Id == dto.ParentCategoryId && c.CompanyId == dto.CompanyId && c.IsActive).AnyAsync();
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

        await _categories.InsertOneAsync(category);
        return ServiceResult<CategoryDTO>.Ok(ToDto(category));
    }

    public async Task<ServiceResult> UpdateCategoryAsync(string id, CreateCategoryDTO dto)
    {
        var existingCategory = await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (existingCategory == null)
            return ServiceResult.Fail("Category not found", ServiceResultStatus.NotFound);

        if (!string.IsNullOrEmpty(dto.ParentCategoryId))
        {
            var parentExists = await _categories.Find(c =>
                c.Id == dto.ParentCategoryId && c.CompanyId == existingCategory.CompanyId && c.IsActive).AnyAsync();
            if (!parentExists)
                return ServiceResult.Fail("Parent category does not exist in this company.");
        }

        existingCategory.Name = dto.Name;
        existingCategory.Description = dto.Description;
        existingCategory.ParentCategoryId = dto.ParentCategoryId;

        await _categories.ReplaceOneAsync(c => c.Id == id, existingCategory);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteCategoryAsync(string id)
    {
        var category = await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (category == null)
            return ServiceResult.Fail("Category not found", ServiceResultStatus.NotFound);

        category.IsActive = false;
        await _categories.ReplaceOneAsync(c => c.Id == id, category);

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