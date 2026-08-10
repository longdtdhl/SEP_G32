using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPatientRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationRecords");

            migrationBuilder.CreateTable(
                name: "PatientRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GeneralNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientRecords_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientRecords_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsultationNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    FollowUpNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Prescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NextAppointmentRecommendedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_PatientRecords_PatientRecordId",
                        column: x => x.PatientRecordId,
                        principalTable: "PatientRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_AppointmentId",
                table: "ConsultationNotes",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_DoctorId",
                table: "ConsultationNotes",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_DoctorProfileId",
                table: "ConsultationNotes",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_PatientProfileId",
                table: "ConsultationNotes",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_PatientRecordId",
                table: "ConsultationNotes",
                column: "PatientRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecords_DoctorId",
                table: "PatientRecords",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecords_PatientId",
                table: "PatientRecords",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationNotes");

            migrationBuilder.DropTable(
                name: "PatientRecords");

            migrationBuilder.CreateTable(
                name: "ConsultationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsultationSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FollowUpNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NextAppointmentRecommendedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Prescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalkInPatientEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WalkInPatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WalkInPatientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationRecords_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultationRecords_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationRecords_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecords_AppointmentId",
                table: "ConsultationRecords",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecords_DoctorId",
                table: "ConsultationRecords",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecords_PatientId",
                table: "ConsultationRecords",
                column: "PatientId");
        }
    }
}
