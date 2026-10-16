using LeaveApi.Data;
using LeaveApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveApi.Services;

public class LeaveService
{
    private readonly AppDbContext _db;
    public const int DefaultYearlyAllowance = 20;

    public LeaveService(AppDbContext db) => _db = db;

    public static int DayCount(DateTime start, DateTime end)
        => (end.Date - start.Date).Days + 1;

    public async Task<LeaveRequest> CreateAsync(int userId, DateTime start, DateTime end, string reason)
    {
        if (end.Date < start.Date)
            throw new InvalidOperationException("EndDate cannot be before StartDate.");

        var req = new LeaveRequest
        {
            UserId = userId,
            StartDate = start.Date,
            EndDate = end.Date,
            Reason = reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        };
        _db.LeaveRequests.Add(req);
        await _db.SaveChangesAsync();
        return req;
    }

    public async Task<LeaveBalance> GetOrCreateBalanceAsync(int userId, int year)
    {
        var balance = await _db.LeaveBalances
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == year);
        if (balance is null)
        {
            balance = new LeaveBalance { UserId = userId, Year = year, TotalDays = DefaultYearlyAllowance };
            _db.LeaveBalances.Add(balance);
            await _db.SaveChangesAsync();
        }
        return balance;
    }

    public async Task<LeaveRequest> ApproveAsync(int id, int decidedBy)
    {
        var req = await _db.LeaveRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("Leave request not found.");
        if (req.Status != "Pending")
            throw new InvalidOperationException($"Request is already {req.Status}.");

        var days = DayCount(req.StartDate, req.EndDate);
        var balance = await GetOrCreateBalanceAsync(req.UserId, req.StartDate.Year);
        if (balance.UsedDays + days > balance.TotalDays)
            throw new InvalidOperationException("Insufficient leave balance.");

        balance.UsedDays += days;
        req.Status = "Approved";
        req.DecidedBy = decidedBy;
        await _db.SaveChangesAsync();
        return req;
    }

    public async Task<LeaveRequest> RejectAsync(int id, int decidedBy)
    {
        var req = await _db.LeaveRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("Leave request not found.");
        if (req.Status != "Pending")
            throw new InvalidOperationException($"Request is already {req.Status}.");

        req.Status = "Rejected";
        req.DecidedBy = decidedBy;
        await _db.SaveChangesAsync();
        return req;
    }
}
