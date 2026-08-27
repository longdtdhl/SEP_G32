using Microsoft.Extensions.Logging;
using Moq;
using OPCBS.Application.Interfaces;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class TreatmentLifecycleTests
{
    private readonly Mock<IRepository<TreatmentPackage>> _packageRepo = new();
    private readonly Mock<IRepository<TreatmentCase>> _caseRepo = new();
    private readonly Mock<IRepository<TreatmentSession>> _sessionRepo = new();
    private readonly Mock<IRepository<Appointment>> _appointmentRepo = new();
    private readonly Mock<IRepository<AppointmentSlot>> _slotRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<TreatmentLifecycleCoordinator>> _coordinatorLogger = new();

    private readonly TreatmentLifecycleCoordinator _coordinator;

    public TreatmentLifecycleTests()
    {
        _coordinator = new TreatmentLifecycleCoordinator(
            _packageRepo.Object,
            _caseRepo.Object,
            _sessionRepo.Object,
            _appointmentRepo.Object,
            _slotRepo.Object,
            _doctorRepo.Object,
            _patientRepo.Object,
            _userRepo.Object,
            _notificationService.Object,
            _uow.Object,
            _coordinatorLogger.Object);
    }

    private static (DoctorProfile Doctor, PatientProfile Patient) CreateProfiles()
    {
        var doctorRole = new Role { Id = Guid.NewGuid(), Name = "Doctor" };
        var patientRole = new Role { Id = Guid.NewGuid(), Name = "Patient" };

        var doctorUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "doctor@opcbs.com",
            FullName = "Dr. Test Doctor",
            PhoneNumber = "0901234567",
            PasswordHash = "hash",
            RoleId = doctorRole.Id,
            Role = doctorRole
        };

        var patientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "patient@opcbs.com",
            FullName = "Test Patient",
            PhoneNumber = "0907654321",
            PasswordHash = "hash",
            RoleId = patientRole.Id,
            Role = patientRole
        };

        var doctor = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            User = doctorUser
        };

        var patient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            User = patientUser
        };

        return (doctor, patient);
    }

    [Fact]
    public async Task ExpireAssignedPackageAsync_Expires_Proposal_When_Deadline_Passed()
    {
        // Arrange
        var (doctor, patient) = CreateProfiles();
        var pkg = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Name = "Anxiety Reduction Package",
            Status = TreatmentPackageStatus.Assigned,
            AcceptanceExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _packageRepo.Setup(r => r.GetByIdAsync(pkg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pkg);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _coordinator.ExpireAssignedPackageAsync(pkg);

        // Assert
        Assert.Equal(TreatmentPackageStatus.Expired, pkg.Status);
        Assert.NotNull(pkg.ExpiredAt);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExpireActivePackageAndCaseAsync_Cancels_Future_Sessions_And_Releases_Slots()
    {
        // Arrange
        var (doctor, patient) = CreateProfiles();
        var slotId = Guid.NewGuid();

        var pkg = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Name = "Depression Management",
            Status = TreatmentPackageStatus.Active,
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        var tc = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            CaseName = "Depression Management Case",
            TreatmentPackageId = pkg.Id,
            Status = TreatmentCaseStatus.Active,
            ExpectedEndDate = DateTime.UtcNow.AddDays(-1),
            CompletedSessions = 2,
            TotalSessions = 5
        };

        var slot = new AppointmentSlot
        {
            Id = slotId,
            DoctorProfileId = doctor.Id,
            DoctorProfile = doctor,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            CurrentBookings = 1,
            MaxPatients = 1,
            Status = AppointmentSlotStatus.Booked
        };

        var apptId = Guid.NewGuid();
        var appt = new Appointment
        {
            Id = apptId,
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            AppointmentSlotId = slotId,
            AppointmentSlot = slot,
            BookingCode = "BK-12345",
            Status = AppointmentStatus.Approved,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(2)
        };

        var futureSession = new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentCaseId = tc.Id,
            TreatmentCase = tc,
            AppointmentId = apptId,
            SessionNumber = 3,
            Status = TreatmentSessionStatus.Planned,
            PlannedStartTime = DateTime.UtcNow.AddDays(2)
        };

        var pastSession = new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentCaseId = tc.Id,
            TreatmentCase = tc,
            SessionNumber = 1,
            Status = TreatmentSessionStatus.Completed,
            PlannedStartTime = DateTime.UtcNow.AddDays(-5)
        };

        _packageRepo.Setup(r => r.GetByIdAsync(pkg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pkg);
        _caseRepo.Setup(r => r.GetByIdAsync(tc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tc);
        _caseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase> { tc });
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { futureSession, pastSession });
        _appointmentRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _coordinator.ExpireActivePackageAndCaseAsync(pkg, tc);

        // Assert
        Assert.Equal(TreatmentPackageStatus.Expired, pkg.Status);
        Assert.Equal(TreatmentCaseStatus.Expired, tc.Status);
        Assert.NotNull(pkg.ExpiredAt);
        Assert.NotNull(tc.ActualEndDate);
        Assert.Contains("validity period has expired", tc.ClosureNote ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        // Future session cancelled
        Assert.Equal(TreatmentSessionStatus.Cancelled, futureSession.Status);

        // Future appointment cancelled
        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
        Assert.Equal("Treatment program validity period has expired.", appt.CancellationReason);

        // Slot released
        Assert.Equal(0, slot.CurrentBookings);
        Assert.Equal(AppointmentSlotStatus.Available, slot.Status);

        // Past session unchanged
        Assert.Equal(TreatmentSessionStatus.Completed, pastSession.Status);
    }

    [Fact]
    public async Task ValidateCaseManageableAsync_Returns_False_When_Case_Is_Expired_Or_Completed()
    {
        // Arrange
        var (doctor, patient) = CreateProfiles();
        var expiredCase = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            CaseName = "Expired Case",
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = TreatmentCaseStatus.Expired
        };

        var completedCase = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            CaseName = "Completed Case",
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = TreatmentCaseStatus.Completed
        };

        var activeCase = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            CaseName = "Active Case",
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = TreatmentCaseStatus.Active
        };

        // Assert
        var (canManageExpired, errorExpired) = await _coordinator.ValidateCaseManageableAsync(expiredCase);
        Assert.False(canManageExpired);
        Assert.NotNull(errorExpired);
        Assert.Contains("read-only mode", errorExpired, StringComparison.OrdinalIgnoreCase);

        var (canManageCompleted, errorCompleted) = await _coordinator.ValidateCaseManageableAsync(completedCase);
        Assert.False(canManageCompleted);
        Assert.NotNull(errorCompleted);
        Assert.Contains("read-only mode", errorCompleted, StringComparison.OrdinalIgnoreCase);

        var (canManageActive, errorActive) = await _coordinator.ValidateCaseManageableAsync(activeCase);
        Assert.True(canManageActive);
        Assert.Null(errorActive);
    }

    [Fact]
    public async Task CancelCaseAndPackageAsync_Synchronizes_Both_Entities_To_Cancelled()
    {
        // Arrange
        var (doctor, patient) = CreateProfiles();

        var pkg = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Name = "Stress Package",
            Status = TreatmentPackageStatus.Active
        };

        var tc = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            CaseName = "Stress Management Case",
            TreatmentPackageId = pkg.Id,
            Status = TreatmentCaseStatus.Active
        };

        _packageRepo.Setup(r => r.GetByIdAsync(pkg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pkg);
        _caseRepo.Setup(r => r.GetByIdAsync(tc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tc);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _coordinator.CancelCaseAndPackageAsync(pkg, tc, "Patient relocated");

        // Assert
        Assert.Equal(TreatmentCaseStatus.Cancelled, tc.Status);
        Assert.Equal(TreatmentPackageStatus.Cancelled, pkg.Status);
        Assert.NotNull(tc.ActualEndDate);
        Assert.Equal("Cancelled: Patient relocated", tc.ClosureNote);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteCaseAndPackageAsync_Synchronizes_Both_Entities_To_Completed()
    {
        // Arrange
        var (doctor, patient) = CreateProfiles();

        var pkg = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Name = "PTSD Protocol",
            Status = TreatmentPackageStatus.Active
        };

        var tc = new TreatmentCase
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            CaseName = "PTSD Protocol Case",
            TreatmentPackageId = pkg.Id,
            Status = TreatmentCaseStatus.Active,
            CompletedSessions = 5,
            TotalSessions = 5,
            OverallProgressPercent = 100
        };

        _packageRepo.Setup(r => r.GetByIdAsync(pkg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pkg);
        _caseRepo.Setup(r => r.GetByIdAsync(tc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tc);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _coordinator.CompleteCaseAndPackageAsync(tc, "Successful treatment completion");

        // Assert
        Assert.Equal(TreatmentCaseStatus.Completed, tc.Status);
        Assert.Equal(TreatmentPackageStatus.Completed, pkg.Status);
        Assert.NotNull(tc.ActualEndDate);
        Assert.Equal("Successful treatment completion", tc.ClosureNote);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
