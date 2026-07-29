using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentCaseEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentCaseId",
                table: "TherapyAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentCaseId",
                table: "PsychometricSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentCaseId",
                table: "EmotionJournals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TreatmentCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CaseDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrimaryConcern = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    CompletedSessions = table.Column<int>(type: "int", nullable: false),
                    RemainingSessions = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosureNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OverallProgressPercent = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AchievedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    SessionSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TherapistNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PatientFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HomeworkAssigned = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MoodBefore = table.Column<int>(type: "int", nullable: true),
                    MoodAfter = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TherapyAssignments_TreatmentCaseId",
                table: "TherapyAssignments",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricSubmissions_TreatmentCaseId",
                table: "PsychometricSubmissions",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionJournals_TreatmentCaseId",
                table: "EmotionJournals",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_DoctorId_PatientId_Status",
                table: "TreatmentCases",
                columns: new[] { "DoctorId", "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_PatientId",
                table: "TreatmentCases",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_TreatmentPackageId",
                table: "TreatmentCases",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoals_TreatmentCaseId",
                table: "TreatmentGoals",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_AppointmentId",
                table: "TreatmentSessions",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_TreatmentCaseId",
                table: "TreatmentSessions",
                column: "TreatmentCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmotionJournals_TreatmentCases_TreatmentCaseId",
                table: "EmotionJournals",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PsychometricSubmissions_TreatmentCases_TreatmentCaseId",
                table: "PsychometricSubmissions",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapyAssignments_TreatmentCases_TreatmentCaseId",
                table: "TherapyAssignments",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmotionJournals_TreatmentCases_TreatmentCaseId",
                table: "EmotionJournals");

            migrationBuilder.DropForeignKey(
                name: "FK_PsychometricSubmissions_TreatmentCases_TreatmentCaseId",
                table: "PsychometricSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TherapyAssignments_TreatmentCases_TreatmentCaseId",
                table: "TherapyAssignments");

            migrationBuilder.DropTable(
                name: "TreatmentGoals");

            migrationBuilder.DropTable(
                name: "TreatmentSessions");

            migrationBuilder.DropTable(
                name: "TreatmentCases");

            migrationBuilder.DropIndex(
                name: "IX_TherapyAssignments_TreatmentCaseId",
                table: "TherapyAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PsychometricSubmissions_TreatmentCaseId",
                table: "PsychometricSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_EmotionJournals_TreatmentCaseId",
                table: "EmotionJournals");

            migrationBuilder.DropColumn(
                name: "TreatmentCaseId",
                table: "TherapyAssignments");

            migrationBuilder.DropColumn(
                name: "TreatmentCaseId",
                table: "PsychometricSubmissions");

            migrationBuilder.DropColumn(
                name: "TreatmentCaseId",
                table: "EmotionJournals");
        }
    }
}
