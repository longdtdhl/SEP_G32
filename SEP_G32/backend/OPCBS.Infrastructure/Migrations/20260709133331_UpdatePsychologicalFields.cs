using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePsychologicalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "PatientRecords");

            migrationBuilder.RenameColumn(
                name: "MedicalHistory",
                table: "PatientRecords",
                newName: "PsychologicalHistory");

            migrationBuilder.RenameColumn(
                name: "Prescription",
                table: "ConsultationNotes",
                newName: "TherapyPlan");

            migrationBuilder.AddColumn<string>(
                name: "CurrentSymptoms",
                table: "PatientRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StressFactors",
                table: "PatientRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSymptoms",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "StressFactors",
                table: "PatientRecords");

            migrationBuilder.RenameColumn(
                name: "PsychologicalHistory",
                table: "PatientRecords",
                newName: "MedicalHistory");

            migrationBuilder.RenameColumn(
                name: "TherapyPlan",
                table: "ConsultationNotes",
                newName: "Prescription");

            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "PatientRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "PatientRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
