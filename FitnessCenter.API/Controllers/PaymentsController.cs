using FitnessCenterr.Core.DTOs.Payments;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PaymentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? memberId = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var query = _db.Payments.Include(p => p.Member).AsQueryable();
        if (memberId.HasValue) query = query.Where(p => p.MemberID == memberId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PaymentDto { PaymentID = p.PaymentID, MemberID = p.MemberID, MemberName = p.Member.Name, Amount = p.Amount, PaymentDate = p.PaymentDate, PaymentType = p.PaymentType })
            .ToListAsync();

        return Ok(new { items, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Payments.Include(p => p.Member).FirstOrDefaultAsync(p => p.PaymentID == id);
        if (p == null) return NotFound(new { message = $"Payment med ID {id} blev ikke fundet." });
        return Ok(new PaymentDto { PaymentID = p.PaymentID, MemberID = p.MemberID, MemberName = p.Member.Name, Amount = p.Amount, PaymentDate = p.PaymentDate, PaymentType = p.PaymentType });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var payment = new Payment { MemberID = dto.MemberID, Amount = dto.Amount, PaymentDate = dto.PaymentDate, PaymentType = dto.PaymentType };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        var memberName = await _db.Members.Where(m => m.MemberID == payment.MemberID).Select(m => m.Name).FirstOrDefaultAsync();
        return CreatedAtAction(nameof(GetById), new { id = payment.PaymentID },
            new PaymentDto { PaymentID = payment.PaymentID, MemberID = payment.MemberID, MemberName = memberName ?? "", Amount = payment.Amount, PaymentDate = payment.PaymentDate, PaymentType = payment.PaymentType });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p == null) return NotFound();
        _db.Payments.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
