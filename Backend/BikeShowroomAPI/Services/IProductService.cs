using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IProductService
{
    Task<List<ProductDTO>> GetProductsAsync(string? companyId = null, string? categoryId = null);
    Task<ProductDTO?> GetProductAsync(string id);
    Task<ServiceResult<ProductDTO>> CreateProductAsync(CreateProductDTO createDto);
    Task<ServiceResult> UpdateProductAsync(string id, CreateProductDTO updateDto);
    Task<ServiceResult> DeleteProductAsync(string id);
    Task<List<ProductDTO>> SearchProductsAsync(string? query = null, string? companyId = null);
}
