using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Members;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Services.MySQL;

public class MemberService : IMemberService
{
    private readonly AppDbContext _db;
    public MemberService(AppDbContext db) => _db = db;

    public async Task<PagedResult<MemberDto>> GetAllAsync(int page, int pageSize, string? search, string? sortBy)
    {
        var query = _db.Members.Include(m => m.Trainer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search) || (m.Email != null && m.Email.Contains(search)));

        query = sortBy switch
        {
            "name"  => query.OrderBy(m => m.Name),
            "email" => query.OrderBy(m => m.Email),
            _       => query.OrderBy(m => m.MemberID)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberDto
            {
                MemberID    = m.MemberID,
                Name        = m.Name,
                Email       = m.Email,
                TrainerID   = m.TrainerID,
                TrainerName = m.Trainer != null ? m.Trainer.Name : null
            })
            .ToListAsync();

        return new PagedResult<MemberDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        var m = await _db.Members.Include(m => m.Trainer).FirstOrDefaultAsync(m => m.MemberID == id);
        if (m == null) return null;
        return new MemberDto
        {
            MemberID    = m.MemberID,
            Name        = m.Name,
            Email       = m.Email,
            TrainerID   = m.TrainerID,
            TrainerName = m.Trainer?.Name
        };
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var member = new Member { Name = dto.Name, Email = dto.Email, TrainerID = dto.TrainerID };
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return new MemberDto
        {
            MemberID  = member.MemberID,
            Name      = member.Name,
            Email     = member.Email,
            TrainerID = member.TrainerID
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return false;
        member.Name      = dto.Name;
        member.Email     = dto.Email;
        member.TrainerID = dto.TrainerID;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return false;
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }
}
