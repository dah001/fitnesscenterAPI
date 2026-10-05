using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    // By med centre (og deres haller) og antal hold – uden null-løkker
    private IQueryable<LocationView> LocationQuery() => _db.Locations.Select(l => new LocationView
    {
        LocationID = l.LocationID,
        City       = l.City,
        ClassCount = l.Classes.Count,
        Centers    = l.Centers
            .OrderBy(c => c.CenterID)
            .Select(c => new LocationCenterView
            {
                CenterID = c.CenterID,
                Halls    = c.Halls.OrderBy(h => h.HallID).Select(h => h.Name ?? "").ToList()
            })
            .ToList()
    });

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Ugyldig page/pageSize." });
        var total = await _db.Locations.CountAsync();
        var items = await LocationQuery().OrderBy(l => l.LocationID).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var l = await LocationQuery().FirstOrDefaultAsync(l => l.LocationID == id);
        if (l == null) return NotFound(new { message = $"Location med ID {id} blev ikke fundet." });
        return Ok(l);
    }

    [HttpPost] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Location dto)
    {
        var location = new Location { City = dto.City };
        _db.Locations.Add(location); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = location.LocationID }, await LocationQuery().FirstAsync(l => l.LocationID == location.LocationID));
    }

    [HttpPut("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Location dto)
    {
        var l = await _db.Locations.FindAsync(id); if (l == null) return NotFound();
        l.City = dto.City; await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var l = await _db.Locations.FindAsync(id); if (l == null) return NotFound();
        _db.Locations.Remove(l); await _db.SaveChangesAsync(); return NoContent();
    }
}

public class LocationView
{
    public int LocationID { get; set; }
    public string City { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public List<LocationCenterView> Centers { get; set; } = new();
}

public class LocationCenterView
{
    public int CenterID { get; set; }
    public List<string> Halls { get; set; } = new();
}