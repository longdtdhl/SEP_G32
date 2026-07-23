using OPCBS.Application.DTOs.Favorites;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Service interface for managing patient's favorite doctors
/// </summary>
public interface IFavoriteDoctorService
{
    /// <summary>Get all favorite doctors for a patient</summary>
    Task<ApiResponse<List<FavoriteDoctorDto>>> GetFavoritesAsync(Guid patientUserId, CancellationToken ct = default);

    /// <summary>Add a doctor to favorites</summary>
    Task<ApiResponse<FavoriteDoctorDto>> AddFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct = default);

    /// <summary>Remove a doctor from favorites</summary>
    Task<ApiResponse<bool>> RemoveFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct = default);

    /// <summary>Check if a doctor is in favorites</summary>
    Task<ApiResponse<bool>> IsFavoriteAsync(Guid patientUserId, Guid doctorId, CancellationToken ct = default);
}
