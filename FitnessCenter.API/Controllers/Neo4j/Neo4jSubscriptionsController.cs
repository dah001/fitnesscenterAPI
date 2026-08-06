using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace FitnessCenterr.API.Controllers.Neo4j;

[ApiController]
[Route("api/neo4j/subscriptions")]
[Authorize]
public class Neo4jSubscriptionsController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    public Neo4jSubscriptionsController(Neo4jContext neo4j) => _neo4j = neo4j;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var countResult = await session.RunAsync("MATCH (s:Subscription) RETURN count(s) AS total");
        var total = (await countResult.SingleAsync())["total"].As<int>();

        var result = await session.RunAsync(
            @"MATCH (s:Subscription)
              OPTIONAL MATCH (m:Member)-[:HAS_SUBSCRIPTION]->(s)
              RETURN s, count(m) AS memberCount
              ORDER BY s.subscriptionID SKIP $skip LIMIT $limit",
            new { skip = (page - 1) * pageSize, limit = pageSize });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var s = r["s"].As<INode>();
            items.Add(new
            {
                subscriptionID = s.Properties["subscriptionID"].As<int>(),
                type           = s.Properties["type"].As<string>(),
                price          = s.Properties["price"].As<double>(),
                memberCount    = r["memberCount"].As<int>()
            });
        });

        return Ok(new { items, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _neo4j.OpenSession();
        var result = await session.RunAsync(
            "MATCH (s:Subscription {subscriptionID: $id}) OPTIONAL MATCH (m:Member)-[:HAS_SUBSCRIPTION]->(s) RETURN s, collect(m) AS members",
            new { id });

        var records = await result.ToListAsync();
        if (records.Count == 0)
            return NotFound(new { message = $"Abonnement med ID {id} ikke fundet i Neo4j." });

        var r = records[0];
        var s = r["s"].As<INode>();
        var members = r["members"].As<List<INode>>();

        return Ok(new
        {
            subscriptionID = s.Properties["subscriptionID"].As<int>(),
            type           = s.Properties["type"].As<string>(),
            price          = s.Properties["price"].As<double>(),
            members        = members.Select(m => new { memberID = m.Properties["memberID"].As<int>(), name = m.Properties["name"].As<string>() })
        });
    }
}
