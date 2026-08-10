using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentReschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProposedSlotId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleReason",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProposedSlotId",
                table: "Appointments",
                column: "ProposedSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AppointmentSlots_ProposedSlotId",
                table: "Appointments",
                column: "ProposedSlotId",
                principalTable: "AppointmentSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AppointmentSlots_ProposedSlotId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ProposedSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProposedSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RescheduleReason",
                table: "Appointments");
        }
    }
}
