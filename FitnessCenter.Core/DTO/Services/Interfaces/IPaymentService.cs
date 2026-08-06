using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Core.DTOs.Payments;

namespace FitnessCenterr.Core.Services.Interfaces;

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetAllAsync(int page, int pageSize, int? memberId);
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task<bool> DeleteAsync(int id);
}
