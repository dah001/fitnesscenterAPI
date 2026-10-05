using FitnessCenterr.Core.DTOs.Classes;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers.MySQL;

[ApiController]
[Route("api/mysql/classes")]
[Authorize]
public class ClassesController : ControllerBase
{
private readonly AppDbContext _db;
    public ClassesController(AppDbContext db) => _db = db;

    // Fælles mapping: udfylder også hal, by og antal bookinger
    private static readonly System.Linq.Expressions.Expression<Func<Class, ClassDto>> ToDto = c => new ClassDto
    {
        ClassID      = c.ClassID,
        Name         = c.Name,
        TrainerID    = c.TrainerID,
        TrainerName  = c.Trainer.Name,
        ClassDate    = c.ClassDate,
        HallID       = c.HallID,
        HallName     = c.Hall != null ? c.Hall.Name : null,
        LocationID   = c.LocationID,
        City         = c.Location != null ? c.Location.City : null,
        BookingCount = c.ClassBookings.Count
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var classes = await _db.Classes
            .OrderBy(c => c.ClassID)
            .Select(ToDto)
            .ToListAsync();
        return Ok(classes);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var now = DateTime.UtcNow;
        var classes = await _db.Classes
            .Where(c => c.ClassDate >= now)
            .OrderBy(c => c.ClassDate)
            .Select(ToDto)
            .ToListAsync();
        return Ok(classes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.Classes.Where(c => c.ClassID == id).Select(ToDto).FirstOrDefaultAsync();
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        return Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        var newClass = new Class
        {
            Name       = dto.Name,
            TrainerID  = dto.TrainerID,
            ClassDate  = dto.ClassDate,
            HallID     = dto.HallID,
            LocationID = dto.LocationID
        };
        _db.Classes.Add(newClass);
        await _db.SaveChangesAsync();
        var created = await _db.Classes.Where(c => c.ClassID == newClass.ClassID).Select(ToDto).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = newClass.ClassID }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
    {
        var c = await _db.Classes.FindAsync(id);
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        c.Name = dto.Name; c.TrainerID = dto.TrainerID; c.ClassDate = dto.ClassDate;
        c.HallID = dto.HallID; c.LocationID = dto.LocationID;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Classes.FindAsync(id);
        if (c == null) return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet." });
        _db.Classes.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}