using Microsoft.EntityFrameworkCore;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

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

    // Appointments
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentHistory> AppointmentHistories => Set<AppointmentHistory>();

    // Consultations
    public DbSet<ConsultationRecord> ConsultationRecords => Set<ConsultationRecord>();

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
                .HasDefaultValue(UserStatus.Active);
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
            entity.HasIndex(e => e.DoctorProfileId).IsUnique();
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
            entity.HasOne(e => e.Appointment)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.AppointmentSlotId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.DoctorProfileId, e.SlotDate, e.StartTime }).IsUnique();
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
            entity.Property(e => e.Notes)
                .HasMaxLength(2000);
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000);
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(1000);
            entity.HasIndex(e => e.BookingCode).IsUnique();
            entity.HasOne(e => e.AppointmentSlot)
                .WithOne(s => s.Appointment)
                .HasForeignKey<Appointment>(e => e.AppointmentSlotId)
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
        });

        // ConsultationRecord
        modelBuilder.Entity<ConsultationRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.ConsultationRecord)
                .HasForeignKey<ConsultationRecord>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.ConsultationRecords)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.ConsultationRecords)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.ConsultationSummary)
                .IsRequired()
                .HasMaxLength(5000);
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(2000);
            entity.Property(e => e.Recommendation)
                .HasMaxLength(5000);
            entity.Property(e => e.FollowUpNotes)
                .HasMaxLength(5000);
            entity.Property(e => e.Prescription)
                .HasMaxLength(2000);
        });
    }

    private static void ConfigurePackageEntities(ModelBuilder modelBuilder)
    {
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
                .IsRequired()
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
}
