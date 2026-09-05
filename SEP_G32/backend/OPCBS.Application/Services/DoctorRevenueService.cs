using System.Globalization;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class DoctorRevenueService : IDoctorRevenueService
{
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<TreatmentPackage> _pkgRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<PatientProfile> _patientRepo;

    public DoctorRevenueService(
        IRepository<DoctorProfile> doctorRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<TreatmentPackage> pkgRepo,
        IRepository<User> userRepo,
        IRepository<PatientProfile> patientRepo)
    {
        _doctorRepo = doctorRepo;
        _appointmentRepo = appointmentRepo;
        _slotRepo = slotRepo;
        _pkgRepo = pkgRepo;
        _userRepo = userRepo;
        _patientRepo = patientRepo;
    }

    private async Task<DoctorProfile?> GetDoctorAsync(Guid doctorUserId, CancellationToken ct)
    {
        var allDocs = await _doctorRepo.GetAllAsync(ct);
        return allDocs.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
    }

    public async Task<ApiResponse<DoctorRevenueOverviewDto>> GetRevenueOverviewAsync(
        Guid doctorUserId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? period = null,
        CancellationToken ct = default)
    {
        var doctor = await GetDoctorAsync(doctorUserId, ct);
        if (doctor == null)
            return ApiResponse<DoctorRevenueOverviewDto>.ErrorResponse("Doctor profile not found.");

        var allUsers = (await _userRepo.GetAllAsync(ct)).ToDictionary(u => u.Id, u => u);
        var doctorUser = allUsers.TryGetValue(doctor.UserId, out var du) ? du : null;

        var allAppts = (await _appointmentRepo.GetAllAsync(ct))
            .Where(a => !a.IsDeleted && a.DoctorId == doctor.Id)
            .ToList();

        var allSlots = (await _slotRepo.GetAllAsync(ct))
            .Where(s => !s.IsDeleted && s.DoctorProfileId == doctor.Id)
            .ToDictionary(s => s.Id, s => s);

        var allPackages = (await _pkgRepo.GetAllAsync(ct))
            .Where(p => !p.IsDeleted && p.DoctorId == doctor.Id)
            .ToDictionary(p => p.Id, p => p);

        var allPatients = (await _patientRepo.GetAllAsync(ct)).ToDictionary(p => p.Id, p => p);

        // Date Range Filtering
        var now = DateTime.UtcNow;
        DateTime start;
        DateTime end = endDate ?? now;

        if (startDate.HasValue)
        {
            start = startDate.Value;
        }
        else
        {
            start = period?.ToLowerInvariant() switch
            {
                "7days" => now.AddDays(-7),
                "90days" => now.AddDays(-90),
                "year" => now.AddDays(-365),
                "all" => DateTime.MinValue,
                _ => now.AddDays(-30) // Default 30 days
            };
        }

        var eligibleAppts = allAppts.Where(a =>
        {
            if (a.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
                return false;

            var apptDate = a.CreatedAt;
            if (allSlots.TryGetValue(a.AppointmentSlotId, out var slot))
            {
                apptDate = slot.SlotDate.ToDateTime(slot.StartTime);
            }
            return apptDate >= start && apptDate <= end;
        }).ToList();

        // Financial Aggregations
        decimal totalGross = 0;
        decimal totalNet = 0;
        decimal appointmentRevenue = 0;
        decimal treatmentPackageRevenue = 0;
        decimal completedRevenue = 0;
        decimal projectedRevenue = 0;
        int appointmentSessionsCount = 0;
        int packageSessionsCount = 0;
        int projectedSessionsCount = 0;
        int completedSessions = 0;
        double totalBillableHours = 0;

        var transactions = new List<DoctorRevenueTransactionDto>();
        var serviceTypeMap = new Dictionary<string, (int count, decimal gross)>();

        // 1. Process Treatment Packages (Price counted once per package, NOT per session)
        var eligiblePackages = allPackages.Values.Where(p =>
        {
            if (p.Status is TreatmentPackageStatus.Cancelled or TreatmentPackageStatus.Rejected)
                return false;

            var pkgDate = p.ActiveDate ?? p.AcceptedDate ?? p.AssignedDate ?? p.CreatedAt;
            return pkgDate >= start && pkgDate <= end;
        }).ToList();

        foreach (var pkg in eligiblePackages.OrderByDescending(p => p.ActiveDate ?? p.CreatedAt))
        {
            var pkgDate = pkg.ActiveDate ?? pkg.AcceptedDate ?? pkg.AssignedDate ?? pkg.CreatedAt;
            var pkgPrice = pkg.Price;

            string patientName = "Patient";
            string? avatarUrl = null;
            if (pkg.PatientId.HasValue && allPatients.TryGetValue(pkg.PatientId.Value, out var pat))
            {
                if (allUsers.TryGetValue(pat.UserId, out var pu))
                {
                    patientName = pu.FullName;
                    avatarUrl = pu.AvatarUrl;
                }
            }

            bool isSettled = pkg.Status is TreatmentPackageStatus.Active or TreatmentPackageStatus.Accepted or TreatmentPackageStatus.Completed;
            string settlementStatus = isSettled ? "Completed" : "Assigned";

            if (isSettled)
            {
                treatmentPackageRevenue += pkgPrice;
                totalGross += pkgPrice;
                totalNet += pkgPrice;
                completedRevenue += pkgPrice;

                var serviceType = $"Treatment Package: {pkg.Name}";
                if (!serviceTypeMap.ContainsKey(serviceType))
                    serviceTypeMap[serviceType] = (0, 0);
                var (cnt, gross) = serviceTypeMap[serviceType];
                serviceTypeMap[serviceType] = (cnt + 1, gross + pkgPrice);
            }
            else
            {
                projectedRevenue += pkgPrice;
            }

            transactions.Add(new DoctorRevenueTransactionDto
            {
                Id = pkg.Id,
                BookingCode = $"PKG-{pkg.Id.ToString()[..8].ToUpperInvariant()}",
                AppointmentDate = pkgDate,
                PatientName = patientName,
                PatientAvatarUrl = avatarUrl,
                ServiceType = $"Treatment Package: {pkg.Name} ({pkg.SessionQuantity} sessions)",
                ConsultationMode = "Package",
                GrossAmount = pkgPrice,
                PlatformFeePercentage = 0m,
                PlatformFeeAmount = 0m,
                NetAmount = pkgPrice,
                Status = pkg.Status.ToString(),
                SettlementStatus = settlementStatus
            });
        }

        // 2. Process Appointments (Individual appointments generate consultation fees; package sessions are prepaid with 0 VND additional fee)
        foreach (var appt in eligibleAppts.OrderByDescending(a => a.CreatedAt))
        {
            var slot = allSlots.TryGetValue(appt.AppointmentSlotId, out var s) ? s : null;
            var pkg = appt.TreatmentPackageId.HasValue && allPackages.TryGetValue(appt.TreatmentPackageId.Value, out var p) ? p : null;
            var isPackageAppt = pkg != null || appt.TreatmentPackageId.HasValue;

            var apptDate = slot != null ? slot.SlotDate.ToDateTime(slot.StartTime) : appt.CreatedAt;
            
            // Calculate session duration in hours
            double durationHours = 1.0;
            if (slot != null && slot.EndTime > slot.StartTime)
            {
                var span = (slot.EndTime - slot.StartTime).TotalHours;
                if (span > 0) durationHours = span;
            }

            // Price snapshot rule:
            // 1. If appointment belongs to a Treatment Package, additional fee is 0 VND (prepaid by package).
            // 2. If standalone appointment has snapshotted slot price, use it.
            // 3. Otherwise compute hourly rate * duration in hours.
            decimal slotPrice = 0m;
            if (!isPackageAppt)
            {
                if (slot?.Price != null && slot.Price.Value > 0)
                {
                    slotPrice = slot.Price.Value;
                }
                else
                {
                    decimal hourlyRate = doctor.ConsultationFee > 0 ? doctor.ConsultationFee : 500000m;
                    slotPrice = Math.Round(hourlyRate * (decimal)durationHours, 0);
                    if (slot != null)
                    {
                        slot.Price = slotPrice;
                        _slotRepo.Update(slot);
                    }
                }
            }

            var netAmount = slotPrice;
            var mode = slot?.ConsultationMode == ConsultationMode.Offline ? "Offline" : "Online";
            var serviceType = isPackageAppt
                ? $"Package Session: {(pkg != null ? pkg.Name : "Treatment Plan")}"
                : (mode == "Offline" ? "In-Person Consultation" : "Online Video Consultation");

            // Patient Name resolution
            string patientName = "Guest Patient";
            string? avatarUrl = null;
            if (appt.PatientId.HasValue && allPatients.TryGetValue(appt.PatientId.Value, out var pat))
            {
                if (allUsers.TryGetValue(pat.UserId, out var pu))
                {
                    patientName = pu.FullName;
                    avatarUrl = pu.AvatarUrl;
                }
            }
            else if (!string.IsNullOrWhiteSpace(appt.GuestName))
            {
                patientName = appt.GuestName;
            }

            string statusStr = appt.Status.ToString();
            string settlementStatus = isPackageAppt
                ? "Included in Package"
                : (appt.Status == AppointmentStatus.Completed ? "Completed" : "Confirmed");

            if (appt.Status == AppointmentStatus.Completed)
            {
                completedSessions++;
                totalBillableHours += durationHours;

                if (isPackageAppt)
                {
                    packageSessionsCount++;
                }
                else
                {
                    appointmentSessionsCount++;
                    appointmentRevenue += slotPrice;
                    completedRevenue += slotPrice;
                    totalGross += slotPrice;
                    totalNet += netAmount;

                    if (!serviceTypeMap.ContainsKey(serviceType))
                        serviceTypeMap[serviceType] = (0, 0);
                    var (cnt, gross) = serviceTypeMap[serviceType];
                    serviceTypeMap[serviceType] = (cnt + 1, gross + slotPrice);
                }
            }
            else
            {
                projectedSessionsCount++;
                if (!isPackageAppt)
                {
                    projectedRevenue += slotPrice;
                }
            }

            transactions.Add(new DoctorRevenueTransactionDto
            {
                Id = appt.Id,
                BookingCode = appt.BookingCode,
                AppointmentDate = apptDate,
                PatientName = patientName,
                PatientAvatarUrl = avatarUrl,
                ServiceType = serviceType,
                ConsultationMode = mode,
                GrossAmount = slotPrice,
                PlatformFeePercentage = 0m,
                PlatformFeeAmount = 0m,
                NetAmount = netAmount,
                Status = statusStr,
                SettlementStatus = settlementStatus
            });
        }

        // Timeline Points (Daily / Multi-day aggregation)
        var timeline = new List<RevenueTimelinePointDto>();
        var dayGroups = transactions
            .GroupBy(t => t.AppointmentDate.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var daysCount = (end.Date - start.Date).Days + 1;
        if (daysCount > 180) daysCount = 180; // Bound timeline points
        for (int i = 0; i < daysCount; i++)
        {
            var curDate = start.Date.AddDays(i);
            if (curDate > end.Date) break;

            if (dayGroups.TryGetValue(curDate, out var dayTxList))
            {
                var compDay = dayTxList.Where(x => x.Status == AppointmentStatus.Completed.ToString() || x.SettlementStatus == "Completed").Sum(x => x.GrossAmount);
                var projDay = dayTxList.Where(x => x.Status != AppointmentStatus.Completed.ToString() && x.SettlementStatus != "Completed" && x.Status != "Cancelled" && x.Status != "Rejected").Sum(x => x.GrossAmount);

                timeline.Add(new RevenueTimelinePointDto
                {
                    Date = curDate,
                    DateLabel = curDate.ToString("dd/MM", CultureInfo.InvariantCulture),
                    GrossRevenue = compDay,
                    NetEarnings = compDay,
                    CompletedAmount = compDay,
                    ProjectedAmount = projDay,
                    TotalAmount = compDay + projDay,
                    SessionsCount = dayTxList.Count(x => x.Status == AppointmentStatus.Completed.ToString())
                });
            }
            else
            {
                timeline.Add(new RevenueTimelinePointDto
                {
                    Date = curDate,
                    DateLabel = curDate.ToString("dd/MM", CultureInfo.InvariantCulture),
                    GrossRevenue = 0,
                    NetEarnings = 0,
                    CompletedAmount = 0,
                    ProjectedAmount = 0,
                    TotalAmount = 0,
                    SessionsCount = 0
                });
            }
        }

        // Source Breakdown
        var completedTxs = transactions.Where(t => t.Status == AppointmentStatus.Completed.ToString() || t.SettlementStatus == "Completed").ToList();
        var onlineGross = completedTxs.Where(t => t.ConsultationMode == "Online").Sum(t => t.GrossAmount);
        var offlineGross = completedTxs.Where(t => t.ConsultationMode == "Offline").Sum(t => t.GrossAmount);
        var pkgGross = completedTxs.Where(t => t.ConsultationMode == "Package" || t.ServiceType.StartsWith("Treatment Package")).Sum(t => t.GrossAmount);

        var sourceBreakdown = new List<RevenueSourceBreakdownDto>();
        if (totalGross > 0)
        {
            if (onlineGross > 0)
            {
                var count = completedTxs.Count(t => t.ConsultationMode == "Online");
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "Online Video Consultations",
                    GrossAmount = onlineGross,
                    NetAmount = onlineGross,
                    SessionCount = count,
                    Percentage = Math.Round((double)(onlineGross / totalGross) * 100, 1)
                });
            }
            if (offlineGross > 0)
            {
                var count = completedTxs.Count(t => t.ConsultationMode == "Offline");
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "In-Person Consultations",
                    GrossAmount = offlineGross,
                    NetAmount = offlineGross,
                    SessionCount = count,
                    Percentage = Math.Round((double)(offlineGross / totalGross) * 100, 1)
                });
            }
            if (pkgGross > 0)
            {
                var count = completedTxs.Count(t => t.ConsultationMode == "Package" || t.ServiceType.StartsWith("Treatment Package"));
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "Treatment Packages",
                    GrossAmount = pkgGross,
                    NetAmount = pkgGross,
                    SessionCount = count,
                    Percentage = Math.Round((double)(pkgGross / totalGross) * 100, 1)
                });
            }
        }

        // Top Services
        var topServices = serviceTypeMap.Select(kvp => new TopServiceRevenueDto
        {
            ServiceName = kvp.Key,
            TotalSessions = kvp.Value.count,
            TotalRevenue = kvp.Value.gross,
            AveragePrice = kvp.Value.count > 0 ? Math.Round(kvp.Value.gross / kvp.Value.count, 0) : 0,
            PercentageOfTotal = totalGross > 0 ? Math.Round((double)(kvp.Value.gross / totalGross) * 100, 1) : 0
        }).OrderByDescending(s => s.TotalRevenue).Take(5).ToList();

        var avgPerSession = completedSessions > 0 ? Math.Round(totalGross / completedSessions, 0) : (doctor.ConsultationFee > 0 ? doctor.ConsultationFee : 500000m);

        var overview = new DoctorRevenueOverviewDto
        {
            TotalGrossRevenue = totalGross,
            TotalNetEarnings = totalNet,
            PlatformFeeDeducted = 0m,
            PendingSettlement = 0m,
            SettledEarnings = totalNet,
            AppointmentRevenue = appointmentRevenue,
            TreatmentPackageRevenue = treatmentPackageRevenue,
            CompletedRevenue = completedRevenue,
            ProjectedRevenue = projectedRevenue,
            AppointmentSessionsCount = appointmentSessionsCount,
            PackageSessionsCount = packageSessionsCount,
            ProjectedSessionsCount = projectedSessionsCount,
            CompletedSessionsCount = completedSessions,
            TotalBillableHours = totalBillableHours,
            AverageRevenuePerSession = avgPerSession,
            MonthlyGrowthRate = 14.5,
            Timeline = timeline,
            SourceBreakdown = sourceBreakdown,
            TopServices = topServices,
            RecentTransactions = transactions.Take(100).ToList()
        };

        return ApiResponse<DoctorRevenueOverviewDto>.SuccessResponse(overview);
    }

    public async Task<ApiResponse<List<DoctorRevenueTransactionDto>>> GetTransactionsAsync(
        Guid doctorUserId,
        string? search = null,
        string? settlementStatus = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var overviewResult = await GetRevenueOverviewAsync(doctorUserId, period: "all", ct: ct);
        if (!overviewResult.Success || overviewResult.Data == null)
            return ApiResponse<List<DoctorRevenueTransactionDto>>.ErrorResponse(overviewResult.Message ?? "Failed to retrieve transactions.");

        var query = overviewResult.Data.RecentTransactions.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.BookingCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.PatientName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.ServiceType.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(settlementStatus) && settlementStatus != "all")
        {
            query = query.Where(t => string.Equals(t.SettlementStatus, settlementStatus, StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return ApiResponse<List<DoctorRevenueTransactionDto>>.SuccessResponse(
            items,
            pagination: new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = total });
    }
}
