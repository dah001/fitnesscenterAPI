using FitnessCenterr.Core.DTOs.Memberships;

namespace FitnessCenterr.Core.DTOs.Services.Interfaces;

public interface IMembershipService
{
    Task<MembershipDto> CreateWithPaymentAsync(CreateMembershipWithPaymentDto dto);
}