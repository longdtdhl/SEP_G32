using Microsoft.Extensions.Logging;
using OPCBS.Application.Interfaces;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Application.Services;

public class TreatmentLifecycleCoordinator : ITreatmentLifecycleCoordinator
{
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<TreatmentCase> _caseRepo;
    private readonly IRepository<TreatmentSession> _sessionRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TreatmentLifecycleCoordinator> _logger;

    public TreatmentLifecycleCoordinator(
        IRepository<TreatmentPackage> packageRepo,
        IRepository<TreatmentCase> caseRepo,
        IRepository<TreatmentSession> sessionRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<User> userRepo,
        IRepository<Notification> notificationRepo,
        INotificationService notificationService,
        IUnitOfWork uow,
        ILogger<TreatmentLifecycleCoordinator> logger)
    {
        _packageRepo = packageRepo;
        _caseRepo = caseRepo;
        _sessionRepo = sessionRepo;
        _appointmentRepo = appointmentRepo;
        _slotRepo = slotRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _notificationRepo = notificationRepo;
        _notificationService = notificationService;
        _uow = uow;
        _logger = logger;
    }

    public async Task ReconcilePackageLifecycleAsync(TreatmentPackage package, CancellationToken ct = default)
    {
        if (package == null || package.IsDeleted) return;

        var now = DateTime.UtcNow;

        // 1. Assigned proposal past acceptance deadline
        if (package.Status == TreatmentPackageStatus.Assigned &&
            package.AcceptanceExpiresAt.HasValue &&
            package.AcceptanceExpiresAt.Value <= now)
        {
            _logger.LogInformation("Reconciling expired assigned package {PackageId} (deadline was {Deadline})",
                package.Id, package.AcceptanceExpiresAt.Value);
            await ExpireAssignedPackageAsync(package, ct);
            return;
        }

        // 2. Active/Accepted package past validity expiration date
        if ((package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted) &&
            package.ExpirationDate <= now)
        {
            _logger.LogInformation("Reconciling expired active package {PackageId} (expiration was {ExpDate})",
                package.Id, package.ExpirationDate);
            await ExpireActivePackageAndCaseAsync(package, null, ct);
            return;
        }

        // 3. Reconcile package state if linked TreatmentCase has reached a terminal outcome
        if (package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted)
        {
            var allCases = await _caseRepo.GetAllAsync(ct);
            var linkedCase = allCases.FirstOrDefault(c => c.TreatmentPackageId == package.Id && !c.IsDeleted);
            if (linkedCase != null)
            {
                if (linkedCase.Status == TreatmentCaseStatus.Completed)
                {
                    package.Status = TreatmentPackageStatus.Completed;
                    package.UpdatedAt = now;
                    _packageRepo.Update(package);
                    await _uow.SaveChangesAsync(ct);
                    return;
                }
                if (linkedCase.Status == TreatmentCaseStatus.Cancelled || linkedCase.Status == TreatmentCaseStatus.Terminated)
                {
                    package.Status = TreatmentPackageStatus.Cancelled;
                    package.UpdatedAt = now;
                    _packageRepo.Update(package);
                    await _uow.SaveChangesAsync(ct);
                    return;
                }
                if (linkedCase.Status == TreatmentCaseStatus.Expired)
                {
                    package.Status = TreatmentPackageStatus.Expired;
                    package.UpdatedAt = now;
                    _packageRepo.Update(package);
                    await _uow.SaveChangesAsync(ct);
                    return;
                }
            }
        }
    }

    public async Task ReconcileCaseLifecycleAsync(TreatmentCase treatmentCase, CancellationToken ct = default)
    {
        if (treatmentCase == null || treatmentCase.IsDeleted) return;

        var now = DateTime.UtcNow;

        var package = treatmentCase.TreatmentPackage;
        if (package == null && treatmentCase.TreatmentPackageId != Guid.Empty)
        {
            package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        }

        // 0. If Case is OnHold and HoldEndDate has passed, auto-resume to Active
        if (treatmentCase.Status == TreatmentCaseStatus.OnHold &&
            treatmentCase.HoldEndDate.HasValue &&
            treatmentCase.HoldEndDate.Value <= now)
        {
            _logger.LogInformation("Auto-resuming treatment case {CaseId} because hold period ended at {HoldEndDate}", treatmentCase.Id, treatmentCase.HoldEndDate.Value);
            treatmentCase.Status = TreatmentCaseStatus.Active;
            treatmentCase.UpdatedAt = now;
            _caseRepo.Update(treatmentCase);
            await _uow.SaveChangesAsync(ct);
        }

        // 1. If Case is Active (or OnHold), check expiration against extended validity
        if (treatmentCase.Status == TreatmentCaseStatus.Active || treatmentCase.Status == TreatmentCaseStatus.OnHold)
        {
            // Do not expire a case if it is currently in its approved hold timeframe
            bool isCurrentlyHolding = treatmentCase.Status == TreatmentCaseStatus.OnHold &&
                treatmentCase.HoldEndDate.HasValue && treatmentCase.HoldEndDate.Value > now;

            bool isPackageExpired = !isCurrentlyHolding && package != null &&
                (package.Status == TreatmentPackageStatus.Expired ||
                 ((package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted) && package.ExpirationDate <= now));

            bool isCaseOverdue = !isCurrentlyHolding && treatmentCase.ExpectedEndDate.HasValue && treatmentCase.ExpectedEndDate.Value <= now;

            if (isPackageExpired || isCaseOverdue)
            {
                _logger.LogInformation("Reconciling expired active case {CaseId}", treatmentCase.Id);
                await ExpireActivePackageAndCaseAsync(package, treatmentCase, ct);
                return;
            }

            if (package != null)
            {
                if (package.Status == TreatmentPackageStatus.Completed)
                {
                    await CompleteCaseAndPackageAsync(treatmentCase, treatmentCase.ClosureNote ?? "Treatment program completed.", ct);
                    return;
                }
                if (package.Status == TreatmentPackageStatus.Cancelled)
                {
                    await CancelCaseAndPackageAsync(package, treatmentCase, package.CancellationReason ?? "Treatment package cancelled.", ct);
                    return;
                }
            }
        }
        // 2. If Case is Completed but package is still Active/Accepted -> reconcile package to Completed
        else if (treatmentCase.Status == TreatmentCaseStatus.Completed)
        {
            if (package != null && (package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted))
            {
                package.Status = TreatmentPackageStatus.Completed;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
                await _uow.SaveChangesAsync(ct);
            }
        }
        // 3. If Case is Expired but package is still Active/Accepted -> reconcile package to Expired
        else if (treatmentCase.Status == TreatmentCaseStatus.Expired)
        {
            if (package != null && (package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted))
            {
                package.Status = TreatmentPackageStatus.Expired;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
                await _uow.SaveChangesAsync(ct);
            }
        }
        // 4. If Case is Cancelled/Terminated but package is still Active/Accepted -> reconcile package to Cancelled
        else if (treatmentCase.Status == TreatmentCaseStatus.Cancelled || treatmentCase.Status == TreatmentCaseStatus.Terminated)
        {
            if (package != null && (package.Status == TreatmentPackageStatus.Active || package.Status == TreatmentPackageStatus.Accepted))
            {
                package.Status = TreatmentPackageStatus.Cancelled;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
                await _uow.SaveChangesAsync(ct);
            }
        }
    }

    public async Task ExpireAssignedPackageAsync(TreatmentPackage package, CancellationToken ct = default)
    {
        if (package.Status == TreatmentPackageStatus.Expired) return;

        var now = DateTime.UtcNow;
        package.Status = TreatmentPackageStatus.Expired;
        package.ExpiredAt = now;
        package.UpdatedAt = now;
        package.RejectionReason ??= "Proposal expired due to no patient acceptance within the deadline.";
        _packageRepo.Update(package);
        await _uow.SaveChangesAsync(ct);

        // Notify patient and doctor (idempotent, deduplicated)
        await SendLifecycleNotificationAsync(
            package.DoctorId,
            package.PatientId,
            "Package Proposal Expired",
            $"The proposed treatment package \"{package.Name}\" was not accepted within the deadline and has expired.",
            package.Id,
            "TreatmentPackage",
            ct);
    }

    public async Task ExpireActivePackageAndCaseAsync(TreatmentPackage? package, TreatmentCase? treatmentCase, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (package == null && treatmentCase != null && treatmentCase.TreatmentPackageId != Guid.Empty)
        {
            package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        }

        if (treatmentCase == null && package != null)
        {
            var allCases = await _caseRepo.GetAllAsync(ct);
            treatmentCase = allCases.FirstOrDefault(c =>
                c.TreatmentPackageId == package.Id &&
                !c.IsDeleted &&
                (c.Status == TreatmentCaseStatus.Active || c.Status == TreatmentCaseStatus.OnHold));
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // 1. Expire package
            if (package != null && package.Status != TreatmentPackageStatus.Expired)
            {
                package.Status = TreatmentPackageStatus.Expired;
                package.ExpiredAt = now;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
            }

            // 2. Expire case (preserve actual progress percentage!)
            if (treatmentCase != null && treatmentCase.Status != TreatmentCaseStatus.Expired)
            {
                treatmentCase.Status = TreatmentCaseStatus.Expired;
                treatmentCase.ActualEndDate = now;
                treatmentCase.ClosureNote ??= "Treatment program validity period has expired.";
                treatmentCase.UpdatedAt = now;
                _caseRepo.Update(treatmentCase);

                // Cancel future uncompleted sessions and release slots
                await CancelFutureSessionsAndReleaseSlotsAsync(treatmentCase.Id, "Treatment program validity period has expired.", ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        // Notify doctor and patient
        if (package != null)
        {
            await SendLifecycleNotificationAsync(
                package.DoctorId,
                package.PatientId,
                "Treatment Program Expired",
                $"The treatment program \"{package.Name}\" reached its expiration date. Historical records remain viewable in read-only mode.",
                treatmentCase?.Id ?? package.Id,
                treatmentCase != null ? "TreatmentCase" : "TreatmentPackage",
                ct);
        }
    }

    public async Task CompleteCaseAndPackageAsync(TreatmentCase treatmentCase, string? closureNote, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var package = treatmentCase.TreatmentPackage;
        if (package == null && treatmentCase.TreatmentPackageId != Guid.Empty)
        {
            package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            treatmentCase.Status = TreatmentCaseStatus.Completed;
            treatmentCase.OverallProgressPercent = 100;
            treatmentCase.ActualEndDate ??= now;
            treatmentCase.ClosureNote = closureNote ?? "Treatment program completed successfully.";
            treatmentCase.UpdatedAt = now;
            _caseRepo.Update(treatmentCase);

            if (package != null && package.Status != TreatmentPackageStatus.Completed)
            {
                package.Status = TreatmentPackageStatus.Completed;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        await SendLifecycleNotificationAsync(
            treatmentCase.DoctorId,
            treatmentCase.PatientId,
            "🎉 Treatment Journey Completed",
            $"Congratulations! Treatment case \"{treatmentCase.CaseName}\" has been marked as completed.",
            treatmentCase.Id,
            "TreatmentCase",
            ct);
    }

    public async Task CancelCaseAndPackageAsync(TreatmentPackage? package, TreatmentCase? treatmentCase, string reason, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (treatmentCase == null && package != null && package.Id != Guid.Empty)
        {
            var allCases = await _caseRepo.GetAllAsync(ct);
            treatmentCase = allCases.FirstOrDefault(c =>
                c.TreatmentPackageId == package.Id &&
                !c.IsDeleted &&
                (c.Status == TreatmentCaseStatus.Active || c.Status == TreatmentCaseStatus.OnHold));
        }

        if (package == null && treatmentCase != null && treatmentCase.TreatmentPackageId != Guid.Empty)
        {
            package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (package != null)
            {
                package.Status = TreatmentPackageStatus.Cancelled;
                package.CancellationReason = reason;
                package.RejectionReason = reason;
                package.UpdatedAt = now;
                _packageRepo.Update(package);
            }

            if (treatmentCase != null && treatmentCase.Status != TreatmentCaseStatus.Cancelled)
            {
                treatmentCase.Status = TreatmentCaseStatus.Cancelled;
                treatmentCase.ActualEndDate = now;
                treatmentCase.ClosureNote = $"Cancelled: {reason}";
                treatmentCase.UpdatedAt = now;
                _caseRepo.Update(treatmentCase);

                await CancelFutureSessionsAndReleaseSlotsAsync(treatmentCase.Id, $"Treatment cancelled: {reason}", ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        if (package != null)
        {
            await SendLifecycleNotificationAsync(
                package.DoctorId,
                package.PatientId,
                "Treatment Program Cancelled",
                $"The treatment program \"{package.Name}\" was cancelled. Reason: {reason}",
                treatmentCase?.Id ?? package.Id,
                treatmentCase != null ? "TreatmentCase" : "TreatmentPackage",
                ct);
        }
    }

    public async Task<(bool CanManage, string? ErrorMessage)> ValidateCaseManageableAsync(TreatmentCase treatmentCase, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        if (treatmentCase == null || treatmentCase.IsDeleted)
        {
            return (false, "Treatment case not found.");
        }

        // Lazy reconcile first
        await ReconcileCaseLifecycleAsync(treatmentCase, ct);

        // If doctorUserId provided, verify ownership
        if (doctorUserId.HasValue && doctorUserId.Value != Guid.Empty)
        {
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId.Value || d.Id == doctorUserId.Value);
            if (doctor != null && treatmentCase.DoctorId != doctor.Id && treatmentCase.DoctorId != doctor.UserId && treatmentCase.DoctorId != Guid.Empty)
            {
                return (false, "You are not authorized to manage this treatment case.");
            }
        }

        if (treatmentCase.Status == TreatmentCaseStatus.OnHold)
        {
            return (false, "This treatment case is currently on hold. Clinical activities, session management, and homework submissions are paused until the hold period ends or treatment is resumed.");
        }

        if (treatmentCase.Status == TreatmentCaseStatus.Cancelled || treatmentCase.Status == TreatmentCaseStatus.Terminated || treatmentCase.Status == TreatmentCaseStatus.Expired)
        {
            return (false, "This treatment program is no longer active. Historical information remains available in read-only mode.");
        }

        var package = treatmentCase.TreatmentPackage;
        if (package == null && treatmentCase.TreatmentPackageId != Guid.Empty)
        {
            package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        }

        if (package != null)
        {
            if (package.Status == TreatmentPackageStatus.CancellationPending)
            {
                return (false, "Treatment program cancellation is currently pending confirmation. Treatment modifications are temporarily suspended.");
            }

            if (package.Status == TreatmentPackageStatus.Cancelled || package.Status == TreatmentPackageStatus.Expired || package.Status == TreatmentPackageStatus.Rejected || package.Status == TreatmentPackageStatus.Archived)
            {
                return (false, "This treatment program is no longer active. Historical information remains available in read-only mode.");
            }

            if (package.ExpirationDate <= DateTime.UtcNow)
            {
                return (false, "This treatment program is no longer active. Historical information remains available in read-only mode.");
            }
        }

        return (true, null);
    }

    private async Task CancelFutureSessionsAndReleaseSlotsAsync(Guid caseId, string cancellationReason, CancellationToken ct)
    {
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var uncompletedSessions = allSessions
            .Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted &&
                        (s.Status == TreatmentSessionStatus.Scheduled ||
                         s.Status == TreatmentSessionStatus.Planned ||
                         s.Status == TreatmentSessionStatus.InProgress))
            .ToList();

        var now = DateTime.UtcNow;
        foreach (var session in uncompletedSessions)
        {
            session.Status = TreatmentSessionStatus.Cancelled;
            session.UpdatedAt = now;
            _sessionRepo.Update(session);

            if (session.AppointmentId.HasValue)
            {
                var appt = await _appointmentRepo.GetByIdAsync(session.AppointmentId.Value, ct);
                if (appt != null &&
                    appt.Status != AppointmentStatus.Completed &&
                    appt.Status != AppointmentStatus.Cancelled)
                {
                    appt.Status = AppointmentStatus.Cancelled;
                    appt.CancelledAt = now;
                    appt.CancellationReason = cancellationReason;
                    appt.UpdatedAt = now;
                    _appointmentRepo.Update(appt);

                    var slot = await _slotRepo.GetByIdAsync(appt.AppointmentSlotId, ct);
                    if (slot != null)
                    {
                        slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
                        if (slot.CurrentBookings < slot.MaxPatients)
                        {
                            slot.Status = AppointmentSlotStatus.Available;
                        }
                        slot.UpdatedAt = now;
                        _slotRepo.Update(slot);
                    }
                }
            }
        }
    }

    private async Task SendLifecycleNotificationAsync(
        Guid doctorProfileId,
        Guid? patientProfileId,
        string title,
        string message,
        Guid relatedEntityId,
        string relatedEntityType,
        CancellationToken ct)
    {
        try
        {
            var allDoctors = await _doctorRepo.GetAllAsync(ct);
            var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorProfileId || d.UserId == doctorProfileId);
            if (doctor != null)
            {
                var existingNotifs = await _notificationRepo.GetAllAsync(ct);
                bool alreadyNotified = existingNotifs.Any(n =>
                    !n.IsDeleted &&
                    n.UserId == doctor.UserId &&
                    n.Title == title &&
                    n.RelatedEntityId == relatedEntityId &&
                    n.RelatedEntityType == relatedEntityType);

                if (!alreadyNotified)
                {
                    await _notificationService.CreateNotificationAsync(
                        doctor.UserId,
                        title,
                        message,
                        NotificationType.Package,
                        relatedEntityId,
                        relatedEntityType,
                        ct);
                }
            }

            if (patientProfileId.HasValue)
            {
                var allPatients = await _patientRepo.GetAllAsync(ct);
                var patient = allPatients.FirstOrDefault(p => p.Id == patientProfileId.Value || p.UserId == patientProfileId.Value);
                if (patient != null)
                {
                    var existingNotifs = await _notificationRepo.GetAllAsync(ct);
                    bool alreadyNotified = existingNotifs.Any(n =>
                        !n.IsDeleted &&
                        n.UserId == patient.UserId &&
                        n.Title == title &&
                        n.RelatedEntityId == relatedEntityId &&
                        n.RelatedEntityType == relatedEntityType);

                    if (!alreadyNotified)
                    {
                        await _notificationService.CreateNotificationAsync(
                            patient.UserId,
                            title,
                            message,
                            NotificationType.Package,
                            relatedEntityId,
                            relatedEntityType,
                            ct);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver lifecycle notification for {RelatedEntityType} {RelatedEntityId}",
                relatedEntityType, relatedEntityId);
        }
    }
}
