using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace FitnessCenterr.API.Controllers.Neo4j;

[ApiController]
[Route("api/neo4j/members")]
[Authorize]
public class Neo4jMembersController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    public Neo4jMembersController(Neo4jContext neo4j) => _neo4j = neo4j;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var countQuery = string.IsNullOrWhiteSpace(search)
            ? "MATCH (m:Member) RETURN count(m) AS total"
            : "MATCH (m:Member) WHERE toLower(m.name) CONTAINS toLower($search) OR toLower(m.email) CONTAINS toLower($search) RETURN count(m) AS total";

        var dataQuery = string.IsNullOrWhiteSpace(search)
            ? "MATCH (m:Member) OPTIONAL MATCH (m)-[:TRAINED_BY]->(t:Trainer) RETURN m, t ORDER BY m.name SKIP $skip LIMIT $limit"
            : "MATCH (m:Member) WHERE toLower(m.name) CONTAINS toLower($search) OR toLower(m.email) CONTAINS toLower($search) OPTIONAL MATCH (m)-[:TRAINED_BY]->(t:Trainer) RETURN m, t ORDER BY m.name SKIP $skip LIMIT $limit";

        var countResult = await session.RunAsync(countQuery, new { search = search ?? "" });
        var countRecord = await countResult.SingleAsync();
        var total = countRecord["total"].As<int>();

        var result = await session.RunAsync(dataQuery, new
        {
            search = search ?? "",
            skip  = (page - 1) * pageSize,
            limit = pageSize
        });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var m = r["m"].As<INode>();
            var t = r["t"] as INode;
            items.Add(new
            {
                memberID    = m.Properties["memberID"].As<int>(),
                name        = m.Properties["name"].As<string>(),
                email       = m.Properties.ContainsKey("email") ? m.Properties["email"].As<string>() : null,
                trainerName = t?.Properties.ContainsKey("name") == true ? t.Properties["name"].As<string>() : null
            });
        });

        return Ok(new
        {
            items,
            totalCount = total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(
            "MATCH (m:Member {memberID: $id}) OPTIONAL MATCH (m)-[:TRAINED_BY]->(t:Trainer) OPTIONAL MATCH (m)-[:BOOKED]->(c:FitnessClass) RETURN m, t, collect(c) AS classes",
            new { id });

        var records = await result.ToListAsync();
        if (records.Count == 0)
            return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet i Neo4j." });

        var r = records[0];
        var m = r["m"].As<INode>();
        var t = r["t"] as INode;
        var classes = r["classes"].As<List<INode>>();

        return Ok(new
        {
            memberID    = m.Properties["memberID"].As<int>(),
            name        = m.Properties["name"].As<string>(),
            email       = m.Properties.ContainsKey("email") ? m.Properties["email"].As<string>() : null,
            trainerName = t?.Properties.ContainsKey("name") == true ? t.Properties["name"].As<string>() : null,
            bookedClasses = classes.Select(c => new
            {
                classID   = c.Properties["classID"].As<int>(),
                className = c.Properties["name"].As<string>()
            })
        });
    }

    [HttpGet("{id:int}/recommendations")]
    public async Task<IActionResult> GetRecommendations(int id)
    {
        await using var session = _neo4j.OpenSession();
        // Graph-specific: find classes that members with the same trainer have booked, but this member hasn't
        var result = await session.RunAsync(@"
            MATCH (m:Member {memberID: $id})-[:TRAINED_BY]->(t:Trainer)<-[:TRAINED_BY]-(other:Member)
            MATCH (other)-[:BOOKED]->(c:FitnessClass)
            WHERE NOT (m)-[:BOOKED]->(c)
            RETURN DISTINCT c.classID AS classID, c.name AS className, count(other) AS popularity
            ORDER BY popularity DESC
            LIMIT 5",
            new { id });

        var items = new List<object>();
        await result.ForEachAsync(r => items.Add(new
        {
            classID    = r["classID"].As<int>(),
            className  = r["className"].As<string>(),
            popularity = r["popularity"].As<int>()
        }));

        return Ok(items);
    }
}
