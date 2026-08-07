using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class AppointmentApiService : ApiServiceBase, IAppointmentApiService
{
    public AppointmentApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<AppointmentListItemDto> Data, PaginationDto? Pagination, string? Error)> GetMyAppointmentsAsync(AppointmentFilterDto? filter = null)
    {
        var url = BuildFilterUrl($"{ApiRoutes.Appointments}/my-appointments", filter);
        var (data, pagination, error) = await GetAsync<List<AppointmentListItemDto>>(url);
        return (data ?? new(), pagination, error);
    }

    public async Task<(List<AppointmentListItemDto> Data, PaginationDto? Pagination, string? Error)> GetDoctorAppointmentsAsync(AppointmentFilterDto? filter = null)
    {
        var url = BuildFilterUrl($"{ApiRoutes.Appointments}/doctor", filter);
        var (data, pagination, error) = await GetAsync<List<AppointmentListItemDto>>(url);
        return (data ?? new(), pagination, error);
    }

    public async Task<(AppointmentDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<AppointmentDto>($"{ApiRoutes.Appointments}/{id}");
        return (data, error);
    }

    public async Task<(AppointmentClinicalContextDto? Data, string? Error)> GetClinicalContextAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<AppointmentClinicalContextDto>($"{ApiRoutes.Appointments}/{id}/clinical-context");
        return (data, error);
    }

    public async Task<(AppointmentDto? Data, string? Error)> BookAsync(CreateAppointmentDto dto)
    {
        var (data, error) = await PostAsync<AppointmentDto>(ApiRoutes.Appointments, dto);
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> RescheduleAsync(Guid id, RescheduleAppointmentDto dto)
        => await PutAsync($"{ApiRoutes.Appointments}/reschedule/{id}", dto);

    public async Task<(bool Success, string? Error)> ApproveRescheduleAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/{id}/approve-reschedule");

    public async Task<(bool Success, string? Error)> RejectRescheduleAsync(Guid id, string? reason = null)
        => await PutAsync($"{ApiRoutes.Appointments}/{id}/reject-reschedule", new { reason });

    public async Task<(bool Success, string? Error)> CancelAsync(Guid id, CancelAppointmentDto dto)
        => await PutAsync($"{ApiRoutes.Appointments}/cancel/{id}", dto);

    public async Task<(bool Success, string? Error)> ConfirmAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/approve/{id}");

    public async Task<(bool Success, string? Error)> ConfirmCompletionAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/{id}/confirm-completion");

    public async Task<(bool Success, string? Error)> StartAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/start/{id}");

    public async Task<(bool Success, string? Error)> CompleteAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/complete/{id}");

    public async Task<(AppointmentDto? Data, string? Error)> TrackAsync(TrackAppointmentRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BookingCode) || string.IsNullOrWhiteSpace(dto.Email))
            return (null, "Vui lòng nhập đầy đủ Mã đặt lịch và Email.");

        var url = $"{ApiRoutes.Appointments}/track/{Uri.EscapeDataString(dto.BookingCode.Trim())}?email={Uri.EscapeDataString(dto.Email.Trim())}";
        var (data, _, error) = await GetAsync<AppointmentDto>(url);
        return (data, error);
    }

    public async Task<(bool Success, string? Message, string? Error)> ResendConfirmationAsync(ResendConfirmationRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BookingCode) || string.IsNullOrWhiteSpace(dto.Email))
            return (false, null, "Mã đặt lịch và Email là bắt buộc.");

        var (res, error) = await PostAsync<dynamic>($"{ApiRoutes.Appointments}/resend-confirmation", dto);
        if (error != null) return (false, null, error);
        return (true, "Đã gửi lại email xác nhận lịch hẹn thành công.", null);
    }

    public async Task<(bool Success, string? Error)> ConfirmGuestAppointmentAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "The confirmation link is invalid.");

        return await PostAsync($"{ApiRoutes.Appointments}/guest/confirm", new ConfirmGuestAppointmentDto { Token = token.Trim() });
    }

    public async Task<(AvailableSlotsDto? Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, string? date = null)
    {
        var url = $"{ApiRoutes.Doctors}/{doctorId}/schedule";
        if (!string.IsNullOrEmpty(date)) url += $"?date={date}";
        var (data, _, error) = await GetAsync<AvailableSlotsDto>(url);
        return (data, error);
    }

    public async Task<(int Count, string? Error)> GetVisitCountAsync(Guid doctorId)
    {
        var (data, _, error) = await GetAsync<int>($"{ApiRoutes.Appointments}/visit-count/{doctorId}");
        return (data, error);
    }

    public async Task<(bool IsReturning, string? Error)> IsReturningAsync(Guid doctorId)
    {
        var (data, _, error) = await GetAsync<bool>($"{ApiRoutes.Appointments}/is-returning/{doctorId}");
        return (data, error);
    }

    private static string BuildFilterUrl(string baseUrl, AppointmentFilterDto? filter)
    {
        if (filter == null) return baseUrl;
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(filter.Status)) parts.Add($"status={filter.Status}");
        if (!string.IsNullOrEmpty(filter.View)) parts.Add($"view={filter.View}");
        if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
        if (filter.FromDate.HasValue) parts.Add($"fromDate={filter.FromDate:yyyy-MM-dd}");
        if (filter.ToDate.HasValue) parts.Add($"toDate={filter.ToDate:yyyy-MM-dd}");
        parts.Add($"page={filter.Page}");
        parts.Add($"pageSize={filter.PageSize}");
        return parts.Count > 0 ? $"{baseUrl}?{string.Join("&", parts)}" : baseUrl;
    }
}
