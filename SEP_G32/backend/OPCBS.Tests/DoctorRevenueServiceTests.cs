using Moq;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class DoctorRevenueServiceTests
{
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepoMock = new();
    private readonly Mock<IRepository<Appointment>> _appointmentRepoMock = new();
    private readonly Mock<IRepository<AppointmentSlot>> _slotRepoMock = new();
    private readonly Mock<IRepository<TreatmentPackage>> _pkgRepoMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepoMock = new();

    private readonly DoctorRevenueService _service;

    public DoctorRevenueServiceTests()
    {
        _service = new DoctorRevenueService(
            _doctorRepoMock.Object,
            _appointmentRepoMock.Object,
            _slotRepoMock.Object,
            _pkgRepoMock.Object,
            _userRepoMock.Object,
            _patientRepoMock.Object);
    }

    [Fact]
    public async Task GetRevenueOverviewAsync_WhenDoctorNotFound_ReturnsError()
    {
        // Arrange
        _doctorRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile>());

        // Act
        var result = await _service.GetRevenueOverviewAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Doctor profile not found.", result.Message);
    }

    [Fact]
    public async Task GetRevenueOverviewAsync_WithCompletedAppointments_CalculatesGrossNetAndFeeCorrectly()
    {
        // Arrange
        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var user = new User
        {
            Id = doctorUserId,
            Email = "doctor@test.com",
            FullName = "Dr. Test Doctor",
            PhoneNumber = "0912345678",
            PasswordHash = "hashed_pw",
            RoleId = Guid.NewGuid(),
            Role = new Role { Name = "Doctor" }
        };
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            User = user,
            ConsultationFee = 500000m
        };

        var slotId = Guid.NewGuid();
        var slot = new AppointmentSlot
        {
            Id = slotId,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Price = 600000m,
            ConsultationMode = ConsultationMode.Online
        };

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-2026-001",
            DoctorId = doctorId,
            Doctor = doctor,
            AppointmentSlotId = slotId,
            AppointmentSlot = slot,
            Status = AppointmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _doctorRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _slotRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSlot> { slot });
        _appointmentRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appt });
        _pkgRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TreatmentPackage>());
        _patientRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile>());

        // Act
        var result = await _service.GetRevenueOverviewAsync(doctorUserId, period: "30days");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(600000m, result.Data.TotalGrossRevenue);
        Assert.Equal(60000m, result.Data.PlatformFeeDeducted); // 10%
        Assert.Equal(540000m, result.Data.TotalNetEarnings); // 90%
        Assert.Equal(1, result.Data.CompletedSessionsCount);
        Assert.Single(result.Data.RecentTransactions);
        Assert.Equal("BK-2026-001", result.Data.RecentTransactions[0].BookingCode);
        Assert.Equal(540000m, result.Data.RecentTransactions[0].NetAmount);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithSearch_FiltersTransactions()
    {
        // Arrange
        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var user = new User
        {
            Id = doctorUserId,
            Email = "doctor@test.com",
            FullName = "Dr. Test Doctor",
            PhoneNumber = "0912345678",
            PasswordHash = "hashed_pw",
            RoleId = Guid.NewGuid(),
            Role = new Role { Name = "Doctor" }
        };
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            User = user,
            ConsultationFee = 500000m
        };

        var slot1 = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Price = 500000m
        };

        var slot2 = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Price = 500000m
        };

        var appt1 = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-ALPHA-01",
            DoctorId = doctorId,
            Doctor = doctor,
            GuestName = "John Doe",
            AppointmentSlotId = slot1.Id,
            AppointmentSlot = slot1,
            Status = AppointmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var appt2 = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-BETA-02",
            DoctorId = doctorId,
            Doctor = doctor,
            GuestName = "Alice Smith",
            AppointmentSlotId = slot2.Id,
            AppointmentSlot = slot2,
            Status = AppointmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _doctorRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _slotRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSlot> { slot1, slot2 });
        _appointmentRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appt1, appt2 });
        _pkgRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TreatmentPackage>());
        _patientRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile>());

        // Act
        var result = await _service.GetTransactionsAsync(doctorUserId, search: "ALPHA", page: 1, pageSize: 10);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("BK-ALPHA-01", result.Data[0].BookingCode);
    }
}
