namespace OPCBS.Web.Constants;

public static class ApiRoutes
{
    private const string Base = "api/v1";

    // Auth
    public const string Auth = $"{Base}/auth";
    public const string Login = $"{Auth}/login";
    public const string Register = $"{Auth}/register";
    public const string RegisterDoctor = $"{Auth}/register-doctor";
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

    // Patient Records
    public const string PatientRecords = $"{Base}/patient-records";

    // Consultation Records
    public const string ConsultationNotes = $"{Base}/consultation-notes";

    // Treatment Packages
    public const string TreatmentPackages = $"{Base}/treatment-packages";

    // Blogs
    public const string Blogs = $"{Base}/blogs";

    // Reviews
    public const string Reviews = $"{Base}/reviews";

    // Verification
    public const string Verification = $"{Base}/verifications";

    // Service Packages
    public const string ServicePackages = $"{Base}/service-packages";

    // Subscriptions
    public const string Subscriptions = $"{Base}/subscriptions";

    // Payments
    public const string Payments = $"{Base}/payments";

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

    // Psychometrics
    public const string Psychometrics = $"{Base}/psychometrics";

    // Therapy (Assignments & Journals)
    public const string Therapy = $"{Base}/therapy";

    // Favorites
    public const string Favorites = $"{Base}/favorites";

    // Messages
    public const string Messages = $"{Base}/messages";

    // Treatment Cases
    public const string TreatmentCases = $"{Base}/treatment-cases";
}
