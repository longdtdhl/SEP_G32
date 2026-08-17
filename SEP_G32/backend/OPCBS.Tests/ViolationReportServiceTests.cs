using Moq;
using OPCBS.Application.DTOs.Violations;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;
using Xunit;

namespace OPCBS.Tests;

public class ViolationReportServiceTests
{
    private readonly Mock<IRepository<ViolationReport>> _mockReportRepo = new();
    private readonly Mock<IRepository<ViolationReportEvidence>> _mockEvidenceRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<IRepository<Role>> _mockRoleRepo = new();
    private readonly Mock<IRepository<Appointment>> _mockAppointmentRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _mockDoctorRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _mockPatientRepo = new();
    private readonly Mock<IRepository<TreatmentCase>> _mockCaseRepo = new();
    private readonly Mock<INotificationService> _mockNotifications = new();
    private readonly Mock<IEmailService> _mockEmail = new();
    private readonly Mock<IFileStorageService> _mockFileStorage = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly ViolationReportService _service;

    public ViolationReportServiceTests()
    {
        _service = new ViolationReportService(
            _mockReportRepo.Object,
            _mockEvidenceRepo.Object,
            _mockUserRepo.Object,
            _mockRoleRepo.Object,
            _mockAppointmentRepo.Object,
            _mockDoctorRepo.Object,
            _mockPatientRepo.Object,
            _mockCaseRepo.Object,
            _mockNotifications.Object,
            _mockEmail.Object,
            _mockFileStorage.Object,
            _mockUow.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidReport_CreatesReportAndNotifiesSupport()
    {
        var reporterId = Guid.NewGuid();
        var reportedId = Guid.NewGuid();
        var patientRoleId = Guid.NewGuid();
        var doctorRoleId = Guid.NewGuid();

        var pRole = new Role { Id = patientRoleId, Name = RoleConstants.Patient };
        var dRole = new Role { Id = doctorRoleId, Name = RoleConstants.Doctor };
        var reporter = new User { Id = reporterId, RoleId = patientRoleId, FullName = "Patient Alice", Email = "a@test.com", PasswordHash = "hash", PhoneNumber = "123", Role = pRole };
        var reported = new User { Id = reportedId, RoleId = doctorRoleId, FullName = "Doctor Bob", Email = "b@test.com", PasswordHash = "hash", PhoneNumber = "456", Role = dRole };
        var roles = new List<Role> { pRole, dRole };

        var patientProfile = new PatientProfile { Id = Guid.NewGuid(), UserId = reporterId, User = reporter };
        var doctorProfile = new DoctorProfile { Id = Guid.NewGuid(), UserId = reportedId, User = reported };
        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-1",
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            Doctor = doctorProfile,
            AppointmentSlot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doctorProfile }
        };

        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { reporter, reported });
        _mockRoleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctorProfile });
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });
        _mockCaseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());
        _mockReportRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReport>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new CreateViolationReportDto
        {
            ReportedUserId = reportedId,
            ReasonCategory = ViolationReason.ProfessionalConduct,
            ReasonDetail = "Unprofessional language during consultation session."
        };

        var result = await _service.CreateAsync(reporterId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        _mockReportRepo.Verify(r => r.AddAsync(It.IsAny<ViolationReport>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SelfReporting_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateViolationReportDto { ReportedUserId = userId, ReasonDetail = "Testing self report reason" };

        var result = await _service.CreateAsync(userId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_MissingOrShortReason_ReturnsError()
    {
        var reporterId = Guid.NewGuid();
        var reportedId = Guid.NewGuid();
        var dto = new CreateViolationReportDto { ReportedUserId = reportedId, ReasonDetail = "Too short" };

        var result = await _service.CreateAsync(reporterId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_NonExistentUsers_ReturnsError()
    {
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var dto = new CreateViolationReportDto { ReportedUserId = Guid.NewGuid(), ReasonDetail = "Valid detail length over 10 chars" };
        var result = await _service.CreateAsync(Guid.NewGuid(), dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_NoCareRelationship_ReturnsError()
    {
        var reporterId = Guid.NewGuid();
        var reportedId = Guid.NewGuid();
        var pRoleId = Guid.NewGuid();
        var dRoleId = Guid.NewGuid();

        var pRole = new Role { Id = pRoleId, Name = RoleConstants.Patient };
        var dRole = new Role { Id = dRoleId, Name = RoleConstants.Doctor };
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>
        {
            new() { Id = reporterId, RoleId = pRoleId, Email = "a@test.com", PasswordHash = "h", FullName = "A", PhoneNumber = "1", Role = pRole },
            new() { Id = reportedId, RoleId = dRoleId, Email = "b@test.com", PasswordHash = "h", FullName = "B", PhoneNumber = "2", Role = dRole }
        });
        _mockRoleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role> { pRole, dRole });
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _mockCaseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());

        var dto = new CreateViolationReportDto
        {
            ReportedUserId = reportedId,
            ReasonDetail = "Valid reason detail over 10 chars long."
        };

        var result = await _service.CreateAsync(reporterId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_DuplicateOpenReport_ReturnsError()
    {
        var reporterId = Guid.NewGuid();
        var reportedId = Guid.NewGuid();
        var pRoleId = Guid.NewGuid();
        var dRoleId = Guid.NewGuid();

        var pRole = new Role { Id = pRoleId, Name = RoleConstants.Patient };
        var dRole = new Role { Id = dRoleId, Name = RoleConstants.Doctor };
        var reporterUser = new User { Id = reporterId, RoleId = pRoleId, Email = "a@test.com", PasswordHash = "h", FullName = "A", PhoneNumber = "1", Role = pRole };
        var doctorUser = new User { Id = reportedId, RoleId = dRoleId, Email = "b@test.com", PasswordHash = "h", FullName = "B", PhoneNumber = "2", Role = dRole };

        var pProfile = new PatientProfile { Id = Guid.NewGuid(), UserId = reporterId, User = reporterUser };
        var dProfile = new DoctorProfile { Id = Guid.NewGuid(), UserId = reportedId, User = doctorUser };

        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { reporterUser, doctorUser });
        _mockRoleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role> { pRole, dRole });
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { pProfile });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { dProfile });
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>
        {
            new() { Id = Guid.NewGuid(), BookingCode = "BK-2", PatientId = pProfile.Id, DoctorId = dProfile.Id, Doctor = dProfile, AppointmentSlot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = dProfile } }
        });
        _mockCaseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());

        var existing = new ViolationReport
        {
            ReporterUserId = reporterId,
            ReportedUserId = reportedId,
            ReasonCategory = ViolationReason.ProfessionalConduct,
            ReasonDetail = "Existing report",
            Status = ViolationReportStatus.Submitted
        };
        _mockReportRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReport> { existing });

        var dto = new CreateViolationReportDto
        {
            ReportedUserId = reportedId,
            ReasonCategory = ViolationReason.ProfessionalConduct,
            ReasonDetail = "Duplicate report reason detail."
        };

        var result = await _service.CreateAsync(reporterId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetMineAsync_ReturnsOnlyUserReports()
    {
        var userId = Guid.NewGuid();
        var reports = new List<ViolationReport>
        {
            new() { Id = Guid.NewGuid(), ReporterUserId = userId, Status = ViolationReportStatus.Submitted, ReasonDetail = "Mine" },
            new() { Id = Guid.NewGuid(), ReporterUserId = Guid.NewGuid(), Status = ViolationReportStatus.Submitted, ReasonDetail = "Other" }
        };
        _mockReportRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(reports);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var result = await _service.GetMineAsync(userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetForCustomerSupportAsync_ReturnsAllActiveReports()
    {
        var reports = new List<ViolationReport>
        {
            new() { Id = Guid.NewGuid(), Status = ViolationReportStatus.Submitted, ReasonDetail = "R1" },
            new() { Id = Guid.NewGuid(), Status = ViolationReportStatus.UnderCustomerSupportReview, ReasonDetail = "R2" }
        };
        _mockReportRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(reports);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var result = await _service.GetForCustomerSupportAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task StartReviewAsync_ValidReport_UpdatesToUnderReview()
    {
        var id = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();
        var report = new ViolationReport { Id = id, Status = ViolationReportStatus.Submitted, ReasonDetail = "Detail" };

        _mockReportRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new ReviewViolationReportDto { Note = "Reviewing case details" };
        var result = await _service.StartReviewAsync(id, supportUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ViolationReportStatus.UnderCustomerSupportReview, report.Status);
        _mockReportRepo.Verify(r => r.Update(report), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IssueWarningAsync_ValidReport_SetsWarningIssued()
    {
        var id = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();
        var report = new ViolationReport { Id = id, Status = ViolationReportStatus.UnderCustomerSupportReview, ReasonDetail = "Detail" };

        _mockReportRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new ReviewViolationReportDto { Note = "First warning issued for tardiness." };
        var result = await _service.IssueWarningAsync(id, supportUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ViolationReportStatus.WarningIssued, report.Status);
        _mockReportRepo.Verify(r => r.Update(report), Times.Once);
    }

    [Fact]
    public async Task EscalateAsync_ValidReport_EscalatesToAdmin()
    {
        var id = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();
        var report = new ViolationReport { Id = id, Status = ViolationReportStatus.UnderCustomerSupportReview, ReasonDetail = "Detail" };

        _mockReportRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new ReviewViolationReportDto { Note = "Escalating severe fraud attempt." };
        var result = await _service.EscalateAsync(id, supportUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ViolationReportStatus.EscalatedToAdmin, report.Status);
        _mockReportRepo.Verify(r => r.Update(report), Times.Once);
    }

    [Fact]
    public async Task DisableAccountAsync_ValidReport_DisablesReportedUser()
    {
        var reportId = Guid.NewGuid();
        var reportedId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var report = new ViolationReport { Id = reportId, ReportedUserId = reportedId, Status = ViolationReportStatus.EscalatedToAdmin, ReasonDetail = "Detail" };
        var reportedUser = new User { Id = reportedId, Status = UserStatus.Active, Email = "u@test.com", PasswordHash = "h", FullName = "U", PhoneNumber = "1", Role = new Role { Name = "Patient" } };

        _mockReportRepo.Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockUserRepo.Setup(r => r.GetByIdAsync(reportedId, It.IsAny<CancellationToken>())).ReturnsAsync(reportedUser);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { reportedUser });
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new ReviewViolationReportDto { Note = "Account permanently disabled for harassment." };
        var result = await _service.DisableAccountAsync(reportId, adminId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(UserStatus.Locked, reportedUser.Status);
        Assert.Equal(ViolationReportStatus.AccountDisabled, report.Status);
        _mockUserRepo.Verify(r => r.Update(reportedUser), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DismissAsync_ValidReport_DismissesReport()
    {
        var id = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var report = new ViolationReport { Id = id, Status = ViolationReportStatus.UnderCustomerSupportReview, ReasonDetail = "Detail" };

        _mockReportRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());

        var dto = new ReviewViolationReportDto { Note = "Insufficient evidence provided." };
        var result = await _service.DismissAsync(id, adminId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ViolationReportStatus.Dismissed, report.Status);
        _mockReportRepo.Verify(r => r.Update(report), Times.Once);
    }

    [Fact]
    public async Task UploadEvidenceAsync_ValidFiles_AddsEvidence()
    {
        var reportId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var report = new ViolationReport { Id = reportId, ReporterUserId = reporterId, Status = ViolationReportStatus.Submitted, ReasonDetail = "Detail" };

        _mockReportRepo.Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _mockEvidenceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ViolationReportEvidence>());
        _mockFileStorage.Setup(f => f.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileUploadResult { Url = "http://storage/evidence.png", PublicId = "evidence.png" });

        var upload = new ViolationEvidenceUpload { FileName = "evidence.png", ContentType = "image/png", Content = new MemoryStream(new byte[] { 1, 2, 3 }), FileSizeBytes = 3 };
        var result = await _service.UploadEvidenceAsync(reportId, reporterId, new[] { upload }, CancellationToken.None);

        Assert.True(result.Success);
        _mockEvidenceRepo.Verify(r => r.AddAsync(It.IsAny<ViolationReportEvidence>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadEvidenceAsync_TooManyFiles_ReturnsError()
    {
        var uploads = Enumerable.Range(1, 6).Select(i => new ViolationEvidenceUpload { FileName = $"file{i}.png", ContentType = "image/png", Content = Stream.Null, FileSizeBytes = 0 }).ToList();

        var result = await _service.UploadEvidenceAsync(Guid.NewGuid(), Guid.NewGuid(), uploads, CancellationToken.None);

        Assert.False(result.Success);
    }
}
