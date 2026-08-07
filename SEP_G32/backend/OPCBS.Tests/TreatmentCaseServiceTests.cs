using Microsoft.Extensions.Logging;
using Moq;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class TreatmentCaseServiceTests
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

    public TreatmentCaseServiceTests()
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

    private static User CreateDummyUser(Guid id, string name) => new()
    {
        Id = id,
        FullName = name,
        Email = $"{id}@test.com",
        PasswordHash = "hash",
        PhoneNumber = "0987654321",
        Role = new Role { Name = "Doctor" }
    };

    [Fact]
    public async Task CreateFromPackageAsync_ValidPackage_CopiesSnapshotsAndCreatesCase()
    {
        // Arrange
        var pkgId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var patId = Guid.NewGuid();

        var dummyUser = CreateDummyUser(docId, "Dr Test");

        var package = new TreatmentPackage
        {
            Id = pkgId,
            DoctorId = docId,
            Name = "CBT Anxiety Protocol",
            Description = "10 sessions of CBT",
            TargetOutcome = "Reduced GAD-7 score",
            RecommendedExercises = "3-column thought record",
            Instructions = "Practice daily",
            SessionQuantity = 10,
            ValidityDays = 60,
            Price = 5000000,
            Status = TreatmentPackageStatus.Active,
            Doctor = new DoctorProfile { UserId = docId, User = dummyUser }
        };

        _packageRepo.Setup(r => r.GetByIdAsync(pkgId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _caseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { new() { Id = docId, UserId = docId, User = dummyUser } });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { new() { Id = patId, UserId = patId, User = dummyUser } });

        var dto = new CreateTreatmentCaseDto
        {
            TreatmentPackageId = pkgId,
            DoctorId = docId,
            PatientId = patId,
            PrimaryConcern = "Generalized Anxiety Disorder"
        };

        // Act
        var result = await _service.CreateFromPackageAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("CBT Anxiety Protocol", result.Data.CaseName);
        Assert.Equal("CBT Anxiety Protocol", result.Data.PackageNameSnapshot);
        Assert.Equal(10, result.Data.TotalSessionsSnapshot);
        Assert.Equal(60, result.Data.DurationDaysSnapshot);
        Assert.Equal(5000000, result.Data.PriceSnapshot);
        _caseRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentCase>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteSessionAsync_UpdatesCaseCounterAndProgress()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var dummyUser = CreateDummyUser(Guid.NewGuid(), "Dr Test");

        var treatmentCase = new TreatmentCase
        {
            Id = caseId,
            CaseName = "Depression Therapy",
            TotalSessions = 5,
            CompletedSessions = 0,
            RemainingSessions = 5,
            Status = TreatmentCaseStatus.Active,
            TreatmentPackageId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            TreatmentPackage = null!,
            Doctor = new DoctorProfile { UserId = Guid.NewGuid(), User = dummyUser },
            Patient = new PatientProfile { UserId = Guid.NewGuid(), User = dummyUser }
        };

        var apptId = Guid.NewGuid();
        var appt = new Appointment
        {
            Id = apptId,
            BookingCode = "BC-999",
            AppointmentSlot = null!,
            Doctor = null!,
            Status = AppointmentStatus.Approved
        };

        var session = new TreatmentSession
        {
            Id = sessionId,
            TreatmentCaseId = caseId,
            SessionNumber = 1,
            Status = TreatmentSessionStatus.Scheduled,
            AppointmentId = apptId,
            TreatmentCase = treatmentCase
        };

        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _appointmentRepo.Setup(r => r.GetByIdAsync(apptId, It.IsAny<CancellationToken>())).ReturnsAsync(appt);
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { session });
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal>());
        _assignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TherapyAssignment>());

        var dto = new CompleteSessionDto
        {
            Title = "Session 1: CBT Introduction",
            SessionSummary = "Introduced CBT concepts",
            DoctorClinicalAssessment = "Patient was receptive",
            PatientFriendlySummary = "Learned about cognitive distortions",
            MoodBefore = 4,
            MoodAfter = 7
        };

        // Act
        var result = await _service.CompleteSessionAsync(sessionId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(TreatmentSessionStatus.Completed, session.Status);
        Assert.Equal(1, treatmentCase.CompletedSessions);
        Assert.Equal(4, treatmentCase.RemainingSessions);
        Assert.Equal(20, treatmentCase.OverallProgressPercent);
    }

    [Fact]
    public async Task RecordGoalProgressAsync_WhenRequiredCriterionPasses_AchievesGoalAndKeepsAuditHistory()
    {
        var caseId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var treatmentCase = new TreatmentCase
        {
            Id = caseId, CaseName = "CBT", TreatmentPackageId = Guid.NewGuid(), DoctorId = Guid.NewGuid(), PatientId = Guid.NewGuid(),
            TreatmentPackage = null!, Doctor = null!, Patient = null!, TotalSessions = 1, RemainingSessions = 1
        };
        var goal = new TreatmentGoal
        {
            Id = goalId, TreatmentCaseId = caseId, Title = "Reduce anxiety", Status = GoalStatus.Draft, TreatmentCase = treatmentCase
        };
        var detail = new GoalDetail
        {
            Id = detailId, GoalId = goalId, Title = "Practice CBT", Goal = goal
        };
        var session = new TreatmentSession
        {
            Id = sessionId, TreatmentCaseId = caseId, SessionNumber = 1, Status = TreatmentSessionStatus.Completed, TreatmentCase = treatmentCase
        };
        var link = new TreatmentSessionGoal
        {
            Id = Guid.NewGuid(), TreatmentSessionId = sessionId, GoalDetailId = detailId, TreatmentSession = session, GoalDetail = detail
        };
        var criterion = new GoalSuccessCriteria
        {
            Id = Guid.NewGuid(), GoalId = goalId, CriteriaType = GoalSuccessCriteriaType.ProgressPercentage,
            DataSource = GoalCriteriaDataSource.GoalProgress, Operator = GoalCriteriaOperator.GreaterThanOrEqual,
            TargetValue = 90, IsRequired = true, Goal = goal
        };

        _goalRepo.Setup(r => r.GetByIdAsync(goalId, It.IsAny<CancellationToken>())).ReturnsAsync(goal);
        _goalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentGoal> { goal });
        _goalDetailRepo.Setup(r => r.GetByIdAsync(detailId, It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _goalDetailRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GoalDetail> { detail });
        _sessionRepo.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _sessionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSession> { session });
        _sessionGoalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentSessionGoal> { link });
        _successCriteriaRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GoalSuccessCriteria> { criterion });
        _assignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TherapyAssignment>());
        _caseRepo.Setup(r => r.GetByIdAsync(caseId, It.IsAny<CancellationToken>())).ReturnsAsync(treatmentCase);

        var result = await _service.RecordGoalProgressAsync(new CreateGoalProgressDto
        {
            GoalId = goalId, GoalDetailId = detailId, TreatmentSessionId = sessionId, ProgressPercent = 100, DoctorComment = "Completed CBT practice"
        });

        Assert.True(result.Success);
        Assert.Equal(GoalDetailStatus.Completed, detail.Status);
        Assert.Equal(GoalStatus.Achieved, goal.Status);
        Assert.Equal(100, goal.ProgressPercent);
        _goalProgressRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentGoalProgress>(), It.IsAny<CancellationToken>()), Times.Once);
        _criteriaEvaluationRepo.Verify(r => r.AddAsync(It.Is<SuccessCriteriaEvaluation>(e => e.IsPassed && e.CurrentValue == 100), It.IsAny<CancellationToken>()), Times.Once);
    }
}
