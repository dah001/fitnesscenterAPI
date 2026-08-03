using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FitnessCenterr.API.Controllers.MongoDB;

[ApiController]
[Route("api/mongodb/centers")]
[Authorize]
public class MongoCentersController : ControllerBase
{
    private readonly MongoDbContext _mongo;
    public MongoCentersController(MongoDbContext mongo) => _mongo = mongo;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var col   = _mongo.GetCollection<BsonDocument>("centers");
        var total = (int)await col.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty);
        var items = await col.Find(Builders<BsonDocument>.Filter.Empty)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return Ok(new
        {
            items      = items.Select(d => BsonTypeMapper.MapToDotNetValue(d)),
            totalCount = total,
            page, pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var col = _mongo.GetCollection<BsonDocument>("centers");
        var doc = await col.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        if (doc == null) return NotFound(new { message = $"Center med ID {id} blev ikke fundet i MongoDB." });
        return Ok(BsonTypeMapper.MapToDotNetValue(doc));
    }
}
