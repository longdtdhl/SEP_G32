using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OPCBS.Infrastructure.Persistence;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    [DbContext(typeof(OpcbsDbContext))]
    [Migration("20260812071000_AddAppointmentGuestZaloNumber")]
    public partial class AddAppointmentGuestZaloNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestZaloNumber",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestZaloNumber",
                table: "Appointments");
        }
    }
}
