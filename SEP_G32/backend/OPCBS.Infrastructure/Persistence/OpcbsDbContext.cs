using Microsoft.EntityFrameworkCore;
using OPCBS.Domain.Common;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using System.Linq.Expressions;

namespace OPCBS.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for OPCBS application
/// Manages all entity configurations and database interactions
/// </summary>
public class OpcbsDbContext : DbContext
{
    public OpcbsDbContext(DbContextOptions<OpcbsDbContext> options) : base(options)
    {
    }

    #region DbSet Properties

    // Identity & Access
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    // Profiles
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();

    // Doctor Professional
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();

    // Schedule
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<DoctorDayOff> DoctorDayOffs => Set<DoctorDayOff>();
    public DbSet<ScheduleNote> ScheduleNotes => Set<ScheduleNote>();

    // Appointments
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentHistory> AppointmentHistories => Set<AppointmentHistory>();
    public DbSet<AppointmentCompletionConfirmation> AppointmentCompletionConfirmations => Set<AppointmentCompletionConfirmation>();

    // Consultations & Patient Records
    public DbSet<PatientRecord> PatientRecords => Set<PatientRecord>();
    public DbSet<ConsultationNote> ConsultationNotes => Set<ConsultationNote>();
    public DbSet<CustomClinicalField> CustomClinicalFields => Set<CustomClinicalField>();

    // Packages
    public DbSet<TreatmentPackage> TreatmentPackages => Set<TreatmentPackage>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<DoctorSubscription> DoctorSubscriptions => Set<DoctorSubscription>();

    // Payments
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    // Blog & Reviews
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogComment> BlogComments => Set<BlogComment>();
    public DbSet<Review> Reviews => Set<Review>();

    // System
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<ViolationReport> ViolationReports => Set<ViolationReport>();
    public DbSet<ViolationReportEvidence> ViolationReportEvidences => Set<ViolationReportEvidence>();

    // Psychometrics
    public DbSet<PsychometricTest> PsychometricTests => Set<PsychometricTest>();
    public DbSet<PsychometricQuestion> PsychometricQuestions => Set<PsychometricQuestion>();
    public DbSet<PsychometricSubmission> PsychometricSubmissions => Set<PsychometricSubmission>();
    public DbSet<PsychometricAnswer> PsychometricAnswers => Set<PsychometricAnswer>();

    // Therapy Features
    public DbSet<TherapyAssignment> TherapyAssignments => Set<TherapyAssignment>();
    public DbSet<EmotionJournal> EmotionJournals => Set<EmotionJournal>();

    // Treatment Management
    public DbSet<TreatmentCase> TreatmentCases => Set<TreatmentCase>();
    public DbSet<TreatmentSession> TreatmentSessions => Set<TreatmentSession>();
    public DbSet<TreatmentGoal> TreatmentGoals => Set<TreatmentGoal>();
    public DbSet<GoalDetail> GoalDetails => Set<GoalDetail>();
    public DbSet<GoalSuccessCriteria> GoalSuccessCriteria => Set<GoalSuccessCriteria>();
    public DbSet<SuccessCriteriaEvaluation> SuccessCriteriaEvaluations => Set<SuccessCriteriaEvaluation>();
    public DbSet<MoodEntry> MoodEntries => Set<MoodEntry>();
    public DbSet<TreatmentGoalProgress> TreatmentGoalProgresses => Set<TreatmentGoalProgress>();
    public DbSet<TreatmentSessionGoal> TreatmentSessionGoals => Set<TreatmentSessionGoal>();

    // Favorites
    public DbSet<FavoriteDoctor> FavoriteDoctors => Set<FavoriteDoctor>();

    // Messaging
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        ConfigureIdentityEntities(modelBuilder);
        ConfigureProfileEntities(modelBuilder);
        ConfigureAppointmentEntities(modelBuilder);
        ConfigurePackageEntities(modelBuilder);
        ConfigureBlogAndNotificationEntities(modelBuilder);
        ConfigureSystemEntities(modelBuilder);
        ConfigurePsychometricEntities(modelBuilder);
        ConfigureFavoriteEntities(modelBuilder);
        ConfigureMessagingEntities(modelBuilder);
        ConfigureTreatmentCaseEntities(modelBuilder);

        // Targeted query filters: exclude soft-deleted slots and appointments
        // (Not applied globally to avoid breaking required navigation properties
        //  e.g. AppointmentHistory -> Appointment, OtpVerification -> User)
        modelBuilder.Entity<AppointmentSlot>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScheduleNote>().HasQueryFilter(e => !e.IsDeleted);
    }

    private static void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasMany(e => e.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            // Unique constraint on RoleId + PermissionId
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasDefaultValue(UserStatus.Active)
                .HasSentinel((UserStatus)(-1));
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PatientProfile)
                .WithOne(p => p.User)
                .HasForeignKey<PatientProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DoctorProfile)
                .WithOne(d => d.User)
                .HasForeignKey<DoctorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OtpVerification
        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10);
            entity.HasOne(e => e.User)
                .WithMany(u => u.OtpVerifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProfileEntities(ModelBuilder modelBuilder)
    {
        // PatientProfile
        modelBuilder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Address)
                .HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName)
                .HasMaxLength(255);
            entity.Property(e => e.EmergencyContactPhone)
                .HasMaxLength(20);
        });

        // Specialization
        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // DoctorProfile
        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<DoctorProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.ProfessionalTitle)
                .HasMaxLength(255);
            entity.Property(e => e.Biography)
                .HasMaxLength(5000);
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(100);
            entity.Property(e => e.VerificationStatus)
                .HasDefaultValue(VerificationStatus.Draft);
            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValue(0m);
        });

        // DoctorSpecialization
        modelBuilder.Entity<DoctorSpecialization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.DoctorSpecializations)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Specialization)
                .WithMany(s => s.DoctorSpecializations)
                .HasForeignKey(e => e.SpecializationId)
                .OnDelete(DeleteBehavior.Cascade);
            // Unique constraint on Doctor + Specialization
            entity.HasIndex(e => new { e.DoctorProfileId, e.SpecializationId }).IsUnique();
        });

        // Certificate
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileUrl)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Name)
                .HasMaxLength(255);
            entity.Property(e => e.IssuingOrganization)
                .HasMaxLength(255);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.Certificates)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // VerificationRequest
        modelBuilder.Entity<VerificationRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.VerificationRequests)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000);
            entity.HasIndex(e => e.DoctorProfileId);
        });
    }

    private static void ConfigureAppointmentEntities(ModelBuilder modelBuilder)
    {
        // Schedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.Schedules)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DoctorDayOff
        modelBuilder.Entity<DoctorDayOff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.DayOffs)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Reason)
                .HasMaxLength(500);
        });

        // AppointmentSlot
        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.AppointmentSlots)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => new { e.DoctorProfileId, e.SlotDate, e.StartTime })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<ScheduleNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.AppointmentSlot)
                .WithMany(s => s.ScheduleNotes)
                .HasForeignKey(e => e.AppointmentSlotId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.AppointmentSlotId);
            entity.HasIndex(e => new { e.DoctorProfileId, e.NoteDate });
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingCode)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.GuestName)
                .HasMaxLength(255);
            entity.Property(e => e.GuestEmail)
                .HasMaxLength(255);
              entity.Property(e => e.GuestPhoneNumber)
                  .HasMaxLength(20);
              entity.Property(e => e.GuestZaloNumber)
                  .HasMaxLength(20);
            entity.Property(e => e.GuestConfirmationTokenHash)
                .HasMaxLength(64);
            entity.Property(e => e.GuestActionTokenHash)
                .HasMaxLength(64);
            entity.Property(e => e.Notes)
                .HasMaxLength(2000);
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000);
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(1000);
            entity.HasIndex(e => e.BookingCode).IsUnique();
            entity.HasOne(e => e.AppointmentSlot)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.AppointmentSlotId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.TreatmentPackage)
                .WithMany(tp => tp.Appointments)
                .HasForeignKey(e => e.TreatmentPackageId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.TreatmentCase)
                .WithMany()
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ProposedSlot)
                .WithMany()
                .HasForeignKey(e => e.ProposedSlotId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AppointmentHistory
        modelBuilder.Entity<AppointmentHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.HistoryEntries)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Reason)
                .HasMaxLength(500);
            entity.Property(e => e.ChangedByRole)
                .HasMaxLength(32);
        });

        modelBuilder.Entity<AppointmentCompletionConfirmation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasIndex(e => new { e.Status, e.ReminderDueAt });
            entity.Property(e => e.DoctorNote).HasMaxLength(2000);
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.GuestTokenHash).HasMaxLength(64);
            entity.Property(e => e.DisputeReason).HasMaxLength(2000);
        });
    }

    private static void ConfigurePackageEntities(ModelBuilder modelBuilder)
    {
        // ConsultationNote
        modelBuilder.Entity<ConsultationNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.ConsultationNote)
                .HasForeignKey<ConsultationNote>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
            entity.HasOne(e => e.NextAppointmentRecommendedSlot)
                .WithMany()
                .HasForeignKey(e => e.NextAppointmentRecommendedSlotId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            entity.HasOne(e => e.FollowUpAppointment)
                .WithMany()
                .HasForeignKey(e => e.FollowUpAppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PatientRecord)
                .WithMany(p => p.ConsultationNotes)
                .HasForeignKey(e => e.PatientRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.ConsultationSummary)
                .IsRequired()
                .HasMaxLength(5000);
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(2000);
            entity.Property(e => e.Recommendation)
                .HasMaxLength(5000);
            entity.Property(e => e.FollowUpNotes)
                .HasMaxLength(5000);
            entity.Property(e => e.TherapyPlan)
                .HasMaxLength(2000);
            entity.Property(e => e.IsPatientConfirmed)
                .HasDefaultValue(false);
            entity.Property(e => e.PatientConfirmedAt)
                .IsRequired(false);
            entity.Property(e => e.PatientConfirmedById)
                .IsRequired(false);
            entity.Property(e => e.LastEditedAt)
                .IsRequired(false);
            entity.Property(e => e.LastEditedByDoctorId)
                .IsRequired(false);
        });

        // PatientRecord
        modelBuilder.Entity<PatientRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
            entity.Property(e => e.GuestName).HasMaxLength(200);
            entity.Property(e => e.GuestPhone).HasMaxLength(20);
            entity.Property(e => e.GuestEmail).HasMaxLength(200);
            entity.Property(e => e.PsychologicalHistory).HasMaxLength(5000);
            entity.Property(e => e.CurrentSymptoms).HasMaxLength(2000);
            entity.Property(e => e.StressFactors).HasMaxLength(2000);
            entity.Property(e => e.GeneralNotes).HasMaxLength(5000);
        });

        // CustomClinicalField
        modelBuilder.Entity<CustomClinicalField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OwnerType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SectionKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).HasMaxLength(5000);
            entity.Property(e => e.FieldType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.OwnerType, e.OwnerId });
            entity.HasIndex(e => new { e.OwnerId, e.SectionKey, e.OrderIndex });
        });

        // ServicePackage
        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TreatmentPackage
        modelBuilder.Entity<TreatmentPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.TreatmentPackages)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.TreatmentPackages)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // DoctorSubscription
        modelBuilder.Entity<DoctorSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(1000);
            entity.HasOne(e => e.DoctorProfile)
                .WithMany(d => d.Subscriptions)
                .HasForeignKey(e => e.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ServicePackage)
                .WithMany(sp => sp.DoctorSubscriptions)
                .HasForeignKey(e => e.ServicePackageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentTransaction)
                .WithOne(pt => pt.DoctorSubscription)
                .HasForeignKey<PaymentTransaction>(pt => pt.DoctorSubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PaymentTransaction
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionCode)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ResponseCode)
                .HasMaxLength(50);
            entity.Property(e => e.ResponseMessage)
                .HasMaxLength(500);
            entity.HasIndex(e => e.TransactionCode).IsUnique();
        });
    }

    private static void ConfigureBlogAndNotificationEntities(ModelBuilder modelBuilder)
    {
        // BlogPost
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Content)
                .IsRequired();
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500);
            entity.Property(e => e.Excerpt)
                .HasMaxLength(1000);
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.BlogPosts)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BlogComment
        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorName)
                .HasMaxLength(255);
            entity.Property(e => e.AuthorEmail)
                .HasMaxLength(255);
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(2000);
            entity.HasOne(e => e.BlogPost)
                .WithMany(b => b.Comments)
                .HasForeignKey(e => e.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.BlogComments)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment)
                .HasMaxLength(2000);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.Review)
                .HasForeignKey<Review>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.Reviews)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
            // Unique constraint on Appointment (one review per appointment)
            entity.HasIndex(e => e.AppointmentId).IsUnique();
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(100);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.ActionDescription)
                .HasMaxLength(1000);
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.EntityName, e.EntityId });
        });
    }

    private static void ConfigureSystemEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ViolationReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReasonDetail).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.CustomerSupportNote).HasMaxLength(2000);
            entity.Property(e => e.AdminNote).HasMaxLength(2000);
            entity.HasIndex(e => new { e.ReportedUserId, e.Status });
            entity.HasIndex(e => new { e.ReporterUserId, e.ReasonCategory, e.Status });
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<ViolationReportEvidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.PublicId).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.ViolationReportId);
            entity.HasOne(e => e.ViolationReport)
                .WithMany(r => r.EvidenceFiles)
                .HasForeignKey(e => e.ViolationReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SystemConfig
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Value)
                .IsRequired();
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            entity.Property(e => e.DataType)
                .HasMaxLength(50);
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }

    private static void ConfigurePsychometricEntities(ModelBuilder modelBuilder)
    {
        // PsychometricTest
        modelBuilder.Entity<PsychometricTest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.TestType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TestType).IsUnique();
        });

        // PsychometricQuestion
        modelBuilder.Entity<PsychometricQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.HasOne(e => e.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(e => e.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PsychometricSubmission
        modelBuilder.Entity<PsychometricSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScoreDataJson).IsRequired();
            entity.Property(e => e.Interpretation).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Test)
                .WithMany(t => t.Submissions)
                .HasForeignKey(e => e.TestId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PsychometricAnswer
        modelBuilder.Entity<PsychometricAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Submission)
                .WithMany(s => s.Answers)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFavoriteEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FavoriteDoctor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PatientId, e.DoctorId }).IsUnique();

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .HasPrincipalKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMessagingEntities(ModelBuilder modelBuilder)
    {
        // Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PatientId, e.DoctorId });

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .HasPrincipalKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TreatmentPackage)
                .WithMany()
                .HasForeignKey(e => e.TreatmentPackageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Message (ImmutableEntity - no soft delete)
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.SenderId);

            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(2048);

            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTreatmentCaseEntities(ModelBuilder modelBuilder)
    {
        // TreatmentCase
        modelBuilder.Entity<TreatmentCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DoctorId, e.PatientId, e.Status });
            entity.HasIndex(e => e.TreatmentPackageId);

            entity.Property(e => e.CaseName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.CaseDescription).HasMaxLength(2000);
            entity.Property(e => e.PrimaryConcern).HasMaxLength(1000);
            entity.Property(e => e.ClosureNote).HasMaxLength(2000);

            entity.HasOne(e => e.TreatmentPackage)
                .WithMany(tp => tp.TreatmentCases)
                .HasForeignKey(e => e.TreatmentPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.TreatmentCases)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.TreatmentCases)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TreatmentSession
        modelBuilder.Entity<TreatmentSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TreatmentCaseId);
            entity.HasIndex(e => e.AppointmentId)
                .IsUnique()
                .HasFilter("[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.TreatmentCaseId, e.SessionNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.Property(e => e.SessionSummary).HasMaxLength(4000);
            entity.Property(e => e.TherapistNotes).HasMaxLength(4000);
            entity.Property(e => e.PatientFeedback).HasMaxLength(2000);
            entity.Property(e => e.HomeworkAssigned).HasMaxLength(2000);

            entity.HasOne(e => e.TreatmentCase)
                .WithMany(tc => tc.Sessions)
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.TreatmentSession)
                .HasForeignKey<TreatmentSession>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TreatmentGoal
        modelBuilder.Entity<TreatmentGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TreatmentCaseId);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.DoctorNotes).HasMaxLength(2000);

            entity.HasOne(e => e.TreatmentCase)
                .WithMany(tc => tc.Goals)
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GoalDetail
        modelBuilder.Entity<GoalDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.GoalId, e.OrderIndex });
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Objective).HasMaxLength(2000);
            entity.Property(e => e.ExpectedOutcome).HasMaxLength(2000);
            entity.HasOne(e => e.Goal).WithMany(g => g.Details)
                .HasForeignKey(e => e.GoalId).OnDelete(DeleteBehavior.Restrict);
        });

        // GoalSuccessCriteria and its immutable evaluations
        modelBuilder.Entity<GoalSuccessCriteria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GoalId);
            entity.Property(e => e.Weight).HasPrecision(8, 2);
            entity.Property(e => e.TargetValue).HasPrecision(18, 4);
            entity.Property(e => e.CurrentValue).HasPrecision(18, 4);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Goal).WithMany(g => g.SuccessCriteria)
                .HasForeignKey(e => e.GoalId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SuccessCriteriaEvaluation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SuccessCriteriaId, e.EvaluatedAt });
            entity.Property(e => e.CurrentValue).HasPrecision(18, 4);
            entity.HasOne(e => e.SuccessCriteria).WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.SuccessCriteriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TreatmentSession).WithMany()
                .HasForeignKey(e => e.TreatmentSessionId).OnDelete(DeleteBehavior.SetNull);
        });

        // TherapyAssignment -> TreatmentCase (optional FK) & TreatmentSession
        modelBuilder.Entity<TherapyAssignment>(entity =>
        {
            entity.HasOne(e => e.TreatmentCase)
                .WithMany(tc => tc.Assignments)
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TreatmentSession)
                .WithMany(ts => ts.Assignments)
                .HasForeignKey(e => e.TreatmentSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MoodEntry
        modelBuilder.Entity<MoodEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TreatmentCaseId);
            entity.HasIndex(e => e.PatientId);

            entity.HasOne(e => e.TreatmentCase)
                .WithMany(tc => tc.MoodEntries)
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TreatmentGoalProgress
        modelBuilder.Entity<TreatmentGoalProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GoalId);

            entity.HasOne(e => e.Goal)
                .WithMany(g => g.ProgressHistory)
                .HasForeignKey(e => e.GoalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TreatmentSession)
                .WithMany()
                .HasForeignKey(e => e.TreatmentSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GoalDetail)
                .WithMany(d => d.ProgressHistory)
                .HasForeignKey(e => e.GoalDetailId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TreatmentSessionGoal (Many-to-Many Join)
        modelBuilder.Entity<TreatmentSessionGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TreatmentSessionId, e.GoalDetailId }).IsUnique();

            entity.HasOne(e => e.TreatmentSession)
                .WithMany(s => s.SessionGoals)
                .HasForeignKey(e => e.TreatmentSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GoalDetail)
                .WithMany(d => d.SessionGoals)
                .HasForeignKey(e => e.GoalDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmotionJournal -> TreatmentCase (optional FK)
        modelBuilder.Entity<EmotionJournal>(entity =>
        {
            entity.HasOne(e => e.TreatmentCase)
                .WithMany()
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PsychometricSubmission -> TreatmentCase (optional FK)
        modelBuilder.Entity<PsychometricSubmission>(entity =>
        {
            entity.HasOne(e => e.TreatmentCase)
                .WithMany()
                .HasForeignKey(e => e.TreatmentCaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
