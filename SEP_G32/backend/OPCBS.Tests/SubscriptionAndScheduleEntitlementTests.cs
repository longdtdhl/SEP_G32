using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Tests;

public class SubscriptionAndScheduleEntitlementTests
{
    [Fact]
    public async Task CreateSubscriptionDirectAsync_ExtendsFromCurrentExpiration()
    {
        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUserId,
            User = CreateUser(doctorUserId, "Doctor")
        };
        var package = new ServicePackage
        {
            Id = Guid.NewGuid(),
            Name = "Professional",
            DurationDays = 30,
            Price = 100000,
            MaxDailySlotsCapacity = 10
        };
        var currentExpiration = DateTime.UtcNow.AddDays(12);
        var currentSubscription = new DoctorSubscription
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctor.Id,
            ServicePackageId = package.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-18),
            ExpirationDate = currentExpiration,
            DoctorProfile = doctor,
            ServicePackage = package
        };

        var subRepo = new Mock<IRepository<DoctorSubscription>>();
        var pkgRepo = new Mock<IRepository<ServicePackage>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var paymentRepo = new Mock<IRepository<PaymentTransaction>>();
        var paymentService = new Mock<IPaymentService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        DoctorSubscription? createdSubscription = null;

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor });
        pkgRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        subRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { currentSubscription });
        subRepo.Setup(r => r.AddAsync(It.IsAny<DoctorSubscription>(), It.IsAny<CancellationToken>()))
            .Callback<DoctorSubscription, CancellationToken>((subscription, _) => createdSubscription = subscription)
            .Returns(Task.CompletedTask);
        paymentRepo.Setup(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new SubscriptionService(
            subRepo.Object, pkgRepo.Object, doctorRepo.Object, userRepo.Object,
            paymentRepo.Object, paymentService.Object, uow.Object, mapper.Object);

        var result = await service.CreateSubscriptionDirectAsync(doctorUserId, package.Id);

        Assert.True(result.Success);
        Assert.NotNull(createdSubscription);
        Assert.Equal(currentExpiration, createdSubscription!.StartDate);
        Assert.Equal(currentExpiration.AddDays(package.DurationDays), createdSubscription.ExpirationDate);
        Assert.Equal(SubscriptionStatus.Active, currentSubscription.Status);
    }

    [Fact]
    public async Task CreateSlotAsync_RejectsWhenDailyPlanQuotaIsReached()
    {
        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUserId,
            User = CreateUser(doctorUserId, "Doctor")
        };
        var slotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var package = new ServicePackage
        {
            Id = Guid.NewGuid(),
            Name = "Basic",
            DurationDays = 30,
            Price = 100000,
            MaxDailySlotsCapacity = 1
        };
        var subscription = new DoctorSubscription
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctor.Id,
            ServicePackageId = package.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpirationDate = DateTime.UtcNow.AddDays(29),
            DoctorProfile = doctor,
            ServicePackage = package
        };
        var existingSlot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctor.Id,
            SlotDate = slotDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Status = AppointmentSlotStatus.Available,
            DoctorProfile = doctor
        };

        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var appointmentRepo = new Mock<IRepository<Appointment>>();
        var subscriptionRepo = new Mock<IRepository<DoctorSubscription>>();
        var packageRepo = new Mock<IRepository<ServicePackage>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { existingSlot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<DoctorDayOff>());
        subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { subscription });
        packageRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var service = new ScheduleService(
            scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object,
            dayOffRepo.Object, appointmentRepo.Object, uow.Object, mapper.Object,
            subscriptionRepo: subscriptionRepo.Object, servicePackageRepo: packageRepo.Object);

        var result = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto
        {
            Date = slotDate.ToString("yyyy-MM-dd"),
            StartTime = "10:00",
            EndTime = "11:00"
        });

        Assert.False(result.Success);
        Assert.Contains("up to 1 slot", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        slotRepo.Verify(r => r.AddAsync(It.IsAny<AppointmentSlot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static User CreateUser(Guid userId, string roleName) => new()
    {
        Id = userId,
        FullName = "Test User",
        Email = "test@example.com",
        PhoneNumber = "0123456789",
        PasswordHash = "hash",
        RoleId = Guid.NewGuid(),
        Role = new Role { Name = roleName }
    };
}
