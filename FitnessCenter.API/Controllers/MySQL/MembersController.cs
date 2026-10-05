using FitnessCenterr.Core.DTOs.Members;
using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace FitnessCenterr.API.Controllers.MySQL;

[ApiController]
[Route("api/mysql/members")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;
    private readonly AppDbContext   _db;

    public MembersController(IMemberService service, AppDbContext db)
    {
        _service = service;
        _db      = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        var result = await _service.GetAllAsync(page, pageSize, search, sortBy);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        if (member == null) return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        return Ok(member);
    }

    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(int id)
    {
        try
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CALL sp_get_member_profile(@id)";
            AddParam(cmd, "@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<object>();
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    MemberID          = reader["MemberID"],
                    Name              = reader["Name"],
                    Email             = reader["Email"],
                    BirthDate         = reader["BirthDate"],
                    TrainerName       = reader["TrainerName"],
                    SubscriptionType  = reader["SubscriptionType"],
                    SubscriptionPrice = reader["SubscriptionPrice"],
                    MemberSince       = reader["MemberSince"]
                });
            }

            if (!results.Any())
                return NotFound(new { message = $"Profil for medlem {id} ikke fundet." });

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Fejl ved kald af stored procedure.", detail = ex.Message });
        }
    }

    [HttpGet("search/sp")]
    public async Task<IActionResult> SearchWithProcedure(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        try
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CALL sp_search_members(@search, @page, @pageSize)";
            AddParam(cmd, "@search", string.IsNullOrWhiteSpace(search) ? null : search);
            AddParam(cmd, "@page", page);
            AddParam(cmd, "@pageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<object>();
            while (await reader.ReadAsync())
            {
                items.Add(new
                {
                    MemberID    = reader["MemberID"],
                    Name        = reader["Name"],
                    Email       = reader["Email"],
                    TrainerName = reader["TrainerName"]
                });
            }

            return Ok(new { items, page, pageSize });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Fejl ved kald af stored procedure.", detail = ex.Message });
        }
    }

    [HttpGet("view/overview")]
    public async Task<IActionResult> GetOverview([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Ugyldig page/pageSize." });

        try
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MemberID, MemberName, Email, BirthDate, TrainerName, SubscriptionType, SubscriptionPrice, MemberSince, TotalBookings, TotalPaid FROM v_member_overview LIMIT @limit OFFSET @offset";
            AddParam(cmd, "@limit", pageSize);
            AddParam(cmd, "@offset", (page - 1) * pageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<object>();
            while (await reader.ReadAsync())
            {
                items.Add(new
                {
                    MemberID          = reader["MemberID"],
                    MemberName        = reader["MemberName"],
                    Email             = reader["Email"],
                    BirthDate         = reader["BirthDate"],
                    TrainerName       = reader["TrainerName"],
                    SubscriptionType  = reader["SubscriptionType"],
                    SubscriptionPrice = reader["SubscriptionPrice"],
                    MemberSince       = reader["MemberSince"],
                    TotalBookings     = reader["TotalBookings"],
                    TotalPaid         = reader["TotalPaid"]
                });
            }

            return Ok(new { items, page, pageSize });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Fejl ved kald af view.", detail = ex.Message });
        }
    }

    // Tilføjer en navngiven parameter til kommandoen. Værdien sendes separat
    // fra SQL-teksten, så input aldrig kan ændre forespørgslens struktur.
    private static void AddParam(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.MemberID }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        if (!await _service.UpdateAsync(id, dto))
            return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _service.DeleteAsync(id))
            return NotFound(new { message = $"Medlem med ID {id} blev ikke fundet." });
        return NoContent();
    }
}