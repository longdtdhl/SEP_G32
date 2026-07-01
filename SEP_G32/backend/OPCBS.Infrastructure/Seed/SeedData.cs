using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Infrastructure.Persistence;

namespace OPCBS.Infrastructure.Seed;

/// <summary>
/// Database seed data for initial setup
/// Seeds roles, permissions, specializations, service packages, and a dev admin account
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(OpcbsDbContext context)
    {
        if (context.Roles.Any())
            return; // Already seeded

        // 1. Seed Roles
        var roles = new Dictionary<string, Role>();
        foreach (var roleName in RoleConstants.AllRoles)
        {
            var role = new Role { Name = roleName, Description = $"{roleName} role" };
            roles[roleName] = role;
            context.Roles.Add(role);
        }
        await context.SaveChangesAsync();

        // 2. Seed Permissions
        var permissionCodes = new[]
        {
            PermissionConstants.ManageOwnProfile,
            PermissionConstants.ViewDoctors,
            PermissionConstants.ManageDoctorProfile,
            PermissionConstants.ManageSchedule,
            PermissionConstants.ManageDoctorAppointments,
            PermissionConstants.ManageConsultationRecords,
            PermissionConstants.ManageTreatmentPackages,
            PermissionConstants.ManageDoctorBlogs,
            PermissionConstants.PurchaseSubscription,
            PermissionConstants.BookAppointment,
            PermissionConstants.ManageOwnAppointments,
            PermissionConstants.ViewConsultationHistory,
            PermissionConstants.SubmitReview,
            PermissionConstants.ViewTreatmentPackages,
            PermissionConstants.ReviewDoctorVerification,
            PermissionConstants.ModerateBlog,
            PermissionConstants.ViewAllAppointments,
            PermissionConstants.ManageServicePackages,
            PermissionConstants.ManageSpecializations,
            PermissionConstants.ViewReports,
            PermissionConstants.ManageUsers,
            PermissionConstants.ManageRoles,
            PermissionConstants.ViewAuditLogs,
            PermissionConstants.ManageSystemConfig
        };

        var permissions = new Dictionary<string, Permission>();
        foreach (var code in permissionCodes)
        {
            var perm = new Permission { Code = code, Description = code.Replace("_", " ") };
            permissions[code] = perm;
            context.Permissions.Add(perm);
        }
        await context.SaveChangesAsync();

        // 3. Seed Role-Permission mappings
        void MapPermission(string roleName, string permCode)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = roles[roleName].Id,
                PermissionId = permissions[permCode].Id,
                Role = roles[roleName],
                Permission = permissions[permCode]
            });
        }

        // Patient permissions
        MapPermission(RoleConstants.Patient, PermissionConstants.ManageOwnProfile);
        MapPermission(RoleConstants.Patient, PermissionConstants.ViewDoctors);
        MapPermission(RoleConstants.Patient, PermissionConstants.BookAppointment);
        MapPermission(RoleConstants.Patient, PermissionConstants.ManageOwnAppointments);
        MapPermission(RoleConstants.Patient, PermissionConstants.ViewConsultationHistory);
        MapPermission(RoleConstants.Patient, PermissionConstants.SubmitReview);
        MapPermission(RoleConstants.Patient, PermissionConstants.ViewTreatmentPackages);

        // Doctor permissions
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageOwnProfile);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageDoctorProfile);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageSchedule);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageDoctorAppointments);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageConsultationRecords);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageTreatmentPackages);
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageDoctorBlogs);
        MapPermission(RoleConstants.Doctor, PermissionConstants.PurchaseSubscription);

        // CustomerSupport permissions
        MapPermission(RoleConstants.CustomerSupport, PermissionConstants.ManageOwnProfile);
        MapPermission(RoleConstants.CustomerSupport, PermissionConstants.ReviewDoctorVerification);
        MapPermission(RoleConstants.CustomerSupport, PermissionConstants.ModerateBlog);
        MapPermission(RoleConstants.CustomerSupport, PermissionConstants.ViewAllAppointments);

        // BusinessManager permissions
        MapPermission(RoleConstants.BusinessManager, PermissionConstants.ManageOwnProfile);
        MapPermission(RoleConstants.BusinessManager, PermissionConstants.ManageServicePackages);
        MapPermission(RoleConstants.BusinessManager, PermissionConstants.ManageSpecializations);
        MapPermission(RoleConstants.BusinessManager, PermissionConstants.ViewReports);

        // SystemAdmin gets ALL permissions
        foreach (var permCode in permissionCodes)
            MapPermission(RoleConstants.SystemAdmin, permCode);

        await context.SaveChangesAsync();

        // 4. Seed Specializations
        var specializations = new[] {
            ("Clinical Psychology", "Assessment and treatment of mental disorders"),
            ("Counseling Psychology", "Help with everyday life stressors and emotional issues"),
            ("Child & Adolescent Psychology", "Specialized care for children and teens"),
            ("Depression & Mood Disorders", "Treatment for depression, bipolar disorder"),
            ("Anxiety & Stress Management", "Treatment for anxiety disorders and stress"),
            ("Trauma & PTSD", "Specialized trauma-focused therapy"),
            ("Addiction & Substance Abuse", "Recovery from substance use disorders"),
            ("Family & Marriage Counseling", "Relationship and family therapy"),
            ("Career Counseling", "Professional development and career guidance"),
            ("Cognitive Behavioral Therapy", "CBT-based therapeutic approaches")
        };

        var specEntities = new List<Specialization>();
        foreach (var (name, desc) in specializations)
        {
            var spec = new Specialization { Name = name, Description = desc };
            specEntities.Add(spec);
            context.Specializations.Add(spec);
        }
        await context.SaveChangesAsync();

        // 5. Seed Service Packages
        var basicPkg = new ServicePackage { Name = "Basic", Description = "Basic plan for new doctors", DurationDays = 30, Price = 299000, MaxPatientCapacity = 10, MaxDailySlotsCapacity = 5, DisplayOrder = 1 };
        var proPkg = new ServicePackage { Name = "Professional", Description = "Professional plan with more capacity", DurationDays = 90, Price = 799000, MaxPatientCapacity = 30, MaxDailySlotsCapacity = 10, IsFeatured = true, DisplayOrder = 2 };
        var premPkg = new ServicePackage { Name = "Premium", Description = "Unlimited premium plan", DurationDays = 365, Price = 2499000, MaxPatientCapacity = 100, MaxDailySlotsCapacity = 20, DisplayOrder = 3 };
        context.ServicePackages.AddRange(basicPkg, proPkg, premPkg);
        await context.SaveChangesAsync();

        // 6. Seed System Config
        context.SystemConfigs.Add(new SystemConfig { Key = "OtpExpirationMinutes", Value = "10", Description = "OTP expiration time in minutes", DataType = "int" });
        context.SystemConfigs.Add(new SystemConfig { Key = "MaxLoginAttempts", Value = "5", Description = "Maximum login attempts before lockout", DataType = "int" });
        context.SystemConfigs.Add(new SystemConfig { Key = "AppName", Value = "MindBridge - Online Psychological Counseling", Description = "Application display name" });
        context.SystemConfigs.Add(new SystemConfig { Key = "DefaultConsultationFee", Value = "500000", Description = "Default consultation fee in VND", DataType = "decimal" });
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 7. STAFF ACCOUNTS
        // ═══════════════════════════════════════════════
        var adminUser = CreateUser(context, "admin@opcbs.com", "Admin@123", "Lê Minh Quản Trị", "0900000001", roles[RoleConstants.SystemAdmin]);
        var csUser = CreateUser(context, "support@opcbs.com", "Support@123", "Nguyễn Thị Hỗ Trợ", "0900000002", roles[RoleConstants.CustomerSupport]);
        var bmUser = CreateUser(context, "manager@opcbs.com", "Manager@123", "Trần Văn Quản Lý", "0900000003", roles[RoleConstants.BusinessManager]);
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 8. PATIENTS (3 patients)
        // ═══════════════════════════════════════════════
        var patient1User = CreateUser(context, "patient@opcbs.com", "Patient@123", "Nguyễn Văn An", "0912345678", roles[RoleConstants.Patient]);
        var patient2User = CreateUser(context, "patient2@opcbs.com", "Patient@123", "Trần Thị Bình", "0912345679", roles[RoleConstants.Patient]);
        var patient3User = CreateUser(context, "patient3@opcbs.com", "Patient@123", "Phạm Hoàng Cường", "0912345680", roles[RoleConstants.Patient]);

        // Extra auth test accounts for login/OTP flows
        CreateUser(context, "unverified@opcbs.com", "Unverified@123", "Người chưa xác thực", "0900000004", roles[RoleConstants.Patient], isEmailVerified: false, status: UserStatus.Inactive);
        CreateUser(context, "locked@opcbs.com", "Locked@123", "Tài khoản bị khóa", "0900000005", roles[RoleConstants.Patient], isEmailVerified: true, status: UserStatus.Locked);
        await context.SaveChangesAsync();

        var patient1 = new PatientProfile { UserId = patient1User.Id, User = patient1User, DateOfBirth = new DateTime(1995, 5, 15), Gender = Gender.Male, Address = "123 Nguyễn Trãi, Q.1, TP.HCM" };
        var patient2 = new PatientProfile { UserId = patient2User.Id, User = patient2User, DateOfBirth = new DateTime(1998, 8, 22), Gender = Gender.Female, Address = "456 Lê Lợi, Q.3, TP.HCM" };
        var patient3 = new PatientProfile { UserId = patient3User.Id, User = patient3User, DateOfBirth = new DateTime(2000, 1, 10), Gender = Gender.Male, Address = "789 Trần Hưng Đạo, Q.5, TP.HCM" };
        context.PatientProfiles.AddRange(patient1, patient2, patient3);
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 9. DOCTORS (3 verified doctors + 1 pending)
        // ═══════════════════════════════════════════════
        var doc1User = CreateUser(context, "doctor@opcbs.com", "Doctor@123", "TS. Trần Thị Bảo Ngọc", "0987654321", roles[RoleConstants.Doctor]);
        var doc2User = CreateUser(context, "doctor2@opcbs.com", "Doctor@123", "ThS. Lê Hoàng Minh", "0987654322", roles[RoleConstants.Doctor]);
        var doc3User = CreateUser(context, "doctor3@opcbs.com", "Doctor@123", "PGS.TS. Nguyễn Phương Thảo", "0987654323", roles[RoleConstants.Doctor]);
        var doc4User = CreateUser(context, "doctor4@opcbs.com", "Doctor@123", "ThS. Võ Thanh Tùng", "0987654324", roles[RoleConstants.Doctor]);
        await context.SaveChangesAsync();

        var doc1 = new DoctorProfile
        {
            UserId = doc1User.Id, User = doc1User,
            ProfessionalTitle = "Tiến sĩ Tâm lý học lâm sàng",
            Biography = "Hơn 12 năm kinh nghiệm trong lĩnh vực tham vấn và trị liệu tâm lý. Tốt nghiệp ĐH Y Dược TP.HCM chuyên ngành Tâm lý lâm sàng. Chuyên gia trị liệu trầm cảm và rối loạn lo âu.",
            ExperienceYears = 12, LicenseNumber = "LIC-2024-001", LicenseExpiryDate = new DateTime(2028, 12, 31),
            VerificationStatus = VerificationStatus.Approved, IsVisible = true, AverageRating = 4.8m, ReviewCount = 32
        };
        var doc2 = new DoctorProfile
        {
            UserId = doc2User.Id, User = doc2User,
            ProfessionalTitle = "Thạc sĩ Tâm lý trị liệu",
            Biography = "Chuyên gia tâm lý trẻ em và vị thành niên với 8 năm kinh nghiệm. Phương pháp trị liệu CBT và Play Therapy. Tốt nghiệp ĐH Sư phạm Hà Nội.",
            ExperienceYears = 8, LicenseNumber = "LIC-2024-002", LicenseExpiryDate = new DateTime(2027, 6, 30),
            VerificationStatus = VerificationStatus.Approved, IsVisible = true, AverageRating = 4.5m, ReviewCount = 18
        };
        var doc3 = new DoctorProfile
        {
            UserId = doc3User.Id, User = doc3User,
            ProfessionalTitle = "Phó Giáo sư, Tiến sĩ Tâm lý học",
            Biography = "20 năm nghiên cứu và thực hành lâm sàng. Giảng viên ĐH Khoa học Xã hội và Nhân văn. Chuyên gia hàng đầu về trị liệu gia đình và các vấn đề hôn nhân.",
            ExperienceYears = 20, LicenseNumber = "LIC-2024-003", LicenseExpiryDate = new DateTime(2029, 12, 31),
            VerificationStatus = VerificationStatus.Approved, IsVisible = true, AverageRating = 4.9m, ReviewCount = 45
        };
        var doc4 = new DoctorProfile
        {
            UserId = doc4User.Id, User = doc4User,
            ProfessionalTitle = "Thạc sĩ Tâm lý học",
            Biography = "Chuyên gia tâm lý nghề nghiệp và stress công sở. 5 năm kinh nghiệm tư vấn tại các doanh nghiệp lớn.",
            ExperienceYears = 5, LicenseNumber = "LIC-2024-004", LicenseExpiryDate = new DateTime(2027, 12, 31),
            VerificationStatus = VerificationStatus.Submitted, IsVisible = false
        };
        context.DoctorProfiles.AddRange(doc1, doc2, doc3, doc4);
        await context.SaveChangesAsync();

        // Doctor specializations
        void AssignSpecs(DoctorProfile doc, params int[] indices)
        {
            foreach (var idx in indices)
            {
                context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorProfileId = doc.Id, SpecializationId = specEntities[idx].Id,
                    DoctorProfile = doc, Specialization = specEntities[idx]
                });
            }
        }
        AssignSpecs(doc1, 0, 3, 4);     // Clinical, Depression, Anxiety
        AssignSpecs(doc2, 2, 9, 1);     // Child, CBT, Counseling
        AssignSpecs(doc3, 7, 0, 5);     // Family, Clinical, Trauma
        AssignSpecs(doc4, 8, 4);        // Career, Anxiety
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 10. SUBSCRIPTIONS (active for 3 verified doctors)
        // ═══════════════════════════════════════════════
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorProfileId = doc1.Id, ServicePackageId = proPkg.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30), ExpirationDate = DateTime.UtcNow.AddDays(60),
            DoctorProfile = doc1, ServicePackage = proPkg
        });
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorProfileId = doc2.Id, ServicePackageId = basicPkg.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-10), ExpirationDate = DateTime.UtcNow.AddDays(20),
            DoctorProfile = doc2, ServicePackage = basicPkg
        });
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorProfileId = doc3.Id, ServicePackageId = premPkg.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-60), ExpirationDate = DateTime.UtcNow.AddDays(305),
            DoctorProfile = doc3, ServicePackage = premPkg
        });
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 11. SCHEDULES & APPOINTMENT SLOTS
        // ═══════════════════════════════════════════════
        var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        void CreateScheduleAndSlots(DoctorProfile doc, int startHour, int endHour, SlotDuration duration)
        {
            var flagDays = DayOfWeekEnum.Monday | DayOfWeekEnum.Tuesday | DayOfWeekEnum.Wednesday | DayOfWeekEnum.Thursday | DayOfWeekEnum.Friday;
            context.Schedules.Add(new Schedule
            {
                DoctorProfileId = doc.Id, WorkingDays = flagDays,
                StartTime = new TimeOnly(startHour, 0), EndTime = new TimeOnly(endHour, 0),
                SlotDuration = duration, IsActive = true, DoctorProfile = doc,
                SlotsPerDay = (endHour - startHour) / ((int)duration == 30 ? 1 : ((int)duration == 60 ? 1 : 1))
            });

            var today = DateTime.UtcNow.Date;
            var slotMinutes = duration == SlotDuration.Minutes30 ? 30 : (duration == SlotDuration.Minutes60 ? 60 : 90);
            for (int i = 0; i <= 21; i++)
            {
                var date = today.AddDays(i);
                if (!daysOfWeek.Contains(date.DayOfWeek)) continue;
                for (int min = startHour * 60; min < endHour * 60; min += slotMinutes)
                {
                    context.AppointmentSlots.Add(new AppointmentSlot
                    {
                        DoctorProfileId = doc.Id,
                        SlotDate = DateOnly.FromDateTime(date),
                        StartTime = new TimeOnly(min / 60, min % 60),
                        EndTime = new TimeOnly((min + slotMinutes) / 60, (min + slotMinutes) % 60),
                        Status = AppointmentSlotStatus.Available,
                        DoctorProfile = doc
                    });
                }
            }
        }

        CreateScheduleAndSlots(doc1, 9, 17, SlotDuration.Minutes60);  // 9-17, 60min slots
        CreateScheduleAndSlots(doc2, 8, 12, SlotDuration.Minutes60);  // 8-12, 60min slots (morning)
        CreateScheduleAndSlots(doc3, 14, 20, SlotDuration.Minutes60); // 14-20, 60min slots (afternoon/evening)
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 12. APPOINTMENTS (various statuses)
        // ═══════════════════════════════════════════════
        var pastSlots = context.AppointmentSlots
            .Where(s => s.SlotDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .ToList();

        var futureSlots = context.AppointmentSlots
            .Where(s => s.SlotDate > DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .ToList();

        // Helper to pick a slot for a specific doctor
        AppointmentSlot? PickSlot(List<AppointmentSlot> slots, Guid doctorId)
        {
            var slot = slots.FirstOrDefault(s => s.DoctorProfileId == doctorId && s.Status == AppointmentSlotStatus.Available);
            if (slot != null) { slot.Status = AppointmentSlotStatus.Booked; slots.Remove(slot); }
            return slot;
        }

        int bookingCounter = 1;
        Appointment CreateAppointment(AppointmentSlot slot, DoctorProfile doc, PatientProfile patient, AppointmentStatus status, string? notes = null)
        {
            var apt = new Appointment
            {
                BookingCode = $"BK-{DateTime.UtcNow:yyyyMMdd}-{bookingCounter++:D4}",
                AppointmentSlotId = slot.Id, DoctorId = doc.Id, PatientId = patient.Id,
                Status = status, Notes = notes,
                AppointmentSlot = slot, Doctor = doc, Patient = patient
            };
            if (status == AppointmentStatus.Approved) apt.ApprovedAt = DateTime.UtcNow.AddDays(-2);
            if (status == AppointmentStatus.Completed) { apt.ApprovedAt = DateTime.UtcNow.AddDays(-5); apt.CompletedAt = DateTime.UtcNow.AddDays(-1); }
            if (status == AppointmentStatus.Cancelled) { apt.CancelledAt = DateTime.UtcNow.AddDays(-1); apt.CancellationReason = "Bệnh nhân bận việc đột xuất."; }
            context.Appointments.Add(apt);
            return apt;
        }

        // Past completed appointments (for reviews)
        var completedApts = new List<Appointment>();
        var slot1 = PickSlot(pastSlots, doc1.Id);
        var slot2 = PickSlot(pastSlots, doc1.Id);
        var slot3 = PickSlot(pastSlots, doc3.Id);
        var slot4 = PickSlot(pastSlots, doc2.Id);

        if (slot1 != null) completedApts.Add(CreateAppointment(slot1, doc1, patient1, AppointmentStatus.Completed, "Tôi cảm thấy lo lắng và mất ngủ gần đây."));
        if (slot2 != null) completedApts.Add(CreateAppointment(slot2, doc1, patient2, AppointmentStatus.Completed, "Stress công việc kéo dài."));
        if (slot3 != null) completedApts.Add(CreateAppointment(slot3, doc3, patient1, AppointmentStatus.Completed, "Vấn đề giao tiếp trong gia đình."));
        if (slot4 != null) completedApts.Add(CreateAppointment(slot4, doc2, patient3, AppointmentStatus.Completed, "Con trai tôi có biểu hiện thu mình."));

        // Future pending/approved appointments
        var fSlot1 = PickSlot(futureSlots, doc1.Id);
        var fSlot2 = PickSlot(futureSlots, doc2.Id);
        var fSlot3 = PickSlot(futureSlots, doc3.Id);
        var fSlot4 = PickSlot(futureSlots, doc1.Id);

        if (fSlot1 != null) CreateAppointment(fSlot1, doc1, patient1, AppointmentStatus.Pending, "Tái khám theo lịch.");
        if (fSlot2 != null) CreateAppointment(fSlot2, doc2, patient2, AppointmentStatus.Approved, "Tư vấn lần đầu.");
        if (fSlot3 != null) CreateAppointment(fSlot3, doc3, patient3, AppointmentStatus.Pending, "Tham vấn gia đình.");

        // Cancelled appointment
        var cSlot = PickSlot(futureSlots, doc1.Id);
        if (cSlot != null) CreateAppointment(cSlot, doc1, patient3, AppointmentStatus.Cancelled);

        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 13. REVIEWS (from completed appointments)
        // ═══════════════════════════════════════════════
        var reviewData = new (int rating, string comment)[]
        {
            (5, "Bác sĩ rất tận tâm và lắng nghe. Tôi cảm thấy thoải mái hơn rất nhiều sau buổi tư vấn."),
            (4, "Tư vấn chuyên nghiệp, tuy nhiên thời gian hơi ngắn. Sẽ quay lại lần sau."),
            (5, "PGS rất giỏi, phân tích vấn đề rõ ràng và cho lời khuyên hữu ích cho gia đình tôi."),
            (4, "Bác sĩ hiểu tâm lý trẻ em rất tốt. Con tôi đã cởi mở hơn sau buổi trị liệu.")
        };

        for (int i = 0; i < completedApts.Count && i < reviewData.Length; i++)
        {
            var apt = completedApts[i];
            var (rating, comment) = reviewData[i];
            context.Reviews.Add(new Review
            {
                AppointmentId = apt.Id, DoctorId = apt.DoctorId, PatientId = apt.PatientId!.Value,
                Rating = rating, Comment = comment, IsVisible = true,
                Appointment = apt, Doctor = apt.Doctor, Patient = apt.Patient!
            });
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 14. BLOG POSTS (various statuses)
        // ═══════════════════════════════════════════════
        context.BlogPosts.Add(new BlogPost
        {
            DoctorId = doc1.Id, Doctor = doc1,
            Title = "5 Dấu Hiệu Bạn Đang Bị Trầm Cảm Mà Không Biết",
            Content = "<p>Trầm cảm là một rối loạn tâm thần phổ biến ảnh hưởng đến hàng triệu người trên thế giới. Nhiều người mắc trầm cảm mà không nhận ra...</p><h2>1. Mất hứng thú với mọi thứ</h2><p>Bạn từng yêu thích nhiều hoạt động nhưng giờ đây không còn quan tâm nữa...</p><h2>2. Thay đổi giấc ngủ</h2><p>Mất ngủ hoặc ngủ quá nhiều đều là dấu hiệu cảnh báo...</p><h2>3. Mệt mỏi kéo dài</h2><p>Cảm giác kiệt sức dù không làm gì nặng nhọc...</p><h2>4. Khó tập trung</h2><p>Trí nhớ kém, không thể đưa ra quyết định...</p><h2>5. Thay đổi cân nặng</h2><p>Ăn quá nhiều hoặc không muốn ăn...</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1541199249251-f713e6145474?w=800",
            Excerpt = "Trầm cảm không phải lúc nào cũng rõ ràng. Hãy nhận biết 5 dấu hiệu thường bị bỏ qua để tìm kiếm sự giúp đỡ kịp thời.",
            Status = BlogStatus.Published, ViewCount = 1250,
            SubmittedAt = DateTime.UtcNow.AddDays(-15), ApprovedAt = DateTime.UtcNow.AddDays(-14), ApprovedBy = csUser.Id,
            PublishedAt = DateTime.UtcNow.AddDays(-14)
        });

        context.BlogPosts.Add(new BlogPost
        {
            DoctorId = doc2.Id, Doctor = doc2,
            Title = "Hướng Dẫn Cha Mẹ: Nhận Biết Khi Con Cần Hỗ Trợ Tâm Lý",
            Content = "<p>Trẻ em thường không biết cách diễn đạt cảm xúc của mình. Cha mẹ cần chú ý đến các dấu hiệu sau...</p><h2>Thay đổi hành vi đột ngột</h2><p>Trẻ từ hoạt bát trở nên thu mình, hoặc từ ngoan ngoãn trở nên hung hăng...</p><h2>Kết quả học tập giảm sút</h2><p>Sự suy giảm trong học tập có thể là dấu hiệu của vấn đề tâm lý...</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?w=800",
            Excerpt = "Làm thế nào để biết con bạn đang gặp vấn đề tâm lý? Hướng dẫn dành cho phụ huynh.",
            Status = BlogStatus.Published, ViewCount = 890,
            SubmittedAt = DateTime.UtcNow.AddDays(-10), ApprovedAt = DateTime.UtcNow.AddDays(-9), ApprovedBy = csUser.Id,
            PublishedAt = DateTime.UtcNow.AddDays(-9)
        });

        context.BlogPosts.Add(new BlogPost
        {
            DoctorId = doc3.Id, Doctor = doc3,
            Title = "Giao Tiếp Hiệu Quả Trong Hôn Nhân: 7 Nguyên Tắc Vàng",
            Content = "<p>Giao tiếp là nền tảng của mọi mối quan hệ. Trong hôn nhân, cách bạn nói chuyện với nhau quyết định sự bền vững của mối quan hệ...</p><h2>1. Lắng nghe chủ động</h2><p>Hãy thực sự lắng nghe, không chỉ chờ đến lượt nói...</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1516589178581-6cd7833ae3b2?w=800",
            Excerpt = "7 nguyên tắc giao tiếp giúp vợ chồng hiểu nhau hơn và xây dựng hôn nhân bền vững.",
            Status = BlogStatus.Published, ViewCount = 2100,
            SubmittedAt = DateTime.UtcNow.AddDays(-20), ApprovedAt = DateTime.UtcNow.AddDays(-19), ApprovedBy = csUser.Id,
            PublishedAt = DateTime.UtcNow.AddDays(-19)
        });

        // Pending blog (waiting for moderation)
        context.BlogPosts.Add(new BlogPost
        {
            DoctorId = doc1.Id, Doctor = doc1,
            Title = "Thiền Định Và Sức Khỏe Tinh Thần: Khoa Học Nói Gì?",
            Content = "<p>Thiền định đã được nghiên cứu rộng rãi trong y học hiện đại...</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1508672019048-805c876b67e2?w=800",
            Excerpt = "Bằng chứng khoa học về lợi ích của thiền định đối với sức khỏe tâm thần.",
            Status = BlogStatus.Pending, ViewCount = 0,
            SubmittedAt = DateTime.UtcNow.AddDays(-1)
        });

        // Draft blog
        context.BlogPosts.Add(new BlogPost
        {
            DoctorId = doc2.Id, Doctor = doc2,
            Title = "Trò Chơi Trị Liệu: Play Therapy Cho Trẻ Em",
            Content = "<p>Play Therapy là phương pháp trị liệu sử dụng trò chơi để giúp trẻ em biểu đạt cảm xúc...</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1587654780291-39c9404d7dd0?w=800",
            Excerpt = "Tìm hiểu về phương pháp Play Therapy và cách nó giúp trẻ em vượt qua khó khăn tâm lý.",
            Status = BlogStatus.Draft, ViewCount = 0
        });
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 15. VERIFICATION REQUEST (for pending doctor)
        // ═══════════════════════════════════════════════
        context.VerificationRequests.Add(new VerificationRequest
        {
            DoctorProfileId = doc4.Id,
            Status = VerificationStatus.Submitted,
            DoctorProfile = doc4
        });
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 16. CONSULTATION RECORDS (for completed appointments)
        // ═══════════════════════════════════════════════
        if (completedApts.Count > 0)
        {
            context.ConsultationRecords.Add(new ConsultationRecord
            {
                AppointmentId = completedApts[0].Id,
                DoctorId = completedApts[0].DoctorId,
                PatientId = completedApts[0].PatientId!.Value,
                ConsultationSummary = "Bệnh nhân có biểu hiện lo lắng quá mức, mất ngủ, khó tập trung. Đã tiến hành CBT phiên 1. Hướng dẫn kỹ thuật thở sâu và ghi nhật ký lo âu.",
                Diagnosis = "Rối loạn lo âu lan tỏa (GAD)",
                Recommendation = "Tiếp tục CBT trong 6-8 phiên. Tập thở sâu 10 phút/ngày. Ghi nhật ký lo âu hàng ngày. Tái khám sau 2 tuần.",
                Appointment = completedApts[0],
                Doctor = completedApts[0].Doctor,
                Patient = completedApts[0].Patient!
            });
        }
        if (completedApts.Count > 2)
        {
            context.ConsultationRecords.Add(new ConsultationRecord
            {
                AppointmentId = completedApts[2].Id,
                DoctorId = completedApts[2].DoctorId,
                PatientId = completedApts[2].PatientId!.Value,
                ConsultationSummary = "Gia đình có xung đột giữa vợ chồng liên quan đến cách nuôi dạy con. Đã tiến hành phiên tham vấn gia đình. Xác định các mẫu giao tiếp tiêu cực.",
                Diagnosis = "Xung đột gia đình - vấn đề giao tiếp",
                Recommendation = "Lên lịch 4 phiên trị liệu gia đình. Tập luyện kỹ năng giao tiếp bất bạo lực. Cả hai vợ chồng cùng tham gia.",
                Appointment = completedApts[2],
                Doctor = completedApts[2].Doctor,
                Patient = completedApts[2].Patient!
            });
        }
        await context.SaveChangesAsync();
    }

    private static User CreateUser(
        OpcbsDbContext context,
        string email,
        string password,
        string fullName,
        string phone,
        Role role,
        bool isEmailVerified = true,
        UserStatus status = UserStatus.Active)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            PhoneNumber = phone,
            RoleId = role.Id,
            Role = role,
            Status = status,
            IsEmailVerified = isEmailVerified
        };
        context.Users.Add(user);
        return user;
    }
}
