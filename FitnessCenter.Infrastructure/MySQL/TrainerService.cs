using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Trainers;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Services.MySQL;

public class TrainerService : ITrainerService
{
    private readonly AppDbContext _db;
    public TrainerService(AppDbContext db) => _db = db;

    public async Task<PagedResult<TrainerDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _db.Trainers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TrainerDto
            {
                TrainerID   = t.TrainerID,
                Name        = t.Name,
                ClassCount  = t.Classes.Count,
                MemberCount = t.Members.Count
            })
            .ToListAsync();

        return new PagedResult<TrainerDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<TrainerDto?> GetByIdAsync(int id)
    {
        var t = await _db.Trainers.Include(t => t.Classes).Include(t => t.Members).FirstOrDefaultAsync(t => t.TrainerID == id);
        if (t == null) return null;
        return new TrainerDto { TrainerID = t.TrainerID, Name = t.Name, ClassCount = t.Classes.Count, MemberCount = t.Members.Count };
    }

    public async Task<TrainerDto> CreateAsync(CreateTrainerDto dto)
    {
        var trainer = new Trainer { Name = dto.Name };
        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync();
        return new TrainerDto { TrainerID = trainer.TrainerID, Name = trainer.Name };
    }

    public async Task<bool> UpdateAsync(int id, UpdateTrainerDto dto)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer == null) return false;
        trainer.Name = dto.Name;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer == null) return false;
        _db.Trainers.Remove(trainer);
        await _db.SaveChangesAsync();
        return true;
    }
}