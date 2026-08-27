using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabaseLatest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy OPCBS databases were maintained by additive startup scripts.
            // Keep this migration idempotent so EF can safely adopt those databases.
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[TreatmentPackages]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'TreatmentPackages', N'AcceptanceExpiresAt') IS NULL
                        ALTER TABLE [TreatmentPackages] ADD [AcceptanceExpiresAt] datetime2 NULL;
                    IF COL_LENGTH(N'TreatmentPackages', N'ExpiredAt') IS NULL
                        ALTER TABLE [TreatmentPackages] ADD [ExpiredAt] datetime2 NULL;
                END

                IF OBJECT_ID(N'[PsychometricTests]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'PsychometricTests', N'Category') IS NULL
                        ALTER TABLE [PsychometricTests] ADD [Category] nvarchar(max) NULL;
                    IF COL_LENGTH(N'PsychometricTests', N'DoctorId') IS NULL
                        ALTER TABLE [PsychometricTests] ADD [DoctorId] uniqueidentifier NULL;
                    IF COL_LENGTH(N'PsychometricTests', N'IsActive') IS NULL
                        ALTER TABLE [PsychometricTests] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_PsychometricTests_IsActive] DEFAULT 0;
                    IF COL_LENGTH(N'PsychometricTests', N'Purpose') IS NULL
                        ALTER TABLE [PsychometricTests] ADD [Purpose] nvarchar(max) NULL;
                    IF COL_LENGTH(N'PsychometricTests', N'ScoreRangesJson') IS NULL
                        ALTER TABLE [PsychometricTests] ADD [ScoreRangesJson] nvarchar(max) NULL;
                END

                IF OBJECT_ID(N'[PsychometricSubmissions]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'PsychometricSubmissions', N'AssignedByDoctorId') IS NULL
                        ALTER TABLE [PsychometricSubmissions] ADD [AssignedByDoctorId] uniqueidentifier NULL;
                    IF COL_LENGTH(N'PsychometricSubmissions', N'DoctorNotes') IS NULL
                        ALTER TABLE [PsychometricSubmissions] ADD [DoctorNotes] nvarchar(max) NULL;
                    IF COL_LENGTH(N'PsychometricSubmissions', N'DueDate') IS NULL
                        ALTER TABLE [PsychometricSubmissions] ADD [DueDate] datetime2 NULL;
                    IF COL_LENGTH(N'PsychometricSubmissions', N'Status') IS NULL
                        ALTER TABLE [PsychometricSubmissions] ADD [Status] nvarchar(max) NOT NULL CONSTRAINT [DF_PsychometricSubmissions_Status] DEFAULT N'';
                END

                IF OBJECT_ID(N'[PsychometricQuestions]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'PsychometricQuestions', N'OptionsJson') IS NULL
                        ALTER TABLE [PsychometricQuestions] ADD [OptionsJson] nvarchar(max) NULL;
                    IF COL_LENGTH(N'PsychometricQuestions', N'QuestionType') IS NULL
                        ALTER TABLE [PsychometricQuestions] ADD [QuestionType] nvarchar(max) NOT NULL CONSTRAINT [DF_PsychometricQuestions_QuestionType] DEFAULT N'';
                END

                IF OBJECT_ID(N'[EmotionJournals]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'EmotionJournals', N'DepressionScale') IS NULL
                        ALTER TABLE [EmotionJournals] ADD [DepressionScale] int NULL;
                    IF COL_LENGTH(N'EmotionJournals', N'SleepHours') IS NULL
                        ALTER TABLE [EmotionJournals] ADD [SleepHours] decimal(18,2) NULL;
                END

                IF OBJECT_ID(N'[DoctorProfiles]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'DoctorProfiles', N'IsConsultationFeePublic') IS NULL
                    ALTER TABLE [DoctorProfiles] ADD [IsConsultationFeePublic] bit NOT NULL CONSTRAINT [DF_DoctorProfiles_IsConsultationFeePublic] DEFAULT 1;

                IF OBJECT_ID(N'[PsychometricTests]', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM sys.indexes
                        WHERE [object_id] = OBJECT_ID(N'[PsychometricTests]')
                          AND [name] = N'IX_PsychometricTests_TestType'
                          AND [is_unique] = 1)
                        DROP INDEX [IX_PsychometricTests_TestType] ON [PsychometricTests];

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.indexes
                        WHERE [object_id] = OBJECT_ID(N'[PsychometricTests]')
                          AND [name] = N'IX_PsychometricTests_TestType')
                        CREATE INDEX [IX_PsychometricTests_TestType] ON [PsychometricTests]([TestType]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally non-destructive. Some columns may predate EF migration tracking
            // because legacy databases were upgraded by guarded startup scripts.
        }
    }
}
