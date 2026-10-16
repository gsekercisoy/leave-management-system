using System.Security.Claims;
using LeaveApi.Data;
using LeaveApi.Dtos;
using LeaveApi.Models;
using LeaveApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveApi.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeavesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LeaveService _leaves;

    public LeavesController(AppDbContext db, LeaveService leaves)
    {
        _db = db;
        _leaves = leaves;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<LeaveRequest>>> Mine()
        => await _db.LeaveRequests
            .Where(l => l.UserId == CurrentUserId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    [HttpGet("all")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<List<LeaveRequest>>> All()
        => await _db.LeaveRequests
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> Create(CreateLeaveDto dto)
    {
        try
        {
            var req = await _leaves.CreateAsync(CurrentUserId, dto.StartDate, dto.EndDate, dto.Reason);
            return CreatedAtAction(nameof(Mine), req);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<LeaveRequest>> Approve(int id)
    {
        try
        {
            return await _leaves.ApproveAsync(id, CurrentUserId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<LeaveRequest>> Reject(int id)
    {
        try
        {
            return await _leaves.RejectAsync(id, CurrentUserId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
