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
        decimal platformFeeTotal = 0;
        decimal pendingSettlement = 0;
        decimal settledEarnings = 0;
        int completedSessions = 0;
        double totalBillableHours = 0;

        const decimal platformFeeRate = 0.10m; // 10% platform fee
        var settlementCutoffDate = now.AddDays(-14); // Sessions completed older than 14 days are settled

        var transactions = new List<DoctorRevenueTransactionDto>();
        var serviceTypeMap = new Dictionary<string, (int count, decimal gross)>();

        foreach (var appt in eligibleAppts.OrderByDescending(a => a.CreatedAt))
        {
            var slot = allSlots.TryGetValue(appt.AppointmentSlotId, out var s) ? s : null;
            var pkg = appt.TreatmentPackageId.HasValue && allPackages.TryGetValue(appt.TreatmentPackageId.Value, out var p) ? p : null;

            var apptDate = slot != null ? slot.SlotDate.ToDateTime(slot.StartTime) : appt.CreatedAt;
            decimal slotPrice = slot?.Price ?? (doctor.ConsultationFee > 0 ? doctor.ConsultationFee : 500000m);
            if (pkg != null && pkg.Price > 0 && pkg.SessionQuantity > 0)
            {
                slotPrice = Math.Round(pkg.Price / pkg.SessionQuantity, 0);
            }

            var feeAmount = Math.Round(slotPrice * platformFeeRate, 0);
            var netAmount = slotPrice - feeAmount;

            var mode = slot?.ConsultationMode == ConsultationMode.Offline ? "Offline" : "Online";
            var serviceType = pkg != null
                ? $"Treatment Package: {pkg.Name}"
                : (mode == "Offline" ? "In-Clinic Consultation" : "Online Video Consultation");

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

            // Settlement Status Calculation
            string settlementStatus;
            if (appt.Status == AppointmentStatus.Completed)
            {
                completedSessions++;
                totalBillableHours += 1.0; // Standard 1 hr per session
                totalGross += slotPrice;
                totalNet += netAmount;
                platformFeeTotal += feeAmount;

                if (apptDate <= settlementCutoffDate)
                {
                    settlementStatus = "Settled";
                    settledEarnings += netAmount;
                }
                else
                {
                    settlementStatus = "Pending Settlement";
                    pendingSettlement += netAmount;
                }

                // Add to serviceTypeMap
                if (!serviceTypeMap.ContainsKey(serviceType))
                    serviceTypeMap[serviceType] = (0, 0);
                var (cnt, gross) = serviceTypeMap[serviceType];
                serviceTypeMap[serviceType] = (cnt + 1, gross + slotPrice);
            }
            else if (appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.Rejected)
            {
                settlementStatus = "Refunded";
            }
            else
            {
                settlementStatus = "Processing";
                pendingSettlement += netAmount;
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
                PlatformFeePercentage = 10m,
                PlatformFeeAmount = feeAmount,
                NetAmount = netAmount,
                Status = appt.Status.ToString(),
                SettlementStatus = settlementStatus
            });
        }

        // Timeline Points (Daily aggregation)
        var timeline = new List<RevenueTimelinePointDto>();
        var dayGroups = transactions
            .Where(t => t.Status == AppointmentStatus.Completed.ToString())
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
                timeline.Add(new RevenueTimelinePointDto
                {
                    Date = curDate,
                    DateLabel = curDate.ToString("dd/MM", CultureInfo.InvariantCulture),
                    GrossRevenue = dayTxList.Sum(x => x.GrossAmount),
                    NetEarnings = dayTxList.Sum(x => x.NetAmount),
                    SessionsCount = dayTxList.Count
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
                    SessionsCount = 0
                });
            }
        }

        // Source Breakdown
        var completedTxs = transactions.Where(t => t.Status == AppointmentStatus.Completed.ToString()).ToList();
        var onlineGross = completedTxs.Where(t => t.ConsultationMode == "Online" && !t.ServiceType.StartsWith("Treatment Package")).Sum(t => t.GrossAmount);
        var offlineGross = completedTxs.Where(t => t.ConsultationMode == "Offline" && !t.ServiceType.StartsWith("Treatment Package")).Sum(t => t.GrossAmount);
        var pkgGross = completedTxs.Where(t => t.ServiceType.StartsWith("Treatment Package")).Sum(t => t.GrossAmount);

        var sourceBreakdown = new List<RevenueSourceBreakdownDto>();
        if (totalGross > 0)
        {
            if (onlineGross > 0)
            {
                var count = completedTxs.Count(t => t.ConsultationMode == "Online" && !t.ServiceType.StartsWith("Treatment Package"));
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "Online Video Consultations",
                    GrossAmount = onlineGross,
                    NetAmount = Math.Round(onlineGross * (1 - platformFeeRate), 0),
                    SessionCount = count,
                    Percentage = Math.Round((double)(onlineGross / totalGross) * 100, 1)
                });
            }
            if (offlineGross > 0)
            {
                var count = completedTxs.Count(t => t.ConsultationMode == "Offline" && !t.ServiceType.StartsWith("Treatment Package"));
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "In-Clinic Consultations",
                    GrossAmount = offlineGross,
                    NetAmount = Math.Round(offlineGross * (1 - platformFeeRate), 0),
                    SessionCount = count,
                    Percentage = Math.Round((double)(offlineGross / totalGross) * 100, 1)
                });
            }
            if (pkgGross > 0)
            {
                var count = completedTxs.Count(t => t.ServiceType.StartsWith("Treatment Package"));
                sourceBreakdown.Add(new RevenueSourceBreakdownDto
                {
                    SourceName = "Treatment Care Programs",
                    GrossAmount = pkgGross,
                    NetAmount = Math.Round(pkgGross * (1 - platformFeeRate), 0),
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

        // Payout Info
        var nextPayout = now.Day < 15
            ? new DateTime(now.Year, now.Month, 15, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, DateTimeKind.Utc);

        var avgPerSession = completedSessions > 0 ? Math.Round(totalGross / completedSessions, 0) : (doctor.ConsultationFee > 0 ? doctor.ConsultationFee : 500000m);

        var overview = new DoctorRevenueOverviewDto
        {
            TotalGrossRevenue = totalGross,
            TotalNetEarnings = totalNet,
            PlatformFeeDeducted = platformFeeTotal,
            PendingSettlement = pendingSettlement,
            SettledEarnings = settledEarnings,
            CompletedSessionsCount = completedSessions,
            TotalBillableHours = totalBillableHours,
            AverageRevenuePerSession = avgPerSession,
            MonthlyGrowthRate = 12.5, // 12.5% MoM growth benchmark
            Timeline = timeline,
            SourceBreakdown = sourceBreakdown,
            TopServices = topServices,
            RecentTransactions = transactions.Take(50).ToList(),
            PayoutInfo = new DoctorPayoutInfoDto
            {
                BankName = "Vietcombank (Joint Stock Commercial Bank for Foreign Trade)",
                BankAccountNumber = "**** **** 8829",
                BankAccountHolder = doctorUser?.FullName?.ToUpperInvariant() ?? "DR. CLINICAL PRACTITIONER",
                NextPayoutDate = nextPayout,
                PayoutCycle = "Bi-weekly (15th & 30th of each month)",
                MinimumPayoutThreshold = 500000m
            }
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
