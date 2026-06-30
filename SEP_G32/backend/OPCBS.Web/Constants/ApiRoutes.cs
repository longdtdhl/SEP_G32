namespace OPCBS.Web.Constants;

public static class ApiRoutes
{
    private const string Base = "api/v1";

    // Auth
    public const string Auth = $"{Base}/auth";
    public const string Login = $"{Auth}/login";
    public const string Register = $"{Auth}/register";
    public const string VerifyOtp = $"{Auth}/verify-otp";
    public const string ForgotPassword = $"{Auth}/forgot-password";
    public const string ResetPassword = $"{Auth}/reset-password";
    public const string ChangePassword = $"{Auth}/change-password";
    public const string RefreshToken = $"{Auth}/refresh-token";

    // Users
    public const string Users = $"{Base}/users";
    public const string UserProfile = $"{Users}/profile";

    // Doctors
    public const string Doctors = $"{Base}/doctors";

    // Appointments
    public const string Appointments = $"{Base}/appointments";
    public const string AppointmentTrack = $"{Appointments}/track";

    // Schedules
    public const string Schedules = $"{Base}/schedules";
    public const string ScheduleDaysOff = $"{Schedules}/days-off";

    // Consultation Records
    public const string ConsultationRecords = $"{Base}/consultation-records";

    // Treatment Packages
    public const string TreatmentPackages = $"{Base}/treatment-packages";

    // Blogs
    public const string Blogs = $"{Base}/blogs";

    // Reviews
    public const string Reviews = $"{Base}/reviews";

    // Verification
    public const string Verification = $"{Base}/verification";

    // Service Packages
    public const string ServicePackages = $"{Base}/service-packages";

    // Subscriptions
    public const string Subscriptions = $"{Base}/subscriptions";

    // Notifications
    public const string Notifications = $"{Base}/notifications";

    // Specializations
    public const string Specializations = $"{Base}/specializations";

    // Admin
    public const string AdminUsers = $"{Base}/admin/users";
    public const string AdminRoles = $"{Base}/admin/roles";
    public const string AdminAuditLogs = $"{Base}/admin/audit-logs";
    public const string AdminReports = $"{Base}/admin/reports";
    public const string AdminSettings = $"{Base}/admin/settings";

    // Customer Support
    public const string CSDoctorApplications = $"{Base}/customer-support/doctor-applications";
    public const string CSBlogModeration = $"{Base}/customer-support/blog-moderation";

    // Business Manager
    public const string BMAnalytics = $"{Base}/business-manager/analytics";
    public const string BMReports = $"{Base}/business-manager/reports";
}
