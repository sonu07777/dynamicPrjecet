using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Category> _categories;
    private readonly IMongoCollection<Company> _companies;

    public ProductService(MongoDbContext context)
    {
        _products = context.Products;
        _categories = context.Categories;
        _companies = context.Companies;
    }

    public async Task<List<ProductDTO>> GetProductsAsync(string? companyId = null, string? categoryId = null)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.IsActive, true);

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Product>.Filter.Eq(p => p.CompanyId, companyId);
        }

        if (!string.IsNullOrEmpty(categoryId))
        {
            filter &= Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        }

        var products = await _products.Find(filter).ToListAsync();

        // Get categories for lookup
        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categories.Find(c => categoryIds.Contains(c.Id)).ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => ToDto(p, categoryDict)).ToList();
    }

    public async Task<ProductDTO?> GetProductAsync(string id)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (product == null) return null;

        var category = await _categories.Find(c => c.Id == product.CategoryId).FirstOrDefaultAsync();
        return ToDto(product, new Dictionary<string, string> { [product.CategoryId] = category?.Name ?? "" });
    }

    public async Task<ServiceResult<ProductDTO>> CreateProductAsync(CreateProductDTO createDto)
    {
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<ProductDTO>.Fail("Company does not exist or is inactive. Please select a valid company.");

        var categoryExists = await _categories.Find(c => c.Id == createDto.CategoryId && c.CompanyId == createDto.CompanyId && c.IsActive).AnyAsync();
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

        await _products.InsertOneAsync(product);

        var category = await _categories.Find(c => c.Id == createDto.CategoryId).FirstOrDefaultAsync();
        return ServiceResult<ProductDTO>.Ok(ToDto(product, new Dictionary<string, string> { [product.CategoryId] = category?.Name ?? "" }));
    }

    public async Task<ServiceResult> UpdateProductAsync(string id, CreateProductDTO updateDto)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null)
            return ServiceResult.Fail("Product not found", ServiceResultStatus.NotFound);

        var categoryExists = await _categories.Find(c => c.Id == updateDto.CategoryId && c.CompanyId == product.CompanyId && c.IsActive).AnyAsync();
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

        await _products.ReplaceOneAsync(p => p.Id == id, product);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(string id)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null)
            return ServiceResult.Fail("Product not found", ServiceResultStatus.NotFound);

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _products.ReplaceOneAsync(p => p.Id == id, product);

        return ServiceResult.Ok();
    }

    public async Task<List<ProductDTO>> SearchProductsAsync(string? query = null, string? companyId = null)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.IsActive, true);

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Product>.Filter.Eq(p => p.CompanyId, companyId);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchFilter = Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(query, "i")),
                Builders<Product>.Filter.Regex(p => p.SKU, new BsonRegularExpression(query, "i")),
                Builders<Product>.Filter.Regex(p => p.Barcode, new BsonRegularExpression(query, "i"))
            );
            filter &= searchFilter;
        }

        var products = await _products.Find(filter).Limit(50).ToListAsync();

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categories.Find(c => categoryIds.Contains(c.Id)).ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => ToDto(p, categoryDict)).ToList();
    }

    private static ProductDTO ToDto(Product p, Dictionary<string, string> categoryDict)
    {
        categoryDict.TryGetValue(p.CategoryId, out var categoryName);
        return new ProductDTO
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
            CategoryName = categoryName
        };
    }
}