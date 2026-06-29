namespace OPCBS.Web.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public string? BookingCode { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorAvatarUrl { get; set; }
    public string? Specialization { get; set; }
    public string? AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public string? CancellationReason { get; set; }
    public decimal? Fee { get; set; }
    public DateTime CreatedAt { get; set; }

    // Aliases for views
    public DateTimeOffset StartAt => ParseDateTime();
    public DateTimeOffset EndAt => ParseEndTime();
    private DateTimeOffset ParseDateTime()
    {
        if (DateTime.TryParse($"{AppointmentDate} {StartTime}", out var dt)) return dt;
        return CreatedAt;
    }
    private DateTimeOffset ParseEndTime()
    {
        if (DateTime.TryParse($"{AppointmentDate} {EndTime}", out var dt)) return dt;
        return StartAt.AddHours(1);
    }
}

public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public string? BookingCode { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? Specialization { get; set; }
    public string? AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal? Fee { get; set; }

    // Alias
    public DateTimeOffset StartAt
    {
        get
        {
            if (DateTime.TryParse($"{AppointmentDate} {StartTime}", out var dt)) return dt;
            return DateTimeOffset.MinValue;
        }
    }
}

public class CreateAppointmentDto
{
    public Guid DoctorId { get; set; }
    public Guid AppointmentSlotId { get; set; }
    public string? Notes { get; set; }
    public Guid? TreatmentPackageId { get; set; }

    // Guest booking
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhoneNumber { get; set; }
}

public class RescheduleAppointmentDto
{
    public Guid NewSlotId { get; set; }
    public string? Reason { get; set; }
}

public class CancelAppointmentDto
{
    public string? Reason { get; set; }
}

public class TrackAppointmentRequestDto
{
    public string? Email { get; set; }
    public string? BookingCode { get; set; }
    // Alias for backend
    public string? TrackingCode { get => BookingCode; set => BookingCode = value; }
}

public class AppointmentFilterDto
{
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AppointmentSlotDto
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Price { get; set; }
}

public class AvailableSlotsDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public List<AppointmentSlotDto>? Slots { get; set; }
}
