using OPCBS.Domain.Entities;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Centralized lifecycle coordinator for TreatmentPackage and TreatmentCase transitions,
/// ensuring consistency, idempotent execution, transaction safety, slot cleanup, and mutation guards.
/// </summary>
public interface ITreatmentLifecycleCoordinator
{
    /// <summary>
    /// Lazy reconciliation of package state against time (expiration, acceptance deadline).
    /// </summary>
    Task ReconcilePackageLifecycleAsync(TreatmentPackage package, CancellationToken ct = default);

    /// <summary>
    /// Lazy reconciliation of case and linked package state against time.
    /// </summary>
    Task ReconcileCaseLifecycleAsync(TreatmentCase treatmentCase, CancellationToken ct = default);

    /// <summary>
    /// Expire an assigned package proposal that passed its acceptance deadline.
    /// </summary>
    Task ExpireAssignedPackageAsync(TreatmentPackage package, CancellationToken ct = default);

    /// <summary>
    /// Expire an active package and its linked treatment case when validity date passes.
    /// Cancels future planned/scheduled sessions, releases slots, and preserves actual progress.
    /// </summary>
    Task ExpireActivePackageAndCaseAsync(TreatmentPackage? package, TreatmentCase? treatmentCase, CancellationToken ct = default);

    /// <summary>
    /// Complete a treatment case and its linked package, setting progress to 100%.
    /// </summary>
    Task CompleteCaseAndPackageAsync(TreatmentCase treatmentCase, string? closureNote, CancellationToken ct = default);

    /// <summary>
    /// Cancel a package and its linked treatment case, releasing future sessions/slots and preserving progress.
    /// </summary>
    Task CancelCaseAndPackageAsync(TreatmentPackage? package, TreatmentCase? treatmentCase, string reason, CancellationToken ct = default);

    /// <summary>
    /// Evaluates whether a treatment case is currently active and manageable for mutating operations.
    /// </summary>
    Task<(bool CanManage, string? ErrorMessage)> ValidateCaseManageableAsync(TreatmentCase treatmentCase, Guid? doctorUserId = null, CancellationToken ct = default);
}
