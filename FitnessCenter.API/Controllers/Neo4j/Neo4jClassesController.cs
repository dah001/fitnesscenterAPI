using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace FitnessCenterr.API.Controllers.Neo4j;

[ApiController]
[Route("api/neo4j/classes")]
[Authorize]
public class Neo4jClassesController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    public Neo4jClassesController(Neo4jContext neo4j) => _neo4j = neo4j;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var whereClause = string.IsNullOrWhiteSpace(search) ? "" : "WHERE toLower(c.name) CONTAINS toLower($search)";
        var orderClause = sortBy == "date" ? "c.classDate" : "c.classID";

        var countResult = await session.RunAsync(
            $"MATCH (c:FitnessClass) {whereClause} RETURN count(c) AS total",
            new { search = search ?? "" });
        var total = (await countResult.SingleAsync())["total"].As<int>();

        var result = await session.RunAsync(
            $@"MATCH (c:FitnessClass) {whereClause}
               OPTIONAL MATCH (t:Trainer)-[:TEACHES]->(c)
               OPTIONAL MATCH (m:Member)-[:BOOKED]->(c)
               OPTIONAL MATCH (c)-[:HELD_AT]->(l:Location)
               RETURN c, t, count(DISTINCT m) AS bookings, l
               ORDER BY {orderClause} SKIP $skip LIMIT $limit",
            new { search = search ?? "", skip = (page - 1) * pageSize, limit = pageSize });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var c = r["c"].As<INode>();
            var t = r["t"] as INode;
            var l = r["l"] as INode;
            items.Add(new
            {
                classID      = c.Properties["classID"].As<int>(),
                name         = c.Properties["name"].As<string>(),
                classDate    = c.Properties["classDate"].As<string>(),
                trainerName  = t?.Properties.ContainsKey("name") == true ? t.Properties["name"].As<string>() : null,
                city         = l?.Properties.ContainsKey("city") == true ? l.Properties["city"].As<string>() : null,
                bookingCount = r["bookings"].As<int>()
            });
        });

        return Ok(new { items, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (c:FitnessClass {classID: $id})
              OPTIONAL MATCH (t:Trainer)-[:TEACHES]->(c)
              OPTIONAL MATCH (m:Member)-[:BOOKED]->(c)
              OPTIONAL MATCH (c)-[:HELD_AT]->(l:Location)
              RETURN c, t, collect(DISTINCT m) AS members, l",
            new { id });

        var records = await result.ToListAsync();
        if (records.Count == 0)
            return NotFound(new { message = $"Klasse med ID {id} blev ikke fundet i Neo4j." });

        var r = records[0];
        var c = r["c"].As<INode>();
        var t = r["t"] as INode;
        var l = r["l"] as INode;
        var members = r["members"].As<List<INode>>();

        return Ok(new
        {
            classID     = c.Properties["classID"].As<int>(),
            name        = c.Properties["name"].As<string>(),
            classDate   = c.Properties["classDate"].As<string>(),
            trainerName = t?.Properties.ContainsKey("name") == true ? t.Properties["name"].As<string>() : null,
            city        = l?.Properties.ContainsKey("city") == true ? l.Properties["city"].As<string>() : null,
            participants = members.Select(m => new
            {
                memberID = m.Properties["memberID"].As<int>(),
                name     = m.Properties["name"].As<string>()
            })
        });
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetMostPopular([FromQuery] int top = 5)
    {
        if (top < 1 || top > 50) return BadRequest(new { message = "Top skal være mellem 1 og 50." });
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(@"
            MATCH (m:Member)-[:BOOKED]->(c:FitnessClass)
            OPTIONAL MATCH (t:Trainer)-[:TEACHES]->(c)
            RETURN c.classID AS classID, c.name AS className, t.name AS trainerName, count(m) AS bookings
            ORDER BY bookings DESC LIMIT $top",
            new { top });

        var items = new List<object>();
        await result.ForEachAsync(r => items.Add(new
        {
            classID     = r["classID"].As<int>(),
            className   = r["className"].As<string>(),
            trainerName = r["trainerName"].As<string?>(),
            bookings    = r["bookings"].As<int>()
        }));

        return Ok(items);
    }
}
