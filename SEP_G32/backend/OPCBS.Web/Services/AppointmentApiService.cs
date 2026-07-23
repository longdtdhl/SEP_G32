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

    public async Task<(bool Success, string? Error)> BookAsync(CreateAppointmentDto dto)
        => await PostAsync(ApiRoutes.Appointments, dto);

    public async Task<(bool Success, string? Error)> RescheduleAsync(Guid id, RescheduleAppointmentDto dto)
        => await PutAsync($"{ApiRoutes.Appointments}/reschedule/{id}", dto);

    public async Task<(bool Success, string? Error)> CancelAsync(Guid id, CancelAppointmentDto dto)
        => await PutAsync($"{ApiRoutes.Appointments}/cancel/{id}", dto);

    public async Task<(bool Success, string? Error)> ConfirmAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/approve/{id}");

    public async Task<(bool Success, string? Error)> CompleteAsync(Guid id)
        => await PutAsync($"{ApiRoutes.Appointments}/complete/{id}");

    public async Task<(List<AppointmentListItemDto> Data, string? Error)> TrackAsync(TrackAppointmentRequestDto dto)
    {
        var (data, error) = await PostAsync<List<AppointmentListItemDto>>(ApiRoutes.AppointmentTrack, dto);
        return (data ?? new(), error);
    }

    public async Task<(AvailableSlotsDto? Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, string? date = null)
    {
        var url = $"{ApiRoutes.Doctors}/{doctorId}/schedule";
        if (!string.IsNullOrEmpty(date)) url += $"?date={date}";
        var (data, _, error) = await GetAsync<AvailableSlotsDto>(url);
        return (data, error);
    }

    private static string BuildFilterUrl(string baseUrl, AppointmentFilterDto? filter)
    {
        if (filter == null) return baseUrl;
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(filter.Status)) parts.Add($"status={filter.Status}");
        if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
        if (filter.FromDate.HasValue) parts.Add($"fromDate={filter.FromDate:yyyy-MM-dd}");
        if (filter.ToDate.HasValue) parts.Add($"toDate={filter.ToDate:yyyy-MM-dd}");
        parts.Add($"page={filter.Page}");
        parts.Add($"pageSize={filter.PageSize}");
        return parts.Count > 0 ? $"{baseUrl}?{string.Join("&", parts)}" : baseUrl;
    }
}
