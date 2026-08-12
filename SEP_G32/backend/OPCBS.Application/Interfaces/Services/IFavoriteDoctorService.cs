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

/// <summary>
/// Sends in-app updates to patients who follow a doctor through Favorites.
/// </summary>
public interface IFavoriteDoctorNotificationService
{
    Task NotifyFollowersAsync(
        Guid doctorProfileId,
        Guid doctorUserId,
        string doctorName,
        string title,
        string message,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken ct = default);
}
