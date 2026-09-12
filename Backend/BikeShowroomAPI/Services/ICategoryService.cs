using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ICategoryService
{
    Task<List<CategoryDTO>> GetCategoriesAsync(string? companyId = null);
    Task<CategoryDTO?> GetCategoryAsync(string id);
    Task<ServiceResult<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO dto);
    Task<ServiceResult> UpdateCategoryAsync(string id, CreateCategoryDTO dto);
    Task<ServiceResult> DeleteCategoryAsync(string id);
}
