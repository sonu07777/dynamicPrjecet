using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IBranchService
{
    Task<List<BranchDTO>> GetBranchesAsync(string? companyId = null);
    Task<BranchDTO?> GetBranchAsync(string id);
    Task<ServiceResult<BranchDTO>> CreateBranchAsync(CreateBranchDTO createDto);
    Task<ServiceResult> UpdateBranchAsync(string id, CreateBranchDTO updateDto);
    Task<ServiceResult> DeleteBranchAsync(string id);
}
