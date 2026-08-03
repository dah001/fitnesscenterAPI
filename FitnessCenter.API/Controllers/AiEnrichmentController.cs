using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiEnrichmentController : ControllerBase
{
    private readonly IAiEnrichmentService _ai;
    private readonly AppDbContext         _db;

    public AiEnrichmentController(IAiEnrichmentService ai, AppDbContext db)
    {
        _ai = ai;
        _db = db;
    }

    /// <summary>Generates and persists an AI bio for a specific trainer.</summary>
    [HttpPost("trainers/{id}/enrich")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnrichTrainer(int id)
    {
        try
        {
            await _ai.EnrichTrainerBioAsync(id);
            var trainer = await _db.Trainers.FindAsync(id);
            return Ok(new { message = "Bio genereret.", aiBio = trainer!.AiBio, generatedAt = trainer.AiBioGeneratedAt });
        }
        catch (KeyNotFoundException) { return NotFound(new { message = $"Træner {id} ikke fundet." }); }
        catch (Exception ex)         { return StatusCode(502, new { message = "AI service fejlede.", detail = ex.Message }); }
    }

    /// <summary>Generates and persists an AI description for a specific class.</summary>
    [HttpPost("classes/{id}/enrich")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnrichClass(int id)
    {
        try
        {
            await _ai.EnrichClassDescriptionAsync(id);
            var cls = await _db.Classes.FindAsync(id);
            return Ok(new { message = "Beskrivelse genereret.", aiDescription = cls!.AiDescription, generatedAt = cls.AiDescriptionGeneratedAt });
        }
        catch (KeyNotFoundException) { return NotFound(new { message = $"Klasse {id} ikke fundet." }); }
        catch (Exception ex)         { return StatusCode(502, new { message = "AI service fejlede.", detail = ex.Message }); }
    }

    /// <summary>Enriches all trainers that do not yet have an AI bio.</summary>
    [HttpPost("trainers/enrich-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnrichAllTrainers()
    {
        await _ai.EnrichAllTrainersAsync();
        var count = await _db.Trainers.CountAsync(t => t.AiBio != null);
        return Ok(new { message = "Alle trænere beriget.", trainersWithBio = count });
    }

    /// <summary>Enriches all classes that do not yet have an AI description.</summary>
    [HttpPost("classes/enrich-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnrichAllClasses()
    {
        await _ai.EnrichAllClassesAsync();
        var count = await _db.Classes.CountAsync(c => c.AiDescription != null);
        return Ok(new { message = "Alle klasser beriget.", classesWithDescription = count });
    }

    /// <summary>Returns all trainers with their AI-generated bios.</summary>
    [HttpGet("trainers/bios")]
    public async Task<IActionResult> GetTrainerBios(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var query = _db.Trainers.Where(t => t.AiBio != null);
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { t.TrainerID, t.Name, t.AiBio, t.AiBioGeneratedAt })
            .ToListAsync();

        return Ok(new { items, totalCount = total, page, pageSize });
    }

    /// <summary>Returns all classes with their AI-generated descriptions.</summary>
    [HttpGet("classes/descriptions")]
    public async Task<IActionResult> GetClassDescriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var query = _db.Classes.Where(c => c.AiDescription != null);
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new { c.ClassID, c.Name, c.AiDescription, c.AiDescriptionGeneratedAt })
            .ToListAsync();

        return Ok(new { items, totalCount = total, page, pageSize });
    }
}
