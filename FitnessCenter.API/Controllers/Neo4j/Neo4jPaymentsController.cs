using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace FitnessCenterr.API.Controllers.Neo4j;

[ApiController]
[Route("api/neo4j/payments")]
[Authorize]
public class Neo4jPaymentsController : ControllerBase
{
    private readonly Neo4jContext _neo4j;
    public Neo4jPaymentsController(Neo4jContext neo4j) => _neo4j = neo4j;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? memberId = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var whereClause = memberId.HasValue
            ? "WHERE p.memberID = $memberId"
            : "";

        var countResult = await session.RunAsync(
            $"MATCH (p:Payment) {whereClause} RETURN count(p) AS total",
            new { memberId = memberId ?? 0 });
        var total = (await countResult.SingleAsync())["total"].As<int>();

        var result = await session.RunAsync(
            $@"MATCH (p:Payment) {whereClause}
               OPTIONAL MATCH (m:Member {{memberID: p.memberID}})
               RETURN p, m.name AS memberName
               ORDER BY p.paymentDate DESC
               SKIP $skip LIMIT $limit",
            new { memberId = memberId ?? 0, skip = (page - 1) * pageSize, limit = pageSize });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var p = r["p"].As<INode>();
            items.Add(new
            {
                paymentID   = p.Properties.ContainsKey("paymentID")   ? p.Properties["paymentID"].As<int>()     : 0,
                memberID    = p.Properties.ContainsKey("memberID")    ? p.Properties["memberID"].As<int>()      : 0,
                memberName  = r["memberName"] as string,
                amount      = p.Properties.ContainsKey("amount")      ? p.Properties["amount"].As<double>()     : 0.0,
                paymentDate = p.Properties.ContainsKey("paymentDate") ? p.Properties["paymentDate"].As<string>() : null,
                paymentType = p.Properties.ContainsKey("paymentType") ? p.Properties["paymentType"].As<string>() : null
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
            @"MATCH (p:Payment {paymentID: $id})
              OPTIONAL MATCH (m:Member {memberID: p.memberID})
              RETURN p, m.name AS memberName",
            new { id });

        var records = await result.ToListAsync();
        if (records.Count == 0)
            return NotFound(new { message = $"Payment med ID {id} blev ikke fundet i Neo4j." });

        var r = records[0];
        var p = r["p"].As<INode>();

        return Ok(new
        {
            paymentID   = p.Properties.ContainsKey("paymentID")   ? p.Properties["paymentID"].As<int>()      : 0,
            memberID    = p.Properties.ContainsKey("memberID")    ? p.Properties["memberID"].As<int>()       : 0,
            memberName  = r["memberName"] as string,
            amount      = p.Properties.ContainsKey("amount")      ? p.Properties["amount"].As<double>()      : 0.0,
            paymentDate = p.Properties.ContainsKey("paymentDate") ? p.Properties["paymentDate"].As<string>() : null,
            paymentType = p.Properties.ContainsKey("paymentType") ? p.Properties["paymentType"].As<string>() : null
        });
    }

    [HttpGet("member/{memberId:int}")]
    public async Task<IActionResult> GetByMember(int memberId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        await using var session = _neo4j.OpenSession();

        var countResult = await session.RunAsync(
            "MATCH (p:Payment {memberID: $memberId}) RETURN count(p) AS total",
            new { memberId });
        var total = (await countResult.SingleAsync())["total"].As<int>();

        var result = await session.RunAsync(
            @"MATCH (p:Payment {memberID: $memberId})
              RETURN p ORDER BY p.paymentDate DESC SKIP $skip LIMIT $limit",
            new { memberId, skip = (page - 1) * pageSize, limit = pageSize });

        var items = new List<object>();
        await result.ForEachAsync(r =>
        {
            var p = r["p"].As<INode>();
            items.Add(new
            {
                paymentID   = p.Properties.ContainsKey("paymentID")   ? p.Properties["paymentID"].As<int>()      : 0,
                amount      = p.Properties.ContainsKey("amount")      ? p.Properties["amount"].As<double>()      : 0.0,
                paymentDate = p.Properties.ContainsKey("paymentDate") ? p.Properties["paymentDate"].As<string>() : null,
                paymentType = p.Properties.ContainsKey("paymentType") ? p.Properties["paymentType"].As<string>() : null
            });
        });

        return Ok(new { items, totalCount = total, page, pageSize });
    }
}
