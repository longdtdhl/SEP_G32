using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

// --- Schedule ---
public class ScheduleApiService : ApiServiceBase, IScheduleApiService
{
    public ScheduleApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<ScheduleDto> Data, string? Error)> GetMySchedulesAsync()
    {
        var (data, _, error) = await GetAsync<List<ScheduleDto>>(ApiRoutes.Schedules);
        return (data ?? new(), error);
    }
    public async Task<(ScheduleDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<ScheduleDto>($"{ApiRoutes.Schedules}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> CreateAsync(CreateScheduleDto dto) => await PostAsync(ApiRoutes.Schedules, dto);
    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateScheduleDto dto) => await PutAsync(ApiRoutes.Schedules, dto);
    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.Schedules}/{id}");
    public async Task<(List<DayOffDto> Data, string? Error)> GetDaysOffAsync()
    {
        var (data, _, error) = await GetAsync<List<DayOffDto>>(ApiRoutes.ScheduleDaysOff);
        return (data ?? new(), error);
    }
    public async Task<(bool Success, string? Error)> CreateDayOffAsync(CreateDayOffDto dto) => await PostAsync($"{ApiRoutes.Schedules}/unavailable-date", dto);
    public async Task<(bool Success, string? Error)> DeleteDayOffAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.ScheduleDaysOff}/{id}");
}

// --- Consultation Record ---
public class ConsultationRecordApiService : ApiServiceBase, IConsultationRecordApiService
{
    public ConsultationRecordApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<ConsultationRecordDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<ConsultationRecordDto>>($"{ApiRoutes.ConsultationRecords}/doctor?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(List<ConsultationRecordDto> Data, PaginationDto? Pagination, string? Error)> GetMyRecordsAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<ConsultationRecordDto>>($"{ApiRoutes.ConsultationRecords}/my-records?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(ConsultationRecordDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<ConsultationRecordDto>($"{ApiRoutes.ConsultationRecords}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> CreateAsync(CreateConsultationRecordDto dto) => await PostAsync(ApiRoutes.ConsultationRecords, dto);
    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateConsultationRecordDto dto) => await PutAsync($"{ApiRoutes.ConsultationRecords}/{id}", dto);
}

// --- Treatment Package ---
public class TreatmentPackageApiService : ApiServiceBase, ITreatmentPackageApiService
{
    public TreatmentPackageApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<TreatmentPackageDto>>($"{ApiRoutes.TreatmentPackages}?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetMyPackagesAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<TreatmentPackageDto>>($"{ApiRoutes.TreatmentPackages}/my-packages?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(TreatmentPackageDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<TreatmentPackageDto>($"{ApiRoutes.TreatmentPackages}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> CreateAsync(CreateTreatmentPackageDto dto) => await PostAsync(ApiRoutes.TreatmentPackages, dto);
    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateTreatmentPackageDto dto) => await PutAsync($"{ApiRoutes.TreatmentPackages}/{id}", dto);
    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.TreatmentPackages}/{id}");
    public async Task<(bool Success, string? Error)> AcceptAsync(Guid id) => await PutAsync($"{ApiRoutes.TreatmentPackages}/accept/{id}");
    public async Task<(bool Success, string? Error)> RejectAsync(Guid id, string? reason = null) => await PutAsync($"{ApiRoutes.TreatmentPackages}/reject/{id}", reason);
}

// --- Review ---
public class ReviewApiService : ApiServiceBase, IReviewApiService
{
    public ReviewApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(bool Success, string? Error)> CreateAsync(CreateReviewDto dto) => await PostAsync(ApiRoutes.Reviews, dto);
    public async Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetMyReviewsAsync(int page = 1)
    {
        var (data, pagination, error) = await GetAsync<List<ReviewDto>>($"{ApiRoutes.Reviews}/my?page={page}");
        return (data ?? new(), pagination, error);
    }
}

// --- Verification ---
public class VerificationApiService : ApiServiceBase, IVerificationApiService
{
    public VerificationApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(VerificationDto? Data, string? Error)> GetMyVerificationAsync()
    {
        var (data, _, error) = await GetAsync<VerificationDto>($"{ApiRoutes.Verification}/my");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> SubmitAsync(SubmitVerificationDto dto) => await PostAsync(ApiRoutes.Verification, dto);
    public async Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetPendingAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<VerificationDto>>($"{ApiRoutes.Verification}?status=Pending&page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(VerificationDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<VerificationDto>($"{ApiRoutes.Verification}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> ReviewAsync(Guid id, ReviewVerificationDto dto) => await PutAsync($"{ApiRoutes.Verification}/{id}/review", dto);
}

// --- Service Package ---
public class ServicePackageApiService : ApiServiceBase, IServicePackageApiService
{
    public ServicePackageApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<ServicePackageDto> Data, string? Error)> GetAllAsync()
    {
        var (data, _, error) = await GetAsync<List<ServicePackageDto>>(ApiRoutes.ServicePackages);
        return (data ?? new(), error);
    }
    public async Task<(ServicePackageDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<ServicePackageDto>($"{ApiRoutes.ServicePackages}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> CreateAsync(CreateServicePackageDto dto) => await PostAsync(ApiRoutes.ServicePackages, dto);
    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateServicePackageDto dto) => await PutAsync($"{ApiRoutes.ServicePackages}/{id}", dto);
    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.ServicePackages}/{id}");
}

// --- Subscription ---
public class SubscriptionApiService : ApiServiceBase, ISubscriptionApiService
{
    public SubscriptionApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(SubscriptionDto? Data, string? Error)> GetCurrentAsync()
    {
        var (data, _, error) = await GetAsync<SubscriptionDto>($"{ApiRoutes.Subscriptions}/current");
        return (data, error);
    }
    public async Task<(List<SubscriptionDto> Data, string? Error)> GetHistoryAsync()
    {
        var (data, _, error) = await GetAsync<List<SubscriptionDto>>($"{ApiRoutes.Subscriptions}/history");
        return (data ?? new(), error);
    }
    public async Task<(bool Success, string? Error)> SubscribeAsync(CreateSubscriptionDto dto) => await PostAsync(ApiRoutes.Subscriptions, dto);
}

// --- Admin ---
public class AdminApiService : ApiServiceBase, IAdminApiService
{
    public AdminApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync()
    {
        var (data, _, error) = await GetAsync<DashboardStatsDto>("api/v1/admin/dashboard");
        return (data, error);
    }
    public async Task<(List<UserListItemDto> Data, PaginationDto? Pagination, string? Error)> GetUsersAsync(UserFilterDto? filter = null)
    {
        var url = ApiRoutes.AdminUsers;
        if (filter != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrEmpty(filter.Role)) parts.Add($"role={filter.Role}");
            if (filter.IsActive.HasValue) parts.Add($"isActive={filter.IsActive}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            url += "?" + string.Join("&", parts);
        }
        var (data, pagination, error) = await GetAsync<List<UserListItemDto>>(url);
        return (data ?? new(), pagination, error);
    }
    public async Task<(UserListItemDto? Data, string? Error)> GetUserByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<UserListItemDto>($"{ApiRoutes.AdminUsers}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> LockUserAsync(Guid id) => await PutAsync($"{ApiRoutes.AdminUsers}/{id}/lock");
    public async Task<(bool Success, string? Error)> UnlockUserAsync(Guid id) => await PutAsync($"{ApiRoutes.AdminUsers}/{id}/unlock");
    public async Task<(List<RoleDto> Data, string? Error)> GetRolesAsync()
    {
        var (data, _, error) = await GetAsync<List<RoleDto>>(ApiRoutes.AdminRoles);
        return (data ?? new(), error);
    }
    public async Task<(List<AuditLogDto> Data, PaginationDto? Pagination, string? Error)> GetAuditLogsAsync(string? entityName = null, int page = 1, int pageSize = 20)
    {
        var url = $"{ApiRoutes.AdminAuditLogs}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(entityName)) url += $"&entityName={Uri.EscapeDataString(entityName)}";
        var (data, pagination, error) = await GetAsync<List<AuditLogDto>>(url);
        return (data ?? new(), pagination, error);
    }
}

// --- Customer Support ---
public class CustomerSupportApiService : ApiServiceBase, ICustomerSupportApiService
{
    public CustomerSupportApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync()
    {
        var (data, _, error) = await GetAsync<DashboardStatsDto>($"{ApiRoutes.CSDoctorApplications}/../dashboard");
        return (data, error);
    }
    public async Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetDoctorApplicationsAsync(int page = 1, string? status = null)
    {
        var url = $"{ApiRoutes.CSDoctorApplications}?page={page}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        var (data, pagination, error) = await GetAsync<List<VerificationDto>>(url);
        return (data ?? new(), pagination, error);
    }
    public async Task<(VerificationDto? Data, string? Error)> GetApplicationByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<VerificationDto>($"{ApiRoutes.CSDoctorApplications}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> ReviewApplicationAsync(Guid id, ReviewVerificationDto dto) => await PutAsync($"{ApiRoutes.CSDoctorApplications}/{id}/review", dto);
    public async Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetBlogModerationQueueAsync(int page = 1)
    {
        var (data, pagination, error) = await GetAsync<List<BlogListItemDto>>($"{ApiRoutes.CSBlogModeration}?page={page}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(BlogDto? Data, string? Error)> GetBlogForModerationAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<BlogDto>($"{ApiRoutes.CSBlogModeration}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> ApproveBlogAsync(Guid id) => await PutAsync($"{ApiRoutes.CSBlogModeration}/{id}/approve");
    public async Task<(bool Success, string? Error)> RejectBlogAsync(Guid id, string? reason = null) => await PutAsync($"{ApiRoutes.CSBlogModeration}/{id}/reject", new { reason });
}

// --- Business Manager ---
public class BusinessManagerApiService : ApiServiceBase, IBusinessManagerApiService
{
    public BusinessManagerApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync()
    {
        var (data, _, error) = await GetAsync<DashboardStatsDto>("api/v1/business-manager/dashboard");
        return (data, error);
    }
    // Service Packages
    public async Task<(List<ServicePackageDto> Data, string? Error)> GetServicePackagesAsync()
    {
        var (data, _, error) = await GetAsync<List<ServicePackageDto>>(ApiRoutes.ServicePackages);
        return (data ?? new(), error);
    }
    public async Task<(ServicePackageDto? Data, string? Error)> GetServicePackageByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<ServicePackageDto>($"{ApiRoutes.ServicePackages}/{id}");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> CreateServicePackageAsync(CreateServicePackageDto dto) => await PostAsync(ApiRoutes.ServicePackages, dto);
    public async Task<(bool Success, string? Error)> UpdateServicePackageAsync(Guid id, UpdateServicePackageDto dto) => await PutAsync($"{ApiRoutes.ServicePackages}/{id}", dto);
    public async Task<(bool Success, string? Error)> DeleteServicePackageAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.ServicePackages}/{id}");
    // Specializations
    public async Task<(List<SpecializationDto> Data, string? Error)> GetSpecializationsAsync()
    {
        var (data, _, error) = await GetAsync<List<SpecializationDto>>(ApiRoutes.Specializations);
        return (data ?? new(), error);
    }
    public async Task<(bool Success, string? Error)> CreateSpecializationAsync(CreateSpecializationDto dto) => await PostAsync(ApiRoutes.Specializations, dto);
    public async Task<(bool Success, string? Error)> UpdateSpecializationAsync(Guid id, CreateSpecializationDto dto) => await PutAsync($"{ApiRoutes.Specializations}/{id}", dto);
    public async Task<(bool Success, string? Error)> DeleteSpecializationAsync(Guid id) => await base.DeleteAsync($"{ApiRoutes.Specializations}/{id}");
}
