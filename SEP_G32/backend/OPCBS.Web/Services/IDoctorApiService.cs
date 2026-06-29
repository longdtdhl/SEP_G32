using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IDoctorApiService
{
    Task<(List<DoctorListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(DoctorFilterDto? filter = null);
    Task<(DoctorDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetReviewsAsync(Guid doctorId, int page = 1);
    Task<(List<TimeSlotDto> Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, DateTime date);
}
