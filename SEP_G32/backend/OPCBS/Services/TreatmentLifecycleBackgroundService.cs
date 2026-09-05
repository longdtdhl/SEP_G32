using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Services;

/// <summary>
/// Background service that periodically reconciles treatment package and treatment case lifecycles,
/// automatically expiring proposals past their acceptance deadline, and expiring active programs
/// past their validity dates with future session cancellation and slot cleanup.
/// </summary>
public class TreatmentLifecycleBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TreatmentLifecycleBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public TreatmentLifecycleBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TreatmentLifecycleBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TreatmentLifecycleBackgroundService started.");

        // Wait a short duration after startup before the initial reconciliation sweep
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileExpiredLifecyclesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during treatment lifecycle reconciliation sweep.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("TreatmentLifecycleBackgroundService stopped.");
    }

    private async Task ReconcileExpiredLifecyclesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<ITreatmentLifecycleCoordinator>();
        var packageRepo = scope.ServiceProvider.GetRequiredService<IRepository<TreatmentPackage>>();
        var caseRepo = scope.ServiceProvider.GetRequiredService<IRepository<TreatmentCase>>();

        var now = DateTime.UtcNow;

        // 1. Reconcile assigned package proposals past acceptance deadline
        var allPackages = await packageRepo.GetAllAsync(ct);
        var unexpiredProposals = allPackages.Where(p =>
            !p.IsDeleted &&
            p.Status == TreatmentPackageStatus.Assigned &&
            p.AcceptanceExpiresAt.HasValue &&
            p.AcceptanceExpiresAt.Value <= now).ToList();

        foreach (var pkg in unexpiredProposals)
        {
            try
            {
                await coordinator.ExpireAssignedPackageAsync(pkg, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire assigned package proposal {PackageId}", pkg.Id);
            }
        }

        // 2. Reconcile active packages past their expiration validity date or synced with cases
        var activePackages = allPackages.Where(p =>
            !p.IsDeleted &&
            (p.Status == TreatmentPackageStatus.Active || p.Status == TreatmentPackageStatus.Accepted)).ToList();

        foreach (var pkg in activePackages)
        {
            try
            {
                await coordinator.ReconcilePackageLifecycleAsync(pkg, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile active package {PackageId}", pkg.Id);
            }
        }

        // 3. Reconcile treatment cases past their expected end date or synced with packages
        var allCases = await caseRepo.GetAllAsync(ct);
        var activeCases = allCases.Where(c =>
            !c.IsDeleted &&
            (c.Status == TreatmentCaseStatus.Active || c.Status == TreatmentCaseStatus.OnHold)).ToList();

        foreach (var tc in activeCases)
        {
            try
            {
                await coordinator.ReconcileCaseLifecycleAsync(tc, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile treatment case {CaseId}", tc.Id);
            }
        }
    }
}
