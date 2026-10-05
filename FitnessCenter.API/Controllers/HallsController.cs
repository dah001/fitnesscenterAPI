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

    // Hal med center, by og holdene i hallen – uden null-løkker
    private IQueryable<HallView> HallQuery() => _db.Halls.Select(h => new HallView
    {
        HallID   = h.HallID,
        Name     = h.Name,
        CenterID = h.CenterID,
        City     = h.Center.Location.City,
        Classes  = h.Classes
            .OrderBy(c => c.ClassDate)
            .Select(c => new HallClassView { ClassID = c.ClassID, Name = c.Name, ClassDate = c.ClassDate, TrainerName = c.Trainer.Name })
            .ToList()
    });

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Ugyldig page/pageSize." });
        var total = await _db.Halls.CountAsync();
        var items = await HallQuery().OrderBy(h => h.HallID).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var h = await HallQuery().FirstOrDefaultAsync(h => h.HallID == id);
        if (h == null) return NotFound(new { message = $"Hall med ID {id} blev ikke fundet." });
        return Ok(h);
    }

    [HttpPost] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Hall dto)
    {
        var hall = new Hall { Name = dto.Name, CenterID = dto.CenterID };
        _db.Halls.Add(hall); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = hall.HallID }, await HallQuery().FirstAsync(h => h.HallID == hall.HallID));
    }

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

public class HallView
{
    public int HallID { get; set; }
    public string? Name { get; set; }
    public int CenterID { get; set; }
    public string? City { get; set; }
    public List<HallClassView> Classes { get; set; } = new();
}

public class HallClassView
{
    public int ClassID { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ClassDate { get; set; }
    public string? TrainerName { get; set; }
}