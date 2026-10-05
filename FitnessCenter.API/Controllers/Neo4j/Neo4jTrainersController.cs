using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace FitnessCenterr.API.Controllers.Neo4j;

[ApiController]
[Route("api/neo4j/trainers")]
[Authorize]
public class Neo4jTrainersController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    public Neo4jTrainersController(Neo4jContext neo4j) => _neo4j = neo4j;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var countQuery = string.IsNullOrWhiteSpace(search)
            ? "MATCH (t:Trainer) RETURN count(t) AS total"
            : "MATCH (t:Trainer) WHERE toLower(t.name) CONTAINS toLower($search) RETURN count(t) AS total";

        var dataQuery = string.IsNullOrWhiteSpace(search)
            ? "MATCH (t:Trainer) OPTIONAL MATCH (m:Member)-[:TRAINED_BY]->(t) OPTIONAL MATCH (t)-[:TEACHES]->(c:FitnessClass) RETURN t, count(DISTINCT m) AS memberCount, count(DISTINCT c) AS classCount ORDER BY t.name SKIP $skip LIMIT $limit"
            : "MATCH (t:Trainer) WHERE toLower(t.name) CONTAINS toLower($search) OPTIONAL MATCH (m:Member)-[:TRAINED_BY]->(t) OPTIONAL MATCH (t)-[:TEACHES]->(c:FitnessClass) RETURN t, count(DISTINCT m) AS memberCount, count(DISTINCT c) AS classCount ORDER BY t.name SKIP $skip LIMIT $limit";

        var countResult = await session.RunAsync(countQuery, new { search = search ?? "" });
        var countRecord = await countResult.SingleAsync();
        var total = countRecord["total"].As<int>();

        var result = await session.RunAsync(dataQuery, new { search = search ?? "", skip = (page - 1) * pageSize, limit = pageSize });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var t = r["t"].As<INode>();
            items.Add(new
            {
                trainerID   = t.Properties["trainerID"].As<int>(),
                name        = t.Properties["name"].As<string>(),
                memberCount = r["memberCount"].As<int>(),
                classCount  = r["classCount"].As<int>()
            });
        });

        return Ok(new { items, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(
            "MATCH (t:Trainer {trainerID: $id}) OPTIONAL MATCH (m:Member)-[:TRAINED_BY]->(t) OPTIONAL MATCH (t)-[:TEACHES]->(c:FitnessClass) RETURN t, collect(DISTINCT m) AS members, collect(DISTINCT c) AS classes",
            new { id });

        var records = await result.ToListAsync();
        if (records.Count == 0)
            return NotFound(new { message = $"Træner med ID {id} blev ikke fundet i Neo4j." });

        var r = records[0];
        var t = r["t"].As<INode>();
        var members = r["members"].As<List<INode>>();
        var classes = r["classes"].As<List<INode>>();

        return Ok(new
        {
            trainerID = t.Properties["trainerID"].As<int>(),
            name      = t.Properties["name"].As<string>(),
            members   = members.Select(m => new { memberID = m.Properties["memberID"].As<int>(), name = m.Properties["name"].As<string>() }),
            classes   = classes.Select(c => new { classID = c.Properties["classID"].As<int>(), name = c.Properties["name"].As<string>() })
        });
    }

    [HttpGet("{id:int}/network")]
    public async Task<IActionResult> GetTrainerNetwork(int id)
    {
        // Graph-specific: find all members of trainer, and what other classes those members have booked
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(@"
            MATCH (t:Trainer {trainerID: $id})<-[:TRAINED_BY]-(m:Member)-[:HAS_BOOKING]->(:ClassBooking)-[:BOOKED]->(c:FitnessClass)
            RETURN m.name AS memberName, c.name AS className, c.classDate AS classDate
            ORDER BY m.name",
            new { id });

        var items = new List<object>();
        await result.ForEachAsync(r => items.Add(new
        {
            memberName = r["memberName"].As<string>(),
            className  = r["className"].As<string>(),
            classDate  = r["classDate"].As<string>()
        }));

        return Ok(items);
    }
}