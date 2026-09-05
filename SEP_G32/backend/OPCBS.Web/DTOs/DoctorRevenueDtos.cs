namespace OPCBS.Web.DTOs;

public class DoctorRevenueOverviewDto
{
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalNetEarnings { get; set; }
    public decimal PlatformFeeDeducted { get; set; }
    public decimal PendingSettlement { get; set; }
    public decimal SettledEarnings { get; set; }
    public decimal AppointmentRevenue { get; set; }
    public decimal TreatmentPackageRevenue { get; set; }
    public decimal CompletedRevenue { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public int AppointmentSessionsCount { get; set; }
    public int PackageSessionsCount { get; set; }
    public int ProjectedSessionsCount { get; set; }
    public int CompletedSessionsCount { get; set; }
    public double TotalBillableHours { get; set; }
    public decimal AverageRevenuePerSession { get; set; }
    public double MonthlyGrowthRate { get; set; }
    public List<RevenueTimelinePointDto> Timeline { get; set; } = new();
    public List<RevenueSourceBreakdownDto> SourceBreakdown { get; set; } = new();
    public List<TopServiceRevenueDto> TopServices { get; set; } = new();
    public List<DoctorRevenueTransactionDto> RecentTransactions { get; set; } = new();
    public DoctorPayoutInfoDto PayoutInfo { get; set; } = new();
}

public class DoctorRevenueTransactionDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientAvatarUrl { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ConsultationMode { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal PlatformFeePercentage { get; set; } = 0m;
    public decimal PlatformFeeAmount { get; set; } = 0m;
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SettlementStatus { get; set; } = string.Empty;
}

public class RevenueTimelinePointDto
{
    public string DateLabel { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetEarnings { get; set; }
    public decimal CompletedAmount { get; set; }
    public decimal ProjectedAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SessionsCount { get; set; }
}

public class RevenueSourceBreakdownDto
{
    public string SourceName { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public int SessionCount { get; set; }
    public double Percentage { get; set; }
}

public class TopServiceRevenueDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalRevenue { get; set; }
    public double PercentageOfTotal { get; set; }
}

public class DoctorPayoutInfoDto
{
    public string BankName { get; set; } = "Vietcombank";
    public string BankAccountNumber { get; set; } = "**** **** 8829";
    public string BankAccountHolder { get; set; } = string.Empty;
    public DateTime NextPayoutDate { get; set; }
    public string PayoutCycle { get; set; } = "Bi-weekly (15th & 30th)";
    public decimal MinimumPayoutThreshold { get; set; } = 500000m;
}
