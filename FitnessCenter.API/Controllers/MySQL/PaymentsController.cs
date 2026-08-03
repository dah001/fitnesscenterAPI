using FitnessCenterr.Core.DTOs.Payments;
using FitnessCenterr.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterr.API.Controllers.MySQL;

[ApiController]
[Route("api/mysql/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? memberId = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var result = await _service.GetAllAsync(page, pageSize, memberId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _service.GetByIdAsync(id);
        if (payment == null) return NotFound(new { message = $"Betaling med ID {id} blev ikke fundet." });
        return Ok(payment);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var payment = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = payment.PaymentID }, payment);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id))
            return NotFound(new { message = $"Betaling med ID {id} blev ikke fundet." });
        return NoContent();
    }
}
