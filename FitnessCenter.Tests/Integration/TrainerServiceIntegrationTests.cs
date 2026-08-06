using FluentAssertions;
using FitnessCenterr.Core.DTOs.Trainers;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using FitnessCenterr.Infrastructure.Services.MySQL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenter.Tests.Integration;

public class TrainerServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly TrainerService _service;

    public TrainerServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new TrainerService(_db);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var trainers = new[]
        {
            new Trainer { TrainerID = 1, Name = "Mads Jensen" },
            new Trainer { TrainerID = 2, Name = "Sofie Andersen" },
            new Trainer { TrainerID = 3, Name = "Lucas Christensen" }
        };
        _db.Trainers.AddRange(trainers);

        _db.Members.AddRange(
            new Member { MemberID = 1, Name = "Alice", Email = "a@test.dk", TrainerID = 1 },
            new Member { MemberID = 2, Name = "Bob",   Email = "b@test.dk", TrainerID = 1 }
        );

        _db.Classes.AddRange(
            new Class { ClassID = 1, Name = "Yoga",  TrainerID = 1, ClassDate = DateTime.UtcNow.AddDays(1) },
            new Class { ClassID = 2, Name = "Spin",  TrainerID = 1, ClassDate = DateTime.UtcNow.AddDays(2) },
            new Class { ClassID = 3, Name = "HIIT",  TrainerID = 2, ClassDate = DateTime.UtcNow.AddDays(3) }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTrainers()
    {
        var result = await _service.GetAllAsync(1, 100, null);
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_SearchByName_FiltersCorrectly()
    {
        var result = await _service.GetAllAsync(1, 100, "Mads");
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Mads Jensen");
    }

    [Fact]
    public async Task GetByIdAsync_WithClassAndMemberCounts_ReturnsCorrectData()
    {
        var trainer = await _service.GetByIdAsync(1);

        trainer.Should().NotBeNull();
        trainer!.ClassCount.Should().Be(2);
        trainer.MemberCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_PersistsTrainer()
    {
        var dto = new CreateTrainerDto { Name = "New Trainer" };
        var created = await _service.CreateAsync(dto);

        created.TrainerID.Should().BeGreaterThan(0);
        created.Name.Should().Be("New Trainer");
        _db.Trainers.Count().Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTrainer_UpdatesName()
    {
        var result = await _service.UpdateAsync(2, new UpdateTrainerDto { Name = "Sofie Updated" });

        result.Should().BeTrue();
        var t = await _db.Trainers.FindAsync(2);
        t!.Name.Should().Be("Sofie Updated");
    }

    [Fact]
    public async Task DeleteAsync_ExistingTrainer_RemovesFromDb()
    {
        var result = await _service.DeleteAsync(3);

        result.Should().BeTrue();
        _db.Trainers.Count().Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFalse()
    {
        (await _service.DeleteAsync(999)).Should().BeFalse();
    }

    public void Dispose() => _db.Dispose();
}
