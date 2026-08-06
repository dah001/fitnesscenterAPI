using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FitnessCenterr.API.Controllers.MongoDB;

[ApiController]
[Route("api/mongodb/staff")]
[Authorize]
public class MongoStaffController : ControllerBase
{
    private readonly MongoDbContext _mongo;
    public MongoStaffController(MongoDbContext mongo) => _mongo = mongo;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var collection = _mongo.GetCollection<BsonDocument>("staff");

        FilterDefinition<BsonDocument> filter;
        if (string.IsNullOrWhiteSpace(search))
        {
            filter = Builders<BsonDocument>.Filter.Empty;
        }
        else
        {
            var regex = new BsonRegularExpression(search, "i");
            filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Regex("name", regex),
                Builders<BsonDocument>.Filter.Regex("role", regex)
            );
        }

        var total = await collection.CountDocumentsAsync(filter);
        var items = await collection.Find(filter)
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
        var collection = _mongo.GetCollection<BsonDocument>("staff");
        var filter = Builders<BsonDocument>.Filter.Eq("staffID", id);
        var staff = await collection.Find(filter).FirstOrDefaultAsync();

        if (staff == null)
            return NotFound(new { message = $"Staff med ID {id} blev ikke fundet i MongoDB." });

        return Ok(BsonTypeMapper.MapToDotNetValue(staff));
    }
}