using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientConfirmationLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPatientConfirmed",
                table: "ConsultationNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "ConsultationNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedByDoctorId",
                table: "ConsultationNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PatientConfirmedAt",
                table: "ConsultationNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientConfirmedById",
                table: "ConsultationNotes",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPatientConfirmed",
                table: "ConsultationNotes");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "ConsultationNotes");

            migrationBuilder.DropColumn(
                name: "LastEditedByDoctorId",
                table: "ConsultationNotes");

            migrationBuilder.DropColumn(
                name: "PatientConfirmedAt",
                table: "ConsultationNotes");

            migrationBuilder.DropColumn(
                name: "PatientConfirmedById",
                table: "ConsultationNotes");
        }
    }
}
