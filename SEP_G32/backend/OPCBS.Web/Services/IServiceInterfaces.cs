using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IScheduleApiService
{
    Task<(List<ScheduleDto> Data, string? Error)> GetMySchedulesAsync();
    Task<(ScheduleDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateScheduleDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateScheduleDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<(List<DayOffDto> Data, string? Error)> GetDaysOffAsync();
    Task<(bool Success, string? Error)> CreateDayOffAsync(CreateDayOffDto dto);
    Task<(bool Success, string? Error)> DeleteDayOffAsync(Guid id);
}

public interface IConsultationRecordApiService
{
    Task<(List<ConsultationRecordDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10);
    Task<(List<ConsultationRecordDto> Data, PaginationDto? Pagination, string? Error)> GetMyRecordsAsync(int page = 1, int pageSize = 10);
    Task<(ConsultationRecordDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateConsultationRecordDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateConsultationRecordDto dto);
}

public interface ITreatmentPackageApiService
{
    Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10);
    Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetMyPackagesAsync(int page = 1, int pageSize = 10);
    Task<(TreatmentPackageDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateTreatmentPackageDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateTreatmentPackageDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<(bool Success, string? Error)> AcceptAsync(Guid id);
    Task<(bool Success, string? Error)> RejectAsync(Guid id, string? reason = null);
}

public interface IReviewApiService
{
    Task<(bool Success, string? Error)> CreateAsync(CreateReviewDto dto);
    Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetMyReviewsAsync(int page = 1);
}

public interface IVerificationApiService
{
    Task<(VerificationDto? Data, string? Error)> GetMyVerificationAsync();
    Task<(bool Success, string? Error)> SubmitAsync(SubmitVerificationDto dto);
    Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetPendingAsync(int page = 1, int pageSize = 10);
    Task<(VerificationDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> ReviewAsync(Guid id, ReviewVerificationDto dto);
}

public interface IServicePackageApiService
{
    Task<(List<ServicePackageDto> Data, string? Error)> GetAllAsync();
    Task<(ServicePackageDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateServicePackageDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateServicePackageDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}

public interface ISubscriptionApiService
{
    Task<(SubscriptionDto? Data, string? Error)> GetCurrentAsync();
    Task<(List<SubscriptionDto> Data, string? Error)> GetHistoryAsync();
    Task<(bool Success, string? Error)> SubscribeAsync(CreateSubscriptionDto dto);
}

public interface IAdminApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<UserListItemDto> Data, PaginationDto? Pagination, string? Error)> GetUsersAsync(UserFilterDto? filter = null);
    Task<(UserListItemDto? Data, string? Error)> GetUserByIdAsync(Guid id);
    Task<(bool Success, string? Error)> ToggleUserActiveAsync(Guid id);
    Task<(List<RoleDto> Data, string? Error)> GetRolesAsync();
    Task<(List<AuditLogDto> Data, PaginationDto? Pagination, string? Error)> GetAuditLogsAsync(int page = 1, int pageSize = 20);
}

public interface ICustomerSupportApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetDoctorApplicationsAsync(int page = 1, string? status = null);
    Task<(VerificationDto? Data, string? Error)> GetApplicationByIdAsync(Guid id);
    Task<(bool Success, string? Error)> ReviewApplicationAsync(Guid id, ReviewVerificationDto dto);
    Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetBlogModerationQueueAsync(int page = 1);
    Task<(BlogDto? Data, string? Error)> GetBlogForModerationAsync(Guid id);
    Task<(bool Success, string? Error)> ApproveBlogAsync(Guid id);
    Task<(bool Success, string? Error)> RejectBlogAsync(Guid id, string? reason = null);
}

public interface IBusinessManagerApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<SpecializationDto> Data, string? Error)> GetSpecializationsAsync();
    Task<(bool Success, string? Error)> CreateSpecializationAsync(CreateSpecializationDto dto);
    Task<(bool Success, string? Error)> UpdateSpecializationAsync(Guid id, CreateSpecializationDto dto);
    Task<(bool Success, string? Error)> DeleteSpecializationAsync(Guid id);
}
