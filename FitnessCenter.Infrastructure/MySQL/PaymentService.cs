using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Payments;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Services.MySQL;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    public PaymentService(AppDbContext db) => _db = db;

    public async Task<PagedResult<PaymentDto>> GetAllAsync(int page, int pageSize, int? memberId)
    {
        var query = _db.Payments.Include(p => p.Member).AsQueryable();

        if (memberId.HasValue)
            query = query.Where(p => p.MemberID == memberId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDto
            {
                PaymentID   = p.PaymentID,
                MemberID    = p.MemberID,
                MemberName  = p.Member.Name,
                Amount      = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentType = p.PaymentType
            })
            .ToListAsync();

        return new PagedResult<PaymentDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var p = await _db.Payments.Include(p => p.Member).FirstOrDefaultAsync(p => p.PaymentID == id);
        if (p == null) return null;
        return new PaymentDto { PaymentID = p.PaymentID, MemberID = p.MemberID, MemberName = p.Member.Name, Amount = p.Amount, PaymentDate = p.PaymentDate, PaymentType = p.PaymentType };
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        var payment = new Payment { MemberID = dto.MemberID, Amount = dto.Amount, PaymentDate = dto.PaymentDate, PaymentType = dto.PaymentType };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        var member = await _db.Members.FindAsync(dto.MemberID);
        return new PaymentDto { PaymentID = payment.PaymentID, MemberID = payment.MemberID, MemberName = member?.Name ?? "", Amount = payment.Amount, PaymentDate = payment.PaymentDate, PaymentType = payment.PaymentType };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null) return false;
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync();
        return true;
    }
}