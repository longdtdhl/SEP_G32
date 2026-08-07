using Moq;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using Xunit;

namespace OPCBS.Tests;

public class FavoriteDoctorNotificationServiceTests
{
    [Fact]
    public async Task NotifyFollowersAsync_NotifiesEachActiveFollowerOnlyOnce()
    {
        var favoriteRepo = new Mock<IRepository<FavoriteDoctor>>();
        var notificationService = new Mock<INotificationService>();
        var doctorProfileId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var firstPatientUserId = Guid.NewGuid();
        var secondPatientUserId = Guid.NewGuid();

        favoriteRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>
        {
            new() { PatientId = firstPatientUserId, DoctorId = doctorUserId, Patient = null!, Doctor = null! },
            new() { PatientId = firstPatientUserId, DoctorId = doctorProfileId, Patient = null!, Doctor = null! },
            new() { PatientId = secondPatientUserId, DoctorId = doctorUserId, Patient = null!, Doctor = null! },
            new() { PatientId = Guid.NewGuid(), DoctorId = doctorUserId, IsDeleted = true, Patient = null!, Doctor = null! }
        });

        var service = new FavoriteDoctorNotificationService(favoriteRepo.Object, notificationService.Object);

        await service.NotifyFollowersAsync(
            doctorProfileId,
            doctorUserId,
            "Dr. Test",
            "New availability",
            "A new slot is available.",
            Guid.NewGuid(),
            "FavoriteDoctor");

        notificationService.Verify(n => n.CreateNotificationAsync(
            firstPatientUserId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<OPCBS.Domain.Enums.NotificationType>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
        notificationService.Verify(n => n.CreateNotificationAsync(
            secondPatientUserId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<OPCBS.Domain.Enums.NotificationType>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
        notificationService.Verify(n => n.CreateNotificationAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<OPCBS.Domain.Enums.NotificationType>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
