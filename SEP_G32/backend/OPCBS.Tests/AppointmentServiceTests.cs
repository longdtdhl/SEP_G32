using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Tests;

/// <summary>
/// Unit tests for AppointmentService covering all business rules:
/// BOOK-03 (no past booking), BOOK-04/DOC-12/SP-01 (doctor verified + active subscription),
/// BOOK-06/07/09 (no double booking), APPT-05 (24-hour cancellation policy)
/// </summary>
public class AppointmentServiceTests
{
    private readonly Mock<IRepository<Appointment>> _apptRepo;
    private readonly Mock<IRepository<AppointmentSlot>> _slotRepo;
    private readonly Mock<IRepository<AppointmentHistory>> _historyRepo;
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo;
    private readonly Mock<IRepository<User>> _userRepo;
    private readonly Mock<IRepository<PatientProfile>> _patientRepo;
    private readonly Mock<IRepository<DoctorSubscription>> _subscriptionRepo;
    private readonly Mock<IRepository<TreatmentPackage>> _packageRepoMock;
    private readonly Mock<IRepository<ConsultationNote>> _consultationNoteRepoMock;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<OPCBS.Application.Interfaces.Services.INotificationService> _notificationServiceMock;
    private readonly Mock<OPCBS.Application.Interfaces.Services.IEmailService> _emailServiceMock;
    private readonly AppointmentService _sut;

    // Shared test data
    private readonly Guid _doctorProfileId = Guid.NewGuid();
    private readonly Guid _doctorUserId = Guid.NewGuid();
    private readonly Guid _patientProfileId = Guid.NewGuid();
    private readonly Guid _patientUserId = Guid.NewGuid();
    private readonly Guid _slotId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public AppointmentServiceTests()
    {
        _apptRepo = new Mock<IRepository<Appointment>>();
        _slotRepo = new Mock<IRepository<AppointmentSlot>>();
        _historyRepo = new Mock<IRepository<AppointmentHistory>>();
        _doctorRepo = new Mock<IRepository<DoctorProfile>>();
        _userRepo = new Mock<IRepository<User>>();
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>
        {
            new User { Id = _doctorUserId, FullName = "Dr. Test", Email = "doc@test.com", PhoneNumber = "0123456789", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        });
        _patientRepo = new Mock<IRepository<PatientProfile>>();
        _subscriptionRepo = new Mock<IRepository<DoctorSubscription>>();
        _packageRepoMock = new Mock<IRepository<TreatmentPackage>>();
        _consultationNoteRepoMock = new Mock<IRepository<ConsultationNote>>();
        _consultationNoteRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote>());
        _uow = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _notificationServiceMock = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        _emailServiceMock = new Mock<OPCBS.Application.Interfaces.Services.IEmailService>();

        // Mock mapper to return a basic AppointmentDto for any Appointment
        _mapperMock.Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
            .Returns((Appointment a) => new AppointmentDto
            {
                Id = a.Id,
                BookingCode = a.BookingCode,
                DoctorId = a.DoctorId,
                DoctorName = "Dr. Test",
                AppointmentDate = "2026-07-01",
                StartTime = "10:00",
                EndTime = "11:00",
                Status = a.Status
            });

        _sut = new AppointmentService(
            _apptRepo.Object,
            _slotRepo.Object,
            _historyRepo.Object,
            _doctorRepo.Object,
            _userRepo.Object,
            _patientRepo.Object,
            _subscriptionRepo.Object,
            _packageRepoMock.Object,
            _consultationNoteRepoMock.Object,
            _notificationServiceMock.Object,
            _emailServiceMock.Object,
            _uow.Object,
            _mapperMock.Object);
    }

    #region Helper Methods

    private DoctorProfile CreateDoctor(VerificationStatus status = VerificationStatus.Approved)
    {
        var user = new User
        {
            Id = _doctorUserId,
            Email = "doctor@test.com",
            FullName = "Dr. Test",
            PhoneNumber = "0901234567",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            Role = new Role { Name = "Doctor" }
        };

        return new DoctorProfile
        {
            Id = _doctorProfileId,
            UserId = _doctorUserId,
            VerificationStatus = status,
            IsVisible = true,
            User = user
        };
    }

    private AppointmentSlot CreateSlot(
        AppointmentSlotStatus status = AppointmentSlotStatus.Available,
        int daysFromNow = 7)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysFromNow));
        var doctor = CreateDoctor();
        return new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = date,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = status,
            DoctorProfile = doctor
        };
    }

    private PatientProfile CreatePatient()
    {
        var user = new User
        {
            Id = _patientUserId,
            Email = "patient@test.com",
            FullName = "Patient Test",
            PhoneNumber = "0907654321",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            Role = new Role { Name = "Patient" }
        };

        return new PatientProfile
        {
            Id = _patientProfileId,
            UserId = _patientUserId,
            User = user
        };
    }

    private DoctorSubscription CreateActiveSubscription()
    {
        var doctor = CreateDoctor();
        return new DoctorSubscription
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = _doctorProfileId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            ExpirationDate = DateTime.UtcNow.AddMonths(1),
            ServicePackageId = Guid.NewGuid(),
            DoctorProfile = doctor,
            ServicePackage = new ServicePackage { Name = "Basic", DurationDays = 30, Price = 100 }
        };
    }

    private Appointment CreateAppointment(AppointmentStatus status = AppointmentStatus.Pending)
    {
        var doctor = CreateDoctor();
        var slot = CreateSlot();
        return new Appointment
        {
            Id = _appointmentId,
            BookingCode = "OPCBS-TEST-001",
            AppointmentSlotId = _slotId,
            DoctorId = _doctorProfileId,
            PatientId = _patientProfileId,
            Status = status,
            AppointmentSlot = slot,
            Doctor = doctor
        };
    }

    private void SetupDefaultMocks()
    {
        var doctor = CreateDoctor();
        var slot = CreateSlot();
        var patient = CreatePatient();
        var subscription = CreateActiveSubscription();

        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { patient });
        _subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorSubscription> { subscription });
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());
    }

    #endregion

    // ──────────────────────────────────────────────
    // CREATE APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CreateAppointment_Success_ReturnsAppointmentDto()
    {
        // Arrange
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Appointment booked successfully", result.Message);
        _uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAppointment_DoctorNotVerified_Fails()
    {
        // Arrange
        var unverifiedDoctor = CreateDoctor(VerificationStatus.Draft);
        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverifiedDoctor);

        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not verified", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_NoActiveSubscription_Fails()
    {
        // Arrange
        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorSubscription>()); // No subscriptions

        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("active service subscription", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_SlotNotAvailable_Fails()
    {
        // Arrange
        var doctor = CreateDoctor();
        var bookedSlot = CreateSlot(AppointmentSlotStatus.Booked);
        var subscription = CreateActiveSubscription();

        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorSubscription> { subscription });
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookedSlot);

        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Slot not available", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_PastSlot_Fails()
    {
        // Arrange
        var doctor = CreateDoctor();
        var subscription = CreateActiveSubscription();

        // Create a slot in the past
        var pastSlotDoctor = CreateDoctor();
        var pastSlot = new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = AppointmentSlotStatus.Available,
            DoctorProfile = pastSlotDoctor
        };

        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorSubscription> { subscription });
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastSlot);

        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("past", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_DoubleBooking_Fails()
    {
        // Arrange
        SetupDefaultMocks();

        // There's already an existing appointment for this patient on this slot
        var existingAppt = CreateAppointment(AppointmentStatus.Pending);
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { existingAppt });

        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId
        };

        // Act
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Khung giờ này đã được đặt trước.", result.Message);
    }

    // ──────────────────────────────────────────────
    // CANCEL APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CancelAppointment_Within24Hours_Fails()
    {
        // Arrange
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Slot is in 12 hours — within 24-hour cancellation window
        var nearSlotDoctor = CreateDoctor();
        var nearSlot = new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(12)),
            StartTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(12)),
            EndTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(13)),
            Status = AppointmentSlotStatus.Booked,
            DoctorProfile = nearSlotDoctor
        };
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nearSlot);

        var dto = new CancelAppointmentDto { Reason = "Changed mind" };

        // Act
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("24 hours", result.Message);
    }

    [Fact]
    public async Task CancelAppointment_Success()
    {
        // Arrange
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Slot is 3 days away — outside 24-hour window
        var futureSlotDoctor = CreateDoctor();
        var futureSlot = new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = AppointmentSlotStatus.Booked,
            DoctorProfile = futureSlotDoctor
        };
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureSlot);

        var dto = new CancelAppointmentDto { Reason = "Changed mind" };

        // Act
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("cancelled", result.Message.ToLower());
    }

    // ──────────────────────────────────────────────
    // APPROVE APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ApproveAppointment_Success()
    {
        // Arrange
        var appointment = CreateAppointment(AppointmentStatus.Pending);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });

        // Act
        var result = await _sut.ApproveAppointmentAsync(_appointmentId, _doctorUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Approved, appointment.Status);
    }

    [Fact]
    public async Task ApproveAppointment_NotPending_Fails()
    {
        // Arrange
        var appointment = CreateAppointment(AppointmentStatus.Completed);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });

        // Act
        var result = await _sut.ApproveAppointmentAsync(_appointmentId, _doctorUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("pending", result.Message.ToLower());
    }

    // ──────────────────────────────────────────────
    // REJECT APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RejectAppointment_Success()
    {
        // Arrange
        var appointment = CreateAppointment(AppointmentStatus.Pending);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });

        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSlot(AppointmentSlotStatus.Booked));

        var dto = new RejectAppointmentDto { Reason = "Not available" };

        // Act
        var result = await _sut.RejectAppointmentAsync(_appointmentId, _doctorUserId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Rejected, appointment.Status);
    }

    // ──────────────────────────────────────────────
    // COMPLETE APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CompleteAppointment_Success()
    {
        // Arrange
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });

        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSlot(AppointmentSlotStatus.Booked));

        // Must have a consultation note to allow completion
        _consultationNoteRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConsultationNote>
            {
                new ConsultationNote
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = _appointmentId,
                    DoctorId = _doctorProfileId,
                    PatientRecordId = Guid.NewGuid(),
                    IsDeleted = false,
                    ConsultationSummary = "Test consultation summary",
                    Diagnosis = "Test diagnosis",
                    Doctor = null!,
                    PatientRecord = null!
                }
            });

        // Act
        var result = await _sut.CompleteAppointmentAsync(_appointmentId, _doctorUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public async Task StartAppointment_Success()
    {
        // Arrange
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var doctor = CreateDoctor();
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { doctor });

        // Act
        var result = await _sut.StartAppointmentAsync(_appointmentId, _doctorUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.InProgress, appointment.Status);
    }

    // ──────────────────────────────────────────────
    // RESCHEDULE APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RescheduleAppointment_PastSlot_Fails()
    {
        // Arrange
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Pending);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var pastNewSlotId = Guid.NewGuid();
        var pastSlotDoctor = CreateDoctor();
        var pastNewSlot = new AppointmentSlot
        {
            Id = pastNewSlotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = AppointmentSlotStatus.Available,
            DoctorProfile = pastSlotDoctor
        };

        _slotRepo.Setup(r => r.GetByIdAsync(pastNewSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastNewSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = pastNewSlotId, Reason = "Need earlier" };

        // Act
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("past", result.Message.ToLower());
    }

    // ──────────────────────────────────────────────
    // MORE CREATE APPOINTMENT TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CreateAppointment_GuestSuccess_ReturnsAppointmentDto()
    {
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId,
            GuestName = "Guest User",
            GuestEmail = "guest@test.com",
            GuestPhoneNumber = "0901234567"
        };
        var result = await _sut.CreateAppointmentAsync(dto, null);
        Assert.True(result.Success);
        Assert.Equal("Appointment booked successfully", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_DoctorProfileNotFound_Fails()
    {
        SetupDefaultMocks();
        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync((DoctorProfile?)null);
        var dto = new CreateAppointmentDto { DoctorId = _doctorProfileId, AppointmentSlotId = _slotId };
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);
        Assert.False(result.Success);
        Assert.Contains("Doctor not found", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_SubscriptionExpired_Fails()
    {
        SetupDefaultMocks();
        var expiredSub = CreateActiveSubscription();
        expiredSub.ExpirationDate = DateTime.UtcNow.AddDays(-1);
        _subscriptionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorSubscription> { expiredSub });
        var dto = new CreateAppointmentDto { DoctorId = _doctorProfileId, AppointmentSlotId = _slotId };
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);
        Assert.False(result.Success);
        Assert.Contains("subscription", result.Message);
    }

    [Fact]
    public async Task CreateAppointment_GuestMissingName_Fails()
    {
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId,
            GuestEmail = "guest@test.com",
            GuestPhoneNumber = "0901234567"
        };
        var result = await _sut.CreateAppointmentAsync(dto, null);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_GuestMissingEmail_Fails()
    {
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId,
            GuestName = "Guest User",
            GuestPhoneNumber = "0901234567"
        };
        var result = await _sut.CreateAppointmentAsync(dto, null);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_GuestMissingPhoneNumber_Fails()
    {
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId,
            GuestName = "Guest User",
            GuestEmail = "guest@test.com"
        };
        var result = await _sut.CreateAppointmentAsync(dto, null);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_InvalidTreatmentPackage_Fails()
    {
        SetupDefaultMocks();
        var dto = new CreateAppointmentDto
        {
            DoctorId = _doctorProfileId,
            AppointmentSlotId = _slotId,
            TreatmentPackageId = Guid.NewGuid() // invalid/non-existent package
        };
        _packageRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentPackage?)null);
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_VerificationStatusDraft_Fails()
    {
        SetupDefaultMocks();
        var doctor = CreateDoctor(VerificationStatus.Draft);
        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        var dto = new CreateAppointmentDto { DoctorId = _doctorProfileId, AppointmentSlotId = _slotId };
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_VerificationStatusRejected_Fails()
    {
        SetupDefaultMocks();
        var doctor = CreateDoctor(VerificationStatus.Rejected);
        _doctorRepo.Setup(r => r.GetByIdAsync(_doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        var dto = new CreateAppointmentDto { DoctorId = _doctorProfileId, AppointmentSlotId = _slotId };
        var result = await _sut.CreateAppointmentAsync(dto, _patientUserId);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_DbSaveFails_RollsBackTransaction()
    {
        SetupDefaultMocks();
        _uow.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));
        var dto = new CreateAppointmentDto { DoctorId = _doctorProfileId, AppointmentSlotId = _slotId };
        
        await Assert.ThrowsAsync<Exception>(() => _sut.CreateAppointmentAsync(dto, _patientUserId));
        _uow.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ──────────────────────────────────────────────
    // MORE CANCEL APPOINTMENT TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CancelAppointment_NotFound_Fails()
    {
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Appointment?)null);
        var dto = new CancelAppointmentDto { Reason = "No reason" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message.ToLower());
    }

    [Fact]
    public async Task CancelAppointment_UnauthorizedUser_Fails()
    {
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var dto = new CancelAppointmentDto { Reason = "No reason" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, Guid.NewGuid(), dto); // Different user ID
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyCompleted_Fails()
    {
        var appointment = CreateAppointment(AppointmentStatus.Completed);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var dto = new CancelAppointmentDto { Reason = "No reason" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyCancelled_Fails()
    {
        var appointment = CreateAppointment(AppointmentStatus.Cancelled);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var dto = new CancelAppointmentDto { Reason = "No reason" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyRejected_Fails()
    {
        var appointment = CreateAppointment(AppointmentStatus.Rejected);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var dto = new CancelAppointmentDto { Reason = "No reason" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAppointment_DoctorSuccess_Bypasses24HourRule()
    {
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        // Near slot (within 24 hours)
        var nearSlot = new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(2)),
            EndTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(3)),
            Status = AppointmentSlotStatus.Booked,
            DoctorProfile = CreateDoctor()
        };
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(nearSlot);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { CreateDoctor() });

        var dto = new CancelAppointmentDto { Reason = "Doctor emergency" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, _doctorUserId, dto); // Cancelled by doctor
        
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public async Task CancelAppointment_SlotReleasedOnSuccess()
    {
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var slot = CreateSlot(AppointmentSlotStatus.Booked, 5);
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var dto = new CancelAppointmentDto { Reason = "Change of plans" };
        await _sut.CancelAppointmentAsync(_appointmentId, _patientUserId, dto);

        Assert.Equal(AppointmentSlotStatus.Available, slot.Status);
    }

    [Fact]
    public async Task CancelAppointment_GuestBookingSuccess()
    {
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        appointment.PatientId = null;
        appointment.GuestEmail = "guest@test.com";
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var slot = CreateSlot(AppointmentSlotStatus.Booked, 5);
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var dto = new CancelAppointmentDto { Reason = "Change of plans" };
        var result = await _sut.CancelAppointmentAsync(_appointmentId, Guid.Empty, dto); // guest cancels

        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    // ──────────────────────────────────────────────
    // MORE RESCHEDULE APPOINTMENT TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RescheduleAppointment_Success()
    {
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var oldSlot = CreateSlot(AppointmentSlotStatus.Booked, 3);
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(oldSlot);

        var newSlotId = Guid.NewGuid();
        var newSlot = new AppointmentSlot
        {
            Id = newSlotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 0),
            Status = AppointmentSlotStatus.Available,
            DoctorProfile = CreateDoctor()
        };
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(newSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId, Reason = "Need different time" };
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);

        Assert.True(result.Success);
        Assert.Equal(newSlotId, appointment.AppointmentSlotId);
        Assert.Equal(AppointmentSlotStatus.Available, oldSlot.Status);
        Assert.Equal(AppointmentSlotStatus.Booked, newSlot.Status);
    }

    [Fact]
    public async Task RescheduleAppointment_Under24Hours_Fails()
    {
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        // Near slot (within 24 hours)
        var nearSlot = new AppointmentSlot
        {
            Id = _slotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(2)),
            EndTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(3)),
            Status = AppointmentSlotStatus.Booked,
            DoctorProfile = CreateDoctor()
        };
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(nearSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = Guid.NewGuid() };
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);

        Assert.False(result.Success);
        Assert.Contains("24 hours", result.Message);
    }

    [Fact]
    public async Task RescheduleAppointment_NewSlotBooked_Fails()
    {
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var oldSlot = CreateSlot(AppointmentSlotStatus.Booked, 3);
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(oldSlot);

        var newSlotId = Guid.NewGuid();
        var newSlot = new AppointmentSlot
        {
            Id = newSlotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 0),
            Status = AppointmentSlotStatus.Booked, // already booked
            DoctorProfile = CreateDoctor()
        };
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(newSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId, Reason = "Need different time" };
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);

        Assert.False(result.Success);
        Assert.Contains("not available", result.Message.ToLower());
    }

    [Fact]
    public async Task RescheduleAppointment_NewSlotUnavailable_Fails()
    {
        SetupDefaultMocks();
        var appointment = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var oldSlot = CreateSlot(AppointmentSlotStatus.Booked, 3);
        _slotRepo.Setup(r => r.GetByIdAsync(_slotId, It.IsAny<CancellationToken>())).ReturnsAsync(oldSlot);

        var newSlotId = Guid.NewGuid();
        var newSlot = new AppointmentSlot
        {
            Id = newSlotId,
            DoctorProfileId = _doctorProfileId,
            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 0),
            Status = AppointmentSlotStatus.Blocked,
            DoctorProfile = CreateDoctor()
        };
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(newSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId };
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);

        Assert.False(result.Success);
        Assert.Contains("not available", result.Message.ToLower());
    }

    [Fact]
    public async Task RescheduleAppointment_Completed_Fails()
    {
        var appointment = CreateAppointment(AppointmentStatus.Completed);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        var dto = new RescheduleAppointmentDto { NewSlotId = Guid.NewGuid() };
        var result = await _sut.RescheduleAppointmentAsync(_appointmentId, _patientUserId, dto);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetClinicalContext_ReturnsRecentConsultations_ExcludingCurrentAppt()
    {
        var appt = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { CreateDoctor() });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { CreatePatient() });

        var doc = CreateDoctor();
        var pRec = new PatientRecord { Id = Guid.NewGuid(), DoctorId = _doctorProfileId, PatientId = _patientProfileId, Doctor = doc };

        var currentNote = new ConsultationNote { Id = Guid.NewGuid(), AppointmentId = _appointmentId, ConsultationSummary = "Current Note", CreatedAt = DateTime.UtcNow, Doctor = doc, PatientRecord = pRec };
        var pastNote1 = new ConsultationNote { Id = Guid.NewGuid(), AppointmentId = Guid.NewGuid(), ConsultationSummary = "Past Note 1", CreatedAt = DateTime.UtcNow.AddDays(-5), Doctor = doc, PatientRecord = pRec };
        var pastNote2 = new ConsultationNote { Id = Guid.NewGuid(), AppointmentId = Guid.NewGuid(), ConsultationSummary = "Past Note 2", CreatedAt = DateTime.UtcNow.AddDays(-10), Doctor = doc, PatientRecord = pRec };

        _consultationNoteRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote> { currentNote, pastNote1, pastNote2 });

        var result = await _sut.GetClinicalContextAsync(_appointmentId, _doctorUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.RecentConsultations.Count);
        Assert.DoesNotContain(result.Data.RecentConsultations, n => n.AppointmentId == _appointmentId);
    }

    [Fact]
    public async Task GetClinicalContext_UnauthorizedUser_ReturnsError()
    {
        var appt = CreateAppointment(AppointmentStatus.Approved);
        _apptRepo.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { CreateDoctor() });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { CreatePatient() });

        var unauthorizedUserId = Guid.NewGuid();
        var result = await _sut.GetClinicalContextAsync(_appointmentId, unauthorizedUserId);

        Assert.False(result.Success);
        Assert.Contains("Unauthorized", result.Message);
    }
}
