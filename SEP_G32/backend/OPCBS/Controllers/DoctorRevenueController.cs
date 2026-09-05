using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/doctor/revenue")]
[Authorize(Roles = RoleConstants.Doctor)]
public class DoctorRevenueController : ControllerBase
{
    private readonly IDoctorRevenueService _revenueService;

    public DoctorRevenueController(IDoctorRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>GET /api/v1/doctor/revenue/overview - Get doctor earnings overview and charts</summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? period,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse.ErrorResponse("Authentication required."));

        var result = await _revenueService.GetRevenueOverviewAsync(userId.Value, startDate, endDate, period, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/doctor/revenue/transactions - Get doctor transaction ledger with pagination</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? search,
        [FromQuery] string? settlementStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse.ErrorResponse("Authentication required."));

        var result = await _revenueService.GetTransactionsAsync(userId.Value, search, settlementStatus, page, pageSize, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
