using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public class BranchService : IBranchService
{
    private readonly BikeShowroomContext _context;

    public BranchService(BikeShowroomContext context)
    {
        _context = context;
    }

    public async Task<List<BranchDTO>> GetBranchesAsync(int? companyId = null)
    {
        var query = _context.Branches.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(b => b.CompanyId == companyId.Value);
        }

        return await query
            .Select(b => ToDto(b))
            .ToListAsync();
    }

    public async Task<BranchDTO?> GetBranchAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        return branch == null ? null : ToDto(branch);
    }

    public async Task<ServiceResult<BranchDTO>> CreateBranchAsync(CreateBranchDTO createDto)
    {
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

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return ServiceResult<BranchDTO>.Ok(ToDto(branch));
    }

    public async Task<ServiceResult> UpdateBranchAsync(int id, CreateBranchDTO updateDto)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
            return ServiceResult.Fail("Branch not found", ServiceResultStatus.NotFound);

        branch.Name = updateDto.Name;
        branch.Code = updateDto.Code;
        branch.Address = updateDto.Address;
        branch.City = updateDto.City;
        branch.State = updateDto.State;
        branch.ZipCode = updateDto.ZipCode;
        branch.Phone = updateDto.Phone;
        branch.Email = updateDto.Email;
        branch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteBranchAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
            return ServiceResult.Fail("Branch not found", ServiceResultStatus.NotFound);

        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

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
