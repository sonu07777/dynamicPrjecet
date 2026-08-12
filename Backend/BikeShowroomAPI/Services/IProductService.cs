using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IProductService
{
    Task<List<ProductDTO>> GetProductsAsync(int? companyId = null, int? categoryId = null);
    Task<ProductDTO?> GetProductAsync(int id);
    Task<ServiceResult<ProductDTO>> CreateProductAsync(CreateProductDTO createDto);
    Task<ServiceResult> UpdateProductAsync(int id, CreateProductDTO updateDto);
    Task<ServiceResult> DeleteProductAsync(int id);
    Task<List<ProductDTO>> SearchProductsAsync(string? query = null, int? companyId = null);
}
