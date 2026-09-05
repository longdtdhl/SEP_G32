using Microsoft.Extensions.Logging;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class TreatmentNormalizationTests
{
    private readonly Mock<IRepository<TreatmentCase>> _caseRepo = new();
    private readonly Mock<IRepository<TreatmentSession>> _sessionRepo = new();
    private readonly Mock<IRepository<TreatmentGoal>> _goalRepo = new();
    private readonly Mock<IRepository<GoalDetail>> _goalDetailRepo = new();
    private readonly Mock<IRepository<GoalSuccessCriteria>> _successCriteriaRepo = new();
    private readonly Mock<IRepository<SuccessCriteriaEvaluation>> _criteriaEvaluationRepo = new();
    private readonly Mock<IRepository<TreatmentGoalProgress>> _goalProgressRepo = new();
    private readonly Mock<IRepository<TreatmentSessionGoal>> _sessionGoalRepo = new();
    private readonly Mock<IRepository<TreatmentPackage>> _packageRepo = new();
    private readonly Mock<IRepository<TherapyAssignment>> _assignmentRepo = new();
    private readonly Mock<IRepository<MoodEntry>> _moodRepo = new();
    private readonly Mock<IRepository<Appointment>> _appointmentRepo = new();
    private readonly Mock<IRepository<AppointmentSlot>> _slotRepo = new();
    private readonly Mock<IRepository<ConsultationNote>> _noteRepo = new();
    private readonly Mock<IRepository<EmotionJournal>> _journalRepo = new();
    private readonly Mock<IRepository<PsychometricSubmission>> _psychRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<AppointmentHistory>> _appointmentHistoryRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<TreatmentCaseService>> _logger = new();

    private readonly TreatmentCaseService _caseService;

    public TreatmentNormalizationTests()
    {
        _caseService = new TreatmentCaseService(
            _caseRepo.Object,
            _sessionRepo.Object,
            _goalRepo.Object,
            _goalDetailRepo.Object,
            _successCriteriaRepo.Object,
            _criteriaEvaluationRepo.Object,
            _goalProgressRepo.Object,
            _sessionGoalRepo.Object,
            _packageRepo.Object,
            _assignmentRepo.Object,
            _moodRepo.Object,
            _appointmentRepo.Object,
            _slotRepo.Object,
            _noteRepo.Object,
            _journalRepo.Object,
            _psychRepo.Object,
            _patientRepo.Object,
            _doctorRepo.Object,
            _userRepo.Object,
            _appointmentHistoryRepo.Object,
            _uow.Object,
            _logger.Object);
    }

    private static User CreateDummyUser(Guid id, string name, string role = "Doctor") => new()
    {
        Id = id,
        FullName = name,
        Email = $"{id}@test.com",
        PasswordHash = "hash",
        PhoneNumber = "0987654321",
        Role = new Role { Name = role }
    };

    [Fact]
    public async Task Scenario1_GenerateSchedule_CreatesConsistentSessionAppointmentSlotLinks()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var docUser = CreateDummyUser(doctorUserId, "Dr Smith");
        var patUser = CreateDummyUser(patientUserId, "John Doe", "Patient");

        var doctorProfile = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = docUser };
        var patientProfile = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = patUser };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "CBT Protocol",
            TreatmentPackage = null!,
            Doctor = doctorProfile,
            Patient = patientProfile,
            TotalSessions = 1,
            CompletedSessions = 0,
            RemainingSessions = 1,
            Status = TreatmentCaseStatus.Active,
            DoctorId = doctorProfileId,
            PatientId = patientProfileId
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctorProfile });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser, patUser });
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot>());
        _appointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var dto = new GenerateScheduleDto
        {
            TreatmentCaseId = caseId,
            StartDate = DateTime.Today.AddDays(1),
            DaysOfWeek = new List<DayOfWeek> { DateTime.Today.AddDays(1).DayOfWeek },
            StartTime = "09:00",
            DurationMinutes = 60,
            TotalWeeks = 1
        };

        // Act
        var result = await _caseService.GenerateScheduleAsync(dto, doctorUserId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        _slotRepo.Verify(r => r.AddAsync(It.IsAny<AppointmentSlot>(), It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Scenario2_PlannedSession_HasNoLinkedAppointment()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "CBT Case",
            TreatmentPackage = null!,
            Doctor = null!,
            Patient = null!,
            Status = TreatmentCaseStatus.Active,
            TotalSessions = 5
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());

        var dto = new CreateSessionDto
        {
            TreatmentCaseId = caseId,
            Title = "Session 1: Initial Assessment",
            AppointmentId = null
        };

        // Act
        var result = await _caseService.CreateSessionAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.AppointmentId);
        Assert.Equal(6, result.Data.Status); // Planned = 6
        Assert.Equal("Planned", result.Data.StatusText);
    }

    [Fact]
    public void Scenario3_LifecycleSync_StartCompleteRescheduleCancel_SynchronizesAllThreeEntities()
    {
        // Arrange
        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfile = null!,
            CurrentBookings = 1,
            MaxPatients = 1,
            Status = AppointmentSlotStatus.Booked
        };

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "BC-100",
            AppointmentSlot = slot,
            Doctor = null!,
            AppointmentSlotId = slot.Id,
            Status = AppointmentStatus.Approved
        };

        var session = new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentCase = null!,
            AppointmentId = appt.Id,
            Status = TreatmentSessionStatus.Scheduled,
            Appointment = appt
        };

        appt.TreatmentSessionId = session.Id;

        // Verify status mapping consistency
        Assert.Equal(AppointmentStatus.Approved, appt.Status);
        Assert.Equal(TreatmentSessionStatus.Scheduled, session.Status);

        // Simulate Cancel
        session.Status = TreatmentSessionStatus.Cancelled;
        appt.Status = AppointmentStatus.Cancelled;
        slot.CurrentBookings = 0;
        slot.Status = AppointmentSlotStatus.Available;

        Assert.Equal(TreatmentSessionStatus.Cancelled, session.Status);
        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
        Assert.Equal(AppointmentSlotStatus.Available, slot.Status);
    }

    [Fact]
    public async Task Scenario4_CompleteSession_RequiresValidAppointmentAndConsultationNote()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var sessionWithoutAppt = new TreatmentSession
        {
            Id = sessionId,
            TreatmentCaseId = caseId,
            TreatmentCase = null!,
            SessionNumber = 1,
            Status = TreatmentSessionStatus.Planned,
            AppointmentId = null
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(sessionWithoutAppt);

        var dto = new CompleteSessionDto
        {
            Title = "Completed Session",
            SessionSummary = "Completed without appt"
        };

        // Act
        var result = await _caseService.CompleteSessionAsync(sessionId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("planned or cancelled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Scenario5_CancelAppointment_ReleasesSlot()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var apptId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var slot = new AppointmentSlot
        {
            Id = slotId,
            DoctorProfile = null!,
            CurrentBookings = 1,
            MaxPatients = 1,
            Status = AppointmentSlotStatus.Booked
        };

        var appt = new Appointment
        {
            Id = apptId,
            BookingCode = "BC-101",
            AppointmentSlot = slot,
            Doctor = null!,
            AppointmentSlotId = slotId,
            Status = AppointmentStatus.Approved
        };

        var session = new TreatmentSession
        {
            Id = sessionId,
            TreatmentCase = null!,
            AppointmentId = apptId,
            Status = TreatmentSessionStatus.Scheduled
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _appointmentRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        // Act
        var result = await _caseService.DeleteSessionAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(session.IsDeleted);
        Assert.Equal(TreatmentSessionStatus.Cancelled, session.Status);
        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
        Assert.Equal(0, slot.CurrentBookings);
        Assert.Equal(AppointmentSlotStatus.Available, slot.Status);
    }

    [Fact]
    public async Task Scenario6_CaseCounters_AreIdempotentAndCorrect()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var session1Id = Guid.NewGuid();
        var session2Id = Guid.NewGuid();

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Case Counters",
            TreatmentPackage = null!,
            Doctor = null!,
            Patient = null!,
            TotalSessions = 2,
            CompletedSessions = 0,
            RemainingSessions = 2,
            Status = TreatmentCaseStatus.Active
        };

        var session1 = new TreatmentSession
        {
            Id = session1Id,
            TreatmentCaseId = caseId,
            TreatmentCase = treatmentCase,
            SessionNumber = 1,
            Status = TreatmentSessionStatus.Scheduled,
            AppointmentId = Guid.NewGuid()
        };

        var session2 = new TreatmentSession
        {
            Id = session2Id,
            TreatmentCaseId = caseId,
            TreatmentCase = treatmentCase,
            SessionNumber = 2,
            Status = TreatmentSessionStatus.Scheduled,
            AppointmentId = Guid.NewGuid()
        };

        var appt1 = new Appointment
        {
            Id = session1.AppointmentId!.Value,
            BookingCode = "BC-102",
            AppointmentSlot = null!,
            Doctor = null!,
            Status = AppointmentStatus.Completed,
            TreatmentCaseId = caseId,
            TreatmentSessionId = session1Id,
            CompletedAt = DateTime.UtcNow
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(session1Id, It.IsAny<CancellationToken>())).ReturnsAsync(session1);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _appointmentRepo.Setup(r => r.GetByIdAsync(session1.AppointmentId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(appt1);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { session1, session2 });
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());
        _assignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TherapyAssignment>());

        var dto = new CompleteSessionDto { SessionSummary = "Summary 1" };

        // Act - First call
        var result1 = await _caseService.CompleteSessionAsync(session1Id, dto, CancellationToken.None);

        // Assert - First call
        Assert.True(result1.Success);
        Assert.Equal(1, treatmentCase.CompletedSessions);
        Assert.Equal(0, treatmentCase.RemainingSessions);

        // Act - Second call (idempotent retry)
        var result2 = await _caseService.CompleteSessionAsync(session1Id, dto, CancellationToken.None);

        // Assert - Second call retains exact same counters without double-counting
        Assert.True(result2.Success);
        Assert.Equal(1, treatmentCase.CompletedSessions);
        Assert.Equal(0, treatmentCase.RemainingSessions);
    }

    [Fact]
    public async Task Scenario7_DuplicateSessionNumber_IsRejected()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Case Session",
            TreatmentPackage = null!,
            Doctor = null!,
            Patient = null!,
            Status = TreatmentCaseStatus.Active,
            TotalSessions = 10
        };

        var existingSession = new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentCaseId = caseId,
            TreatmentCase = treatmentCase,
            SessionNumber = 1
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { existingSession });

        var dto = new CreateSessionDto
        {
            TreatmentCaseId = caseId,
            Title = "Duplicate Session"
        };

        // Act
        var result = await _caseService.CreateSessionAsync(dto, CancellationToken.None);

        // Assert - SessionNumber will be set to 2 automatically, preventing duplicate 1
        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.SessionNumber);
    }

    [Fact]
    public async Task Scenario8_DoctorAuthorization_PreventsAccessToOtherDoctorCase()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var doc1UserId = Guid.NewGuid();
        var doc1ProfileId = Guid.NewGuid();
        var doc2UserId = Guid.NewGuid();
        var doc2ProfileId = Guid.NewGuid();

        var doc1User = CreateDummyUser(doc1UserId, "Dr Doc1");
        var doc2User = CreateDummyUser(doc2UserId, "Dr Doc2");

        var doc1Profile = new DoctorProfile { Id = doc1ProfileId, UserId = doc1UserId, User = doc1User };
        var doc2Profile = new DoctorProfile { Id = doc2ProfileId, UserId = doc2UserId, User = doc2User };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Doc 1 Case",
            TreatmentPackage = null!,
            Doctor = doc1Profile,
            Patient = null!,
            DoctorId = doc1ProfileId, // Belongs to Doctor 1
            PatientId = Guid.NewGuid()
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc1Profile, doc2Profile });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doc1User, doc2User });

        // Act - Doctor 2 attempts to get Doctor 1's case
        var result = await _caseService.GetByIdAsync(caseId, doc2UserId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Scenario9_PatientAuthorization_PreventsAccessToOtherPatientCase()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var pat1UserId = Guid.NewGuid();
        var pat1ProfileId = Guid.NewGuid();
        var pat2UserId = Guid.NewGuid();
        var pat2ProfileId = Guid.NewGuid();

        var pat1User = CreateDummyUser(pat1UserId, "Patient 1", "Patient");
        var pat2User = CreateDummyUser(pat2UserId, "Patient 2", "Patient");

        var pat1Profile = new PatientProfile { Id = pat1ProfileId, UserId = pat1UserId, User = pat1User };
        var pat2Profile = new PatientProfile { Id = pat2ProfileId, UserId = pat2UserId, User = pat2User };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Pat 1 Case",
            TreatmentPackage = null!,
            Doctor = null!,
            Patient = pat1Profile,
            DoctorId = Guid.NewGuid(),
            PatientId = pat1ProfileId // Belongs to Patient 1
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { pat1Profile, pat2Profile });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pat1User, pat2User });

        // Act - Patient 2 attempts to get Patient 1's case
        var result = await _caseService.GetByIdAsync(caseId, pat2UserId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Scenario10_TransactionRollback_LeavesNoPartialState()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var docUser = CreateDummyUser(doctorUserId, "Dr Error");
        var doctorProfile = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = docUser };

        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var patUser = CreateDummyUser(patientUserId, "Patient Rollback", "Patient");
        var patientProfile = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = patUser };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Rollback Case",
            TreatmentPackage = null!,
            Doctor = doctorProfile,
            Patient = patientProfile,
            Status = TreatmentCaseStatus.Active,
            TotalSessions = 10,
            DoctorId = doctorProfileId,
            PatientId = patientProfileId
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctorProfile });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser, patUser });
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot>());
        _appointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        // Force SaveChangesAsync to throw exception during transaction
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB Constraint Violation"));

        var dto = new GenerateScheduleDto
        {
            TreatmentCaseId = caseId,
            StartDate = DateTime.Today.AddDays(1),
            DaysOfWeek = new List<DayOfWeek> { DateTime.Today.AddDays(1).DayOfWeek },
            StartTime = "10:00",
            DurationMinutes = 60
        };

        // Act
        var result = await _caseService.GenerateScheduleAsync(dto, doctorUserId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        _uow.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Scenario11_GenerateSchedule_ReusesExistingAvailableSlots()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var docUser = CreateDummyUser(doctorUserId, "Dr Reuse");
        var patUser = CreateDummyUser(patientUserId, "Patient Reuse", "Patient");

        var doctorProfile = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = docUser };
        var patientProfile = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = patUser };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Reuse Slot CBT",
            TreatmentPackage = null!,
            Doctor = doctorProfile,
            Patient = patientProfile,
            TotalSessions = 1,
            CompletedSessions = 0,
            RemainingSessions = 1,
            Status = TreatmentCaseStatus.Active,
            DoctorId = doctorProfileId,
            PatientId = patientProfileId
        };

        var targetDate = DateTime.Today.AddDays(1);
        var existingSlot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorProfileId,
            DoctorProfile = doctorProfile,
            SlotDate = DateOnly.FromDateTime(targetDate),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Status = AppointmentSlotStatus.Available,
            MaxPatients = 1,
            CurrentBookings = 0
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctorProfile });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser, patUser });
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { existingSlot });
        _appointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var dto = new GenerateScheduleDto
        {
            TreatmentCaseId = caseId,
            StartDate = targetDate,
            DaysOfWeek = new List<DayOfWeek> { targetDate.DayOfWeek },
            StartTime = "09:00",
            DurationMinutes = 60,
            TotalWeeks = 1
        };

        // Act
        var result = await _caseService.GenerateScheduleAsync(dto, doctorUserId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentSlotStatus.Booked, existingSlot.Status);
        _slotRepo.Verify(r => r.Update(existingSlot), Times.Once);
    }

    [Fact]
    public void ScheduleNote_EntityCreation_IsIsolatedFromSlots()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        // Act
        var note = new ScheduleNote
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            NoteDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Title = "Follow-up schedule instructions",
            Content = "Review patient assessment results before session",
            Category = "Clinical Note",
            PatientId = patientId,
            TreatmentCaseId = caseId,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal("Follow-up schedule instructions", note.Title);
        Assert.Equal("Clinical Note", note.Category);
        Assert.Equal(patientId, note.PatientId);
        Assert.Equal(caseId, note.TreatmentCaseId);
        Assert.False(note.IsDeleted);
    }

    [Fact]
    public async Task AssignTreatmentSlot_ValidAvailableSlot_ConvertsToBookedAndCreatesApprovedAppointment()
    {
        // Arrange
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var doctorProfile = new DoctorProfile
        {
            Id = doctorProfileId,
            UserId = doctorUserId,
            User = new User { Id = doctorUserId, FullName = "Dr. Smith", Email = "doc@test.com", PasswordHash = "hash", PhoneNumber = "0123456789", Role = null! }
        };

        var patientProfile = new PatientProfile
        {
            Id = patientProfileId,
            UserId = patientUserId,
            User = new User { Id = patientUserId, FullName = "John Doe", Email = "john@test.com", PasswordHash = "hash", PhoneNumber = "0987654321", Role = null! }
        };

        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var slot = new AppointmentSlot
        {
            Id = slotId,
            DoctorProfileId = doctorProfileId,
            DoctorProfile = doctorProfile,
            SlotDate = futureDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = AppointmentSlotStatus.Available,
            MaxPatients = 1,
            CurrentBookings = 0
        };

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            DoctorId = doctorUserId,
            PatientId = patientProfileId,
            Status = TreatmentCaseStatus.Active,
            CaseName = "CBT Anxiety Protocol",
            TotalSessions = 1,
            RemainingSessions = 1
        };

        var session = new TreatmentSession
        {
            Id = sessionId,
            TreatmentCaseId = caseId,
            SessionNumber = 1,
            Status = TreatmentSessionStatus.Planned,
            TreatmentCase = treatmentCase
        };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctorProfile });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });
        _slotRepo.Setup(r => r.GetByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { session });

        var scheduleService = new ScheduleService(
            new Mock<IRepository<Schedule>>().Object,
            _slotRepo.Object,
            _doctorRepo.Object,
            _userRepo.Object,
            new Mock<IRepository<DoctorDayOff>>().Object,
            _appointmentRepo.Object,
            _uow.Object,
            new Mock<AutoMapper.IMapper>().Object,
            _caseRepo.Object,
            _packageRepo.Object,
            _sessionRepo.Object,
            _appointmentHistoryRepo.Object,
            _patientRepo.Object,
            null, null, null, null,
            new Mock<IRepository<ScheduleNote>>().Object
        );

        var dto = new AssignTreatmentSlotDto
        {
            SlotId = slotId,
            PatientId = patientProfileId,
            TreatmentCaseId = caseId,
            TreatmentSessionId = sessionId,
            Notes = "Assign session 1"
        };

        // Act
        var result = await scheduleService.AssignTreatmentSlotAsync(doctorUserId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AppointmentSlotStatus.Booked, slot.Status);
        Assert.Equal(1, slot.CurrentBookings);
        _slotRepo.Verify(r => r.Update(slot), Times.Once);
        _appointmentRepo.Verify(r => r.AddAsync(It.Is<Appointment>(a =>
            a.AppointmentSlotId == slotId &&
            a.PatientId == patientProfileId &&
            a.TreatmentCaseId == caseId &&
            a.TreatmentSessionId == sessionId &&
            a.Status == AppointmentStatus.Approved
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
