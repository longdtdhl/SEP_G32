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
            IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END

            IF COL_LENGTH(N'Appointments', N'GuestConfirmationTokenHash') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationTokenHash] nvarchar(64) NULL;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmationLastSentAt') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationLastSentAt] datetime2 NULL;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmationSendCount') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmationSendCount] int NOT NULL CONSTRAINT [DF_Appointments_GuestConfirmationSendCount] DEFAULT 0;
            IF COL_LENGTH(N'Appointments', N'GuestConfirmedAt') IS NULL
                ALTER TABLE [Appointments] ADD [GuestConfirmedAt] datetime2 NULL;
            IF COL_LENGTH(N'Appointments', N'GuestActionTokenHash') IS NULL
                ALTER TABLE [Appointments] ADD [GuestActionTokenHash] nvarchar(64) NULL;

            -- A slot keeps booking history, so AppointmentSlotId must not remain unique.
            DECLARE @slotAppointmentIndex sysname;
            DECLARE @isUniqueConstraint bit;
            DECLARE @dropSql nvarchar(500);
            SELECT TOP (1) @slotAppointmentIndex = i.[name], @isUniqueConstraint = i.[is_unique_constraint]
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON ic.[object_id] = i.[object_id] AND ic.[index_id] = i.[index_id]
            INNER JOIN sys.columns c ON c.[object_id] = ic.[object_id] AND c.[column_id] = ic.[column_id]
            WHERE i.[object_id] = OBJECT_ID(N'[Appointments]')
              AND i.[is_unique] = 1
              AND i.[is_primary_key] = 0
              AND c.[name] = N'AppointmentSlotId';
            IF @slotAppointmentIndex IS NOT NULL
            BEGIN
                IF @isUniqueConstraint = 1
                    SET @dropSql = N'ALTER TABLE [Appointments] DROP CONSTRAINT ' + QUOTENAME(@slotAppointmentIndex) + N';';
                ELSE
                    SET @dropSql = N'DROP INDEX ' + QUOTENAME(@slotAppointmentIndex) + N' ON [Appointments];';
                EXEC sp_executesql @dropSql;
            END

            IF COL_LENGTH(N'Appointments', N'GuestZaloNumber') IS NULL
                ALTER TABLE [Appointments] ADD [GuestZaloNumber] nvarchar(20) NULL;
            IF COL_LENGTH(N'Appointments', N'AppointmentDate') IS NULL
                ALTER TABLE [Appointments] ADD [AppointmentDate] datetime2 NULL;
            IF COL_LENGTH(N'Appointments', N'ConsultationMode') IS NULL
                ALTER TABLE [Appointments] ADD [ConsultationMode] int NOT NULL CONSTRAINT [DF_Appointments_ConsultationMode] DEFAULT 0;

            IF COL_LENGTH(N'Schedules', N'ConsultationMode') IS NULL
                ALTER TABLE [Schedules] ADD [ConsultationMode] int NOT NULL CONSTRAINT [DF_Schedules_ConsultationMode] DEFAULT 2;

            IF COL_LENGTH(N'AppointmentSlots', N'RowVersion') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [RowVersion] rowversion NOT NULL;
            IF COL_LENGTH(N'AppointmentSlots', N'ConsultationMode') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [ConsultationMode] int NOT NULL CONSTRAINT [DF_AppointmentSlots_ConsultationMode] DEFAULT 2;
            IF COL_LENGTH(N'AppointmentSlots', N'PreAppointmentNoteTitle') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [PreAppointmentNoteTitle] nvarchar(200) NULL;
            IF COL_LENGTH(N'AppointmentSlots', N'IsPreAppointmentNoteRequired') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [IsPreAppointmentNoteRequired] bit NOT NULL CONSTRAINT [DF_AppointmentSlots_IsPreAppointmentNoteRequired] DEFAULT 0;
            IF COL_LENGTH(N'AppointmentSlots', N'Price') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [Price] decimal(18,2) NULL;
            IF COL_LENGTH(N'AppointmentSlots', N'Notes') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [Notes] nvarchar(max) NULL;
            IF COL_LENGTH(N'AppointmentSlots', N'MaxPatients') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [MaxPatients] int NOT NULL CONSTRAINT [DF_AppointmentSlots_MaxPatients] DEFAULT 1;
            IF COL_LENGTH(N'AppointmentSlots', N'CurrentBookings') IS NULL
                ALTER TABLE [AppointmentSlots] ADD [CurrentBookings] int NOT NULL CONSTRAINT [DF_AppointmentSlots_CurrentBookings] DEFAULT 0;

            IF COL_LENGTH(N'EmotionJournals', N'SleepHours') IS NULL
                ALTER TABLE [EmotionJournals] ADD [SleepHours] decimal(4,1) NULL;
            IF COL_LENGTH(N'EmotionJournals', N'DepressionScale') IS NULL
                ALTER TABLE [EmotionJournals] ADD [DepressionScale] int NULL;

            IF COL_LENGTH(N'PsychometricTests', N'Category') IS NULL
                ALTER TABLE [PsychometricTests] ADD [Category] nvarchar(100) NULL;
            IF COL_LENGTH(N'PsychometricTests', N'Purpose') IS NULL
                ALTER TABLE [PsychometricTests] ADD [Purpose] nvarchar(255) NULL;
            IF COL_LENGTH(N'PsychometricTests', N'DoctorId') IS NULL
                ALTER TABLE [PsychometricTests] ADD [DoctorId] uniqueidentifier NULL;
            IF COL_LENGTH(N'PsychometricTests', N'ScoreRangesJson') IS NULL
                ALTER TABLE [PsychometricTests] ADD [ScoreRangesJson] nvarchar(max) NULL;
            IF COL_LENGTH(N'PsychometricTests', N'IsActive') IS NULL
                ALTER TABLE [PsychometricTests] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_PsychometricTests_IsActive] DEFAULT 1;

            IF COL_LENGTH(N'PsychometricQuestions', N'QuestionType') IS NULL
                ALTER TABLE [PsychometricQuestions] ADD [QuestionType] nvarchar(50) NOT NULL CONSTRAINT [DF_PsychometricQuestions_QuestionType] DEFAULT 'Rating1To5';
            IF COL_LENGTH(N'PsychometricQuestions', N'OptionsJson') IS NULL
                ALTER TABLE [PsychometricQuestions] ADD [OptionsJson] nvarchar(max) NULL;

            IF COL_LENGTH(N'PsychometricSubmissions', N'AssignedByDoctorId') IS NULL
                ALTER TABLE [PsychometricSubmissions] ADD [AssignedByDoctorId] uniqueidentifier NULL;
            IF COL_LENGTH(N'PsychometricSubmissions', N'DoctorNotes') IS NULL
                ALTER TABLE [PsychometricSubmissions] ADD [DoctorNotes] nvarchar(max) NULL;
            IF COL_LENGTH(N'PsychometricSubmissions', N'DueDate') IS NULL
                ALTER TABLE [PsychometricSubmissions] ADD [DueDate] datetime2 NULL;
            IF COL_LENGTH(N'PsychometricSubmissions', N'Status') IS NULL
                ALTER TABLE [PsychometricSubmissions] ADD [Status] nvarchar(50) NOT NULL CONSTRAINT [DF_PsychometricSubmissions_Status] DEFAULT 'Completed';

            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PsychometricTests_TestType' AND is_unique = 1 AND object_id = OBJECT_ID('PsychometricTests'))
            BEGIN
                DROP INDEX [IX_PsychometricTests_TestType] ON [PsychometricTests];
                CREATE INDEX [IX_PsychometricTests_TestType] ON [PsychometricTests]([TestType]);
            END
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[CustomClinicalFields]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CustomClinicalFields] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [OwnerType] nvarchar(100) NOT NULL,
                    [OwnerId] uniqueidentifier NOT NULL,
                    [SectionKey] nvarchar(100) NOT NULL,
                    [Title] nvarchar(255) NOT NULL,
                    [Content] nvarchar(max) NULL,
                    [FieldType] nvarchar(50) NOT NULL,
                    [OrderIndex] int NOT NULL CONSTRAINT [DF_CustomClinicalFields_OrderIndex] DEFAULT 0,
                    [CreatedByDoctorId] uniqueidentifier NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedBy] uniqueidentifier NULL,
                    [IsDeleted] bit NOT NULL
                );
                CREATE INDEX [IX_CustomClinicalFields_OwnerType_OwnerId] ON [CustomClinicalFields]([OwnerType], [OwnerId]);
                CREATE INDEX [IX_CustomClinicalFields_OwnerId_SectionKey_OrderIndex] ON [CustomClinicalFields]([OwnerId], [SectionKey], [OrderIndex]);
            END
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'AppointmentHistories', N'ChangedByUserId') IS NULL
                ALTER TABLE [AppointmentHistories] ADD [ChangedByUserId] uniqueidentifier NULL;
            IF COL_LENGTH(N'AppointmentHistories', N'ChangedByRole') IS NULL
                ALTER TABLE [AppointmentHistories] ADD [ChangedByRole] nvarchar(32) NULL;
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationRequestedByUserId') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationRequestedByUserId] uniqueidentifier NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationRequestedAt') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationRequestedAt] datetime2 NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'CancellationReason') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [CancellationReason] nvarchar(1000) NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'RecommendedSessionsPerWeek') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [RecommendedSessionsPerWeek] int NOT NULL CONSTRAINT [DF_TreatmentPackages_RecommendedSessionsPerWeek] DEFAULT 1;
            IF COL_LENGTH(N'TreatmentPackages', N'TargetOutcome') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [TargetOutcome] nvarchar(max) NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'RecommendedExercises') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [RecommendedExercises] nvarchar(max) NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'Instructions') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [Instructions] nvarchar(max) NULL;
            IF COL_LENGTH(N'TreatmentPackages', N'ValidityDays') IS NULL
                ALTER TABLE [TreatmentPackages] ADD [ValidityDays] int NOT NULL CONSTRAINT [DF_TreatmentPackages_ValidityDays] DEFAULT 90;
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'VerificationRequests', N'CertificatePublicId') IS NULL
                ALTER TABLE [VerificationRequests] ADD [CertificatePublicId] nvarchar(500) NULL;
            IF COL_LENGTH(N'VerificationRequests', N'CertificateFileName') IS NULL
                ALTER TABLE [VerificationRequests] ADD [CertificateFileName] nvarchar(255) NULL;
            IF COL_LENGTH(N'VerificationRequests', N'CertificateContentType') IS NULL
                ALTER TABLE [VerificationRequests] ADD [CertificateContentType] nvarchar(100) NULL;
            IF COL_LENGTH(N'VerificationRequests', N'CertificateUploadedAt') IS NULL
                ALTER TABLE [VerificationRequests] ADD [CertificateUploadedAt] datetime2 NULL;
            IF COL_LENGTH(N'VerificationRequests', N'SubmittedAt') IS NULL
                ALTER TABLE [VerificationRequests] ADD [SubmittedAt] datetime2 NOT NULL CONSTRAINT [DF_VerificationRequests_SubmittedAt] DEFAULT SYSUTCDATETIME();
            """, ct);

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'ConsultationNotes', N'NextAppointmentRecommendedSlotId') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [NextAppointmentRecommendedSlotId] uniqueidentifier NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'FollowUpAppointmentId') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [FollowUpAppointmentId] uniqueidentifier NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'IsPatientConfirmed') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [IsPatientConfirmed] bit NOT NULL CONSTRAINT [DF_ConsultationNotes_IsPatientConfirmed] DEFAULT 0;
            IF COL_LENGTH(N'ConsultationNotes', N'PatientConfirmedAt') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [PatientConfirmedAt] datetime2 NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'PatientConfirmedById') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [PatientConfirmedById] uniqueidentifier NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'LastEditedAt') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [LastEditedAt] datetime2 NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'LastEditedByDoctorId') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [LastEditedByDoctorId] uniqueidentifier NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'ConsultationDate') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [ConsultationDate] datetime2 NULL;
            IF COL_LENGTH(N'ConsultationNotes', N'Visibility') IS NULL
                ALTER TABLE [ConsultationNotes] ADD [Visibility] int NOT NULL CONSTRAINT [DF_ConsultationNotes_Visibility] DEFAULT 0;
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
            IF OBJECT_ID(N'[AppointmentCompletionConfirmations]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'AppointmentCompletionConfirmations', N'GuestEmail') IS NULL
                    ALTER TABLE [AppointmentCompletionConfirmations] ADD [GuestEmail] nvarchar(255) NULL;
                IF COL_LENGTH(N'AppointmentCompletionConfirmations', N'GuestTokenHash') IS NULL
                    ALTER TABLE [AppointmentCompletionConfirmations] ADD [GuestTokenHash] nvarchar(64) NULL;
                IF COL_LENGTH(N'AppointmentCompletionConfirmations', N'DisputedAt') IS NULL
                    ALTER TABLE [AppointmentCompletionConfirmations] ADD [DisputedAt] datetime2 NULL;
                IF COL_LENGTH(N'AppointmentCompletionConfirmations', N'DisputeReason') IS NULL
                    ALTER TABLE [AppointmentCompletionConfirmations] ADD [DisputeReason] nvarchar(2000) NULL;
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
                    [AppointmentSlotId] uniqueidentifier NULL,
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
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [FK_ScheduleNotes_AppointmentSlots_AppointmentSlotId]
                        FOREIGN KEY ([AppointmentSlotId]) REFERENCES [AppointmentSlots]([Id]) ON DELETE SET NULL
                );
                CREATE INDEX [IX_ScheduleNotes_DoctorProfileId_NoteDate] ON [ScheduleNotes]([DoctorProfileId], [NoteDate]);
                CREATE INDEX [IX_ScheduleNotes_AppointmentSlotId] ON [ScheduleNotes]([AppointmentSlotId]);
            END

            IF COL_LENGTH(N'ScheduleNotes', N'AppointmentSlotId') IS NULL
                ALTER TABLE [ScheduleNotes] ADD [AppointmentSlotId] uniqueidentifier NULL;

            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE [name] = N'FK_ScheduleNotes_AppointmentSlots_AppointmentSlotId'
                  AND [parent_object_id] = OBJECT_ID(N'[ScheduleNotes]'))
                ALTER TABLE [ScheduleNotes]
                    ADD CONSTRAINT [FK_ScheduleNotes_AppointmentSlots_AppointmentSlotId]
                    FOREIGN KEY ([AppointmentSlotId]) REFERENCES [AppointmentSlots]([Id]) ON DELETE SET NULL;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_ScheduleNotes_AppointmentSlotId'
                  AND [object_id] = OBJECT_ID(N'[ScheduleNotes]'))
                CREATE INDEX [IX_ScheduleNotes_AppointmentSlotId] ON [ScheduleNotes]([AppointmentSlotId]);
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
