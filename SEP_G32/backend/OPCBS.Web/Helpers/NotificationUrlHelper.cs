using System;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Helpers;

public static class NotificationUrlHelper
{
    /// <summary>
    /// Resolves the correct application route URL for a notification based on its related entity and the recipient's role.
    /// </summary>
    public static string ResolveUrl(string? entityType, Guid? entityId, string? userRole)
    {
        var role = userRole?.Trim().ToLowerInvariant() ?? "";
        var type = entityType?.Trim() ?? "";

        // 1. DOCTOR
        if (role == "doctor")
        {
            switch (type)
            {
                case "Appointment":
                case "AppointmentReminder":
                case "AppointmentCompletionConfirmation":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/Appointments/Details/{entityId.Value}"
                        : "/Doctor/Appointments/Index";

                case "ConsultationNote":
                case "FollowUpReminder":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/ConsultationNotes/Details/{entityId.Value}"
                        : "/Doctor/ConsultationNotes/Index";

                case "TreatmentPackage":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/TreatmentPackages/Details/{entityId.Value}"
                        : "/Doctor/TreatmentPackages/Index";

                case "TreatmentCase":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/TreatmentCases/Details/{entityId.Value}"
                        : "/Doctor/TreatmentCases/Index";

                case "Conversation":
                case "Message":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/Messages/Index?conversationId={entityId.Value}"
                        : "/Doctor/Messages/Index";

                case "Verification":
                case "DoctorVerification":
                    return "/Doctor/VerificationStatus";

                case "Subscription":
                case "DoctorSubscription":
                    return "/Doctor/Subscriptions/Status";

                case "BlogPost":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Blog/Details/{entityId.Value}"
                        : "/Doctor/Blogs/Index";

                case "Patient":
                case "PatientRecord":
                case "PatientProfile":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctor/Patients/Details/{entityId.Value}"
                        : "/Doctor/Patients/Index";

                case "Review":
                    return "/Doctor/Profile";

                default:
                    return "/Doctor/Dashboard";
            }
        }

        // 2. PATIENT
        if (role == "patient")
        {
            switch (type)
            {
                case "Appointment":
                case "AppointmentReminder":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Patient/Appointments/Details/{entityId.Value}"
                        : "/Patient/Appointments/Index";

                case "AppointmentCompletionConfirmation":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Patient/Appointments/Details/{entityId.Value}"
                        : "/Patient/ConsultationRecords/Index";

                case "ConsultationNote":
                case "FollowUpReminder":
                    return "/Patient/ConsultationRecords/Index";

                case "TreatmentPackage":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Patient/TreatmentPackages/Details/{entityId.Value}"
                        : "/Patient/TreatmentPackages/Index";

                case "TreatmentCase":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Patient/TreatmentCases/Details/{entityId.Value}"
                        : "/Patient/TreatmentCases/Index";

                case "Conversation":
                case "Message":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Patient/Messages/Index?conversationId={entityId.Value}"
                        : "/Patient/Messages/Index";

                case "BlogPost":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Blog/Details/{entityId.Value}"
                        : "/Blog/Index";

                case "FavoriteDoctor":
                case "Doctor":
                    return entityId.HasValue && entityId.Value != Guid.Empty
                        ? $"/Doctors/Details/{entityId.Value}"
                        : "/Patient/Favorites/Index";

                case "PsychometricTest":
                    return "/Patient/Psychometrics/TakeTest";

                case "PsychometricSubmission":
                    return "/Patient/Psychometrics/Result";

                case "Review":
                    return "/Patient/Reviews/Index";

                default:
                    return "/Patient/Appointments/Index";
            }
        }

        // 3. BUSINESS MANAGER
        if (role == "businessmanager" || role == "manager")
        {
            switch (type)
            {
                case "Subscription":
                case "DoctorSubscription":
                    return "/BusinessManager/Subscriptions/Index";

                case "ServicePackage":
                case "Package":
                    return "/BusinessManager/ServicePackages/Index";

                case "PsychometricTest":
                    return "/BusinessManager/Psychometrics/Index";

                default:
                    return "/BusinessManager/Dashboard";
            }
        }

        // 4. CUSTOMER SUPPORT
        if (role == "customersupport" || role == "support")
        {
            switch (type)
            {
                case "Verification":
                case "DoctorVerification":
                    return "/CustomerSupport/Verifications/Index";

                case "ViolationReport":
                    return "/CustomerSupport/Reports/Index";

                default:
                    return "/CustomerSupport/Dashboard";
            }
        }

        // 5. ADMIN / SYSTEM ADMIN
        if (role == "admin" || role == "systemadmin")
        {
            switch (type)
            {
                case "Verification":
                case "DoctorVerification":
                    return "/Admin/Verifications/Index";

                case "ViolationReport":
                    return "/Admin/Reports/Index";

                default:
                    return "/Admin/Dashboard";
            }
        }

        // 6. DEFAULT / GUEST
        switch (type)
        {
            case "BlogPost":
                return entityId.HasValue && entityId.Value != Guid.Empty
                    ? $"/Blog/Details/{entityId.Value}"
                    : "/Blog/Index";

            case "Doctor":
            case "FavoriteDoctor":
                return entityId.HasValue && entityId.Value != Guid.Empty
                    ? $"/Doctors/Details/{entityId.Value}"
                    : "/Doctors/Index";

            default:
                return "/Notifications";
        }
    }

    /// <summary>
    /// Enriches a notification DTO with its calculated ActionUrl based on user role.
    /// </summary>
    public static void EnrichActionUrl(this NotificationDto dto, string? userRole)
    {
        dto.ActionUrl = ResolveUrl(dto.RelatedEntityType, dto.RelatedEntityId, userRole);
    }

    /// <summary>
    /// Enriches a collection of notification DTOs with their calculated ActionUrls based on user role.
    /// </summary>
    public static void EnrichActionUrls(this IEnumerable<NotificationDto> dtos, string? userRole)
    {
        foreach (var dto in dtos)
        {
            dto.EnrichActionUrl(userRole);
        }
    }
}
