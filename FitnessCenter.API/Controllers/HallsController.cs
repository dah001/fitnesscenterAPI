using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HallsController : ControllerBase
{
    private readonly AppDbContext _db;
    public HallsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Ugyldig page/pageSize." });
        var total = await _db.Halls.CountAsync();
        var items = await _db.Halls.Include(h => h.Center).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var h = await _db.Halls.Include(h => h.Center).FirstOrDefaultAsync(h => h.HallID == id);
        if (h == null) return NotFound(new { message = $"Hall med ID {id} blev ikke fundet." });
        return Ok(h);
    }

    [HttpPost] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Hall dto) { _db.Halls.Add(dto); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(GetById), new { id = dto.HallID }, dto); }

    [HttpPut("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Hall dto)
    {
        var h = await _db.Halls.FindAsync(id); if (h == null) return NotFound();
        h.Name = dto.Name; h.CenterID = dto.CenterID; await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var h = await _db.Halls.FindAsync(id); if (h == null) return NotFound();
        _db.Halls.Remove(h); await _db.SaveChangesAsync(); return NoContent();
    }
}
