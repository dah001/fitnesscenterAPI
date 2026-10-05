using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendingMachinesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VendingMachinesController(AppDbContext db) => _db = db;

    // Automat med varer – uden null-løkker
    private IQueryable<VendingMachineView> MachineQuery() => _db.VendingMachines.Select(v => new VendingMachineView
    {
        VendingMachineID = v.VendingMachineID,
        Name             = v.Name,
        Location         = v.Location,
        CenterID         = v.CenterID,
        Stocks           = v.Stocks
            .OrderBy(s => s.StockID)
            .Select(s => new StockView { StockID = s.StockID, ProductName = s.ProductName, Quantity = s.Quantity, Price = s.Price })
            .ToList()
    });

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Ugyldig page/pageSize." });
        var total = await _db.VendingMachines.CountAsync();
        var items = await MachineQuery().OrderBy(v => v.VendingMachineID).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var v = await MachineQuery().FirstOrDefaultAsync(v => v.VendingMachineID == id);
        if (v == null) return NotFound(new { message = $"VendingMachine med ID {id} ikke fundet." });
        return Ok(v);
    }

    [HttpPost] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] VendingMachine dto)
    {
        var machine = new VendingMachine { Name = dto.Name, Location = dto.Location, CenterID = dto.CenterID };
        _db.VendingMachines.Add(machine); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = machine.VendingMachineID }, await MachineQuery().FirstAsync(v => v.VendingMachineID == machine.VendingMachineID));
    }

    [HttpPut("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] VendingMachine dto)
    {
        var v = await _db.VendingMachines.FindAsync(id); if (v == null) return NotFound();
        v.Name = dto.Name; v.Location = dto.Location; v.CenterID = dto.CenterID; await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id}")] [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.VendingMachines.FindAsync(id); if (v == null) return NotFound();
        _db.VendingMachines.Remove(v); await _db.SaveChangesAsync(); return NoContent();
    }
}

public class VendingMachineView
{
    public int VendingMachineID { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public int? CenterID { get; set; }
    public List<StockView> Stocks { get; set; } = new();
}

public class StockView
{
    public int StockID { get; set; }
    public string? ProductName { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
}