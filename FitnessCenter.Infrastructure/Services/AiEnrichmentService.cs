using System.Net.Http.Json;
using System.Text.Json;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace FitnessCenterr.Infrastructure.Services;

/// <summary>
/// AI Enrichment Service – calls a local Ollama instance to generate trainer bios
/// and class descriptions, then persists the results in MySQL.
/// </summary>
public class AiEnrichmentService : IAiEnrichmentService
{
    private readonly AppDbContext               _db;
    private readonly IHttpClientFactory         _httpFactory;
    private readonly IConfiguration             _config;
    private readonly ILogger<AiEnrichmentService> _logger;

    public AiEnrichmentService(
        AppDbContext db,
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<AiEnrichmentService> logger)
    {
        _db          = db;
        _httpFactory = httpFactory;
        _config      = config;
        _logger      = logger;
    }

    public async Task EnrichTrainerBioAsync(int trainerId)
    {
        var trainer = await _db.Trainers
            .Include(t => t.Members)
            .Include(t => t.Classes)
            .FirstOrDefaultAsync(t => t.TrainerID == trainerId)
            ?? throw new KeyNotFoundException($"Trainer {trainerId} not found.");

        var prompt = $"""
            You are writing a professional fitness center website bio.
            Generate a 2-3 sentence professional bio for a fitness trainer named "{trainer.Name}".
            They train {trainer.Members.Count} members and teach {trainer.Classes.Count} classes.
            Class types: {string.Join(", ", trainer.Classes.Select(c => c.Name).Distinct().Take(5))}.
            Write in a warm, motivating tone. Return only the bio text, no extra formatting.
            """;

        var bio = await CallOllamaAsync(prompt);
        if (bio != null)
        {
            trainer.AiBio            = bio;
            trainer.AiBioGeneratedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("AI bio generated for trainer {TrainerID}", trainerId);
        }
    }

    public async Task EnrichClassDescriptionAsync(int classId)
    {
        var cls = await _db.Classes
            .Include(c => c.Trainer)
            .Include(c => c.ClassBookings)
            .Include(c => c.Location)
            .FirstOrDefaultAsync(c => c.ClassID == classId)
            ?? throw new KeyNotFoundException($"Class {classId} not found.");

        var prompt = $"""
            You are writing descriptions for a fitness center booking website.
            Generate a 2-3 sentence engaging description for a fitness class named "{cls.Name}".
            The class is taught by trainer "{cls.Trainer.Name}" and has {cls.ClassBookings.Count} bookings.
            {(cls.Location != null ? $"It takes place in {cls.Location.City}." : "")}
            Focus on what participants can expect, the energy, and the benefits.
            Return only the description text, no extra formatting.
            """;

        var description = await CallOllamaAsync(prompt);
        if (description != null)
        {
            cls.AiDescription            = description;
            cls.AiDescriptionGeneratedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("AI description generated for class {ClassID}", classId);
        }
    }

    public async Task EnrichAllTrainersAsync()
    {
        var trainerIds = await _db.Trainers
            .Where(t => t.AiBio == null)
            .Select(t => t.TrainerID)
            .ToListAsync();

        foreach (var id in trainerIds)
        {
            try   { await EnrichTrainerBioAsync(id); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to enrich trainer {ID}", id); }
        }
    }

    public async Task EnrichAllClassesAsync()
    {
        var classIds = await _db.Classes
            .Where(c => c.AiDescription == null)
            .Select(c => c.ClassID)
            .ToListAsync();

        foreach (var id in classIds)
        {
            try   { await EnrichClassDescriptionAsync(id); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to enrich class {ID}", id); }
        }
    }

    private async Task<string?> CallOllamaAsync(string prompt)
    {
        try
        {
            var ollamaUrl = _config["Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model     = _config["Ollama:Model"]   ?? "llama3.2";

            var client  = _httpFactory.CreateClient("OllamaClient");
            var request = new
            {
                model,
                prompt,
                stream = false
            };

            var response = await client.PostAsJsonAsync($"{ollamaUrl}/api/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama returned {Status}", response.StatusCode);
                return null;
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return doc.RootElement.GetProperty("response").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama");
            return null;
        }
    }
}
