using FitnessCenterr.Core.DTOs.Trainers;
using FitnessCenterr.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterr.API.Controllers.MySQL;

[ApiController]
[Route("api/mysql/trainers")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _service;
    public TrainersController(ITrainerService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var result = await _service.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trainer = await _service.GetByIdAsync(id);
        if (trainer == null) return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        return Ok(trainer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTrainerDto dto)
    {
        var trainer = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = trainer.TrainerID }, trainer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTrainerDto dto)
    {
        if (!await _service.UpdateAsync(id, dto))
            return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id))
            return NotFound(new { message = $"Træner med ID {id} blev ikke fundet." });
        return NoContent();
    }
}
