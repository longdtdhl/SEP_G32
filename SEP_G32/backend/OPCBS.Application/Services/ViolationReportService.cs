using OPCBS.Application.DTOs.Violations;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

/// <summary>Coordinates report intake, support warnings, escalation, and administrative account disabling.</summary>
public class ViolationReportService : IViolationReportService
{
    private readonly IRepository<ViolationReport> _reportRepo;
    private readonly IRepository<ViolationReportEvidence> _evidenceRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<TreatmentCase> _caseRepo;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _uow;

    public ViolationReportService(
        IRepository<ViolationReport> reportRepo,
        IRepository<ViolationReportEvidence> evidenceRepo,
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<TreatmentCase> caseRepo,
        INotificationService notifications,
        IEmailService email,
        IFileStorageService fileStorage,
        IUnitOfWork uow)
    {
        _reportRepo = reportRepo;
        _evidenceRepo = evidenceRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _appointmentRepo = appointmentRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _caseRepo = caseRepo;
        _notifications = notifications;
        _email = email;
        _fileStorage = fileStorage;
        _uow = uow;
    }

    public async Task<ApiResponse<ViolationReportDto>> CreateAsync(Guid reporterUserId, CreateViolationReportDto dto, CancellationToken ct = default)
    {
        if (dto.ReportedUserId == Guid.Empty || dto.ReportedUserId == reporterUserId)
            return ApiResponse<ViolationReportDto>.ErrorResponse("A report must identify another user.");
        if (string.IsNullOrWhiteSpace(dto.ReasonDetail) || dto.ReasonDetail.Trim().Length < 10)
            return ApiResponse<ViolationReportDto>.ErrorResponse("Please provide a clear reason of at least 10 characters.");

        var users = await _userRepo.GetAllAsync(ct);
        var roles = await _roleRepo.GetAllAsync(ct);
        var reporter = users.FirstOrDefault(u => u.Id == reporterUserId && !u.IsDeleted);
        var reported = users.FirstOrDefault(u => u.Id == dto.ReportedUserId && !u.IsDeleted);
        if (reporter == null || reported == null)
            return ApiResponse<ViolationReportDto>.ErrorResponse("Reporter or reported user was not found.");
        if (!IsPatientOrDoctor(reporter, roles) || !IsPatientOrDoctor(reported, roles))
            return ApiResponse<ViolationReportDto>.ErrorResponse("Only patient-doctor reports are supported.");
        if (!await ShareCareRelationshipAsync(reporterUserId, dto.ReportedUserId, ct))
            return ApiResponse<ViolationReportDto>.ErrorResponse("You can only report a patient or doctor with whom you share an appointment or treatment case.");

        var reports = await _reportRepo.GetAllAsync(ct);
        var duplicate = reports.Any(r => !r.IsDeleted && r.ReporterUserId == reporterUserId &&
            r.ReportedUserId == dto.ReportedUserId && r.ReasonCategory == dto.ReasonCategory &&
            r.Status is not ViolationReportStatus.Dismissed and not ViolationReportStatus.Resolved and not ViolationReportStatus.AccountDisabled);
        if (duplicate)
            return ApiResponse<ViolationReportDto>.ErrorResponse("An open report for this reason already exists.");

        var report = new ViolationReport
        {
            ReporterUserId = reporterUserId,
            ReportedUserId = dto.ReportedUserId,
            Source = IsPatient(reporter, roles) ? ViolationReportSource.Patient : ViolationReportSource.Doctor,
            ReasonCategory = dto.ReasonCategory,
            ReasonDetail = dto.ReasonDetail.Trim(),
            RelatedAppointmentId = dto.RelatedAppointmentId,
            RelatedTreatmentCaseId = dto.RelatedTreatmentCaseId,
            CreatedBy = reporterUserId
        };
        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);
        await NotifyRoleAsync(RoleConstants.CustomerSupport, "New conduct report", $"A {report.Source.ToString().ToLowerInvariant()} submitted a report that needs review.", report.Id, ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), "Report submitted to Customer Support.");
    }

    public async Task<ApiResponse<List<ViolationReportEvidenceDto>>> UploadEvidenceAsync(Guid reportId, Guid reporterUserId, IReadOnlyCollection<ViolationEvidenceUpload> files, CancellationToken ct = default)
    {
        if (files.Count is < 1 or > 5)
            return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("Upload from 1 to 5 evidence files at a time.");

        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        if (report == null || report.IsDeleted || report.ReporterUserId != reporterUserId)
            return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("Report not found or you are not allowed to add evidence.");
        if (report.Status != ViolationReportStatus.Submitted)
            return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("Evidence cannot be changed after Customer Support begins reviewing the report.");

        var existing = (await _evidenceRepo.GetAllAsync(ct)).Where(e => !e.IsDeleted && e.ViolationReportId == reportId).ToList();
        if (existing.Count + files.Count > 5)
            return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("A report can contain at most 5 evidence files.");

        foreach (var file in files)
        {
            if (file.FileSizeBytes <= 0 || file.FileSizeBytes > 10 * 1024 * 1024)
                return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("Each evidence file must be between 1 byte and 10 MB.");
            if (!IsAllowedEvidenceFile(file.FileName, file.ContentType))
                return ApiResponse<List<ViolationReportEvidenceDto>>.ErrorResponse("Only JPEG, PNG, WebP images and PDF documents are accepted as evidence.");
        }

        var uploadedPublicIds = new List<string>();
        var added = new List<ViolationReportEvidence>();
        try
        {
            foreach (var file in files)
            {
                var upload = await _fileStorage.UploadFileAsync(file.Content, file.FileName, $"violation-reports/{reportId}", ct);
                uploadedPublicIds.Add(upload.PublicId);
                var evidence = new ViolationReportEvidence
                {
                    ViolationReportId = reportId,
                    FileUrl = upload.Url,
                    PublicId = upload.PublicId,
                    FileName = Path.GetFileName(file.FileName),
                    ContentType = file.ContentType.ToLowerInvariant(),
                    FileSizeBytes = file.FileSizeBytes,
                    UploadedByUserId = reporterUserId,
                    CreatedBy = reporterUserId,
                    ViolationReport = report
                };
                await _evidenceRepo.AddAsync(evidence, ct);
                added.Add(evidence);
            }
            await _uow.SaveChangesAsync(ct);
            return ApiResponse<List<ViolationReportEvidenceDto>>.SuccessResponse(added.Select(ToEvidenceDto).ToList(), "Evidence uploaded successfully.");
        }
        catch
        {
            foreach (var publicId in uploadedPublicIds)
            {
                try { await _fileStorage.DeleteAsync(publicId, ct); } catch { }
            }
            throw;
        }
    }

    public async Task<ApiResponse<List<ViolationReportDto>>> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var reports = await _reportRepo.GetAllAsync(ct);
        return ApiResponse<List<ViolationReportDto>>.SuccessResponse(await ToDtosAsync(reports.Where(r => !r.IsDeleted && r.ReporterUserId == userId), ct));
    }

    public async Task<ApiResponse<List<ViolationReportDto>>> GetForCustomerSupportAsync(CancellationToken ct = default)
    {
        var reports = await _reportRepo.GetAllAsync(ct);
        return ApiResponse<List<ViolationReportDto>>.SuccessResponse(await ToDtosAsync(reports.Where(r => !r.IsDeleted && r.Status is not ViolationReportStatus.Dismissed and not ViolationReportStatus.Resolved), ct));
    }

    public async Task<ApiResponse<List<ViolationReportDto>>> GetForAdminAsync(CancellationToken ct = default)
    {
        var reports = await _reportRepo.GetAllAsync(ct);
        return ApiResponse<List<ViolationReportDto>>.SuccessResponse(await ToDtosAsync(reports.Where(r => !r.IsDeleted && r.Status == ViolationReportStatus.EscalatedToAdmin), ct));
    }

    public async Task<ApiResponse<ViolationReportDto>> StartReviewAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default)
    {
        var report = await GetOpenReportAsync(reportId, ct);
        if (report == null) return ApiResponse<ViolationReportDto>.ErrorResponse("Open report not found.");
        report.Status = ViolationReportStatus.UnderCustomerSupportReview;
        report.CustomerSupportUserId = supportUserId;
        report.CustomerSupportNote = dto.Note?.Trim();
        report.UpdatedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), "Report is under Customer Support review.");
    }

    public async Task<ApiResponse<ViolationReportDto>> IssueWarningAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default)
    {
        var report = await GetOpenReportAsync(reportId, ct);
        if (report == null) return ApiResponse<ViolationReportDto>.ErrorResponse("Open report not found.");
        var reports = await _reportRepo.GetAllAsync(ct);
        var priorWarnings = reports.Count(r => !r.IsDeleted && r.ReportedUserId == report.ReportedUserId && r.WarningIssuedAt.HasValue);
        report.WarningNumber = priorWarnings + 1;
        report.WarningIssuedAt = DateTime.UtcNow;
        report.CustomerSupportUserId = supportUserId;
        report.CustomerSupportNote = dto.Note?.Trim();
        report.Status = report.WarningNumber >= 3 ? ViolationReportStatus.EscalatedToAdmin : ViolationReportStatus.WarningIssued;
        report.EscalatedAt = report.Status == ViolationReportStatus.EscalatedToAdmin ? DateTime.UtcNow : null;
        report.UpdatedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);

        var subject = report.Status == ViolationReportStatus.EscalatedToAdmin ? "Account review escalated" : "Policy warning from Customer Support";
        var message = report.Status == ViolationReportStatus.EscalatedToAdmin
            ? "You have received a third policy warning. Your account has been escalated to System Admin for review."
            : $"Customer Support issued policy warning {report.WarningNumber}/3. Please review platform policies to avoid account suspension.";
        await NotifyUserAndEmailAsync(report.ReportedUserId, subject, message, report.Id, ct);
        if (report.Status == ViolationReportStatus.EscalatedToAdmin)
            await NotifyRoleAsync(RoleConstants.SystemAdmin, "Account review required", "A user reached three policy warnings and requires an account decision.", report.Id, ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), report.Status == ViolationReportStatus.EscalatedToAdmin ? "Third warning issued and report escalated to System Admin." : "Warning issued.");
    }

    public async Task<ApiResponse<ViolationReportDto>> EscalateAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default)
    {
        var report = await GetOpenReportAsync(reportId, ct);
        if (report == null) return ApiResponse<ViolationReportDto>.ErrorResponse("Open report not found.");
        report.Status = ViolationReportStatus.EscalatedToAdmin;
        report.CustomerSupportUserId = supportUserId;
        report.CustomerSupportNote = dto.Note?.Trim();
        report.EscalatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);
        await NotifyRoleAsync(RoleConstants.SystemAdmin, "Account review required", "Customer Support escalated a conduct report for account action.", report.Id, ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), "Report escalated to System Admin.");
    }

    public async Task<ApiResponse<ViolationReportDto>> DisableAccountAsync(Guid reportId, Guid adminUserId, ReviewViolationReportDto dto, CancellationToken ct = default)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        if (report == null || report.IsDeleted || report.Status != ViolationReportStatus.EscalatedToAdmin)
            return ApiResponse<ViolationReportDto>.ErrorResponse("An escalated report is required to disable an account.");
        if (report.ReportedUserId == adminUserId)
            return ApiResponse<ViolationReportDto>.ErrorResponse("Administrators cannot disable their own account.");
        var user = await _userRepo.GetByIdAsync(report.ReportedUserId, ct);
        if (user == null || user.IsDeleted) return ApiResponse<ViolationReportDto>.ErrorResponse("Reported user not found.");
        var roles = await _roleRepo.GetAllAsync(ct);
        if (roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name == RoleConstants.SystemAdmin)
            return ApiResponse<ViolationReportDto>.ErrorResponse("System Administrator accounts cannot be disabled here.");

        user.Status = UserStatus.Locked;
        report.Status = ViolationReportStatus.AccountDisabled;
        report.AdminUserId = adminUserId;
        report.AdminNote = dto.Note?.Trim();
        report.ResolvedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);
        await NotifyUserAndEmailAsync(user.Id, "Account disabled", "Your OPCBS account has been disabled after a policy review. Contact Customer Support by email to request reactivation.", report.Id, ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), "Account disabled. Reactivation remains a manual Customer Support process.");
    }

    public async Task<ApiResponse<ViolationReportDto>> DismissAsync(Guid reportId, Guid adminUserId, ReviewViolationReportDto dto, CancellationToken ct = default)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        if (report == null || report.IsDeleted) return ApiResponse<ViolationReportDto>.ErrorResponse("Report not found.");
        report.Status = ViolationReportStatus.Dismissed;
        report.AdminUserId = adminUserId;
        report.AdminNote = dto.Note?.Trim();
        report.ResolvedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<ViolationReportDto>.SuccessResponse(await ToDtoAsync(report, ct), "Report dismissed.");
    }

    public async Task<ApiResponse> CreateSystemNoShowReportAsync(Guid patientUserId, Guid doctorUserId, Guid appointmentId, Guid? treatmentCaseId, CancellationToken ct = default)
    {
        var reports = await _reportRepo.GetAllAsync(ct);
        if (reports.Any(r => !r.IsDeleted && r.ReportedUserId == patientUserId && r.ReasonCategory == ViolationReason.RepeatedNoShow && r.Status is not ViolationReportStatus.Dismissed and not ViolationReportStatus.Resolved))
            return ApiResponse.SuccessResponse("Repeated no-show report already exists.");
        var report = new ViolationReport
        {
            ReporterUserId = doctorUserId,
            ReportedUserId = patientUserId,
            Source = ViolationReportSource.System,
            ReasonCategory = ViolationReason.RepeatedNoShow,
            ReasonDetail = "System detected three patient no-shows within the last 90 days.",
            RelatedAppointmentId = appointmentId,
            RelatedTreatmentCaseId = treatmentCaseId,
            CreatedBy = doctorUserId
        };
        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);
        await NotifyRoleAsync(RoleConstants.CustomerSupport, "Repeated no-show review", "A patient reached three no-shows within 90 days and requires policy review.", report.Id, ct);
        return ApiResponse.SuccessResponse("Repeated no-show report created for Customer Support.");
    }

    public async Task<ApiResponse> CreateSystemCompletionDisputeReportAsync(Guid? reporterUserId, Guid doctorUserId, Guid appointmentId, Guid? treatmentCaseId, string reason, CancellationToken ct = default)
    {
        var reports = await _reportRepo.GetAllAsync(ct);
        if (reports.Any(r => !r.IsDeleted && r.RelatedAppointmentId == appointmentId &&
                             r.ReasonCategory == ViolationReason.AppointmentCompletionDispute &&
                             r.Status is not ViolationReportStatus.Dismissed and not ViolationReportStatus.Resolved))
            return ApiResponse.SuccessResponse("Completion dispute report already exists.");

        var report = new ViolationReport
        {
            ReporterUserId = reporterUserId,
            ReportedUserId = doctorUserId,
            Source = ViolationReportSource.System,
            ReasonCategory = ViolationReason.AppointmentCompletionDispute,
            ReasonDetail = reason,
            RelatedAppointmentId = appointmentId,
            RelatedTreatmentCaseId = treatmentCaseId,
            CreatedBy = reporterUserId
        };
        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);
        await NotifyRoleAsync(RoleConstants.CustomerSupport, "Appointment completion dispute",
            "A patient or guest disputed an appointment completion request and requires review.", report.Id, ct);
        return ApiResponse.SuccessResponse("Completion dispute report created for Customer Support.");
    }

    private async Task<ViolationReport?> GetOpenReportAsync(Guid reportId, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        return report == null || report.IsDeleted || report.Status is ViolationReportStatus.Dismissed or ViolationReportStatus.Resolved or ViolationReportStatus.AccountDisabled ? null : report;
    }

    private async Task<bool> ShareCareRelationshipAsync(Guid firstUserId, Guid secondUserId, CancellationToken ct)
    {
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var patients = await _patientRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == firstUserId) ?? doctors.FirstOrDefault(d => d.UserId == secondUserId);
        var patient = patients.FirstOrDefault(p => p.UserId == firstUserId) ?? patients.FirstOrDefault(p => p.UserId == secondUserId);
        if (doctor == null || patient == null) return false;
        var appointments = await _appointmentRepo.GetAllAsync(ct);
        if (appointments.Any(a => !a.IsDeleted && a.DoctorId == doctor.Id && a.PatientId == patient.Id)) return true;
        var cases = await _caseRepo.GetAllAsync(ct);
        return cases.Any(c => !c.IsDeleted && c.DoctorId == doctor.UserId && c.PatientId == patient.UserId);
    }

    private async Task<List<ViolationReportDto>> ToDtosAsync(IEnumerable<ViolationReport> reports, CancellationToken ct)
    {
        var users = (await _userRepo.GetAllAsync(ct)).Where(u => !u.IsDeleted).ToDictionary(u => u.Id, u => u.FullName);
        var evidenceByReport = (await _evidenceRepo.GetAllAsync(ct)).Where(e => !e.IsDeleted)
            .GroupBy(e => e.ViolationReportId).ToDictionary(g => g.Key, g => g.Select(ToEvidenceDto).ToList());
        return reports.OrderByDescending(r => r.CreatedAt).Select(r => ToDto(r, users, evidenceByReport.GetValueOrDefault(r.Id) ?? new())).ToList();
    }

    private async Task<ViolationReportDto> ToDtoAsync(ViolationReport report, CancellationToken ct)
    {
        var users = (await _userRepo.GetAllAsync(ct)).Where(u => !u.IsDeleted).ToDictionary(u => u.Id, u => u.FullName);
        var evidence = (await _evidenceRepo.GetAllAsync(ct)).Where(e => !e.IsDeleted && e.ViolationReportId == report.Id).Select(ToEvidenceDto).ToList();
        return ToDto(report, users, evidence);
    }

    private static ViolationReportDto ToDto(ViolationReport report, IReadOnlyDictionary<Guid, string> users, List<ViolationReportEvidenceDto> evidenceFiles) => new()
    {
        Id = report.Id, ReporterUserId = report.ReporterUserId,
        ReporterName = report.ReporterUserId.HasValue ? users.GetValueOrDefault(report.ReporterUserId.Value) : "System",
        ReportedUserId = report.ReportedUserId, ReportedUserName = users.GetValueOrDefault(report.ReportedUserId),
        Source = report.Source, ReasonCategory = report.ReasonCategory, ReasonDetail = report.ReasonDetail,
        RelatedAppointmentId = report.RelatedAppointmentId, RelatedTreatmentCaseId = report.RelatedTreatmentCaseId,
        Status = report.Status, CustomerSupportNote = report.CustomerSupportNote,
        WarningIssuedAt = report.WarningIssuedAt, WarningNumber = report.WarningNumber,
        EscalatedAt = report.EscalatedAt, AdminNote = report.AdminNote, CreatedAt = report.CreatedAt, ResolvedAt = report.ResolvedAt,
        EvidenceFiles = evidenceFiles
    };

    private static ViolationReportEvidenceDto ToEvidenceDto(ViolationReportEvidence evidence) => new()
    {
        Id = evidence.Id,
        FileUrl = evidence.FileUrl,
        FileName = evidence.FileName,
        ContentType = evidence.ContentType,
        FileSizeBytes = evidence.FileSizeBytes,
        CreatedAt = evidence.CreatedAt
    };

    private static bool IsAllowedEvidenceFile(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return (extension, contentType.ToLowerInvariant()) switch
        {
            (".jpg" or ".jpeg", "image/jpeg") => true,
            (".png", "image/png") => true,
            (".webp", "image/webp") => true,
            (".pdf", "application/pdf") => true,
            _ => false
        };
    }

    private static bool IsPatientOrDoctor(User user, IEnumerable<Role> roles) => IsPatient(user, roles) || roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name == RoleConstants.Doctor;
    private static bool IsPatient(User user, IEnumerable<Role> roles) => roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name == RoleConstants.Patient;

    private async Task NotifyRoleAsync(string roleName, string title, string message, Guid reportId, CancellationToken ct)
    {
        var users = await _userRepo.GetAllAsync(ct);
        var roles = await _roleRepo.GetAllAsync(ct);
        var roleId = roles.FirstOrDefault(r => r.Name == roleName)?.Id;
        if (!roleId.HasValue) return;
        foreach (var user in users.Where(u => !u.IsDeleted && u.Status == UserStatus.Active && u.RoleId == roleId.Value))
            await _notifications.CreateNotificationAsync(user.Id, title, message, NotificationType.System, reportId, "ViolationReport", ct);
    }

    private async Task NotifyUserAndEmailAsync(Guid userId, string title, string message, Guid reportId, CancellationToken ct)
    {
        await _notifications.CreateNotificationAsync(userId, title, message, NotificationType.System, reportId, "ViolationReport", ct);
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user?.Email != null)
            await _email.SendEmailAsync(user.Email, $"OPCBS - {title}", $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>", ct);
    }
}
