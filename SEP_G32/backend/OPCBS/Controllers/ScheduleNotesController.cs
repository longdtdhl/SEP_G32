using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/schedule-notes")]
[Authorize(Roles = "Doctor")]
public class ScheduleNotesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleNotesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>GET /api/v1/schedule-notes — Get list of doctor's schedule notes</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotes(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _scheduleService.GetNotesAsync(userId.Value, search, category, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>POST /api/v1/schedule-notes — Create a new schedule note</summary>
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] CreateScheduleNoteDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _scheduleService.CreateNoteAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/schedule-notes/{id} — Update existing schedule note</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateScheduleNoteDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _scheduleService.UpdateNoteAsync(id, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/schedule-notes/{id} — Delete schedule note</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _scheduleService.DeleteNoteAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var guid) ? guid : null;
    }
}
