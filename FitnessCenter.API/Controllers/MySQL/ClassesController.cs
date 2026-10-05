using FitnessCenterr.Core.DTOs.Classes;
using FitnessCenterr.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterr.API.Controllers.MySQL;

[ApiController]
[Route("api/mysql/classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassService _service;
    public ClassesController(IClassService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var result = await _service.GetAllAsync(page, pageSize, search, sortBy);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cls = await _service.GetByIdAsync(id);
        if (cls == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        return Ok(cls);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        var cls = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = cls.ClassID }, cls);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
    {
        if (!await _service.UpdateAsync(id, dto))
            return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id))
            return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        return NoContent();
    }
}