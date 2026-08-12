using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ICategoryService
{
    Task<List<CategoryDTO>> GetCategoriesAsync(int? companyId = null);
    Task<CategoryDTO?> GetCategoryAsync(int id);
    Task<ServiceResult<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO dto);
    Task<ServiceResult> UpdateCategoryAsync(int id, CreateCategoryDTO dto);
    Task<ServiceResult> DeleteCategoryAsync(int id);
}
