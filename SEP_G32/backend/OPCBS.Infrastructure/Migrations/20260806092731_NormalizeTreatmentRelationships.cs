using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPCBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTreatmentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PsychometricTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychometricTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxPatientCapacity = table.Column<int>(type: "int", nullable: true),
                    MaxDailySlotsCapacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PsychometricQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychometricQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychometricQuestions_PsychometricTests_TestId",
                        column: x => x.TestId,
                        principalTable: "PsychometricTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfessionalTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CareerBackground = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CareApproach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Languages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsultationTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                    table.UniqueConstraint("AK_DoctorProfiles_UserId", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.UniqueConstraint("AK_PatientProfiles_UserId", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxPatients = table.Column<int>(type: "int", nullable: false),
                    CurrentBookings = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Excerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CertificateType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuingOrganization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorDayOffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorDayOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorDayOffs_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpecializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpecializations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSpecializations_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSpecializations_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveConsultationCount = table.Column<int>(type: "int", nullable: false),
                    CompletedConsultationCount = table.Column<int>(type: "int", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSubscriptions_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSubscriptions_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SlotDuration = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SlotsPerDay = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificatePublicId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationRequests_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteDoctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDoctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteDoctors_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FavoriteDoctors_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    PsychologicalHistory = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CurrentSymptoms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StressFactors = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                name: "TreatmentPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TargetOutcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedExercises = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionQuantity = table.Column<int>(type: "int", nullable: false),
                    RemainingSessions = table.Column<int>(type: "int", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    RecommendedSessionsPerWeek = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPackages_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentPackages_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlogComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlogPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AuthorEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogComments_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogComments_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResponseMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_DoctorSubscriptions_DoctorSubscriptionId",
                        column: x => x.DoctorSubscriptionId,
                        principalTable: "DoctorSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CaseDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrimaryConcern = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PackageNameSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackageDescriptionSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalSessionsSnapshot = table.Column<int>(type: "int", nullable: false),
                    DurationDaysSnapshot = table.Column<int>(type: "int", nullable: false),
                    RecommendedSessionsPerWeekSnapshot = table.Column<int>(type: "int", nullable: false),
                    PriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetOutcomesSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedExercisesSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientGuidanceSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    CompletedSessions = table.Column<int>(type: "int", nullable: false),
                    RemainingSessions = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosureNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OverallProgressPercent = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentCases_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AppointmentSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuestPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expectations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProposedSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RescheduleReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentSlots_AppointmentSlotId",
                        column: x => x.AppointmentSlotId,
                        principalTable: "AppointmentSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentSlots_ProposedSlotId",
                        column: x => x.ProposedSlotId,
                        principalTable: "AppointmentSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmotionJournals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoodScale = table.Column<int>(type: "int", nullable: false),
                    StressScale = table.Column<int>(type: "int", nullable: false),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmotionJournals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmotionJournals_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmotionJournals_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                name: "TreatmentGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AchievedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentGoals_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentHistories_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    TherapyPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NextAppointmentRecommendedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    IsPatientConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PatientConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientConfirmedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastEditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEditedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Conversations_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PsychometricSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    ScoreDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Interpretation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychometricSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychometricSubmissions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PsychometricSubmissions_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PsychometricSubmissions_PsychometricTests_TestId",
                        column: x => x.TestId,
                        principalTable: "PsychometricTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PsychometricSubmissions_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TreatmentSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlannedStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DoctorClinicalAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientFriendlySummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoctorPrivateNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TherapistNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PatientFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HomeworkAssigned = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MoodBefore = table.Column<int>(type: "int", nullable: true),
                    MoodAfter = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentSessions_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PsychometricAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychometricAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychometricAnswers_PsychometricQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "PsychometricQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PsychometricAnswers_PsychometricSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "PsychometricSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TherapyAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TreatmentSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailedInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PatientSubmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientSubmissionUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapyAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TherapyAssignments_TreatmentCases_TreatmentCaseId",
                        column: x => x.TreatmentCaseId,
                        principalTable: "TreatmentCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TherapyAssignments_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TherapyAssignments_TreatmentSessions_TreatmentSessionId",
                        column: x => x.TreatmentSessionId,
                        principalTable: "TreatmentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_AppointmentHistories_AppointmentId",
                table: "AppointmentHistories",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentSlotId",
                table: "Appointments",
                column: "AppointmentSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingCode",
                table: "Appointments",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProposedSlotId",
                table: "Appointments",
                column: "ProposedSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentCaseId",
                table: "Appointments",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentPackageId",
                table: "Appointments",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_DoctorProfileId_SlotDate_StartTime",
                table: "AppointmentSlots",
                columns: new[] { "DoctorProfileId", "SlotDate", "StartTime" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_BlogPostId",
                table: "BlogComments",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_PatientId",
                table: "BlogComments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_DoctorId",
                table: "BlogPosts",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_DoctorProfileId",
                table: "Certificates",
                column: "DoctorProfileId");

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
                name: "IX_Conversations_AppointmentId",
                table: "Conversations",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_DoctorId",
                table: "Conversations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PatientId_DoctorId",
                table: "Conversations",
                columns: new[] { "PatientId", "DoctorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_TreatmentPackageId",
                table: "Conversations",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDayOffs_DoctorProfileId",
                table: "DoctorDayOffs",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpecializations_DoctorProfileId_SpecializationId",
                table: "DoctorSpecializations",
                columns: new[] { "DoctorProfileId", "SpecializationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpecializations_SpecializationId",
                table: "DoctorSpecializations",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_DoctorProfileId",
                table: "DoctorSubscriptions",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_ServicePackageId",
                table: "DoctorSubscriptions",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionJournals_PatientId",
                table: "EmotionJournals",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionJournals_TreatmentCaseId",
                table: "EmotionJournals",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDoctors_DoctorId",
                table: "FavoriteDoctors",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDoctors_PatientId_DoctorId",
                table: "FavoriteDoctors",
                columns: new[] { "PatientId", "DoctorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_PatientId",
                table: "MoodEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_TreatmentCaseId",
                table: "MoodEntries",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_UserId",
                table: "OtpVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecords_DoctorId",
                table: "PatientRecords",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecords_PatientId",
                table: "PatientRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_DoctorSubscriptionId",
                table: "PaymentTransactions",
                column: "DoctorSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TransactionCode",
                table: "PaymentTransactions",
                column: "TransactionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricAnswers_QuestionId",
                table: "PsychometricAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricAnswers_SubmissionId",
                table: "PsychometricAnswers",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricQuestions_TestId",
                table: "PsychometricQuestions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricSubmissions_AppointmentId",
                table: "PsychometricSubmissions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricSubmissions_PatientId",
                table: "PsychometricSubmissions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricSubmissions_TestId",
                table: "PsychometricSubmissions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricSubmissions_TreatmentCaseId",
                table: "PsychometricSubmissions",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychometricTests_TestType",
                table: "PsychometricTests",
                column: "TestType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppointmentId",
                table: "Reviews",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_DoctorId",
                table: "Reviews",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PatientId",
                table: "Reviews",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorProfileId",
                table: "Schedules",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_Name",
                table: "ServicePackages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_Name",
                table: "Specializations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigs_Key",
                table: "SystemConfigs",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TherapyAssignments_TreatmentCaseId",
                table: "TherapyAssignments",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapyAssignments_TreatmentPackageId",
                table: "TherapyAssignments",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapyAssignments_TreatmentSessionId",
                table: "TherapyAssignments",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_DoctorId_PatientId_Status",
                table: "TreatmentCases",
                columns: new[] { "DoctorId", "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_PatientId",
                table: "TreatmentCases",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentCases_TreatmentPackageId",
                table: "TreatmentCases",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoalProgresses_GoalId",
                table: "TreatmentGoalProgresses",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoalProgresses_TreatmentSessionId",
                table: "TreatmentGoalProgresses",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoals_TreatmentCaseId",
                table: "TreatmentGoals",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_DoctorId",
                table: "TreatmentPackages",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_PatientId",
                table: "TreatmentPackages",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessionGoals_TreatmentGoalId",
                table: "TreatmentSessionGoals",
                column: "TreatmentGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_AppointmentId",
                table: "TreatmentSessions",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_TreatmentCaseId",
                table: "TreatmentSessions",
                column: "TreatmentCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_TreatmentCaseId_SessionNumber",
                table: "TreatmentSessions",
                columns: new[] { "TreatmentCaseId", "SessionNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_DoctorProfileId",
                table: "VerificationRequests",
                column: "DoctorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentHistories");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BlogComments");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "ConsultationNotes");

            migrationBuilder.DropTable(
                name: "DoctorDayOffs");

            migrationBuilder.DropTable(
                name: "DoctorSpecializations");

            migrationBuilder.DropTable(
                name: "EmotionJournals");

            migrationBuilder.DropTable(
                name: "FavoriteDoctors");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "MoodEntries");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OtpVerifications");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PsychometricAnswers");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "TherapyAssignments");

            migrationBuilder.DropTable(
                name: "TreatmentGoalProgresses");

            migrationBuilder.DropTable(
                name: "TreatmentSessionGoals");

            migrationBuilder.DropTable(
                name: "VerificationRequests");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "PatientRecords");

            migrationBuilder.DropTable(
                name: "Specializations");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "DoctorSubscriptions");

            migrationBuilder.DropTable(
                name: "PsychometricQuestions");

            migrationBuilder.DropTable(
                name: "PsychometricSubmissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "TreatmentGoals");

            migrationBuilder.DropTable(
                name: "TreatmentSessions");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "PsychometricTests");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentSlots");

            migrationBuilder.DropTable(
                name: "TreatmentCases");

            migrationBuilder.DropTable(
                name: "TreatmentPackages");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");

            migrationBuilder.DropTable(
                name: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
