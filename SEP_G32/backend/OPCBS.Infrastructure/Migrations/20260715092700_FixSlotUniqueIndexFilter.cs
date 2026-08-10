using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSlotUniqueIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_DoctorProfileId_SlotDate_StartTime",
                table: "AppointmentSlots");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_DoctorProfileId_SlotDate_StartTime",
                table: "AppointmentSlots",
                columns: new[] { "DoctorProfileId", "SlotDate", "StartTime" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_DoctorProfileId_SlotDate_StartTime",
                table: "AppointmentSlots");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_DoctorProfileId_SlotDate_StartTime",
                table: "AppointmentSlots",
                columns: new[] { "DoctorProfileId", "SlotDate", "StartTime" },
                unique: true);
        }
    }
}
