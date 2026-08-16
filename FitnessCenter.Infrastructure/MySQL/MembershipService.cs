using FitnessCenterr.Core.DTOs.Memberships;
using FitnessCenterr.Core.DTOs.Services.Interfaces;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.Infrastructure.Services.MySQL;

public class MembershipService : IMembershipService
{
    private readonly AppDbContext _db;
    public MembershipService(AppDbContext db) => _db = db;

    public async Task<MembershipDto> CreateWithPaymentAsync(CreateMembershipWithPaymentDto dto)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var membership = new Membership
            {
                MemberID       = dto.MemberID,
                SubscriptionID = dto.SubscriptionID,
                StartDate      = dto.StartDate
            };
            _db.Memberships.Add(membership);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                MemberID    = dto.MemberID,
                Amount      = dto.PaymentAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentType = dto.PaymentType
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return new MembershipDto
            {
                MembershipID   = membership.MembershipID,
                MemberID       = membership.MemberID,
                SubscriptionID = membership.SubscriptionID,
                StartDate      = membership.StartDate
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}