using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IDoctorApiService
{
    Task<(List<DoctorListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(DoctorFilterDto? filter = null);
    Task<(DoctorDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetReviewsAsync(Guid doctorId, int page = 1);
    Task<(List<TimeSlotDto> Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, DateTime date);
    Task<List<string>> GetSpecializationsAsync();
    Task<List<SpecializationDto>> GetSpecializationDtosAsync();
    Task<(DoctorDto? Data, string? Error)> GetMyProfileAsync();
    Task<(bool Success, string? Error)> UpdateMyProfileAsync(UpdateDoctorProfileDto dto);
    Task<(string? Url, string? Error)> UploadAvatarAsync(Stream fileStream, string fileName);
    Task<(string? Url, string? Error)> UploadCertificateAsync(Stream fileStream, string fileName);
    Task<(CertificateUploadResultDto? Data, string? Error)> UploadCertificateFullAsync(Stream fileStream, string fileName);
}

public class CertificateUploadResultDto
{
    public string? CertificateUrl { get; set; }
    public string? CertificatePublicId { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificateContentType { get; set; }
}
