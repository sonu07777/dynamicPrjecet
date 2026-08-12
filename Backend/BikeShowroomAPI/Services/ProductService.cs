using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public class ProductService : IProductService
{
    private readonly BikeShowroomContext _context;

    public ProductService(BikeShowroomContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDTO>> GetProductsAsync(int? companyId = null, int? categoryId = null)
    {
        var query = _context.Products.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDTO?> GetProductAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : ToDto(product);
    }

    public async Task<ServiceResult<ProductDTO>> CreateProductAsync(CreateProductDTO createDto)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == createDto.CompanyId && c.IsActive);
        if (!companyExists)
            return ServiceResult<ProductDTO>.Fail("Company does not exist or is inactive. Please select a valid company.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == createDto.CategoryId && c.CompanyId == createDto.CompanyId && c.IsActive);
        if (!categoryExists)
            return ServiceResult<ProductDTO>.Fail("Category does not exist in this company. Please select a valid category.");

        var product = new Product
        {
            CompanyId = createDto.CompanyId,
            CategoryId = createDto.CategoryId,
            Name = createDto.Name,
            SKU = createDto.SKU,
            Barcode = createDto.Barcode,
            Description = createDto.Description,
            CostPrice = createDto.CostPrice,
            SellingPrice = createDto.SellingPrice,
            Unit = createDto.Unit,
            MinStockLevel = createDto.MinStockLevel,
            MaxStockLevel = createDto.MaxStockLevel,
            ImageUrl = createDto.ImageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Note: product.Category is not loaded, so build the DTO without CategoryName.
        return ServiceResult<ProductDTO>.Ok(new ProductDTO
        {
            Id = product.Id,
            CompanyId = product.CompanyId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            SKU = product.SKU,
            Barcode = product.Barcode,
            Description = product.Description,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            Unit = product.Unit,
            MinStockLevel = product.MinStockLevel,
            MaxStockLevel = product.MaxStockLevel,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive
        });
    }

    public async Task<ServiceResult> UpdateProductAsync(int id, CreateProductDTO updateDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return ServiceResult.Fail("Product not found", ServiceResultStatus.NotFound);

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == updateDto.CategoryId && c.CompanyId == product.CompanyId && c.IsActive);
        if (!categoryExists)
            return ServiceResult.Fail("Category does not exist in this company. Please select a valid category.");

        product.CategoryId = updateDto.CategoryId;
        product.Name = updateDto.Name;
        product.SKU = updateDto.SKU;
        product.Barcode = updateDto.Barcode;
        product.Description = updateDto.Description;
        product.CostPrice = updateDto.CostPrice;
        product.SellingPrice = updateDto.SellingPrice;
        product.Unit = updateDto.Unit;
        product.MinStockLevel = updateDto.MinStockLevel;
        product.MaxStockLevel = updateDto.MaxStockLevel;
        product.ImageUrl = updateDto.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return ServiceResult.Fail("Product not found", ServiceResultStatus.NotFound);

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<List<ProductDTO>> SearchProductsAsync(string? query = null, int? companyId = null)
    {
        var productsQuery = _context.Products.AsQueryable();

        if (companyId.HasValue)
            productsQuery = productsQuery.Where(p => p.CompanyId == companyId.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query) ||
                p.SKU.Contains(query) ||
                p.Barcode.Contains(query));
        }

        return await productsQuery
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .Take(50)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    private static ProductDTO ToDto(Product p) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        CategoryId = p.CategoryId,
        Name = p.Name,
        SKU = p.SKU,
        Barcode = p.Barcode,
        Description = p.Description,
        CostPrice = p.CostPrice,
        SellingPrice = p.SellingPrice,
        Unit = p.Unit,
        MinStockLevel = p.MinStockLevel,
        MaxStockLevel = p.MaxStockLevel,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        CategoryName = p.Category.Name
    };
}
