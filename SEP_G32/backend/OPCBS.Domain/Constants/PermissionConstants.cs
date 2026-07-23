namespace OPCBS.Domain.Constants;

/// <summary>
/// Permission code constants for RBAC
/// </summary>
public static class PermissionConstants
{
    // Auth
    public const string ManageOwnProfile = "MANAGE_OWN_PROFILE";

    // Doctor
    public const string ViewDoctors = "VIEW_DOCTORS";
    public const string ManageDoctorProfile = "MANAGE_DOCTOR_PROFILE";
    public const string ManageSchedule = "MANAGE_SCHEDULE";
    public const string ManageDoctorAppointments = "MANAGE_DOCTOR_APPOINTMENTS";
    public const string ManageConsultationRecords = "MANAGE_CONSULTATION_RECORDS";
    public const string ManageTreatmentPackages = "MANAGE_TREATMENT_PACKAGES";
    public const string ManageDoctorBlogs = "MANAGE_DOCTOR_BLOGS";
    public const string PurchaseSubscription = "PURCHASE_SUBSCRIPTION";

    // Patient
    public const string BookAppointment = "BOOK_APPOINTMENT";
    public const string ManageOwnAppointments = "MANAGE_OWN_APPOINTMENTS";
    public const string ViewConsultationHistory = "VIEW_CONSULTATION_HISTORY";
    public const string SubmitReview = "SUBMIT_REVIEW";
    public const string ViewTreatmentPackages = "VIEW_TREATMENT_PACKAGES";

    // Customer Support
    public const string ReviewDoctorVerification = "REVIEW_DOCTOR_VERIFICATION";
    public const string ModerateBlog = "MODERATE_BLOG";
    public const string ViewAllAppointments = "VIEW_ALL_APPOINTMENTS";

    // Business Manager
    public const string ManageServicePackages = "MANAGE_SERVICE_PACKAGES";
    public const string ManageSpecializations = "MANAGE_SPECIALIZATIONS";
    public const string ViewReports = "VIEW_REPORTS";

    // System Admin
    public const string ManageUsers = "MANAGE_USERS";
    public const string ManageRoles = "MANAGE_ROLES";
    public const string ViewAuditLogs = "VIEW_AUDIT_LOGS";
    public const string ManageSystemConfig = "MANAGE_SYSTEM_CONFIG";
}
