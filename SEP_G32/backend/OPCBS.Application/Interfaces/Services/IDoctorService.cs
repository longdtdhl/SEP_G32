using OPCBS.Application.DTOs.Auth;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Doctor service - public doctor listing, search, details
/// </summary>
public interface IDoctorService
{
    Task<ApiResponse<List<DoctorProfileDto>>> GetDoctorsAsync(string? search, Guid? specializationId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<DoctorProfileDto>> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default);
    Task<ApiResponse<DoctorProfileDto>> GetDoctorProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<DoctorProfileDto>> UpdateDoctorProfileAsync(Guid userId, UpdateDoctorProfileDto dto, CancellationToken ct = default);
}
