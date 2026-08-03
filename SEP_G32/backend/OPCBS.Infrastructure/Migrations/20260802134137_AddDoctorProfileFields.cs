using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareApproach",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareerBackground",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "DoctorProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConsultationTypes",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "DoctorProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "DoctorProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "CareApproach",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "CareerBackground",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "ConsultationFee",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "ConsultationTypes",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "DoctorProfiles");
        }
    }
}
