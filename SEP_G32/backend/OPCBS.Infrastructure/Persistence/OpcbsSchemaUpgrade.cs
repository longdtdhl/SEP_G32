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
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationRequestedByUserId') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationRequestedByUserId] uniqueidentifier NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationRequestedAt') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationRequestedAt] datetime2 NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationReason') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationReason] nvarchar(1000) NULL;
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

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ScheduleNotes]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ScheduleNotes] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [DoctorProfileId] uniqueidentifier NOT NULL,
                    [NoteDate] date NOT NULL,
                    [StartTime] time NULL,
                    [EndTime] time NULL,
                    [Title] nvarchar(200) NOT NULL,
                    [Content] nvarchar(max) NOT NULL,
                    [Category] nvarchar(50) NOT NULL,
                    [PatientId] uniqueidentifier NULL,
                    [TreatmentCaseId] uniqueidentifier NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL
                );
                CREATE INDEX [IX_ScheduleNotes_DoctorProfileId_NoteDate] ON [ScheduleNotes]([DoctorProfileId], [NoteDate]);
            END
            """, ct);

        // Treatment-goal expansion is additive. It preserves existing goal/session links by
        // materializing a legacy detail for each old goal before switching the link target.
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'TreatmentGoals', N'TemplateId') IS NULL
                ALTER TABLE [TreatmentGoals] ADD [TemplateId] uniqueidentifier NULL;
            IF COL_LENGTH(N'TreatmentGoals', N'OrderIndex') IS NULL
                ALTER TABLE [TreatmentGoals] ADD [OrderIndex] int NOT NULL CONSTRAINT [DF_TreatmentGoals_OrderIndex] DEFAULT 0;
            IF COL_LENGTH(N'TreatmentGoals', N'StartDate') IS NULL
                ALTER TABLE [TreatmentGoals] ADD [StartDate] datetime2 NULL;

            IF OBJECT_ID(N'[GoalDetails]', N'U') IS NULL
            BEGIN
                CREATE TABLE [GoalDetails] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [GoalId] uniqueidentifier NOT NULL,
                    [Title] nvarchar(300) NOT NULL,
                    [Description] nvarchar(2000) NULL,
                    [Objective] nvarchar(2000) NULL,
                    [ExpectedOutcome] nvarchar(2000) NULL,
                    [OrderIndex] int NOT NULL,
                    [ProgressPercent] int NOT NULL,
                    [Status] int NOT NULL,
                    [EstimatedSessions] int NULL,
                    [CompletedDate] datetime2 NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [FK_GoalDetails_TreatmentGoals_GoalId]
                        FOREIGN KEY ([GoalId]) REFERENCES [TreatmentGoals]([Id])
                );
                CREATE INDEX [IX_GoalDetails_GoalId_OrderIndex] ON [GoalDetails]([GoalId], [OrderIndex]);
            END

            IF OBJECT_ID(N'[GoalSuccessCriteria]', N'U') IS NULL
            BEGIN
                CREATE TABLE [GoalSuccessCriteria] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [GoalId] uniqueidentifier NOT NULL,
                    [CriteriaType] int NOT NULL,
                    [DataSource] int NOT NULL,
                    [Operator] int NOT NULL,
                    [TargetValue] decimal(18,4) NULL,
                    [CurrentValue] decimal(18,4) NULL,
                    [Weight] decimal(8,2) NOT NULL,
                    [IsRequired] bit NOT NULL,
                    [Description] nvarchar(1000) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [FK_GoalSuccessCriteria_TreatmentGoals_GoalId]
                        FOREIGN KEY ([GoalId]) REFERENCES [TreatmentGoals]([Id])
                );
                CREATE INDEX [IX_GoalSuccessCriteria_GoalId] ON [GoalSuccessCriteria]([GoalId]);
            END

            IF OBJECT_ID(N'[SuccessCriteriaEvaluations]', N'U') IS NULL
            BEGIN
                CREATE TABLE [SuccessCriteriaEvaluations] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [SuccessCriteriaId] uniqueidentifier NOT NULL,
                    [TreatmentSessionId] uniqueidentifier NULL,
                    [CurrentValue] decimal(18,4) NULL,
                    [IsPassed] bit NOT NULL,
                    [EvaluatedAt] datetime2 NOT NULL,
                    [EvaluatedBy] uniqueidentifier NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    CONSTRAINT [FK_SuccessCriteriaEvaluations_GoalSuccessCriteria_SuccessCriteriaId]
                        FOREIGN KEY ([SuccessCriteriaId]) REFERENCES [GoalSuccessCriteria]([Id]),
                    CONSTRAINT [FK_SuccessCriteriaEvaluations_TreatmentSessions_TreatmentSessionId]
                        FOREIGN KEY ([TreatmentSessionId]) REFERENCES [TreatmentSessions]([Id]) ON DELETE SET NULL
                );
                CREATE INDEX [IX_SuccessCriteriaEvaluations_SuccessCriteriaId_EvaluatedAt]
                    ON [SuccessCriteriaEvaluations]([SuccessCriteriaId], [EvaluatedAt]);
            END

            IF COL_LENGTH(N'TreatmentGoalProgresses', N'GoalDetailId') IS NULL
            BEGIN
                ALTER TABLE [TreatmentGoalProgresses] ADD [GoalDetailId] uniqueidentifier NULL;
                ALTER TABLE [TreatmentGoalProgresses] ADD CONSTRAINT [FK_TreatmentGoalProgresses_GoalDetails_GoalDetailId]
                    FOREIGN KEY ([GoalDetailId]) REFERENCES [GoalDetails]([Id]) ON DELETE SET NULL;
            END
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[TreatmentSessionGoals]', N'U') IS NOT NULL
               AND COL_LENGTH(N'TreatmentSessionGoals', N'TreatmentGoalId') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_TreatmentSessionGoals_GoalDetails_GoalDetailId')
            BEGIN
                IF COL_LENGTH(N'TreatmentSessionGoals', N'GoalDetailId') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [GoalDetailId] uniqueidentifier NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'Id') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [Id] uniqueidentifier NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'OrderIndex') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [OrderIndex] int NOT NULL CONSTRAINT [DF_TreatmentSessionGoals_OrderIndex] DEFAULT 0;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'PlannedActivity') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [PlannedActivity] nvarchar(max) NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'CreatedAt') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_TreatmentSessionGoals_CreatedAt] DEFAULT SYSUTCDATETIME();
                IF COL_LENGTH(N'TreatmentSessionGoals', N'UpdatedAt') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [UpdatedAt] datetime2 NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'CreatedBy') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [CreatedBy] uniqueidentifier NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'UpdatedBy') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [UpdatedBy] uniqueidentifier NULL;
                IF COL_LENGTH(N'TreatmentSessionGoals', N'IsDeleted') IS NULL
                    ALTER TABLE [TreatmentSessionGoals] ADD [IsDeleted] bit NOT NULL CONSTRAINT [DF_TreatmentSessionGoals_IsDeleted] DEFAULT 0;
            END
            """, ct);

        // This must run as a new SQL batch: SQL Server binds column names before it executes ALTER TABLE.
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[TreatmentSessionGoals]', N'U') IS NOT NULL
               AND COL_LENGTH(N'TreatmentSessionGoals', N'TreatmentGoalId') IS NOT NULL
               AND COL_LENGTH(N'TreatmentSessionGoals', N'GoalDetailId') IS NOT NULL
               AND COL_LENGTH(N'TreatmentSessionGoals', N'Id') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_TreatmentSessionGoals_GoalDetails_GoalDetailId')
            BEGIN
                INSERT INTO [GoalDetails] ([Id], [GoalId], [Title], [OrderIndex], [ProgressPercent], [Status], [CreatedAt], [IsDeleted])
                SELECT NEWID(), g.[Id], N'Legacy session linkage', 0, g.[ProgressPercent],
                    CASE WHEN g.[ProgressPercent] >= 100 THEN 2 WHEN g.[ProgressPercent] > 0 THEN 1 ELSE 0 END,
                    SYSUTCDATETIME(), 0
                FROM [TreatmentGoals] g
                WHERE NOT EXISTS (SELECT 1 FROM [GoalDetails] d WHERE d.[GoalId] = g.[Id] AND d.[Title] = N'Legacy session linkage');

                UPDATE link SET [GoalDetailId] = detail.[Id]
                FROM [TreatmentSessionGoals] link
                INNER JOIN [GoalDetails] detail ON detail.[GoalId] = link.[TreatmentGoalId] AND detail.[Title] = N'Legacy session linkage'
                WHERE link.[GoalDetailId] IS NULL;
                UPDATE [TreatmentSessionGoals] SET [Id] = NEWID() WHERE [Id] IS NULL;

                IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE [name] = N'PK_TreatmentSessionGoals')
                    ALTER TABLE [TreatmentSessionGoals] DROP CONSTRAINT [PK_TreatmentSessionGoals];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_TreatmentSessionGoals_TreatmentGoals_TreatmentGoalId')
                    ALTER TABLE [TreatmentSessionGoals] DROP CONSTRAINT [FK_TreatmentSessionGoals_TreatmentGoals_TreatmentGoalId];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_TreatmentSessionGoals_TreatmentGoalId' AND [object_id] = OBJECT_ID(N'[TreatmentSessionGoals]'))
                    DROP INDEX [IX_TreatmentSessionGoals_TreatmentGoalId] ON [TreatmentSessionGoals];
                ALTER TABLE [TreatmentSessionGoals] ALTER COLUMN [TreatmentGoalId] uniqueidentifier NULL;
                ALTER TABLE [TreatmentSessionGoals] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
                ALTER TABLE [TreatmentSessionGoals] ADD CONSTRAINT [PK_TreatmentSessionGoals] PRIMARY KEY ([Id]);
                ALTER TABLE [TreatmentSessionGoals] ADD CONSTRAINT [FK_TreatmentSessionGoals_GoalDetails_GoalDetailId]
                    FOREIGN KEY ([GoalDetailId]) REFERENCES [GoalDetails]([Id]);
                CREATE UNIQUE INDEX [IX_TreatmentSessionGoals_TreatmentSessionId_GoalDetailId]
                    ON [TreatmentSessionGoals]([TreatmentSessionId], [GoalDetailId]);
            END
            """, ct);
    }
}
