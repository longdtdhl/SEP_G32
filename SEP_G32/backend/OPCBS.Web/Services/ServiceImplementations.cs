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
    
    public async Task<(AvailableSlotsDto? Data, string? Error)> GetMySlotsAsync(DateOnly? date = null)
    {
        var url = $"{ApiRoutes.Schedules}/slots";
        if (date.HasValue) url += $"?date={date.Value:yyyy-MM-dd}";
        var (data, _, error) = await GetAsync<AvailableSlotsDto>(url);
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> ToggleBlockSlotAsync(Guid slotId) => await PutAsync($"{ApiRoutes.Schedules}/slots/{slotId}/toggle-block");

    public async Task<(AppointmentSlotDto? Data, string? Error)> CreateSlotAsync(CreateSlotDto dto)
    {
        var (data, error) = await PostAsync<AppointmentSlotDto>($"{ApiRoutes.Schedules}/slots", dto);
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> DeleteSlotAsync(Guid slotId) => await base.DeleteAsync($"{ApiRoutes.Schedules}/slots/{slotId}");
    public async Task<(bool Success, string? Error)> UpdateSlotNotesAsync(Guid slotId, string? notes) => await PutAsync($"{ApiRoutes.Schedules}/slots/{slotId}/notes", new { Notes = notes });
    public async Task<(bool Success, string? Error)> UpdateSlotAsync(Guid slotId, UpdateSlotDto dto) => await PutAsync($"{ApiRoutes.Schedules}/slots/{slotId}", dto);
}

// --- Consultation Record ---
public class PatientRecordApiService : ApiServiceBase, IPatientRecordApiService
{
    public PatientRecordApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<PatientRecordDto> Data, string? Error)> GetAllAsync()
    {
        var (data, _, error) = await GetAsync<List<PatientRecordDto>>(ApiRoutes.PatientRecords);
        return (data ?? new(), error);
    }

    public async Task<(List<PatientRecordDto> Data, string? Error)> GetMyPatientsAsync()
    {
        var (data, _, error) = await GetAsync<List<PatientRecordDto>>($"{ApiRoutes.PatientRecords}/my-patients");
        return (data ?? new(), error);
    }

    public async Task<(List<PatientRecordDto> Data, string? Error)> GetSystemPatientsAsync()
    {
        var (data, _, error) = await GetAsync<List<PatientRecordDto>>($"{ApiRoutes.PatientRecords}/system");
        return (data ?? new(), error);
    }

    public async Task<(List<PatientRecordDto> Data, string? Error)> GetGuestPatientsAsync()
    {
        var (data, _, error) = await GetAsync<List<PatientRecordDto>>($"{ApiRoutes.PatientRecords}/guest");
        return (data ?? new(), error);
    }

    public async Task<(PatientRecordDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<PatientRecordDto>($"{ApiRoutes.PatientRecords}/{id}");
        return (data, error);
    }

    public async Task<(PatientRecordDto? Data, string? Error)> GetByUserIdAsync(Guid userId)
    {
        var (data, _, error) = await GetAsync<PatientRecordDto>($"{ApiRoutes.PatientRecords}/user/{userId}");
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(CreatePatientRecordDto dto)
    {
        return await PostAsync(ApiRoutes.PatientRecords, dto);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdatePatientRecordDto dto)
    {
        return await PutAsync($"{ApiRoutes.PatientRecords}/{id}", dto);
    }
}

public class ConsultationNoteApiService : ApiServiceBase, IConsultationNoteApiService
{
    public ConsultationNoteApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<ConsultationNoteDto>>($"{ApiRoutes.ConsultationNotes}/doctor?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetMyRecordsAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<ConsultationNoteDto>>($"{ApiRoutes.ConsultationNotes}/my-records?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }
    public async Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetByPatientRecordIdAsync(Guid patientRecordId, int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<ConsultationNoteDto>>($"{ApiRoutes.ConsultationNotes}/patient-record/{patientRecordId}?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }

    public async Task<(ConsultationNoteDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<ConsultationNoteDto>($"{ApiRoutes.ConsultationNotes}/{id}");
        return (data, error);
    }
    public async Task<(ConsultationNoteDto? Data, string? Error)> GetByAppointmentIdAsync(Guid appointmentId)
    {
        var (data, _, error) = await GetAsync<List<ConsultationNoteDto>>($"{ApiRoutes.ConsultationNotes}/appointment/{appointmentId}");
        return (data != null && data.Any() ? data.First() : null, error);
    }
    public async Task<(bool Success, string? Error)> CreateAsync(CreateConsultationNoteDto dto) => await PostAsync(ApiRoutes.ConsultationNotes, dto);
    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateConsultationNoteDto dto) => await PutAsync($"{ApiRoutes.ConsultationNotes}/{id}", dto);
    public async Task<(bool Success, string? Error)> ConfirmAsync(Guid recordId) => await PostAsync($"{ApiRoutes.ConsultationNotes}/{recordId}/confirm", new { });
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
    public async Task<(bool Success, string? Error)> CancelAsync(Guid id, string? reason = null) => await PutAsync($"{ApiRoutes.TreatmentPackages}/cancel/{id}", reason);
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
        var (data, _, error) = await GetAsync<VerificationDto>($"{ApiRoutes.Verification}/status");
        return (data, error);
    }
    public async Task<(bool Success, string? Error)> SubmitAsync(SubmitVerificationDto dto) => await PostAsync($"{ApiRoutes.Verification}/submit", dto);
    public async Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetPendingAsync(int page = 1, int pageSize = 10)
    {
        var (data, pagination, error) = await GetAsync<List<VerificationDto>>($"{ApiRoutes.Verification}/pending?page={page}&pageSize={pageSize}");
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
    
    public async Task<(SubscriptionDto? Data, string? Error)> PurchaseAsync(Guid packageId, string returnUrl)
    {
        var (data, error) = await PostAsync<SubscriptionDto>($"{ApiRoutes.Payments}/create-vnpay", new { ServicePackageId = packageId, ReturnUrl = returnUrl });
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> ProcessCallbackAsync(IDictionary<string, string> queryParams)
    {
        var queryStr = string.Join("&", queryParams.Select(kv => $"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}"));
        var (_, _, error) = await GetAsync<object>($"{ApiRoutes.Payments}/callback?{queryStr}");
        return (error == null, error);
    }
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
    public async Task<(Dictionary<string, string> Data, string? Error)> GetSystemSettingsAsync()
    {
        var (data, _, error) = await GetAsync<Dictionary<string, string>>("api/v1/admin/settings");
        return (data ?? new(), error);
    }
    public async Task<(bool Success, string? Error)> UpdateSystemSettingsAsync(Dictionary<string, string> settings)
    {
        return await PutAsync("api/v1/admin/settings", settings);
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
        var (data, _, error) = await GetAsync<List<ServicePackageDto>>($"{ApiRoutes.ServicePackages}?includeInactive=true");
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
        var (data, _, error) = await GetAsync<List<SpecializationDto>>("api/v1/doctors/specializations");
        return (data ?? new(), error);
    }
    public async Task<(bool Success, string? Error)> CreateSpecializationAsync(CreateSpecializationDto dto) => await PostAsync("api/v1/business-manager/specializations", dto);
    public async Task<(bool Success, string? Error)> UpdateSpecializationAsync(Guid id, CreateSpecializationDto dto) => await PutAsync($"api/v1/business-manager/specializations/{id}", dto);
    public async Task<(bool Success, string? Error)> DeleteSpecializationAsync(Guid id) => await base.DeleteAsync($"api/v1/business-manager/specializations/{id}");
}

public class PsychometricApiService : ApiServiceBase, IPsychometricApiService
{
    public PsychometricApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<PsychometricTestDto> Data, string? Error)> GetTestsAsync()
    {
        var (data, _, error) = await GetAsync<List<PsychometricTestDto>>($"{ApiRoutes.Psychometrics}/tests");
        return (data ?? new(), error);
    }

    public async Task<(List<PsychometricQuestionDto> Data, string? Error)> GetQuestionsAsync(Guid testId)
    {
        var (data, _, error) = await GetAsync<List<PsychometricQuestionDto>>($"{ApiRoutes.Psychometrics}/tests/{testId}/questions");
        return (data ?? new(), error);
    }

    public async Task<(PsychometricSubmissionDto? Data, string? Error)> SubmitTestAsync(SubmitTestDto dto)
    {
        var (data, error) = await PostAsync<PsychometricSubmissionDto>($"{ApiRoutes.Psychometrics}/submissions", dto);
        return (data, error);
    }

    public async Task<(PsychometricSubmissionDto? Data, string? Error)> GetSubmissionByAppointmentAsync(Guid appointmentId)
    {
        var (data, _, error) = await GetAsync<PsychometricSubmissionDto>($"{ApiRoutes.Psychometrics}/submissions/appointment/{appointmentId}");
        return (data, error);
    }

    public async Task<(PsychometricSubmissionDto? Data, string? Error)> GetSubmissionByIdAsync(Guid submissionId)
    {
        var (data, _, error) = await GetAsync<PsychometricSubmissionDto>($"{ApiRoutes.Psychometrics}/submissions/{submissionId}");
        return (data, error);
    }

    public async Task<(List<PsychometricSubmissionDto> Data, string? Error)> GetMySubmissionsAsync()
    {
        var (data, _, error) = await GetAsync<List<PsychometricSubmissionDto>>($"{ApiRoutes.Psychometrics}/submissions/my");
        return (data ?? new(), error);
    }
}

// --- Notification ---
public class NotificationApiService : ApiServiceBase, INotificationApiService
{
    public NotificationApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<NotificationDto> Data, PaginationDto? Pagination, string? Error)> GetNotificationsAsync(int page = 1, int pageSize = 20)
    {
        var (data, pagination, error) = await GetAsync<List<NotificationDto>>($"{ApiRoutes.Notifications}?page={page}&pageSize={pageSize}");
        return (data ?? new(), pagination, error);
    }

    public async Task<(int Count, string? Error)> GetUnreadCountAsync()
    {
        var (data, _, error) = await GetAsync<int>($"{ApiRoutes.Notifications}/unread-count");
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> MarkAsReadAsync(Guid notificationId) =>
        await PutAsync($"{ApiRoutes.Notifications}/mark-read/{notificationId}");

    public async Task<(bool Success, string? Error)> MarkAllAsReadAsync() =>
        await PutAsync($"{ApiRoutes.Notifications}/mark-read-all");
}

// --- Therapy (Assignments & Journals) ---
public class TherapyApiService : ApiServiceBase, ITherapyApiService
{
    public TherapyApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    // Assignments
    public async Task<(List<TherapyAssignmentDto> Data, string? Error)> GetAssignmentsByPackageAsync(Guid packageId)
    {
        var (data, _, error) = await GetAsync<List<TherapyAssignmentDto>>($"{ApiRoutes.Therapy}/assignments/package/{packageId}");
        return (data ?? new(), error);
    }

    public async Task<(TherapyAssignmentDto? Data, string? Error)> GetAssignmentByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<TherapyAssignmentDto>($"{ApiRoutes.Therapy}/assignments/{id}");
        return (data, error);
    }

    public async Task<(TherapyAssignmentDto? Data, string? Error)> CreateAssignmentAsync(CreateAssignmentDto dto)
    {
        var (data, error) = await PostAsync<TherapyAssignmentDto>($"{ApiRoutes.Therapy}/assignments", dto);
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> SubmitAssignmentAsync(Guid id, SubmitAssignmentDto dto) =>
        await PutAsync($"{ApiRoutes.Therapy}/assignments/{id}/submit", dto);

    public async Task<(bool Success, string? Error)> FeedbackAssignmentAsync(Guid id, FeedbackAssignmentDto dto) =>
        await PutAsync($"{ApiRoutes.Therapy}/assignments/{id}/feedback", dto);

    public async Task<(bool Success, string? Error)> DeleteAssignmentAsync(Guid id) =>
        await DeleteAsync($"{ApiRoutes.Therapy}/assignments/{id}");

    // Journals
    public async Task<(List<EmotionJournalDto> Data, string? Error)> GetMyJournalsAsync()
    {
        var (data, _, error) = await GetAsync<List<EmotionJournalDto>>($"{ApiRoutes.Therapy}/journals/my");
        return (data ?? new(), error);
    }

    public async Task<(List<EmotionJournalDto> Data, string? Error)> GetPatientSharedJournalsAsync(Guid patientId)
    {
        var (data, _, error) = await GetAsync<List<EmotionJournalDto>>($"{ApiRoutes.Therapy}/journals/patient/{patientId}");
        return (data ?? new(), error);
    }

    public async Task<(EmotionJournalDto? Data, string? Error)> CreateJournalAsync(CreateJournalDto dto)
    {
        var (data, error) = await PostAsync<EmotionJournalDto>($"{ApiRoutes.Therapy}/journals", dto);
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> DeleteJournalAsync(Guid id) =>
        await DeleteAsync($"{ApiRoutes.Therapy}/journals/{id}");
}

// --- Favorites ---
public class FavoriteApiService : ApiServiceBase, IFavoriteApiService
{
    public FavoriteApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<FavoriteDoctorWebDto> Data, string? Error)> GetFavoritesAsync()
    {
        var (data, _, error) = await GetAsync<List<FavoriteDoctorWebDto>>(ApiRoutes.Favorites);
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> AddFavoriteAsync(Guid doctorId) =>
        await PostAsync($"{ApiRoutes.Favorites}/{doctorId}");

    public async Task<(bool Success, string? Error)> RemoveFavoriteAsync(Guid doctorId) =>
        await DeleteAsync($"{ApiRoutes.Favorites}/{doctorId}");

    public async Task<(bool IsFavorite, string? Error)> IsFavoriteAsync(Guid doctorId)
    {
        var (data, _, error) = await GetAsync<bool>($"{ApiRoutes.Favorites}/{doctorId}/check");
        return (data, error);
    }
}

// --- Messaging ---
public class MessagingApiService : ApiServiceBase, IMessagingApiService
{
    public MessagingApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<ConversationWebDto> Data, string? Error)> GetConversationsAsync()
    {
        var (data, _, error) = await GetAsync<List<ConversationWebDto>>(ApiRoutes.Messages);
        return (data ?? new(), error);
    }

    public async Task<(List<MessageWebDto> Data, string? Error)> GetMessagesAsync(Guid conversationId)
    {
        var (data, _, error) = await GetAsync<List<MessageWebDto>>($"{ApiRoutes.Messages}/{conversationId}");
        return (data ?? new(), error);
    }

    public async Task<(MessageWebDto? Data, string? Error)> SendMessageAsync(Guid conversationId, object dto)
    {
        var (data, error) = await PostAsync<MessageWebDto>($"{ApiRoutes.Messages}/{conversationId}", dto);
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> MarkAsReadAsync(Guid conversationId) =>
        await PutAsync($"{ApiRoutes.Messages}/read?conversationId={conversationId}");

    public async Task<(ConversationWebDto? Data, string? Error)> GetOrCreateConversationAsync(
        Guid doctorUserId, Guid? appointmentId = null, Guid? treatmentPackageId = null)
    {
        var url = $"{ApiRoutes.Messages}/conversation?doctorUserId={doctorUserId}";
        if (appointmentId.HasValue) url += $"&appointmentId={appointmentId}";
        if (treatmentPackageId.HasValue) url += $"&treatmentPackageId={treatmentPackageId}";
        var (data, error) = await PostAsync<ConversationWebDto>(url);
        return (data, error);
    }

    public async Task<(ConversationWebDto? Data, string? Error)> GetOrCreateConversationByPatientAsync(
        Guid patientUserId, Guid? appointmentId = null, Guid? treatmentPackageId = null)
    {
        var url = $"{ApiRoutes.Messages}/conversation?patientUserId={patientUserId}";
        if (appointmentId.HasValue) url += $"&appointmentId={appointmentId}";
        if (treatmentPackageId.HasValue) url += $"&treatmentPackageId={treatmentPackageId}";
        var (data, error) = await PostAsync<ConversationWebDto>(url);
        return (data, error);
    }

    public async Task<(int Count, string? Error)> GetUnreadCountAsync()
    {
        var (data, _, error) = await GetAsync<int>($"{ApiRoutes.Messages}/unread");
        return (data, error);
    }
}

// --- Treatment Cases ---
public class TreatmentCaseApiService : ApiServiceBase, ITreatmentCaseApiService
{
    public TreatmentCaseApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<TreatmentCaseListWebDto> Data, string? Error)> GetByDoctorAsync(Guid doctorUserId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentCaseListWebDto>>($"{ApiRoutes.TreatmentCases}/doctor/{doctorUserId}");
        return (data ?? new(), error);
    }

    public async Task<(List<TreatmentCaseListWebDto> Data, string? Error)> GetByPatientAsync(Guid patientUserId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentCaseListWebDto>>($"{ApiRoutes.TreatmentCases}/patient/{patientUserId}");
        return (data ?? new(), error);
    }

    public async Task<(TreatmentCaseWebDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<TreatmentCaseWebDto>($"{ApiRoutes.TreatmentCases}/{id}");
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(object dto) =>
        await PostAsync(ApiRoutes.TreatmentCases, dto);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/{id}", dto);

    public async Task<(bool Success, string? Error)> CloseAsync(Guid id, object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/{id}/close", dto);

    // Schedule Generation
    public async Task<(bool Success, string? Error)> GenerateScheduleAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/generate-schedule", dto);

    // Sessions
    public async Task<(List<TreatmentSessionWebDto> Data, string? Error)> GetSessionsAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentSessionWebDto>>($"{ApiRoutes.TreatmentCases}/{caseId}/sessions");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> CreateSessionAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/sessions", dto);

    public async Task<(bool Success, string? Error)> UpdateSessionAsync(Guid sessionId, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/sessions/{sessionId}", dto);

    public async Task<(bool Success, string? Error)> DeleteSessionAsync(Guid sessionId) =>
        await DeleteAsync($"{ApiRoutes.TreatmentCases}/sessions/{sessionId}");

    public async Task<(bool Success, string? Error)> ReorderSessionsAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/sessions/reorder", dto);

    public async Task<(bool Success, string? Error)> CompleteSessionAsync(Guid sessionId, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/sessions/{sessionId}/complete", dto);

    // Goals
    public async Task<(List<TreatmentGoalWebDto> Data, string? Error)> GetGoalsAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentGoalWebDto>>($"{ApiRoutes.TreatmentCases}/{caseId}/goals");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> CreateGoalAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/goals", dto);

    public async Task<(bool Success, string? Error)> UpdateGoalAsync(Guid goalId, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/goals/{goalId}", dto);

    public async Task<(bool Success, string? Error)> RecordGoalProgressAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/goals/progress", dto);

    public async Task<(List<TreatmentGoalProgressWebDto> Data, string? Error)> GetGoalProgressHistoryAsync(Guid goalId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentGoalProgressWebDto>>($"{ApiRoutes.TreatmentCases}/goals/{goalId}/progress");
        return (data ?? new(), error);
    }

    // Homework
    public async Task<(List<HomeworkWebDto> Data, string? Error)> GetHomeworkAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<List<HomeworkWebDto>>($"{ApiRoutes.TreatmentCases}/{caseId}/homework");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> CreateHomeworkAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/homework", dto);

    public async Task<(bool Success, string? Error)> SubmitHomeworkAsync(Guid homeworkId, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/homework/{homeworkId}/submit", dto);

    public async Task<(bool Success, string? Error)> ReviewHomeworkAsync(Guid homeworkId, object dto) =>
        await PutAsync($"{ApiRoutes.TreatmentCases}/homework/{homeworkId}/review", dto);

    // Mood Tracking
    public async Task<(List<MoodEntryWebDto> Data, string? Error)> GetMoodEntriesAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<List<MoodEntryWebDto>>($"{ApiRoutes.TreatmentCases}/{caseId}/mood");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> AddMoodEntryAsync(object dto) =>
        await PostAsync($"{ApiRoutes.TreatmentCases}/mood", dto);

    // Progress & Timeline
    public async Task<(TreatmentProgressWebDto? Data, string? Error)> GetProgressAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<TreatmentProgressWebDto>($"{ApiRoutes.TreatmentCases}/{caseId}/progress");
        return (data, error);
    }

    public async Task<(List<TreatmentTimelineWebDto> Data, string? Error)> GetTimelineAsync(Guid caseId)
    {
        var (data, _, error) = await GetAsync<List<TreatmentTimelineWebDto>>($"{ApiRoutes.TreatmentCases}/{caseId}/timeline");
        return (data ?? new(), error);
    }
}
