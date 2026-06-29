using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class DoctorApiService : ApiServiceBase, IDoctorApiService
{
    public DoctorApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<DoctorListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(DoctorFilterDto? filter = null)
    {
        var query = ApiRoutes.Doctors;
        if (filter != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrEmpty(filter.Specialization)) parts.Add($"specialization={Uri.EscapeDataString(filter.Specialization)}");
            if (filter.MinRating.HasValue) parts.Add($"minRating={filter.MinRating}");
            if (filter.MaxFee.HasValue) parts.Add($"maxFee={filter.MaxFee}");
            if (!string.IsNullOrEmpty(filter.Gender)) parts.Add($"gender={filter.Gender}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            if (parts.Count > 0) query += "?" + string.Join("&", parts);
        }
        var (data, pagination, error) = await GetAsync<List<DoctorListItemDto>>(query);
        return (data ?? new List<DoctorListItemDto>(), pagination, error);
    }

    public async Task<(DoctorDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<DoctorDto>($"{ApiRoutes.Doctors}/{id}");
        return (data, error);
    }

    public async Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetReviewsAsync(Guid doctorId, int page = 1)
    {
        var (data, pagination, error) = await GetAsync<List<ReviewDto>>($"{ApiRoutes.Doctors}/{doctorId}/reviews?page={page}");
        return (data ?? new List<ReviewDto>(), pagination, error);
    }

    public async Task<(List<TimeSlotDto> Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, DateTime date)
    {
        var (data, _, error) = await GetAsync<List<TimeSlotDto>>($"{ApiRoutes.Doctors}/{doctorId}/slots?date={date:yyyy-MM-dd}");
        return (data ?? new List<TimeSlotDto>(), error);
    }
}
