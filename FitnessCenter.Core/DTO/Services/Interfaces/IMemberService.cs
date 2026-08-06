using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Members;

namespace FitnessCenterr.Core.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResult<MemberDto>> GetAllAsync(int page, int pageSize, string? search, string? sortBy);
    Task<MemberDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<bool> UpdateAsync(int id, UpdateMemberDto dto);
    Task<bool> DeleteAsync(int id);
}
