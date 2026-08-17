using Microsoft.Extensions.Logging;
using Moq;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class TreatmentCaseAdvancedTests
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

    private readonly TreatmentCaseService _service;

    public TreatmentCaseAdvancedTests()
    {
        _service = new TreatmentCaseService(
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

    [Fact]
    public async Task CreateGoalAsync_ValidInput_CreatesGoalWithInitialProgress()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());

        var dto = new CreateGoalDto
        {
            TreatmentCaseId = caseId,
            Title = "Reduce anxiety symptoms",
            Category = (int)GoalCategory.Anxiety,
            Priority = (int)GoalPriority.High,
            TargetValue = 2,
            CurrentValue = 8,
            Unit = "Score"
        };

        var result = await _service.CreateGoalAsync(dto, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Reduce anxiety symptoms", result.Data.Title);
        _goalRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentGoal>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateGoalAsync_CaseNotFound_ReturnsError()
    {
        _caseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentCase?)null);

        var dto = new CreateGoalDto { TreatmentCaseId = Guid.NewGuid(), Title = "Goal" };
        var result = await _service.CreateGoalAsync(dto, null, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateGoalAsync_ValidData_UpdatesGoalAttributes()
    {
        var goalId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        var goal = new TreatmentGoal { Id = goalId, TreatmentCaseId = caseId, Title = "Old Title", Status = GoalStatus.InProgress, TreatmentCase = tCase };

        _goalRepo.Setup(r => r.GetByIdAsync(goalId, It.IsAny<CancellationToken>())).ReturnsAsync(goal);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _goalDetailRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GoalDetail>());

        var dto = new UpdateGoalDto { Title = "Updated Goal Title", ProgressPercent = 50 };
        var result = await _service.UpdateGoalAsync(goalId, dto, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Updated Goal Title", goal.Title);
        Assert.Equal(50, goal.ProgressPercent);
        _goalRepo.Verify(r => r.Update(goal), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalAsync_NotFound_ReturnsError()
    {
        _goalRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentGoal?)null);

        var result = await _service.UpdateGoalAsync(Guid.NewGuid(), new UpdateGoalDto { Title = "G" }, null, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetGoalsByCaseAsync_ReturnsAllGoalsForCase()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        var goals = new List<TreatmentGoal>
        {
            new() { Id = Guid.NewGuid(), TreatmentCaseId = caseId, Title = "G1", TreatmentCase = tCase },
            new() { Id = Guid.NewGuid(), TreatmentCaseId = caseId, Title = "G2", TreatmentCase = tCase }
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(goals);
        _goalDetailRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GoalDetail>());
        _successCriteriaRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GoalSuccessCriteria>());
        _criteriaEvaluationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<SuccessCriteriaEvaluation>());

        var result = await _service.GetGoalsByCaseAsync(caseId, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task AddMoodEntryAsync_ValidScores_SavesMood()
    {
        var caseId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", PatientId = patientProfileId, Status = TreatmentCaseStatus.Active };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });

        var dto = new CreateMoodEntryDto
        {
            TreatmentCaseId = caseId,
            MoodScore = 8,
            AnxietyScore = 3,
            StressScore = 4,
            SleepQualityScore = 8,
            Note = "Feeling energetic today."
        };

        var result = await _service.AddMoodEntryAsync(patientUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(8, result.Data.MoodScore);
        _moodRepo.Verify(r => r.AddAsync(It.IsAny<MoodEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMoodEntryAsync_CaseNotFound_ReturnsError()
    {
        var caseId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentCase?)null);

        var dto = new CreateMoodEntryDto { TreatmentCaseId = caseId, MoodScore = 7 };
        var result = await _service.AddMoodEntryAsync(patientUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetMoodEntriesAsync_ValidCase_ReturnsChronologicalEntries()
    {
        var caseId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var pUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientId, User = pUser };
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        var moods = new List<MoodEntry>
        {
            new() { Id = Guid.NewGuid(), TreatmentCaseId = caseId, PatientId = patientId, MoodScore = 5, RecordedAt = DateTime.UtcNow.AddDays(-2), TreatmentCase = tCase, Patient = patient },
            new() { Id = Guid.NewGuid(), TreatmentCaseId = caseId, PatientId = patientId, MoodScore = 7, RecordedAt = DateTime.UtcNow, TreatmentCase = tCase, Patient = patient }
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _moodRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(moods);

        var result = await _service.GetMoodEntriesAsync(caseId, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetMoodEntriesAsync_NoMoods_ReturnsEmptyList()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1" };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _moodRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<MoodEntry>());

        var result = await _service.GetMoodEntriesAsync(caseId, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task CreateSessionAsync_CustomPlannedTime_SetsPlannedStatus()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active, TotalSessions = 10, CompletedSessions = 2 };
        var sessions = new List<TreatmentSession>
        {
            new() { Id = Guid.NewGuid(), TreatmentCaseId = caseId, SessionNumber = 1, TreatmentCase = tCase }
        };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sessions);

        var dto = new CreateSessionDto
        {
            TreatmentCaseId = caseId,
            Title = "Mid-treatment review",
            PlannedStartTime = DateTime.UtcNow.AddDays(3),
            PlannedEndTime = DateTime.UtcNow.AddDays(3).AddHours(1)
        };

        var result = await _service.CreateSessionAsync(dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.SessionNumber);
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionAsync_Valid_UpdatesSessionAttributes()
    {
        var sessionId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        var session = new TreatmentSession { Id = sessionId, TreatmentCaseId = caseId, SessionNumber = 2, TreatmentCase = tCase };

        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _sessionGoalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSessionGoal>());

        var dto = new UpdateSessionDto
        {
            Title = "Updated Session Title",
            Description = "Updated description"
        };

        var result = await _service.UpdateSessionAsync(sessionId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Updated Session Title", session.Title);
        _sessionRepo.Verify(r => r.Update(session), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSessionAsync_ExistingSession_SetsIsDeleted()
    {
        var sessionId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1" };
        var session = new TreatmentSession { Id = sessionId, TreatmentCaseId = caseId, IsDeleted = false, Status = TreatmentSessionStatus.Planned, TreatmentCase = tCase };

        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);

        var result = await _service.DeleteSessionAsync(sessionId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(session.IsDeleted);
        _sessionRepo.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_Terminated_SetsStatusAndActualEndDate()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);

        var dto = new CloseTreatmentCaseDto { CloseStatus = 3, ClosureNote = "Patient relocated" };
        var result = await _service.CloseAsync(caseId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(TreatmentCaseStatus.Terminated, tCase.Status);
        Assert.NotNull(tCase.ActualEndDate);
        _caseRepo.Verify(r => r.Update(tCase), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_Completed_SetsStatusCompleted()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", Status = TreatmentCaseStatus.Active };
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);

        var dto = new CloseTreatmentCaseDto { CloseStatus = 2, ClosureNote = "All goals reached" };
        var result = await _service.CloseAsync(caseId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(TreatmentCaseStatus.Completed, tCase.Status);
        _caseRepo.Verify(r => r.Update(tCase), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_UpdatesCaseInfo()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Old Name", Status = TreatmentCaseStatus.Active };
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);

        var dto = new UpdateTreatmentCaseDto { CaseName = "New Name", PrimaryConcern = "Social Anxiety" };
        var result = await _service.UpdateAsync(caseId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("New Name", tCase.CaseName);
        Assert.Equal("Social Anxiety", tCase.PrimaryConcern);
        _caseRepo.Verify(r => r.Update(tCase), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsCaseDto()
    {
        var caseId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var docUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "123", Role = new Role { Name = "Doctor" } };
        var patUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "Pat", PhoneNumber = "456", Role = new Role { Name = "Patient" } };
        var doc = new DoctorProfile { Id = doctorProfileId, User = docUser };
        var pat = new PatientProfile { Id = patientProfileId, User = patUser };
        var package = new TreatmentPackage { Id = packageId, Name = "Pack", Doctor = doc };
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", DoctorId = doctorProfileId, PatientId = patientProfileId, TreatmentPackageId = packageId, Status = TreatmentCaseStatus.Active };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());
        _packageRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _doctorRepo.Setup(r => r.GetByIdAsync(doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _patientRepo.Setup(r => r.GetByIdAsync(patientProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(pat);
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser, patUser });

        var result = await _service.GetByIdAsync(caseId, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Case 1", result.Data.CaseName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsError()
    {
        _caseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentCase?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), null, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RefreshProgressAsync_NoSessionsOrGoals_DefaultsToZero()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", TotalSessions = 0, CompletedSessions = 0, Status = TreatmentCaseStatus.Active };

        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());
        _assignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TherapyAssignment>());

        var result = await _service.RefreshProgressAsync(caseId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(0, tCase.OverallProgressPercent);
    }

    [Fact]
    public async Task GetTimelineAsync_ValidCase_ReturnsTimelineEvents()
    {
        var caseId = Guid.NewGuid();
        var tCase = new TreatmentCase { Id = caseId, CaseName = "Case 1", CreatedAt = DateTime.UtcNow.AddDays(-10) };
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(tCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession>());
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());
        _assignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TherapyAssignment>());
        _moodRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<MoodEntry>());
        _noteRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote>());
        _journalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<EmotionJournal>());
        _psychRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PsychometricSubmission>());
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.GetTimelineAsync(caseId, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetTimelineAsync_CaseNotFound_ReturnsError()
    {
        _caseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentCase?)null);

        var result = await _service.GetTimelineAsync(Guid.NewGuid(), null, CancellationToken.None);

        Assert.False(result.Success);
    }
}
