using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IBranchService
{
    Task<List<BranchDTO>> GetBranchesAsync(int? companyId = null);
    Task<BranchDTO?> GetBranchAsync(int id);
    Task<ServiceResult<BranchDTO>> CreateBranchAsync(CreateBranchDTO createDto);
    Task<ServiceResult> UpdateBranchAsync(int id, CreateBranchDTO updateDto);
    Task<ServiceResult> DeleteBranchAsync(int id);
}
