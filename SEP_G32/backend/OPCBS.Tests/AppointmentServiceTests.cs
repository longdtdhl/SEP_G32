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
    private readonly Mock<IRepository<PatientProfile>> _patientRepo;
    private readonly Mock<IRepository<DoctorSubscription>> _subscriptionRepo;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IMapper> _mapperMock;
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
        _patientRepo = new Mock<IRepository<PatientProfile>>();
        _subscriptionRepo = new Mock<IRepository<DoctorSubscription>>();
        _uow = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

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
            _patientRepo.Object,
            _subscriptionRepo.Object,
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
        Assert.Contains("already has an appointment", result.Message);
    }

    // ──────────────────────────────────────────────
    // CANCEL APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CancelAppointment_Within24Hours_Fails()
    {
        // Arrange
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
        Assert.Contains("cancelled", result.Message);
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

        // Act
        var result = await _sut.CompleteAppointmentAsync(_appointmentId, _doctorUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    // ──────────────────────────────────────────────
    // RESCHEDULE APPOINTMENT TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RescheduleAppointment_PastSlot_Fails()
    {
        // Arrange
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
}
