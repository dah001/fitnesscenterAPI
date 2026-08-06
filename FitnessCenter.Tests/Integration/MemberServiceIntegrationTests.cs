using FluentAssertions;
using FitnessCenterr.Core.DTOs.Members;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using FitnessCenterr.Infrastructure.Services.MySQL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenter.Tests.Integration;

/// <summary>
/// Integration tests for MemberService against an in-memory database.
/// These tests verify the real service logic including EF Core queries.
/// </summary>
public class MemberServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MemberService _service;

    public MemberServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new MemberService(_db);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var trainer = new Trainer { TrainerID = 1, Name = "Test Trainer" };
        _db.Trainers.Add(trainer);

        _db.Members.AddRange(
            new Member { MemberID = 1, Name = "Alice Hansen",  Email = "alice@test.dk",  TrainerID = 1 },
            new Member { MemberID = 2, Name = "Bob Jensen",    Email = "bob@test.dk",    TrainerID = 1 },
            new Member { MemberID = 3, Name = "Clara Nielsen", Email = "clara@test.dk",  TrainerID = null },
            new Member { MemberID = 4, Name = "David Møller",  Email = "david@test.dk",  TrainerID = null },
            new Member { MemberID = 5, Name = "Eva Andersen",  Email = "eva@test.dk",    TrainerID = 1 }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_WithNoPaging_ReturnsAllMembers()
    {
        var result = await _service.GetAllAsync(1, 100, null, null);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WithPaging_ReturnsCorrectPage()
    {
        var result = await _service.GetAllAsync(1, 2, null, null);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersCorrectly()
    {
        var result = await _service.GetAllAsync(1, 100, "alice", null);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Alice Hansen");
    }

    [Fact]
    public async Task GetAllAsync_SearchByEmail_ReturnsMatch()
    {
        var result = await _service.GetAllAsync(1, 100, "bob@test", null);

        result.Items.Should().HaveCount(1);
        result.Items[0].Email.Should().Be("bob@test.dk");
    }

    [Fact]
    public async Task GetAllAsync_SortByName_ReturnsSortedResults()
    {
        var result = await _service.GetAllAsync(1, 100, null, "name");

        result.Items.First().Name.Should().Be("Alice Hansen");
        result.Items.Last().Name.Should().BeOneOf("Eva Andersen", "David Møller");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMember()
    {
        var member = await _service.GetByIdAsync(1);

        member.Should().NotBeNull();
        member!.Name.Should().Be("Alice Hansen");
        member.TrainerName.Should().Be("Test Trainer");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var member = await _service.GetByIdAsync(999);
        member.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesAndReturnsMember()
    {
        var dto = new CreateMemberDto { Name = "Frank Larsen", Email = "frank@test.dk", TrainerID = 1 };

        var created = await _service.CreateAsync(dto);

        created.MemberID.Should().BeGreaterThan(0);
        created.Name.Should().Be("Frank Larsen");
        created.Email.Should().Be("frank@test.dk");
        _db.Members.Count().Should().Be(6);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMember_UpdatesCorrectly()
    {
        var dto = new UpdateMemberDto { Name = "Alice Updated", Email = "alice.new@test.dk", TrainerID = null };

        var result = await _service.UpdateAsync(1, dto);

        result.Should().BeTrue();
        var updated = await _db.Members.FindAsync(1);
        updated!.Name.Should().Be("Alice Updated");
        updated.Email.Should().Be("alice.new@test.dk");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingMember_ReturnsFalse()
    {
        var dto = new UpdateMemberDto { Name = "Ghost", Email = "ghost@test.dk", TrainerID = null };
        var result = await _service.UpdateAsync(999, dto);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingMember_DeletesSuccessfully()
    {
        var result = await _service.DeleteAsync(5);

        result.Should().BeTrue();
        _db.Members.Count().Should().Be(4);
        (await _db.Members.FindAsync(5)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingMember_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    public void Dispose() => _db.Dispose();
}
