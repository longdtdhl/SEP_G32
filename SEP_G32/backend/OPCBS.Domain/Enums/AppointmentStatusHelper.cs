using System.Linq;

namespace OPCBS.Domain.Enums;

public static class AppointmentStatusHelper
{
    public static readonly AppointmentStatus[] ActiveStatuses = new[]
    {
        AppointmentStatus.Pending,
        AppointmentStatus.Approved,
        AppointmentStatus.InProgress,
        AppointmentStatus.RescheduleRequested,
        AppointmentStatus.AwaitingPatientConfirmation,
        AppointmentStatus.AwaitingGuestConfirmation
    };

    public static readonly AppointmentStatus[] HistoryStatuses = new[]
    {
        AppointmentStatus.Completed,
        AppointmentStatus.Cancelled,
        AppointmentStatus.Rejected,
        AppointmentStatus.NoShow
    };

    public static bool IsActive(AppointmentStatus status) => ActiveStatuses.Contains(status);

    public static bool IsHistory(AppointmentStatus status) => HistoryStatuses.Contains(status);

    public static bool IsActiveInt(int statusVal)
        => Enum.IsDefined(typeof(AppointmentStatus), statusVal) && IsActive((AppointmentStatus)statusVal);

    public static bool IsHistoryInt(int statusVal)
        => Enum.IsDefined(typeof(AppointmentStatus), statusVal) && IsHistory((AppointmentStatus)statusVal);
}
