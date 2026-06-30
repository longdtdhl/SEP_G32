using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IAppointmentApiService
{
    Task<(List<AppointmentListItemDto> Data, PaginationDto? Pagination, string? Error)> GetMyAppointmentsAsync(AppointmentFilterDto? filter = null);
    Task<(List<AppointmentListItemDto> Data, PaginationDto? Pagination, string? Error)> GetDoctorAppointmentsAsync(AppointmentFilterDto? filter = null);
    Task<(AppointmentDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> BookAsync(CreateAppointmentDto dto);
    Task<(bool Success, string? Error)> RescheduleAsync(Guid id, RescheduleAppointmentDto dto);
    Task<(bool Success, string? Error)> CancelAsync(Guid id, CancelAppointmentDto dto);
    Task<(bool Success, string? Error)> ConfirmAsync(Guid id);
    Task<(bool Success, string? Error)> CompleteAsync(Guid id);
    Task<(List<AppointmentListItemDto> Data, string? Error)> TrackAsync(TrackAppointmentRequestDto dto);
    Task<(AvailableSlotsDto? Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, string? date = null);
}
