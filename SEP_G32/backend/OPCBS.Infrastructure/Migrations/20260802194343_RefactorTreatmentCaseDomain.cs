using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTreatmentCaseDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TherapyAssignments_TreatmentPackages_TreatmentPackageId",
                table: "TherapyAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                table: "TreatmentGoals");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                table: "TreatmentSessions");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TreatmentSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorClinicalAssessment",
                table: "TreatmentSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorPrivateNotes",
                table: "TreatmentSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientFriendlySummary",
                table: "TreatmentSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndTime",
                table: "TreatmentSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartTime",
                table: "TreatmentSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TreatmentSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "TreatmentGoals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByDoctorId",
                table: "TreatmentGoals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentValue",
                table: "TreatmentGoals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetValue",
                table: "TreatmentGoals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "TreatmentGoals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationDaysSnapshot",
                table: "TreatmentCases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PackageDescriptionSnapshot",
                table: "TreatmentCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageNameSnapshot",
                table: "TreatmentCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientGuidanceSnapshot",
                table: "TreatmentCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceSnapshot",
                table: "TreatmentCases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedExercisesSnapshot",
                table: "TreatmentCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedSessionsPerWeekSnapshot",
                table: "TreatmentCases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetOutcomesSnapshot",
                table: "TreatmentCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSessionsSnapshot",
                table: "TreatmentCases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentPackageId",
                table: "TherapyAssignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentSessionId",
                table: "TherapyAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentCaseId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentSessionId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MoodEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoodScore = table.Column<int>(type: "int", nullable: false),
                    AnxietyScore = table.Column<int>(type: "int", nullable: true),
                    StressScore = table.Column<int>(type: "int", nullable: true),
                    SleepQualityScore = table.Column<int>(type: "int", nullable: true),
                    DepressionScore = table.Column<int>(type: "int", nullable: true),
                    RelationshipScore = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoodEntries_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoodEntries_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentGoalProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DoctorComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentGoalProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentGoalProgresses_TreatmentGoals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "TreatmentGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentGoalProgresses_TreatmentSessions_TreatmentSessionId",
                        column: x => x.TreatmentSessionId,
                        principalTable: "TreatmentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentSessionGoals",
                columns: table => new
                {
                    TreatmentSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentSessionGoals", x => new { x.TreatmentSessionId, x.TreatmentGoalId });
                    table.ForeignKey(
                        name: "FK_TreatmentSessionGoals_TreatmentGoals_TreatmentGoalId",
                        column: x => x.TreatmentGoalId,
                        principalTable: "TreatmentGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentSessionGoals_TreatmentSessions_TreatmentSessionId",
                        column: x => x.TreatmentSessionId,
                        principalTable: "TreatmentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TherapyAssignments_TreatmentSessionId",
                table: "TherapyAssignments",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentCaseId",
                table: "Appointments",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentSessionId",
                table: "Appointments",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_PatientId",
                table: "MoodEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_TreatmentCaseId",
                table: "MoodEntries",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoalProgresses_GoalId",
                table: "TreatmentGoalProgresses",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoalProgresses_TreatmentSessionId",
                table: "TreatmentGoalProgresses",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessionGoals_TreatmentGoalId",
                table: "TreatmentSessionGoals",
                column: "TreatmentGoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_TreatmentCases_TreatmentCaseId",
                table: "Appointments",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_TreatmentSessions_TreatmentSessionId",
                table: "Appointments",
                column: "TreatmentSessionId",
                principalTable: "TreatmentSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapyAssignments_TreatmentPackages_TreatmentPackageId",
                table: "TherapyAssignments",
                column: "TreatmentPackageId",
                principalTable: "TreatmentPackages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TherapyAssignments_TreatmentSessions_TreatmentSessionId",
                table: "TherapyAssignments",
                column: "TreatmentSessionId",
                principalTable: "TreatmentSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                table: "TreatmentGoals",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                table: "TreatmentSessions",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_TreatmentCases_TreatmentCaseId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_TreatmentSessions_TreatmentSessionId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_TherapyAssignments_TreatmentPackages_TreatmentPackageId",
                table: "TherapyAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TherapyAssignments_TreatmentSessions_TreatmentSessionId",
                table: "TherapyAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                table: "TreatmentGoals");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                table: "TreatmentSessions");

            migrationBuilder.DropTable(
                name: "MoodEntries");

            migrationBuilder.DropTable(
                name: "TreatmentGoalProgresses");

            migrationBuilder.DropTable(
                name: "TreatmentSessionGoals");

            migrationBuilder.DropIndex(
                name: "IX_TherapyAssignments_TreatmentSessionId",
                table: "TherapyAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TreatmentCaseId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TreatmentSessionId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "DoctorClinicalAssessment",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "DoctorPrivateNotes",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "PatientFriendlySummary",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "PlannedEndTime",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "PlannedStartTime",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "TreatmentGoals");

            migrationBuilder.DropColumn(
                name: "CreatedByDoctorId",
                table: "TreatmentGoals");

            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "TreatmentGoals");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "TreatmentGoals");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "TreatmentGoals");

            migrationBuilder.DropColumn(
                name: "DurationDaysSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "PackageDescriptionSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "PackageNameSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "PatientGuidanceSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "PriceSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "RecommendedExercisesSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "RecommendedSessionsPerWeekSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "TargetOutcomesSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "TotalSessionsSnapshot",
                table: "TreatmentCases");

            migrationBuilder.DropColumn(
                name: "TreatmentSessionId",
                table: "TherapyAssignments");

            migrationBuilder.DropColumn(
                name: "TreatmentCaseId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TreatmentSessionId",
                table: "Appointments");

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentPackageId",
                table: "TherapyAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapyAssignments_TreatmentPackages_TreatmentPackageId",
                table: "TherapyAssignments",
                column: "TreatmentPackageId",
                principalTable: "TreatmentPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                table: "TreatmentGoals",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                table: "TreatmentSessions",
                column: "TreatmentCaseId",
                principalTable: "TreatmentCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
