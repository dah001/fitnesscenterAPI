using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Classes;

namespace FitnessCenterr.Core.Services.Interfaces;

public interface IClassService
{
    Task<PagedResult<ClassDto>> GetAllAsync(int page, int pageSize, string? search, string? sortBy);
    Task<ClassDto?> GetByIdAsync(int id);
    Task<ClassDto> CreateAsync(CreateClassDto dto);
    Task<bool> UpdateAsync(int id, UpdateClassDto dto);
    Task<bool> DeleteAsync(int id);
}
