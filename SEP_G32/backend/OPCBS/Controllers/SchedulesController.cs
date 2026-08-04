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

    /// <summary>GET /api/v1/schedules/calendar — Get calendar events within date range</summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GetCalendarEventsAsync(userId.Value, start, end);
        return Ok(result);
    }

    /// <summary>GET /api/v1/schedules/days-off — Get unavailable dates</summary>
    [HttpGet("days-off")]
    public async Task<IActionResult> GetDaysOff()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GetDoctorDaysOffAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>DELETE /api/v1/schedules/days-off/{id} — Remove unavailable date</summary>
    [HttpDelete("days-off/{dayOffId}")]
    public async Task<IActionResult> DeleteDayOff(Guid dayOffId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.DeleteDayOffAsync(dayOffId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/schedules/slots — Get doctor's own generated slots</summary>
    [HttpGet("slots")]
    public async Task<IActionResult> GetMySlots([FromQuery] DateOnly? date)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GetDoctorAllSlotsAsync(userId.Value, date);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/schedules/slots/{slotId}/toggle-block — Toggle block slot</summary>
    [HttpPut("slots/{slotId}/toggle-block")]
    public async Task<IActionResult> ToggleBlockSlot(Guid slotId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.ToggleBlockSlotAsync(slotId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/schedules/slots — Create individual slot</summary>
    [HttpPost("slots")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.CreateSlotAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/schedules/slots/{slotId} — Delete individual slot</summary>
    [HttpDelete("slots/{slotId}")]
    public async Task<IActionResult> DeleteSlot(Guid slotId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.DeleteSlotAsync(slotId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/schedules/slots/{slotId}/notes — Update slot notes</summary>
    [HttpPut("slots/{slotId}/notes")]
    public async Task<IActionResult> UpdateSlotNotes(Guid slotId, [FromBody] UpdateSlotNotesRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.UpdateSlotNotesAsync(slotId, userId.Value, request.Notes);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/schedules/slots/{slotId} — Update slot (time, notes, capacity)</summary>
    [HttpPut("slots/{slotId}")]
    public async Task<IActionResult> UpdateSlot(Guid slotId, [FromBody] UpdateSlotDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.UpdateSlotAsync(slotId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/schedules/eligible-treatment-patients — Get patients eligible for treatment appointment</summary>
    [HttpGet("eligible-treatment-patients")]
    public async Task<IActionResult> GetEligibleTreatmentPatients()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GetEligibleTreatmentPatientsAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>POST /api/v1/schedules/treatment-appointments — Create slot + appointment + link treatment session in transaction</summary>
    [HttpPost("treatment-appointments")]
    public async Task<IActionResult> CreateTreatmentAppointment([FromBody] CreateTreatmentAppointmentDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.CreateTreatmentAppointmentAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/schedules/preview-weekly — Preview weekly schedule slot generation</summary>
    [HttpPost("preview-weekly")]
    public async Task<IActionResult> PreviewWeeklySchedule([FromBody] WeeklyScheduleConfigDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.PreviewWeeklyScheduleAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/schedules/generate-weekly — Generate slots based on weekly schedule config</summary>
    [HttpPost("generate-weekly")]
    public async Task<IActionResult> GenerateWeeklySchedule([FromBody] WeeklyScheduleConfigDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _scheduleService.GenerateWeeklyScheduleAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
