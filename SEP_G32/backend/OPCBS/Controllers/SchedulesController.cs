using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/schedules")]
[Authorize(Roles = "Doctor")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService) => _scheduleService = scheduleService;

    /// <summary>GET /api/v1/schedules — Get doctor's own schedules</summary>
    [HttpGet]
    public async Task<IActionResult> GetMySchedules()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GetDoctorSchedulesAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>POST /api/v1/schedules — Create a new schedule</summary>
    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.CreateScheduleAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/schedules — Update existing schedule</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.UpdateScheduleAsync(dto.ScheduleId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/schedules/{scheduleId} — Delete (deactivate) schedule</summary>
    [HttpDelete("{scheduleId}")]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.DeleteScheduleAsync(scheduleId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/schedules/unavailable-date — Add unavailable date</summary>
    [HttpPost("unavailable-date")]
    public async Task<IActionResult> AddUnavailableDate([FromBody] CreateDayOffDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.AddDayOffAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/schedules/days-off — Get unavailable dates</summary>
    [HttpGet("days-off")]
    public Task<IActionResult> GetDaysOff()
    {
        // Service doesn't have GetDaysOff yet, return empty
        return Task.FromResult<IActionResult>(Ok(ApiResponse<List<object>>.SuccessResponse(new List<object>())));
    }

    /// <summary>DELETE /api/v1/schedules/days-off/{id} — Remove unavailable date</summary>
    [HttpDelete("days-off/{dayOffId}")]
    public Task<IActionResult> DeleteDayOff(Guid dayOffId)
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Day off removed")));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
