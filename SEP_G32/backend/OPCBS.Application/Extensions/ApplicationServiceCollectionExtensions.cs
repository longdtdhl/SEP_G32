using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using System.Reflection;

namespace OPCBS.Application.Extensions;

/// <summary>
/// Dependency injection extension for Application layer
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper (v16+ API)
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IConsultationNoteService, ConsultationNoteService>();
        services.AddScoped<IPatientRecordService, PatientRecordService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<ITreatmentPackageService, Services.TreatmentPackageService>();
        services.AddScoped<IServicePackageService, ServicePackageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPsychometricService, PsychometricService>();
        services.AddScoped<ITherapyAssignmentService, TherapyAssignmentService>();
        services.AddScoped<IEmotionJournalService, EmotionJournalService>();
        services.AddScoped<IFavoriteDoctorService, FavoriteDoctorService>();
        services.AddScoped<IFavoriteDoctorNotificationService, FavoriteDoctorNotificationService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<ITreatmentCaseService, TreatmentCaseService>();
        services.AddScoped<IViolationReportService, ViolationReportService>();
        services.AddScoped<IDoctorRevenueService, DoctorRevenueService>();

        return services;
    }
}
