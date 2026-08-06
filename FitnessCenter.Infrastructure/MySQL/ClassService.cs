using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Classes;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Services.MySQL;

public class ClassService : IClassService
{
    private readonly AppDbContext _db;
    public ClassService(AppDbContext db) => _db = db;

    public async Task<PagedResult<ClassDto>> GetAllAsync(int page, int pageSize, string? search, string? sortBy)
    {
        var query = _db.Classes
            .Include(c => c.Trainer)
            .Include(c => c.Hall)
            .Include(c => c.Location)
            .Include(c => c.ClassBookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search));

        query = sortBy switch
        {
            "date" => query.OrderBy(c => c.ClassDate),
            "name" => query.OrderBy(c => c.Name),
            _      => query.OrderBy(c => c.ClassID)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClassDto
            {
                ClassID      = c.ClassID,
                Name         = c.Name,
                TrainerID    = c.TrainerID,
                TrainerName  = c.Trainer.Name,
                ClassDate    = c.ClassDate,
                HallID       = c.HallID,
                HallName     = c.Hall != null ? c.Hall.Name : null,
                LocationID   = c.LocationID,
                City         = c.Location != null ? c.Location.City : null,
                BookingCount = c.ClassBookings.Count
            })
            .ToListAsync();

        return new PagedResult<ClassDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<ClassDto?> GetByIdAsync(int id)
    {
        var c = await _db.Classes
            .Include(c => c.Trainer)
            .Include(c => c.Hall)
            .Include(c => c.Location)
            .Include(c => c.ClassBookings)
            .FirstOrDefaultAsync(c => c.ClassID == id);
        if (c == null) return null;
        return new ClassDto
        {
            ClassID      = c.ClassID,
            Name         = c.Name,
            TrainerID    = c.TrainerID,
            TrainerName  = c.Trainer.Name,
            ClassDate    = c.ClassDate,
            HallID       = c.HallID,
            HallName     = c.Hall?.Name,
            LocationID   = c.LocationID,
            City         = c.Location?.City,
            BookingCount = c.ClassBookings.Count
        };
    }

    public async Task<ClassDto> CreateAsync(CreateClassDto dto)
    {
        var cls = new Class
        {
            Name       = dto.Name,
            TrainerID  = dto.TrainerID,
            ClassDate  = dto.ClassDate,
            HallID     = dto.HallID,
            LocationID = dto.LocationID
        };
        _db.Classes.Add(cls);
        await _db.SaveChangesAsync();
        var trainer = await _db.Trainers.FindAsync(dto.TrainerID);
        return new ClassDto
        {
            ClassID     = cls.ClassID,
            Name        = cls.Name,
            TrainerID   = cls.TrainerID,
            TrainerName = trainer?.Name ?? "",
            ClassDate   = cls.ClassDate
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateClassDto dto)
    {
        var cls = await _db.Classes.FindAsync(id);
        if (cls == null) return false;
        cls.Name       = dto.Name;
        cls.TrainerID  = dto.TrainerID;
        cls.ClassDate  = dto.ClassDate;
        cls.HallID     = dto.HallID;
        cls.LocationID = dto.LocationID;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cls = await _db.Classes.FindAsync(id);
        if (cls == null) return false;
        _db.Classes.Remove(cls);
        await _db.SaveChangesAsync();
        return true;
    }
}
