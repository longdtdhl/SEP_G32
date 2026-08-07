using Microsoft.EntityFrameworkCore;

namespace OPCBS.Infrastructure.Persistence;

/// <summary>
/// Bridges legacy databases created with EnsureCreated until the application fully adopts EF migrations.
/// Every statement is guarded, additive, and leaves existing records untouched.
/// </summary>
public static class OpcbsSchemaUpgrade
{
    public static async Task ApplyAdditiveUpgradesAsync(OpcbsDbContext context, CancellationToken ct = default)
    {
        if (!context.Database.IsSqlServer()) return;

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'Appointments', N'GuestConfirmationTokenHash') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationTokenHash] nvarchar(64) NULL;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmationLastSentAt') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationLastSentAt] datetime2 NULL;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmationSendCount') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationSendCount] int NOT NULL CONSTRAINT [DF_Appointments_GuestConfirmationSendCount] DEFAULT 0;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmedAt') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmedAt] datetime2 NULL;
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[AppointmentCompletionConfirmations]', N'U') IS NULL
            BEGIN
                CREATE TABLE [AppointmentCompletionConfirmations] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [AppointmentId] uniqueidentifier NOT NULL,
                    [DoctorUserId] uniqueidentifier NOT NULL,
                    [PatientUserId] uniqueidentifier NOT NULL,
                    [Status] int NOT NULL,
                    [RequestedAt] datetime2 NOT NULL,
                    [ReminderDueAt] datetime2 NOT NULL,
                    [EscalationDueAt] datetime2 NOT NULL,
                    [ReminderSentAt] datetime2 NULL,
                    [ConfirmedAt] datetime2 NULL,
                    [LockedAt] datetime2 NULL,
                    [DoctorNote] nvarchar(2000) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL
                );
                CREATE UNIQUE INDEX [IX_AppointmentCompletionConfirmations_AppointmentId] ON [AppointmentCompletionConfirmations]([AppointmentId]);
                CREATE INDEX [IX_AppointmentCompletionConfirmations_Status_ReminderDueAt] ON [AppointmentCompletionConfirmations]([Status], [ReminderDueAt]);
            END
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ViolationReports]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ViolationReports] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [ReporterUserId] uniqueidentifier NULL,
                    [ReportedUserId] uniqueidentifier NOT NULL,
                    [Source] int NOT NULL,
                    [ReasonCategory] int NOT NULL,
                    [ReasonDetail] nvarchar(2000) NOT NULL,
                    [RelatedAppointmentId] uniqueidentifier NULL,
                    [RelatedTreatmentCaseId] uniqueidentifier NULL,
                    [Status] int NOT NULL,
                    [CustomerSupportUserId] uniqueidentifier NULL,
                    [CustomerSupportNote] nvarchar(2000) NULL,
                    [WarningIssuedAt] datetime2 NULL,
                    [WarningNumber] int NOT NULL,
                    [EscalatedAt] datetime2 NULL,
                    [AdminUserId] uniqueidentifier NULL,
                    [AdminNote] nvarchar(2000) NULL,
                    [ResolvedAt] datetime2 NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL
                );
                CREATE INDEX [IX_ViolationReports_ReportedUserId_Status] ON [ViolationReports]([ReportedUserId], [Status]);
                CREATE INDEX [IX_ViolationReports_ReporterUserId_ReasonCategory_Status] ON [ViolationReports]([ReporterUserId], [ReasonCategory], [Status]);
                CREATE INDEX [IX_ViolationReports_CreatedAt] ON [ViolationReports]([CreatedAt]);
            END
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ViolationReportEvidences]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ViolationReportEvidences] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [ViolationReportId] uniqueidentifier NOT NULL,
                    [FileUrl] nvarchar(1000) NOT NULL,
                    [PublicId] nvarchar(500) NOT NULL,
                    [FileName] nvarchar(255) NOT NULL,
                    [ContentType] nvarchar(100) NOT NULL,
                    [FileSizeBytes] bigint NOT NULL,
                    [UploadedByUserId] uniqueidentifier NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [FK_ViolationReportEvidences_ViolationReports_ViolationReportId]
                        FOREIGN KEY ([ViolationReportId]) REFERENCES [ViolationReports]([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_ViolationReportEvidences_ViolationReportId] ON [ViolationReportEvidences]([ViolationReportId]);
            END
            """, ct);
    }
}
