using FluentAssertions;
using FitnessCenterr.Core.DTOs.Classes;
using FitnessCenterr.Core.DTOs.Payments;
using FitnessCenterr.Core.Models;
using FitnessCenterr.Infrastructure.Data;
using FitnessCenterr.Infrastructure.Services.MySQL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenter.Tests.Integration;

public class ClassServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ClassService _service;

    public ClassServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new ClassService(_db);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Trainers.Add(new Trainer { TrainerID = 1, Name = "Test Trainer" });

        var classes = new[]
        {
            new Class { ClassID = 1, Name = "Morning Yoga", TrainerID = 1, ClassDate = new DateTime(2025, 6, 1, 7, 0, 0) },
            new Class { ClassID = 2, Name = "Spin Class",   TrainerID = 1, ClassDate = new DateTime(2025, 6, 2, 9, 0, 0) },
            new Class { ClassID = 3, Name = "HIIT Blast",   TrainerID = 1, ClassDate = new DateTime(2025, 6, 3, 6, 0, 0) },
            new Class { ClassID = 4, Name = "Pilates",      TrainerID = 1, ClassDate = new DateTime(2025, 6, 4, 10, 0, 0) },
            new Class { ClassID = 5, Name = "Boxing",       TrainerID = 1, ClassDate = new DateTime(2025, 6, 5, 8, 0, 0) }
        };
        _db.Classes.AddRange(classes);
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClasses()
    {
        var result = await _service.GetAllAsync(1, 100, null, null);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_SortByDate_ReturnsSortedByClassDate()
    {
        var result = await _service.GetAllAsync(1, 100, null, "date");

        var dates = result.Items.Select(c => c.ClassDate).ToList();
        dates.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortByName_ReturnsSortedByName()
    {
        var result = await _service.GetAllAsync(1, 100, null, "name");

        var names = result.Items.Select(c => c.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SearchByName_FiltersCorrectly()
    {
        var result = await _service.GetAllAsync(1, 100, "yoga", null);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Morning Yoga");
    }

    [Fact]
    public async Task CreateAsync_ValidDto_PersistsClass()
    {
        var dto = new CreateClassDto
        {
            Name      = "New Class",
            TrainerID = 1,
            ClassDate = DateTime.UtcNow.AddDays(7)
        };

        var created = await _service.CreateAsync(dto);

        created.ClassID.Should().BeGreaterThan(0);
        created.Name.Should().Be("New Class");
        _db.Classes.Count().Should().Be(6);
    }

    [Fact]
    public async Task UpdateAsync_ExistingClass_UpdatesFields()
    {
        var dto = new UpdateClassDto { Name = "Updated Yoga", TrainerID = 1, ClassDate = DateTime.UtcNow.AddDays(1) };
        var result = await _service.UpdateAsync(1, dto);

        result.Should().BeTrue();
        var cls = await _db.Classes.FindAsync(1);
        cls!.Name.Should().Be("Updated Yoga");
    }

    [Fact]
    public async Task DeleteAsync_ExistingClass_RemovesIt()
    {
        var result = await _service.DeleteAsync(5);
        result.Should().BeTrue();
        _db.Classes.Count().Should().Be(4);
    }

    public void Dispose() => _db.Dispose();
}

public class PaymentServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PaymentService _service;

    public PaymentServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new PaymentService(_db);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Members.Add(new Member { MemberID = 1, Name = "Test Member", Email = "t@test.dk" });

        _db.Payments.AddRange(
            new Payment { PaymentID = 1, MemberID = 1, Amount = 199.00m, PaymentDate = DateTime.UtcNow.AddDays(-10), PaymentType = "Credit Card" },
            new Payment { PaymentID = 2, MemberID = 1, Amount = 349.00m, PaymentDate = DateTime.UtcNow.AddDays(-5),  PaymentType = "MobilePay" },
            new Payment { PaymentID = 3, MemberID = 1, Amount = 149.00m, PaymentDate = DateTime.UtcNow.AddDays(-1),  PaymentType = "Cash" }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPayments()
    {
        var result = await _service.GetAllAsync(1, 100, null);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_FilterByMemberId_ReturnsOnlyMemberPayments()
    {
        var result = await _service.GetAllAsync(1, 100, 1);
        result.Items.Should().AllSatisfy(p => p.MemberID.Should().Be(1));
    }

    [Fact]
    public async Task GetAllAsync_OrderedByDateDesc_MostRecentFirst()
    {
        var result = await _service.GetAllAsync(1, 100, null);
        var dates = result.Items.Select(p => p.PaymentDate).ToList();
        dates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPayment_ReturnsIt()
    {
        var p = await _service.GetByIdAsync(1);
        p.Should().NotBeNull();
        p!.Amount.Should().Be(199.00m);
        p.PaymentType.Should().Be("Credit Card");
    }

    [Fact]
    public async Task CreateAsync_ValidDto_PersistsPayment()
    {
        var dto = new CreatePaymentDto
        {
            MemberID    = 1,
            Amount      = 299.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentType = "Credit Card"
        };

        var created = await _service.CreateAsync(dto);

        created.PaymentID.Should().BeGreaterThan(0);
        created.Amount.Should().Be(299.00m);
        _db.Payments.Count().Should().Be(4);
    }

    [Fact]
    public async Task DeleteAsync_ExistingPayment_RemovesIt()
    {
        var result = await _service.DeleteAsync(3);
        result.Should().BeTrue();
        _db.Payments.Count().Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFalse()
    {
        (await _service.DeleteAsync(999)).Should().BeFalse();
    }

    public void Dispose() => _db.Dispose();
}
