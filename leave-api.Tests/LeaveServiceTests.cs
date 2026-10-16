using LeaveApi.Data;
using LeaveApi.Models;
using LeaveApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveApi.Tests;

public class LeaveServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new LeaveService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(LeaveRequest req, LeaveBalance bal)> SeedRequestAsync(
        int days = 3, int totalDays = 20, int usedDays = 0, int userId = 1, int year = 2026)
    {
        _db.LeaveBalances.Add(new LeaveBalance
        {
            UserId = userId, Year = year, TotalDays = totalDays, UsedDays = usedDays
        });
        var start = new DateTime(year, 6, 1);
        var req = new LeaveRequest
        {
            UserId = userId,
            StartDate = start,
            EndDate = start.AddDays(days - 1),
            Reason = "Vacation",
            Status = "Pending",
        };
        _db.LeaveRequests.Add(req);
        await _db.SaveChangesAsync();
        var bal = await _db.LeaveBalances.FirstAsync(b => b.UserId == userId && b.Year == year);
        return (req, bal);
    }

    [Fact]
    public async Task Approve_IncreasesUsedDays_ByRequestLength()
    {
        var (req, _) = await SeedRequestAsync(days: 3);
        await _sut.ApproveAsync(req.Id, decidedBy: 99);
        var bal = await _db.LeaveBalances.FirstAsync();
        Assert.Equal("Approved", req.Status);
        Assert.Equal(3, bal.UsedDays);
        Assert.Equal(99, req.DecidedBy);
    }

    [Fact]
    public async Task Approve_WithInsufficientBalance_Throws()
    {
        var (req, _) = await SeedRequestAsync(days: 5, totalDays: 4, usedDays: 0);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ApproveAsync(req.Id, decidedBy: 99));
        Assert.Equal("Pending", req.Status);
    }

    [Fact]
    public async Task Approve_Twice_Throws()
    {
        var (req, _) = await SeedRequestAsync(days: 2);
        await _sut.ApproveAsync(req.Id, decidedBy: 99);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ApproveAsync(req.Id, decidedBy: 99));
    }

    [Fact]
    public async Task Reject_DoesNotTouchBalance()
    {
        var (req, _) = await SeedRequestAsync(days: 3, usedDays: 5);
        await _sut.RejectAsync(req.Id, decidedBy: 99);
        var bal = await _db.LeaveBalances.FirstAsync();
        Assert.Equal("Rejected", req.Status);
        Assert.Equal(5, bal.UsedDays);
    }

    [Fact]
    public async Task Reject_AfterApprove_Throws()
    {
        var (req, _) = await SeedRequestAsync(days: 2);
        await _sut.ApproveAsync(req.Id, decidedBy: 99);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RejectAsync(req.Id, decidedBy: 99));
    }

    [Fact]
    public async Task Create_EndBeforeStart_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(1, new DateTime(2026, 6, 10), new DateTime(2026, 6, 5), "oops"));
    }
}
