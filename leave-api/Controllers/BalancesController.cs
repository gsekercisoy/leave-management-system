using System.Security.Claims;
using LeaveApi.Models;
using LeaveApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveApi.Controllers;

[ApiController]
[Route("api/balances")]
[Authorize]
public class BalancesController : ControllerBase
{
    private readonly LeaveService _leaves;

    public BalancesController(LeaveService leaves) => _leaves = leaves;

    [HttpGet("me")]
    public async Task<ActionResult<LeaveBalance>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _leaves.GetOrCreateBalanceAsync(userId, DateTime.UtcNow.Year);
    }
}
