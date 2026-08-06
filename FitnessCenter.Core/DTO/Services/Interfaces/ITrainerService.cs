using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Trainers;

namespace FitnessCenterr.Core.Services.Interfaces;

public interface ITrainerService
{
    Task<PagedResult<TrainerDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<TrainerDto?> GetByIdAsync(int id);
    Task<TrainerDto> CreateAsync(CreateTrainerDto dto);
    Task<bool> UpdateAsync(int id, UpdateTrainerDto dto);
    Task<bool> DeleteAsync(int id);
}
