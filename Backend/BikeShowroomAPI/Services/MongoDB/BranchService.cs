using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class BranchService : IBranchService
{
    private readonly IMongoCollection<Branch> _branches;
    private readonly IMongoCollection<Company> _companies;

    public BranchService(MongoDbContext context)
    {
        _branches = context.Branches;
        _companies = context.Companies;
    }

    public async Task<List<BranchDTO>> GetBranchesAsync(string? companyId = null)
    {
        var filter = Builders<Branch>.Filter.Empty;
        if (!string.IsNullOrEmpty(companyId))
        {
            filter = Builders<Branch>.Filter.Eq(b => b.CompanyId, companyId);
        }

        var branches = await _branches.Find(filter).ToListAsync();
        return branches.Select(ToDto).ToList();
    }

    public async Task<BranchDTO?> GetBranchAsync(string id)
    {
        var branch = await _branches.Find(b => b.Id == id).FirstOrDefaultAsync();
        return branch == null ? null : ToDto(branch);
    }

    public async Task<ServiceResult<BranchDTO>> CreateBranchAsync(CreateBranchDTO createDto)
    {
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId).AnyAsync();
        if (!companyExists)
            return ServiceResult<BranchDTO>.Fail("Company not found", ServiceResultStatus.NotFound);

        var branch = new Branch
        {
            CompanyId = createDto.CompanyId,
            Name = createDto.Name,
            Code = createDto.Code,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            ZipCode = createDto.ZipCode,
            Phone = createDto.Phone,
            Email = createDto.Email
        };

        await _branches.InsertOneAsync(branch);
        return ServiceResult<BranchDTO>.Ok(ToDto(branch));
    }

    public async Task<ServiceResult> UpdateBranchAsync(string id, CreateBranchDTO updateDto)
    {
        var branch = await _branches.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (branch == null)
            return ServiceResult.Fail("Branch not found", ServiceResultStatus.NotFound);

        var companyExists = await _companies.Find(c => c.Id == updateDto.CompanyId).AnyAsync();
        if (!companyExists)
            return ServiceResult.Fail("Company not found", ServiceResultStatus.NotFound);

        branch.Name = updateDto.Name;
        branch.Code = updateDto.Code;
        branch.Address = updateDto.Address;
        branch.City = updateDto.City;
        branch.State = updateDto.State;
        branch.ZipCode = updateDto.ZipCode;
        branch.Phone = updateDto.Phone;
        branch.Email = updateDto.Email;
        branch.CompanyId = updateDto.CompanyId;
        branch.UpdatedAt = DateTime.UtcNow;

        await _branches.ReplaceOneAsync(b => b.Id == id, branch);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteBranchAsync(string id)
    {
        var branch = await _branches.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (branch == null)
            return ServiceResult.Fail("Branch not found", ServiceResultStatus.NotFound);

        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;
        await _branches.ReplaceOneAsync(b => b.Id == id, branch);

        return ServiceResult.Ok();
    }

    private static BranchDTO ToDto(Branch b) => new()
    {
        Id = b.Id,
        CompanyId = b.CompanyId,
        Name = b.Name,
        Code = b.Code,
        Address = b.Address,
        City = b.City,
        State = b.State,
        ZipCode = b.ZipCode,
        Phone = b.Phone,
        Email = b.Email,
        IsActive = b.IsActive
    };
}