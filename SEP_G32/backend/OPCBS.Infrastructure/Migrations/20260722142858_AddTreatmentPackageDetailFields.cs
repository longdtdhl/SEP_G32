using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentPackageDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "TreatmentPackages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "TreatmentPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedExercises",
                table: "TreatmentPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetOutcome",
                table: "TreatmentPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailedInstructions",
                table: "TherapyAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceUrl",
                table: "TherapyAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "TreatmentPackages");

            migrationBuilder.DropColumn(
                name: "RecommendedExercises",
                table: "TreatmentPackages");

            migrationBuilder.DropColumn(
                name: "TargetOutcome",
                table: "TreatmentPackages");

            migrationBuilder.DropColumn(
                name: "DetailedInstructions",
                table: "TherapyAssignments");

            migrationBuilder.DropColumn(
                name: "ResourceUrl",
                table: "TherapyAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "TreatmentPackages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
