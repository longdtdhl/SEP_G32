using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WalkInPatientSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsultationRecords_AppointmentId",
                table: "ConsultationRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "ConsultationRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "ConsultationRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "WalkInPatientEmail",
                table: "ConsultationRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalkInPatientName",
                table: "ConsultationRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalkInPatientPhone",
                table: "ConsultationRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecords_AppointmentId",
                table: "ConsultationRecords",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsultationRecords_AppointmentId",
                table: "ConsultationRecords");

            migrationBuilder.DropColumn(
                name: "WalkInPatientEmail",
                table: "ConsultationRecords");

            migrationBuilder.DropColumn(
                name: "WalkInPatientName",
                table: "ConsultationRecords");

            migrationBuilder.DropColumn(
                name: "WalkInPatientPhone",
                table: "ConsultationRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "ConsultationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "ConsultationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecords_AppointmentId",
                table: "ConsultationRecords",
                column: "AppointmentId",
                unique: true);
        }
    }
}
