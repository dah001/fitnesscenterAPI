using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly AppDbContext _db;
    public EquipmentController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Ugyldig page/pageSize." });
        var total = await _db.Equipments.CountAsync();
        var items = await _db.Equipments.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Equipments.FindAsync(id);
        if (e == null) return NotFound(new { message = $"Equipment med ID {id} ikke fundet." });
        return Ok(e);
    }

    [HttpPost] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Equipment dto) { _db.Equipments.Add(dto); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(GetById), new { id = dto.EquipmentID }, dto); }

    [HttpPut("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Equipment dto)
    {
        var e = await _db.Equipments.FindAsync(id); if (e == null) return NotFound();
        e.Name = dto.Name; e.CenterID = dto.CenterID; await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Equipments.FindAsync(id); if (e == null) return NotFound();
        _db.Equipments.Remove(e); await _db.SaveChangesAsync(); return NoContent();
    }
}
