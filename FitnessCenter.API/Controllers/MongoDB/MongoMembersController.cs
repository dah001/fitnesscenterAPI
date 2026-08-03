using FitnessCenterr.Core.DTOs;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FitnessCenterr.API.Controllers.MongoDB;

[ApiController]
[Route("api/mongodb/members")]
[Authorize]
public class MongoMembersController : ControllerBase
{
    private readonly MongoDbContext _mongo;
    public MongoMembersController(MongoDbContext mongo) => _mongo = mongo;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var col    = _mongo.GetCollection<BsonDocument>("members");
        var filter = string.IsNullOrWhiteSpace(search)
            ? Builders<BsonDocument>.Filter.Empty
            : Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Regex("name",  new BsonRegularExpression(search, "i")),
                Builders<BsonDocument>.Filter.Regex("email", new BsonRegularExpression(search, "i")));

        var total = (int)await col.CountDocumentsAsync(filter);
        var items = await col.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return Ok(new
        {
            items      = items.Select(d => BsonTypeMapper.MapToDotNetValue(d)),
            totalCount = total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var col  = _mongo.GetCollection<BsonDocument>("members");
        var doc  = await col.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        if (doc == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet i MongoDB." });
        return Ok(BsonTypeMapper.MapToDotNetValue(doc));
    }

    [HttpGet("{id:int}/bookings")]
    public async Task<IActionResult> GetBookings(int id)
    {
        var col = _mongo.GetCollection<BsonDocument>("members");
        var doc = await col.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        if (doc == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        var bookings = doc.Contains("bookings") ? BsonTypeMapper.MapToDotNetValue(doc["bookings"]) : new List<object>();
        return Ok(bookings);
    }
}
