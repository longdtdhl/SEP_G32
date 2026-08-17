using Moq;
using OPCBS.Application.DTOs.Therapy;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class TherapyServicesTests
{
    private readonly Mock<IRepository<TherapyAssignment>> _mockAssignmentRepo = new();
    private readonly Mock<IRepository<TreatmentPackage>> _mockPackageRepo = new();
    private readonly Mock<IRepository<EmotionJournal>> _mockJournalRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _mockPatientRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly TherapyAssignmentService _assignmentService;
    private readonly EmotionJournalService _journalService;

    public TherapyServicesTests()
    {
        _assignmentService = new TherapyAssignmentService(
            _mockAssignmentRepo.Object,
            _mockPackageRepo.Object,
            _mockUow.Object);

        _journalService = new EmotionJournalService(
            _mockJournalRepo.Object,
            _mockPatientRepo.Object,
            _mockUserRepo.Object,
            _mockUow.Object);
    }

    // ==================== TherapyAssignmentService Tests (14 Tests) ====================

    [Fact]
    public async Task GetByPackageAsync_ValidPackage_ReturnsSortedAssignments()
    {
        var packageId = Guid.NewGuid();
        var assignments = new List<TherapyAssignment>
        {
            new() { Id = Guid.NewGuid(), TreatmentPackageId = packageId, Title = "A1", CreatedAt = DateTime.UtcNow.AddDays(-2), Status = HomeworkStatus.Assigned },
            new() { Id = Guid.NewGuid(), TreatmentPackageId = packageId, Title = "A2", CreatedAt = DateTime.UtcNow, Status = HomeworkStatus.Submitted }
        };
        _mockAssignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(assignments);

        var result = await _assignmentService.GetByPackageAsync(packageId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("A2", result.Data[0].Title);
    }

    [Fact]
    public async Task GetByPackageAsync_ExcludesDeletedAssignments()
    {
        var packageId = Guid.NewGuid();
        var assignments = new List<TherapyAssignment>
        {
            new() { Id = Guid.NewGuid(), TreatmentPackageId = packageId, Title = "Active", IsDeleted = false },
            new() { Id = Guid.NewGuid(), TreatmentPackageId = packageId, Title = "Deleted", IsDeleted = true }
        };
        _mockAssignmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(assignments);

        var result = await _assignmentService.GetByPackageAsync(packageId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        Assert.Equal("Active", result.Data![0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAssignment_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Exercise 1", Status = HomeworkStatus.Assigned };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var result = await _assignmentService.GetByIdAsync(id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Exercise 1", result.Data.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsError()
    {
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TherapyAssignment?)null);

        var result = await _assignmentService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedAssignment_ReturnsError()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Deleted", IsDeleted = true };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var result = await _assignmentService.GetByIdAsync(id, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_ValidPackage_CreatesAssignedStatus()
    {
        var packageId = Guid.NewGuid();
        var doc = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new TreatmentPackage { Id = packageId, Name = "CBT Pack", Doctor = doc };
        _mockPackageRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var dto = new CreateAssignmentDto
        {
            TreatmentPackageId = packageId,
            Title = "Thought Record",
            Description = "Log daily thoughts",
            DetailedInstructions = "Step 1: Notice triggers",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = await _assignmentService.CreateAsync(dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Thought Record", result.Data.Title);
        Assert.Equal((int)HomeworkStatus.Assigned, result.Data.Status);
        _mockAssignmentRepo.Verify(r => r.AddAsync(It.IsAny<TherapyAssignment>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PackageNotFound_ReturnsError()
    {
        _mockPackageRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TreatmentPackage?)null);

        var dto = new CreateAssignmentDto { TreatmentPackageId = Guid.NewGuid(), Title = "Assignment" };
        var result = await _assignmentService.CreateAsync(dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SubmitAsync_Valid_UpdatesStatusToSubmittedAndTimestamps()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Homework", Status = HomeworkStatus.Assigned };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var dto = new SubmitAssignmentDto { PatientSubmission = "Completed exercise sheet", PatientSubmissionUrl = "http://files/sheet.pdf" };
        var result = await _assignmentService.SubmitAsync(id, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal((int)HomeworkStatus.Submitted, result.Data!.Status);
        Assert.Equal("Completed exercise sheet", assignment.PatientSubmission);
        Assert.NotNull(assignment.SubmittedAt);
        _mockAssignmentRepo.Verify(r => r.Update(assignment), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_NotFound_ReturnsError()
    {
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TherapyAssignment?)null);

        var result = await _assignmentService.SubmitAsync(Guid.NewGuid(), new SubmitAssignmentDto(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task FeedbackAsync_SubmittedAssignment_UpdatesToReviewed()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Homework", Status = HomeworkStatus.Submitted };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var dto = new FeedbackAssignmentDto { DoctorFeedback = "Great progress on cognitive restructuring!" };
        var result = await _assignmentService.FeedbackAsync(id, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal((int)HomeworkStatus.Reviewed, result.Data!.Status);
        Assert.Equal("Great progress on cognitive restructuring!", assignment.DoctorFeedback);
        Assert.NotNull(assignment.FeedbackAt);
        _mockAssignmentRepo.Verify(r => r.Update(assignment), Times.Once);
    }

    [Fact]
    public async Task FeedbackAsync_AssignedNotSubmitted_ReturnsError()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Homework", Status = HomeworkStatus.Assigned };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var dto = new FeedbackAssignmentDto { DoctorFeedback = "Feedback before submit" };
        var result = await _assignmentService.FeedbackAsync(id, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task FeedbackAsync_NotFound_ReturnsError()
    {
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TherapyAssignment?)null);

        var result = await _assignmentService.FeedbackAsync(Guid.NewGuid(), new FeedbackAssignmentDto(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAsync_Existing_SetsIsDeletedAndSaves()
    {
        var id = Guid.NewGuid();
        var assignment = new TherapyAssignment { Id = id, Title = "Homework", IsDeleted = false };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var result = await _assignmentService.DeleteAsync(id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(assignment.IsDeleted);
        _mockAssignmentRepo.Verify(r => r.Update(assignment), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsError()
    {
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TherapyAssignment?)null);

        var result = await _assignmentService.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    // ==================== EmotionJournalService Tests (6 Tests) ====================

    [Fact]
    public async Task Journal_GetByPatientAsync_ValidPatient_ReturnsJournals()
    {
        var userId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var pUser = new User { Id = userId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientId, UserId = userId, User = pUser };
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(pUser);

        var journals = new List<EmotionJournal>
        {
            new() { Id = Guid.NewGuid(), PatientId = patientId, Title = "Day 1", Content = "Good day", MoodScale = 4, StressScale = 2, CreatedAt = DateTime.UtcNow.AddHours(-1), Patient = patient },
            new() { Id = Guid.NewGuid(), PatientId = patientId, Title = "Day 2", Content = "Calm day", MoodScale = 5, StressScale = 1, CreatedAt = DateTime.UtcNow, Patient = patient }
        };
        _mockJournalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(journals);

        var result = await _journalService.GetByPatientAsync(userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.Equal("Day 2", result.Data[0].Title);
    }

    [Fact]
    public async Task Journal_GetByPatientAsync_PatientNotFound_ReturnsError()
    {
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());

        var result = await _journalService.GetByPatientAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Journal_GetSharedByPatientAsync_ValidPatient_ReturnsSharedOnly()
    {
        var patientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pUser = new User { Id = userId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientId, UserId = userId, User = pUser };

        var journals = new List<EmotionJournal>
        {
            new() { Id = Guid.NewGuid(), PatientId = patientId, Title = "Shared Journal", Content = "Details", IsShared = true, Patient = patient },
            new() { Id = Guid.NewGuid(), PatientId = patientId, Title = "Private Journal", Content = "Secret", IsShared = false, Patient = patient }
        };

        _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(pUser);
        _mockJournalRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(journals);

        var result = await _journalService.GetSharedByPatientAsync(patientId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        Assert.Equal("Shared Journal", result.Data![0].Title);
    }

    [Fact]
    public async Task Journal_CreateAsync_ValidData_CreatesJournal()
    {
        var userId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var pUser = new User { Id = userId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientId, UserId = userId, User = pUser };

        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(pUser);

        var dto = new CreateJournalDto
        {
            Title = "Morning reflection",
            Content = "Feeling hopeful and focused.",
            MoodScale = 4,
            StressScale = 2,
            IsShared = true
        };

        var result = await _journalService.CreateAsync(dto, userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Morning reflection", result.Data!.Title);
        Assert.Equal(4, result.Data.MoodScale);
        _mockJournalRepo.Verify(r => r.AddAsync(It.IsAny<EmotionJournal>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Journal_CreateAsync_InvalidScale_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = userId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });

        var dto = new CreateJournalDto { Title = "Invalid", MoodScale = 10, StressScale = 2 };
        var result = await _journalService.CreateAsync(dto, userId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Journal_DeleteAsync_Existing_SetsIsDeleted()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new PatientProfile { Id = patientId, UserId = userId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var journal = new EmotionJournal { Id = id, PatientId = patientId, IsDeleted = false, Title = "J", Patient = patient };

        _mockJournalRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });

        var result = await _journalService.DeleteAsync(id, userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(journal.IsDeleted);
        _mockJournalRepo.Verify(r => r.Update(journal), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
