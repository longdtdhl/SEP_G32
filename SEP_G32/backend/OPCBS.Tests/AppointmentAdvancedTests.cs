using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class AppointmentAdvancedTests
{
    private readonly Mock<IRepository<Appointment>> _apptRepo = new();
    private readonly Mock<IRepository<AppointmentSlot>> _slotRepo = new();
    private readonly Mock<IRepository<AppointmentHistory>> _historyRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepo = new();
    private readonly Mock<IRepository<DoctorSubscription>> _subscriptionRepo = new();
    private readonly Mock<IRepository<TreatmentPackage>> _packageRepo = new();
    private readonly Mock<IRepository<ConsultationNote>> _consultationNoteRepo = new();
    private readonly Mock<IRepository<AppointmentCompletionConfirmation>> _completionRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly AppointmentService _service;

    public AppointmentAdvancedTests()
    {
        _completionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentCompletionConfirmation>());
        _consultationNoteRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote>());
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());

        _mapper.Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
            .Returns((Appointment a) => new AppointmentDto
            {
                Id = a.Id,
                BookingCode = a.BookingCode,
                DoctorId = a.DoctorId,
                DoctorName = "Dr Bob",
                AppointmentDate = "2026-08-20",
                StartTime = "09:00",
                EndTime = "10:00",
                Status = a.Status,
                PatientName = "Patient Alice"
            });

        _mapper.Setup(m => m.Map<List<AppointmentListItemDto>>(It.IsAny<List<Appointment>>()))
            .Returns((List<Appointment> list) => list.Select(a => new AppointmentListItemDto
            {
                Id = a.Id,
                BookingCode = a.BookingCode,
                DoctorId = a.DoctorId,
                DoctorName = "Dr Bob",
                AppointmentDate = "2026-08-20",
                StartTime = "09:00",
                PatientName = "Patient Alice"
            }).ToList());

        _service = new AppointmentService(
            _apptRepo.Object,
            _slotRepo.Object,
            _historyRepo.Object,
            _doctorRepo.Object,
            _userRepo.Object,
            _patientRepo.Object,
            _subscriptionRepo.Object,
            _packageRepo.Object,
            _consultationNoteRepo.Object,
            _notificationService.Object,
            _emailService.Object,
            _uow.Object,
            _mapper.Object,
            completionConfirmationRepo: _completionRepo.Object);
    }

    [Fact]
    public async Task PatientRescheduleAsync_ValidSlot_MovesAppointmentAndUpdatesHistory()
    {
        var apptId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var oldSlotId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var docUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var doc = new DoctorProfile { Id = doctorProfileId, User = docUser };

        var oldSlot = new AppointmentSlot { Id = oldSlotId, DoctorProfileId = doctorProfileId, Status = AppointmentSlotStatus.Booked, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), DoctorProfile = doc };
        var newSlot = new AppointmentSlot { Id = newSlotId, DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)), StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 0), Status = AppointmentSlotStatus.Available, DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, PatientId = patientProfileId, AppointmentSlotId = oldSlotId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = oldSlot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _slotRepo.Setup(r => r.GetByIdAsync(oldSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(oldSlot);
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(newSlot);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _historyRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentHistory>());

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId, Reason = "Conflict with schedule" };
        var result = await _service.RescheduleAppointmentAsync(apptId, patientUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(newSlotId, appt.AppointmentSlotId);
        Assert.Equal(AppointmentSlotStatus.Available, oldSlot.Status);
        Assert.Equal(AppointmentSlotStatus.Booked, newSlot.Status);
        _apptRepo.Verify(r => r.Update(appt), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DoctorRescheduleAsync_PastSlot_Fails()
    {
        var apptId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var doc = new DoctorProfile { Id = doctorProfileId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "123", Role = new Role { Name = "Doctor" } } };
        var oldSlot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, PatientId = patientProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = oldSlot };
        var pastSlot = new AppointmentSlot { Id = newSlotId, DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), Status = AppointmentSlotStatus.Available, DoctorProfile = doc };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(pastSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId };
        var result = await _service.RescheduleAppointmentAsync(apptId, patientUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DoctorRescheduleAsync_OccupiedSlot_Fails()
    {
        var apptId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var doc = new DoctorProfile { Id = doctorProfileId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "123", Role = new Role { Name = "Doctor" } } };
        var oldSlot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, PatientId = patientProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = oldSlot };
        var bookedSlot = new AppointmentSlot { Id = newSlotId, DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), Status = AppointmentSlotStatus.Booked, DoctorProfile = doc };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _slotRepo.Setup(r => r.GetByIdAsync(newSlotId, It.IsAny<CancellationToken>())).ReturnsAsync(bookedSlot);

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId };
        var result = await _service.RescheduleAppointmentAsync(apptId, patientUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DoctorRescheduleAsync_CompletedAppointment_Fails()
    {
        var apptId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var doc = new DoctorProfile { Id = doctorProfileId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "123", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = doctorProfileId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, PatientId = patientProfileId, Status = AppointmentStatus.Completed, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var dto = new RescheduleAppointmentDto { NewSlotId = newSlotId };
        var result = await _service.RescheduleAppointmentAsync(apptId, patientUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ConfirmCompletionAsync_PatientConfirms_MarksConfirmed()
    {
        var apptId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", PatientId = patientProfileId, Doctor = doc, AppointmentSlot = slot, Status = AppointmentStatus.Completed };
        var confirmation = new AppointmentCompletionConfirmation { AppointmentId = apptId, PatientUserId = patientUserId, Status = AppointmentCompletionConfirmationStatus.Pending };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _completionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentCompletionConfirmation> { confirmation });

        var result = await _service.ConfirmCompletionAsync(apptId, patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(AppointmentCompletionConfirmationStatus.Confirmed, confirmation.Status);
        _completionRepo.Verify(r => r.Update(confirmation), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisputeCompletionAsync_PatientDisputes_SetsDisputeStatus()
    {
        var apptId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", PatientId = patientProfileId, Doctor = doc, AppointmentSlot = slot, Status = AppointmentStatus.AwaitingPatientConfirmation };
        var confirmation = new AppointmentCompletionConfirmation { AppointmentId = apptId, PatientUserId = patientUserId, Status = AppointmentCompletionConfirmationStatus.Pending };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _completionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentCompletionConfirmation> { confirmation });

        var dto = new DisputeCompletionDto { Reason = "Doctor did not join session" };
        var result = await _service.DisputeCompletionAsync(apptId, patientUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(AppointmentCompletionConfirmationStatus.Disputed, confirmation.Status);
        Assert.Equal("Doctor did not join session", confirmation.DisputeReason);
        _completionRepo.Verify(r => r.Update(confirmation), Times.Once);
    }

    [Fact]
    public async Task ConfirmGuestAppointmentAsync_ValidToken_ConfirmsAppointment()
    {
        var apptId = Guid.NewGuid();
        var token = "12345678901234567890123456789012";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slotId = Guid.NewGuid();
        var slot = new AppointmentSlot { Id = slotId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-123456", GuestEmail = "guest@test.com", AppointmentSlotId = slotId, Status = AppointmentStatus.AwaitingGuestConfirmation, GuestConfirmationTokenHash = tokenHash, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var dto = new ConfirmGuestAppointmentDto { Token = token };
        var result = await _service.ConfirmGuestAppointmentAsync(dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.Pending, appt.Status);
        _apptRepo.Verify(r => r.Update(appt), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmGuestAppointmentAsync_InvalidBooking_Fails()
    {
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var dto = new ConfirmGuestAppointmentDto { Token = "12345678901234567890123456789012" };
        var result = await _service.ConfirmGuestAppointmentAsync(dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task StartAppointmentAsync_ScheduledDateToday_UpdatesStatusToInProgress()
    {
        var apptId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { Id = slotId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(0, 0), EndTime = new TimeOnly(23, 59), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, AppointmentSlotId = slotId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var result = await _service.StartAppointmentAsync(apptId, doctorUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.InProgress, appt.Status);
        _apptRepo.Verify(r => r.Update(appt), Times.Once);
    }

    [Fact]
    public async Task StartAppointmentAsync_FutureDate_Fails()
    {
        var apptId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { Id = slotId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, AppointmentSlotId = slotId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var result = await _service.StartAppointmentAsync(apptId, doctorUserId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_ExistingAppointment_ReturnsDto()
    {
        var apptId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", PatientId = patientProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot };
        var patient = new PatientProfile { Id = patientProfileId, UserId = userId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "Pat", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.GetAppointmentByIdAsync(apptId, userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_NotFound_ReturnsError()
    {
        _apptRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Appointment?)null);

        var result = await _service.GetAppointmentByIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetMyAppointmentsAsync_PatientRole_ReturnsOnlyPatientAppointments()
    {
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appts = new List<Appointment>
        {
            new() { Id = Guid.NewGuid(), BookingCode = "BK-1", PatientId = patientProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot }
        };

        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(appts);
        _slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { slot });

        var result = await _service.GetMyAppointmentsAsync(patientUserId, 1, 10, null, null, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetDoctorAppointmentsAsync_ReturnsDoctorAppointments()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var docUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Doctor", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = docUser };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appts = new List<Appointment>
        {
            new() { Id = Guid.NewGuid(), BookingCode = "BK-1", DoctorId = doctorProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot }
        };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(appts);
        _slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { slot });

        var result = await _service.GetDoctorAppointmentsAsync(doctorUserId, 1, 10, null, null, null, null, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task MarkPatientNoShowAsync_ValidAppointment_UpdatesStatus()
    {
        var apptId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { Id = slotId, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doctorProfileId, AppointmentSlotId = slotId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var result = await _service.MarkPatientNoShowAsync(apptId, doctorUserId, "Patient did not attend", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(AppointmentStatus.NoShow, appt.Status);
        _apptRepo.Verify(r => r.Update(appt), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_BookingFound_SendsEmail()
    {
        var bookingCode = "BK-123456";
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = Guid.NewGuid(), BookingCode = bookingCode, GuestEmail = "guest@test.com", Status = AppointmentStatus.AwaitingGuestConfirmation, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });

        var dto = new ResendConfirmationDto { BookingCode = bookingCode, Email = "guest@test.com" };
        var result = await _service.ResendConfirmationEmailAsync(dto, CancellationToken.None);

        Assert.True(result.Success);
        _emailService.Verify(e => e.SendAppointmentBookingConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_BookingNotFound_ReturnsError()
    {
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var dto = new ResendConfirmationDto { BookingCode = "INVALID_CODE", Email = "guest@test.com" };
        var result = await _service.ResendConfirmationEmailAsync(dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetClinicalContextAsync_UnauthorizedUser_ReturnsError()
    {
        var apptId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "Pat", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var slot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc };
        var appt = new Appointment { Id = apptId, BookingCode = "BK-1", DoctorId = doc.Id, PatientId = patient.Id, Doctor = doc, AppointmentSlot = slot };

        _apptRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });

        var result = await _service.GetClinicalContextAsync(apptId, unauthorizedUserId, CancellationToken.None);

        Assert.False(result.Success);
    }
}
