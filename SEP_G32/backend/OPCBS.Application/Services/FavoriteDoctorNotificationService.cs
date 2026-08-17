using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Application.Services;

/// <summary>
/// Delivers activity updates without exposing which patients follow a doctor.
/// </summary>
public sealed class FavoriteDoctorNotificationService : IFavoriteDoctorNotificationService
{
    private readonly IRepository<FavoriteDoctor> _favoriteRepo;
    private readonly INotificationService _notificationService;

    public FavoriteDoctorNotificationService(
        IRepository<FavoriteDoctor> favoriteRepo,
        INotificationService notificationService)
    {
        _favoriteRepo = favoriteRepo;
        _notificationService = notificationService;
    }

    public async Task NotifyFollowersAsync(
        Guid doctorProfileId,
        Guid doctorUserId,
        string doctorName,
        string title,
        string message,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken ct = default)
    {
        var followers = (await _favoriteRepo.GetAllAsync(ct))
            .Where(f => !f.IsDeleted &&
                        (f.DoctorId == doctorUserId || f.DoctorId == doctorProfileId))
            .Select(f => f.PatientId)
            .Distinct()
            .ToList();

        foreach (var patientUserId in followers)
        {
            await _notificationService.CreateNotificationAsync(
                patientUserId,
                title,
                message,
                NotificationType.System,
                relatedEntityId,
                relatedEntityType,
                ct);
        }
    }
}
