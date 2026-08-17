using Microsoft.EntityFrameworkCore;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Infrastructure.Persistence;

namespace OPCBS.Infrastructure.Seed;

/// <summary>
/// Database seed data for initial setup.
/// Seeds roles, permissions, specializations, service packages, 10 approved doctors with schedules,
/// appointments, consultation notes, reviews, blogs, and psychometric assessments in English.
/// Fully idempotent — safely seeds missing data even on partially existing databases.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(OpcbsDbContext context)
    {
        // ═══════════════════════════════════════════════
        // 1. SEED ROLES
        // ═══════════════════════════════════════════════
        var existingRoles = await context.Roles.ToListAsync();
        var roles = existingRoles.ToDictionary(r => r.Name, r => r);
        foreach (var roleName in RoleConstants.AllRoles)
        {
            if (!roles.ContainsKey(roleName))
            {
                var role = new Role { Name = roleName, Description = $"{roleName} role" };
                roles[roleName] = role;
                context.Roles.Add(role);
            }
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 2. SEED PERMISSIONS
        // ═══════════════════════════════════════════════
        var permissionCodes = new[]
        {
            PermissionConstants.ManageOwnProfile,
            PermissionConstants.ViewDoctors,
            PermissionConstants.ManageDoctorProfile,
            PermissionConstants.ManageSchedule,
            PermissionConstants.ManageDoctorAppointments,
            PermissionConstants.ManageConsultationNotes,
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

        var existingPerms = await context.Permissions.ToListAsync();
        var permissions = existingPerms.ToDictionary(p => p.Code, p => p);
        foreach (var code in permissionCodes)
        {
            if (!permissions.ContainsKey(code))
            {
                var perm = new Permission { Code = code, Description = code.Replace("_", " ") };
                permissions[code] = perm;
                context.Permissions.Add(perm);
            }
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 3. SEED ROLE-PERMISSION MAPPINGS
        // ═══════════════════════════════════════════════
        var existingRolePermPairs = await context.RolePermissions
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();
        var rolePermSet = existingRolePermPairs
            .Select(rp => (rp.RoleId, rp.PermissionId))
            .ToHashSet();

        void MapPermission(string roleName, string permCode)
        {
            if (!roles.ContainsKey(roleName) || !permissions.ContainsKey(permCode)) return;
            var r = roles[roleName];
            var p = permissions[permCode];
            var key = (r.Id, p.Id);
            if (!rolePermSet.Contains(key))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = r.Id,
                    PermissionId = p.Id,
                    Role = r,
                    Permission = p
                });
                rolePermSet.Add(key);
            }
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
        MapPermission(RoleConstants.Doctor, PermissionConstants.ManageConsultationNotes);
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

        // ═══════════════════════════════════════════════
        // 4. SEED SPECIALIZATIONS
        // ═══════════════════════════════════════════════
        var specializations = new[] {
            ("Clinical Psychology", "Assessment and evidence-based treatment of moderate to severe psychological disorders"),
            ("Counseling Psychology", "Holistic support for life transitions, stress management, and personal growth"),
            ("Child & Adolescent Psychology", "Developmental and emotional mental health care tailored for kids and teens"),
            ("Depression & Mood Disorders", "Specialized therapeutic interventions for major depression and bipolar mood fluctuations"),
            ("Anxiety & Stress Management", "Targeted strategies for panic disorder, generalized anxiety, social phobia, and acute stress"),
            ("Trauma & PTSD", "Specialized trauma processing, EMDR, and recovery for complex trauma survivors"),
            ("Addiction & Substance Abuse", "Comprehensive recovery programs for substance dependency and behavioral addictions"),
            ("Family & Marriage Counseling", "Systemic therapy for relationship enrichment, conflict resolution, and marital stability"),
            ("Career Counseling", "Guidance on workplace stress, executive burnout, leadership resilience, and career redirection"),
            ("Cognitive Behavioral Therapy", "Structured cognitive restructuring and behavioral modification therapy (CBT & DBT)")
        };

        var existingSpecs = await context.Specializations.ToListAsync();
        var specDict = existingSpecs.ToDictionary(s => s.Name, s => s);
        var specEntities = new List<Specialization>();

        foreach (var (name, desc) in specializations)
        {
            if (specDict.TryGetValue(name, out var existingSpec))
            {
                specEntities.Add(existingSpec);
            }
            else
            {
                var spec = new Specialization { Name = name, Description = desc };
                specEntities.Add(spec);
                specDict[name] = spec;
                context.Specializations.Add(spec);
            }
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 5. SEED SERVICE PACKAGES
        // ═══════════════════════════════════════════════
        var existingPkgs = await context.ServicePackages.ToListAsync();
        var freePkg = existingPkgs.FirstOrDefault(p => p.Name == "Free Trial")
            ?? new ServicePackage { Name = "Free Trial", Description = "Free trial for evaluation — no payment required", DurationDays = 30, Price = 0, MaxPatientCapacity = 5, MaxDailySlotsCapacity = 3, DisplayOrder = 0 };
        var basicPkg = existingPkgs.FirstOrDefault(p => p.Name == "Basic Practice" || p.Name == "Basic")
            ?? new ServicePackage { Name = "Basic Practice", Description = "Starter plan for independent clinical practitioners", DurationDays = 30, Price = 299000, MaxPatientCapacity = 10, MaxDailySlotsCapacity = 5, DisplayOrder = 1 };
        var proPkg = existingPkgs.FirstOrDefault(p => p.Name == "Professional Practice" || p.Name == "Professional")
            ?? new ServicePackage { Name = "Professional Practice", Description = "Expanded tier with elevated capacity and featured listings", DurationDays = 90, Price = 799000, MaxPatientCapacity = 35, MaxDailySlotsCapacity = 12, IsFeatured = true, DisplayOrder = 2 };
        var premPkg = existingPkgs.FirstOrDefault(p => p.Name == "Premium Practice" || p.Name == "Premium")
            ?? new ServicePackage { Name = "Premium Practice", Description = "Unlimited enterprise access with top priority booking", DurationDays = 365, Price = 2499000, MaxPatientCapacity = 120, MaxDailySlotsCapacity = 25, DisplayOrder = 3 };

        if (!existingPkgs.Any())
        {
            context.ServicePackages.AddRange(freePkg, basicPkg, proPkg, premPkg);
            await context.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════
        // 6. SEED SYSTEM CONFIG
        // ═══════════════════════════════════════════════
        var existingConfigs = await context.SystemConfigs.ToDictionaryAsync(c => c.Key, c => c);
        void EnsureConfig(string key, string val, string desc, string type = "string")
        {
            if (!existingConfigs.ContainsKey(key))
            {
                context.SystemConfigs.Add(new SystemConfig { Key = key, Value = val, Description = desc, DataType = type });
            }
        }
        EnsureConfig("OtpExpirationMinutes", "10", "OTP expiration time in minutes", "int");
        EnsureConfig("MaxLoginAttempts", "5", "Maximum login attempts before lockout", "int");
        EnsureConfig("AppName", "MindBridge - Online Psychological Counseling & Therapy Platform", "Application display name");
        EnsureConfig("DefaultConsultationFee", "500000", "Default consultation fee in VND", "decimal");
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 7. STAFF ACCOUNTS
        // ═══════════════════════════════════════════════
        var adminUser = await EnsureUserAsync(context, "admin@opcbs.com", "Admin@123", "Alexander Vance (System Admin)", "0900000001", roles[RoleConstants.SystemAdmin]);
        var csUser = await EnsureUserAsync(context, "support@opcbs.com", "Support@123", "Sarah Jenkins (Support Specialist)", "0900000002", roles[RoleConstants.CustomerSupport]);
        var bmUser = await EnsureUserAsync(context, "manager@opcbs.com", "Manager@123", "David Sterling (Business Manager)", "0900000003", roles[RoleConstants.BusinessManager]);
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 8. PATIENTS (6 active patient accounts)
        // ═══════════════════════════════════════════════
        var patUsers = new[]
        {
            await EnsureUserAsync(context, "patient@opcbs.com", "Patient@123", "Jonathan Miller", "0912345001", roles[RoleConstants.Patient]),
            await EnsureUserAsync(context, "patient2@opcbs.com", "Patient@123", "Emily Watson", "0912345002", roles[RoleConstants.Patient]),
            await EnsureUserAsync(context, "patient3@opcbs.com", "Patient@123", "Michael Chang", "0912345003", roles[RoleConstants.Patient]),
            await EnsureUserAsync(context, "patient4@opcbs.com", "Patient@123", "Olivia Bennett", "0912345004", roles[RoleConstants.Patient]),
            await EnsureUserAsync(context, "patient5@opcbs.com", "Patient@123", "Lucas Campbell", "0912345005", roles[RoleConstants.Patient]),
            await EnsureUserAsync(context, "patient6@opcbs.com", "Patient@123", "Rachel Adams", "0912345006", roles[RoleConstants.Patient])
        };

        // Test accounts for verification & auth testing
        await EnsureUserAsync(context, "unverified@opcbs.com", "Unverified@123", "Unverified User Account", "0900000004", roles[RoleConstants.Patient], isEmailVerified: false, status: UserStatus.Inactive);
        await EnsureUserAsync(context, "locked@opcbs.com", "Locked@123", "Locked User Account", "0900000005", roles[RoleConstants.Patient], isEmailVerified: true, status: UserStatus.Locked);
        await context.SaveChangesAsync();

        var existingPatProfiles = await context.PatientProfiles.Include(p => p.User).ToListAsync();
        var patientProfiles = new List<PatientProfile>();

        for (int i = 0; i < patUsers.Length; i++)
        {
            var u = patUsers[i];
            var pProf = existingPatProfiles.FirstOrDefault(p => p.UserId == u.Id);
            if (pProf == null)
            {
                pProf = new PatientProfile
                {
                    UserId = u.Id,
                    User = u,
                    DateOfBirth = new DateTime(1993 + i, (i % 12) + 1, (i % 25) + 1),
                    Gender = (i % 2 == 0) ? Gender.Male : Gender.Female,
                    Address = $"{100 + i * 20} Main Street, Suite {i + 1}"
                };
                context.PatientProfiles.Add(pProf);
            }
            patientProfiles.Add(pProf);
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 9. DOCTORS (10 Approved & Fully Verified Doctors)
        // ═══════════════════════════════════════════════
        var doctorData = new[]
        {
            new
            {
                Email = "doctor@opcbs.com",
                Name = "Dr. Eleanor Vance, Ph.D.",
                Phone = "0987654301",
                Title = "Senior Clinical Psychologist",
                Bio = "Over 14 years of clinical experience specializing in adult mood disorders, complex trauma, and evidence-based Cognitive Behavioral Therapy (CBT). Ph.D. in Clinical Psychology from Stanford University.",
                Exp = 14,
                Lic = "LIC-US-2024-001",
                Rating = 4.9m,
                Reviews = 42,
                Specs = new[] { 0, 3, 9 }, // Clinical, Depression, CBT
                Pkg = premPkg
            },
            new
            {
                Email = "doctor2@opcbs.com",
                Name = "Dr. Marcus Sterling, Psy.D.",
                Phone = "0987654302",
                Title = "Child & Adolescent Psychologist",
                Bio = "Dedicated to supporting children, teenagers, and family systems through developmental transitions, ADHD management, and emotional regulation using play and behavioral therapy. 9 years of hospital experience.",
                Exp = 9,
                Lic = "LIC-US-2024-002",
                Rating = 4.8m,
                Reviews = 28,
                Specs = new[] { 2, 7, 1 }, // Child, Family, Counseling
                Pkg = proPkg
            },
            new
            {
                Email = "doctor3@opcbs.com",
                Name = "Prof. Sophia Ramirez, Ph.D.",
                Phone = "0987654303",
                Title = "Professor of Marital & Family Therapy",
                Bio = "21 years of research and clinical practice in relational dynamics, conflict de-escalation, and marriage rejuvenation. Author of multiple peer-reviewed relational psychology studies.",
                Exp = 21,
                Lic = "LIC-US-2024-003",
                Rating = 4.95m,
                Reviews = 65,
                Specs = new[] { 7, 1, 5 }, // Family, Counseling, Trauma
                Pkg = premPkg
            },
            new
            {
                Email = "doctor4@opcbs.com",
                Name = "Dr. Julian Hayes, M.D., Ph.D.",
                Phone = "0987654304",
                Title = "Neuropsychiatrist & Stress Management Specialist",
                Bio = "Combines neurobiology and psychotherapy to assist corporate professionals and executives overcoming chronic burnout, panic attacks, and severe workplace stressors. 11 years in private practice.",
                Exp = 11,
                Lic = "LIC-US-2024-004",
                Rating = 4.75m,
                Reviews = 34,
                Specs = new[] { 4, 8, 0 }, // Anxiety, Career, Clinical
                Pkg = proPkg
            },
            new
            {
                Email = "doctor5@opcbs.com",
                Name = "Dr. Clara Bennett, Psy.D.",
                Phone = "0987654305",
                Title = "Trauma & PTSD Specialist (Certified EMDR)",
                Bio = "Certified EMDR consultant and somatic experiencing practitioner focusing on acute trauma recovery, complex PTSD, and emotional resilience for survivors of trauma. 10 years of clinical practice.",
                Exp = 10,
                Lic = "LIC-US-2024-005",
                Rating = 4.85m,
                Reviews = 31,
                Specs = new[] { 5, 0, 4 }, // Trauma, Clinical, Anxiety
                Pkg = proPkg
            },
            new
            {
                Email = "doctor6@opcbs.com",
                Name = "Dr. David Kim, Ph.D.",
                Phone = "0987654306",
                Title = "Addiction & Behavioral Health Consultant",
                Bio = "12 years assisting individuals in overcoming chemical dependencies, behavioral addictions, and digital overuse through motivational interviewing and structured relapse prevention.",
                Exp = 12,
                Lic = "LIC-US-2024-006",
                Rating = 4.7m,
                Reviews = 22,
                Specs = new[] { 6, 9, 1 }, // Addiction, CBT, Counseling
                Pkg = basicPkg
            },
            new
            {
                Email = "doctor7@opcbs.com",
                Name = "Dr. Olivia Patel, Psy.D.",
                Phone = "0987654307",
                Title = "Mood Disorders & Mindfulness-Based Therapist",
                Bio = "Expert in Acceptance and Commitment Therapy (ACT) and Dialectical Behavior Therapy (DBT) for persistent depression, mood swings, and perfectionism-induced stress. 8 years clinical experience.",
                Exp = 8,
                Lic = "LIC-US-2024-007",
                Rating = 4.8m,
                Reviews = 25,
                Specs = new[] { 3, 9, 0 }, // Depression, CBT, Clinical
                Pkg = proPkg
            },
            new
            {
                Email = "doctor8@opcbs.com",
                Name = "Dr. Ethan Wright, Ph.D.",
                Phone = "0987654308",
                Title = "Vocational Psychologist & Executive Counselor",
                Bio = "Focuses on career transitions, imposter syndrome, leadership psychology, and high-performance burnout prevention. Consults with leading creative and technology enterprises.",
                Exp = 13,
                Lic = "LIC-US-2024-008",
                Rating = 4.65m,
                Reviews = 19,
                Specs = new[] { 8, 4, 1 }, // Career, Anxiety, Counseling
                Pkg = basicPkg
            },
            new
            {
                Email = "doctor9@opcbs.com",
                Name = "Dr. Hannah Schmidt, Psy.D.",
                Phone = "0987654309",
                Title = "Adolescent Mental Health & Family Systems Therapist",
                Bio = "Specializes in adolescent emotional challenges, peer anxiety, identity development, and repairing strained parent-teen communication through compassionate systemic family counseling.",
                Exp = 7,
                Lic = "LIC-US-2024-009",
                Rating = 4.9m,
                Reviews = 29,
                Specs = new[] { 2, 7, 3 }, // Child, Family, Depression
                Pkg = proPkg
            },
            new
            {
                Email = "doctor10@opcbs.com",
                Name = "Dr. Alexander Brooks, M.D.",
                Phone = "0987654310",
                Title = "Integrative Mental Health & Sleep Specialist",
                Bio = "Board-certified psychiatrist and sleep specialist focusing on non-pharmacological sleep CBT-I, circadian rhythm alignment, and anxiety reduction for chronic insomnia patients. 15 years experience.",
                Exp = 15,
                Lic = "LIC-US-2024-010",
                Rating = 4.92m,
                Reviews = 47,
                Specs = new[] { 4, 0, 9 }, // Anxiety, Clinical, CBT
                Pkg = premPkg
            }
        };

        var existingDocProfiles = await context.DoctorProfiles.Include(d => d.User).ToListAsync();
        var doctorProfiles = new List<DoctorProfile>();

        for (int i = 0; i < doctorData.Length; i++)
        {
            var data = doctorData[i];
            var user = await EnsureUserAsync(context, data.Email, "Doctor@123", data.Name, data.Phone, roles[RoleConstants.Doctor]);
            var docProfile = existingDocProfiles.FirstOrDefault(d => d.UserId == user.Id);
            if (docProfile == null)
            {
                docProfile = new DoctorProfile
                {
                    UserId = user.Id,
                    User = user,
                    ProfessionalTitle = data.Title,
                    Biography = data.Bio,
                    ExperienceYears = data.Exp,
                    LicenseNumber = data.Lic,
                    LicenseExpiryDate = new DateTime(2029, 12, 31),
                    VerificationStatus = VerificationStatus.Approved,
                    IsVisible = true,
                    AverageRating = data.Rating,
                    ReviewCount = data.Reviews
                };
                context.DoctorProfiles.Add(docProfile);
                await context.SaveChangesAsync();

                // Active Subscription
                context.DoctorSubscriptions.Add(new DoctorSubscription
                {
                    DoctorProfile = docProfile,
                    DoctorProfileId = docProfile.Id,
                    ServicePackage = data.Pkg,
                    ServicePackageId = data.Pkg.Id,
                    Status = SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow.AddDays(-45),
                    ExpirationDate = DateTime.UtcNow.AddDays(data.Pkg.DurationDays)
                });

                // Doctor Specializations
                foreach (var idx in data.Specs)
                {
                    if (idx < specEntities.Count)
                    {
                        context.DoctorSpecializations.Add(new DoctorSpecialization
                        {
                            DoctorProfileId = docProfile.Id,
                            SpecializationId = specEntities[idx].Id,
                            DoctorProfile = docProfile,
                            Specialization = specEntities[idx]
                        });
                    }
                }
                await context.SaveChangesAsync();
            }
            else
            {
                // Ensure approved and visible
                docProfile.VerificationStatus = VerificationStatus.Approved;
                docProfile.IsVisible = true;
                docProfile.ProfessionalTitle = data.Title;
                docProfile.Biography = data.Bio;
                docProfile.AverageRating = data.Rating;
                docProfile.ReviewCount = data.Reviews;
                context.DoctorProfiles.Update(docProfile);
            }
            doctorProfiles.Add(docProfile);
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 10. SCHEDULES & SLOTS FOR ALL 10 DOCTORS
        // ═══════════════════════════════════════════════
        var workDays = DayOfWeekEnum.Monday | DayOfWeekEnum.Tuesday | DayOfWeekEnum.Wednesday | DayOfWeekEnum.Thursday | DayOfWeekEnum.Friday;
        var today = DateTime.UtcNow.Date;
        var validWeekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        var existingSchedules = await context.Schedules.ToListAsync();
        for (int i = 0; i < doctorProfiles.Count; i++)
        {
            var doc = doctorProfiles[i];
            var sched = existingSchedules.FirstOrDefault(s => s.DoctorProfileId == doc.Id);
            int startHour = (i % 2 == 0) ? 8 : 13;
            int endHour = (i % 2 == 0) ? 14 : 19;

            if (sched == null)
            {
                context.Schedules.Add(new Schedule
                {
                    DoctorProfileId = doc.Id,
                    WorkingDays = workDays,
                    StartTime = new TimeOnly(startHour, 0),
                    EndTime = new TimeOnly(endHour, 0),
                    SlotDuration = SlotDuration.Minutes60,
                    IsActive = true,
                    DoctorProfile = doc,
                    SlotsPerDay = endHour - startHour
                });
            }

            var hasSlots = await context.AppointmentSlots.AnyAsync(s => s.DoctorProfileId == doc.Id);
            if (!hasSlots)
            {
                for (int dayOffset = -7; dayOffset <= 21; dayOffset++)
                {
                    var date = today.AddDays(dayOffset);
                    if (!validWeekdays.Contains(date.DayOfWeek)) continue;

                    for (int hour = startHour; hour < endHour; hour++)
                    {
                        context.AppointmentSlots.Add(new AppointmentSlot
                        {
                            DoctorProfileId = doc.Id,
                            SlotDate = DateOnly.FromDateTime(date),
                            StartTime = new TimeOnly(hour, 0),
                            EndTime = new TimeOnly(hour + 1, 0),
                            Status = AppointmentSlotStatus.Available,
                            ConsultationMode = (hour % 2 == 0) ? ConsultationMode.Online : ConsultationMode.Offline,
                            Price = 500000m + (i * 20000m),
                            DoctorProfile = doc
                        });
                    }
                }
            }
        }
        await context.SaveChangesAsync();

        // ═══════════════════════════════════════════════
        // 11. APPOINTMENTS (1-2 Patients per Doctor with Past Completed & Future Appointments)
        // ═══════════════════════════════════════════════
        var hasAppointments = await context.Appointments.AnyAsync();
        if (!hasAppointments)
        {
            var allSlots = await context.AppointmentSlots.OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime).ToListAsync();
            var pastSlots = allSlots.Where(s => s.SlotDate < DateOnly.FromDateTime(today)).ToList();
            var futureSlots = allSlots.Where(s => s.SlotDate > DateOnly.FromDateTime(today)).ToList();

            AppointmentSlot? PickSlot(List<AppointmentSlot> slots, Guid doctorId)
            {
                var slot = slots.FirstOrDefault(s => s.DoctorProfileId == doctorId && s.Status == AppointmentSlotStatus.Available);
                if (slot != null)
                {
                    slot.Status = AppointmentSlotStatus.Booked;
                    slot.CurrentBookings = 1;
                    slots.Remove(slot);
                }
                return slot;
            }

            int bookingCounter = 100;
            var completedAppointments = new List<(Appointment Apt, string Diagnosis, string Summary, string Rec, int Rating, string ReviewText)>();

            for (int i = 0; i < doctorProfiles.Count; i++)
            {
                var doc = doctorProfiles[i];
                var primaryPatient = patientProfiles[i % patientProfiles.Count];
                var secondaryPatient = patientProfiles[(i + 1) % patientProfiles.Count];

                // 1. Past Completed Appointment with Primary Patient
                var pastSlot1 = PickSlot(pastSlots, doc.Id);
                if (pastSlot1 != null)
                {
                    var apt = new Appointment
                    {
                        BookingCode = $"BK-{DateTime.UtcNow:yyyyMMdd}-{bookingCounter++:D4}",
                        AppointmentSlotId = pastSlot1.Id,
                        DoctorId = doc.Id,
                        PatientId = primaryPatient.Id,
                        Status = AppointmentStatus.Completed,
                        Notes = "Initial consultation and mental health diagnostic assessment.",
                        ApprovedAt = DateTime.UtcNow.AddDays(-6),
                        CompletedAt = DateTime.UtcNow.AddDays(-2),
                        AppointmentSlot = pastSlot1,
                        Doctor = doc,
                        Patient = primaryPatient
                    };
                    context.Appointments.Add(apt);

                    completedAppointments.Add((
                        apt,
                        Diagnosis: GetSampleDiagnosis(i),
                        Summary: $"Completed full clinical intake session with {primaryPatient.User?.FullName ?? "the patient"}. Patient presented with symptoms aligned with therapeutic focus. Commenced preliminary cognitive restructuring exercises.",
                        Rec: "Schedule 6 bi-weekly follow-up sessions. Practice mindfulness breathing for 10 minutes daily. Maintain symptom tracking log.",
                        Rating: (i % 3 == 0) ? 5 : 4,
                        ReviewText: GetSampleReview(i, primaryPatient.User?.FullName ?? "Patient")
                    ));
                }

                // 2. Future Approved / Pending Appointment with Secondary Patient
                var futureSlot1 = PickSlot(futureSlots, doc.Id);
                if (futureSlot1 != null)
                {
                    var apt = new Appointment
                    {
                        BookingCode = $"BK-{DateTime.UtcNow:yyyyMMdd}-{bookingCounter++:D4}",
                        AppointmentSlotId = futureSlot1.Id,
                        DoctorId = doc.Id,
                        PatientId = secondaryPatient.Id,
                        Status = (i % 2 == 0) ? AppointmentStatus.Approved : AppointmentStatus.Pending,
                        Notes = "Follow-up consultation regarding progress and behavioral homework review.",
                        ApprovedAt = (i % 2 == 0) ? DateTime.UtcNow.AddDays(-1) : null,
                        AppointmentSlot = futureSlot1,
                        Doctor = doc,
                        Patient = secondaryPatient
                    };
                    context.Appointments.Add(apt);
                }
            }
            await context.SaveChangesAsync();

            // ═══════════════════════════════════════════════
            // 12. PATIENT RECORDS, CONSULTATION NOTES & REVIEWS
            // ═══════════════════════════════════════════════
            foreach (var item in completedAppointments)
            {
                var pRecord = new PatientRecord
                {
                    DoctorId = item.Apt.DoctorId,
                    PatientId = item.Apt.PatientId,
                    Doctor = item.Apt.Doctor,
                    Patient = item.Apt.Patient,
                    GeneralNotes = $"Established clinical record for {item.Apt.Patient?.User?.FullName ?? "Patient"}. Primary diagnostic pathway initiated."
                };
                context.PatientRecords.Add(pRecord);

                context.ConsultationNotes.Add(new ConsultationNote
                {
                    AppointmentId = item.Apt.Id,
                    DoctorId = item.Apt.DoctorId,
                    PatientRecord = pRecord,
                    ConsultationSummary = item.Summary,
                    Diagnosis = item.Diagnosis,
                    Recommendation = item.Rec,
                    Appointment = item.Apt,
                    Doctor = item.Apt.Doctor
                });

                context.Reviews.Add(new Review
                {
                    AppointmentId = item.Apt.Id,
                    DoctorId = item.Apt.DoctorId,
                    PatientId = item.Apt.PatientId!.Value,
                    Rating = item.Rating,
                    Comment = item.ReviewText,
                    IsVisible = true,
                    Appointment = item.Apt,
                    Doctor = item.Apt.Doctor,
                    Patient = item.Apt.Patient!
                });
            }
            await context.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════
        // 13. SEED BLOG POSTS (2-3 articles per doctor)
        // ═══════════════════════════════════════════════
        var hasBlogs = await context.BlogPosts.AnyAsync();
        if (!hasBlogs)
        {
            var blogSeedData = new[]
            {
                // Doctor 1
                (
                    DocIdx: 0,
                    Title: "5 Hidden Signs of High-Functioning Depression You Shouldn't Ignore",
                    Excerpt: "High-functioning depression often disguises itself as productivity and composure. Learn the subtle psychological signs and when to seek clinical guidance.",
                    Content: "<p>Depression does not always look like someone unable to get out of bed. High-functioning depression (persistent depressive disorder or dysthymia) frequently hides behind a mask of professional success, punctuality, and social smiles.</p><h3>1. Constant Underlying Exhaustion</h3><p>Even after 8 hours of sleep, individuals experience a persistent heavy fatigue that caffeine or rest fails to alleviate.</p><h3>2. The 'Impostor' Happiness Phenomenon</h3><p>Laughing in social settings while feeling completely detached and empty inside.</p><h3>3. Relentless Self-Criticism</h3><p>Viewing personal accomplishments through a lens of inadequacy, feeling you're always one mistake away from failure.</p><h3>4. Gradual Loss of Genuine Joy (Anhedonia)</h3><p>Participating in hobbies mechanically without the spark of true satisfaction.</p><h3>5. Overwhelming Need for Isolation After Socializing</h3><p>The immense emotional effort required to appear 'normal' drains energy rapidly.</p><p>If you recognize these symptoms, remember that seeking therapy is a proactive step toward emotional vitality, not a sign of weakness.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1541199249251-f713e6145474?w=800",
                    Views: 1420
                ),
                (
                    DocIdx: 0,
                    Title: "Cognitive Restructuring: How to Rewire Negative Thought Loops",
                    Excerpt: "Cognitive Behavioral Therapy offers proven tools to challenge automatic negative thoughts and build mental resilience.",
                    Content: "<p>Our thoughts dictate our emotions, which in turn drive our behaviors. When cognitive distortions like catastrophizing or black-and-white thinking take hold, our reality becomes distorted.</p><h3>The 3-Step Cognitive Shift:</h3><ol><li><strong>Catch It:</strong> Identify the automatic negative thought as soon as emotional distress spikes.</li><li><strong>Check It:</strong> Ask yourself: What objective evidence supports this thought? What evidence contradicts it?</li><li><strong>Change It:</strong> Formulate a balanced, realistic replacement thought grounded in facts rather than fear.</li></ol><p>Consistent practice trains neural pathways to default to constructive problem-solving rather than self-defeating loops.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1499209974431-9dddcece7f88?w=800",
                    Views: 980
                ),
                (
                    DocIdx: 0,
                    Title: "The Neuroscience of Chronic Anxiety and How Somatic Grounding Helps",
                    Excerpt: "Understand what happens in your amygdala during an anxiety spiral and how somatic grounding calms your nervous system.",
                    Content: "<p>When the amygdala perceives a threat, it triggers the autonomic nervous system to flood the body with cortisol and adrenaline. Somatic grounding techniques provide physical anchors that signal safety directly to the brainstem.</p><p>Techniques such as diaphragmatic breathing with extended exhales activate the vagus nerve, initiating parasympathetic recovery within minutes.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=800",
                    Views: 1150
                ),

                // Doctor 2
                (
                    DocIdx: 1,
                    Title: "Helping Children Navigate School Anxiety and Social Pressure",
                    Excerpt: "A clinical guide for parents on identifying anxiety in school-age children and fostering emotional resilience.",
                    Content: "<p>Children often lack the vocabulary to articulate anxiety directly. Instead, school-related stress frequently manifests as stomachaches, morning tantrums, bedtime resistance, or sudden academic decline.</p><h3>Practical Strategies for Parents:</h3><ul><li><strong>Validate Emotions First:</strong> Avoid dismissing fears with 'You will be fine.' Instead, try: 'I can see how overwhelming this feels. We will figure it out together.'</li><li><strong>Create Predictable Routines:</strong> Consistency in morning and evening rituals provides emotional safety.</li><li><strong>Break Challenges into Micro-Steps:</strong> Desensitize school fears gradually through small, achievable goals.</li></ul>",
                    Thumbnail: "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?w=800",
                    Views: 850
                ),
                (
                    DocIdx: 1,
                    Title: "Effective Discipline Without Drama: The Connection-Before-Correction Rule",
                    Excerpt: "Learn why emotional regulation in parents is the cornerstone of healthy behavioral guidance in developing children.",
                    Content: "<p>When children act out, their logical prefrontal cortex is offline. Yelling or harsh punishment pushes their nervous system deeper into fight-or-flight mode. Establishing connection before attempting behavioral correction creates receptivity and long-term emotional intelligence.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1485546246426-74dc88dec4d9?w=800",
                    Views: 720
                ),

                // Doctor 3
                (
                    DocIdx: 2,
                    Title: "The 7 Principles for Long-Term Relationship Longevity",
                    Excerpt: "Insights from 20 years of marital therapy on building enduring trust, emotional intimacy, and collaborative conflict resolution.",
                    Content: "<p>Lasting relationships are built on deliberate micro-habits rather than grand romantic gestures. Research demonstrates that couples who nurture 'emotional bids'—small moments of daily connection—navigate inevitable conflicts with far greater resilience.</p><h3>Core Pillars of Relational Health:</h3><ol><li>Maintain positive sentiment override.</li><li>Turn toward each other's emotional bids.</li><li>Practice gentle startup during disagreements.</li><li>Accept influence and value mutual perspectives.</li><li>Create shared relational rituals and meaning.</li></ol>",
                    Thumbnail: "https://images.unsplash.com/photo-1516589178581-6cd7833ae3b2?w=800",
                    Views: 2150
                ),
                (
                    DocIdx: 2,
                    Title: "De-escalating Heated Arguments: Nonviolent Communication at Home",
                    Excerpt: "How adopting observations, feelings, needs, and requests can transform destructive marital disputes into deeper connection.",
                    Content: "<p>Arguments become toxic when criticism triggers defensive walls. Transitioning from accusatory 'You always' statements to vulnerable 'I feel / I need' expressions allows partners to listen without preparing a counter-attack.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800",
                    Views: 1640
                ),

                // Doctor 4
                (
                    DocIdx: 3,
                    Title: "The Physiology of Executive Burnout: Prevention & Biological Recovery",
                    Excerpt: "Burnout is not a mental flaw—it is physiological adrenal and neurological exhaustion. Here is how modern leaders recover.",
                    Content: "<p>Prolonged high-stakes decision-making depletes dopamine reserves and dysregulates circadian cortisol curves. Recovery requires aggressive boundary setting, biological sleep optimization, and structured cognitive disengagement.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=800",
                    Views: 1320
                ),
                (
                    DocIdx: 3,
                    Title: "Overcoming Panic Attacks: What to Do When Your Fight-or-Flight System Misfires",
                    Excerpt: "A physician's blueprint to stopping panic attacks in their tracks using the mammalian dive reflex and physiological sighs.",
                    Content: "<p>Panic attacks are essentially false alarms triggered by your nervous system. By utilizing the double inhale followed by a long sigh (the physiological sigh), you instantly trigger pulmonary gas exchange that decelerates your heart rate.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1474418397713-7ede21d49118?w=800",
                    Views: 1890
                ),

                // Doctor 5
                (
                    DocIdx: 4,
                    Title: "Demystifying EMDR: How Bilateral Stimulation Rewires Traumatic Memory",
                    Excerpt: "An inside look at Eye Movement Desensitization and Reprocessing (EMDR) and how it helps the brain process unintegrated trauma.",
                    Content: "<p>Traumatic memories often remain 'frozen' in their original sensory state within the amygdala and hippocampus. EMDR uses bilateral sensory stimulation to facilitate natural neurobiological memory consolidation.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1518241353330-0f7941c2d9b5?w=800",
                    Views: 1450
                ),
                (
                    DocIdx: 4,
                    Title: "Somatic Grounding Techniques for Acute Anxiety Triggers",
                    Excerpt: "Practical body-based anchors to bring your prefrontal cortex back online when feeling triggered.",
                    Content: "<p>The 5-4-3-2-1 sensory technique and progressive muscle release directly interrupt sympathetic arousal, anchoring awareness safely in the present moment.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1508672019048-805c876b67e2?w=800",
                    Views: 990
                ),

                // Doctor 6
                (
                    DocIdx: 5,
                    Title: "The Dopamine Loop: Breaking Free from Digital and Behavioral Compulsions",
                    Excerpt: "Why modern apps and digital environments hijack our reward pathways and how to implement a sustainable dopamine reset.",
                    Content: "<p>Behavioral addiction thrives on intermittent variable rewards. Reclaiming autonomy requires friction architectures—placing intentional barriers between impulse and consumption.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1517849845537-4d257902454a?w=800",
                    Views: 1120
                ),
                (
                    DocIdx: 5,
                    Title: "Motivational Interviewing: Unlocking Your Internal Drive for Lasting Change",
                    Excerpt: "Why willpower alone fails and how clarifying personal core values drives sustainable psychological transformation.",
                    Content: "<p>Sustainable change happens when the discrepancy between our daily behaviors and our deepest values is explored without judgment, cultivating intrinsic motivation.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1519834785169-98be25ec3f84?w=800",
                    Views: 830
                ),

                // Doctor 7
                (
                    DocIdx: 6,
                    Title: "Acceptance & Commitment Therapy: Moving Beyond the Battle with Thoughts",
                    Excerpt: "Learn how psychological flexibility and values-aligned action provide freedom from chronic emotional struggle.",
                    Content: "<p>Rather than exhausting energy trying to eliminate negative emotions, ACT teaches psychological defusion: observing thoughts like leaves floating down a stream while committing to values-aligned living.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1470240731273-7821a6eeb6bd?w=800",
                    Views: 1210
                ),
                (
                    DocIdx: 6,
                    Title: "Breaking the Cycle of Chronic Rumination and Overthinking",
                    Excerpt: "Distinguish between constructive problem-solving and toxic mental churning, and learn clinical tools to disrupt the pattern.",
                    Content: "<p>Rumination masquerades as preparation but creates paralysis. Establishing a scheduled 15-minute 'worry window' confines anxiety while keeping your day productive.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1516302752625-fcc3c50ae61f?w=800",
                    Views: 1040
                ),

                // Doctor 8
                (
                    DocIdx: 7,
                    Title: "Conquering Imposter Syndrome: Owning Your Value in Competitive Workplaces",
                    Excerpt: "Why high achievers struggle with feeling like frauds and how to build internal confidence grounded in reality.",
                    Content: "<p>Imposter syndrome is exceptionally prevalent among high performers who attribute their success to luck while internalizing every mistake. Keeping an objective 'evidence of competency' log dismantles cognitive distortions.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=800",
                    Views: 1670
                ),
                (
                    DocIdx: 7,
                    Title: "Navigating Career Crossroads Without Paralyzing Anxiety",
                    Excerpt: "A psychological framework for decision-making during pivotal professional transitions.",
                    Content: "<p>Career transitions evoke existential vulnerability. Approaching decisions as iterative experiments rather than irreversible leaps alleviates fear of failure.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800",
                    Views: 940
                ),

                // Doctor 9
                (
                    DocIdx: 8,
                    Title: "Understanding Teen Social Media Use, Comparison, and Mental Well-being",
                    Excerpt: "Clinical guidance on helping adolescents maintain self-worth in an algorithmically curated digital world.",
                    Content: "<p>Adolescent brain development is uniquely vulnerable to social validation metrics. Encouraging offline mastery experiences builds authentic self-esteem uncoupled from digital likes.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1529333166437-7750a6dd5a70?w=800",
                    Views: 1380
                ),
                (
                    DocIdx: 8,
                    Title: "Creating Safe Emotional Spaces for Adolescents at Home",
                    Excerpt: "How non-reactive active listening encourages teens to open up and seek support during difficult times.",
                    Content: "<p>Teens clam up when they expect lectures. Shifting from reactive advice-giving to empathetic curiosity transforms parent-teen relationship dynamics.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1511895426328-dc8714191300?w=800",
                    Views: 890
                ),

                // Doctor 10
                (
                    DocIdx: 9,
                    Title: "CBT for Insomnia (CBT-I): Rewiring Your Sleep Architecture Naturally",
                    Excerpt: "The gold-standard clinical protocol for curing chronic insomnia without long-term sedative dependence.",
                    Content: "<p>CBT-I addresses the conditioned arousal that connects the bed with frustration and wakefulness. Stimulus control, sleep restriction, and circadian alignment restore natural sleep drive within weeks.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1511295742362-92c96b124e52?w=800",
                    Views: 2450
                ),
                (
                    DocIdx: 9,
                    Title: "The Sleep-Mental Health Axis: Why Deep Sleep Protects Against Anxiety",
                    Excerpt: "How slow-wave sleep and REM sleep recalibrate emotional processing circuits and bolster daily mental resilience.",
                    Content: "<p>During REM sleep, the brain reprocesses emotional memories in a neurochemically calm environment. Deprivation of REM sleep drastically lowers our threshold for anxiety triggers.</p>",
                    Thumbnail: "https://images.unsplash.com/photo-1541781774459-bb2af2f05b55?w=800",
                    Views: 1910
                )
            };

            foreach (var blog in blogSeedData)
            {
                if (blog.DocIdx < doctorProfiles.Count)
                {
                    var author = doctorProfiles[blog.DocIdx];
                    context.BlogPosts.Add(new BlogPost
                    {
                        DoctorId = author.Id,
                        Doctor = author,
                        Title = blog.Title,
                        Excerpt = blog.Excerpt,
                        Content = blog.Content,
                        ThumbnailUrl = blog.Thumbnail,
                        Status = BlogStatus.Published,
                        ViewCount = blog.Views,
                        SubmittedAt = DateTime.UtcNow.AddDays(-20),
                        ApprovedAt = DateTime.UtcNow.AddDays(-19),
                        ApprovedBy = csUser.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-19)
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════
        // 14. PSYCHOMETRIC ASSESSMENTS (PHQ-9 & DASS-21)
        // ═══════════════════════════════════════════════
        await SeedPsychometricsAsync(context);
    }

    private static async Task<User> EnsureUserAsync(
        OpcbsDbContext context,
        string email,
        string password,
        string fullName,
        string phone,
        Role role,
        bool isEmailVerified = true,
        UserStatus status = UserStatus.Active)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
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
            await context.SaveChangesAsync();
        }
        return user;
    }

    private static string GetSampleDiagnosis(int index) => (index % 5) switch
    {
        0 => "Generalized Anxiety Disorder (GAD) - Moderate",
        1 => "Persistent Depressive Disorder (Dysthymia)",
        2 => "Adjustment Disorder with Mixed Anxiety & Depressed Mood",
        3 => "Occupational Burnout Syndrome & Acute Stress",
        _ => "Relational Distress & Communication Impairment"
    };

    private static string GetSampleReview(int index, string patientName) => (index % 4) switch
    {
        0 => "Exceptional clinical care. The doctor was deeply empathetic, structured, and provided practical tools that helped me make noticeable progress immediately.",
        1 => "Very thorough and insightful assessment. I felt genuinely heard and understood throughout the entire session.",
        2 => "Professional, compassionate, and highly skilled in evidence-based therapy. Highly recommend to anyone seeking mental health support.",
        _ => "Great counseling experience. The doctor helped break down complex emotional challenges into actionable, manageable steps."
    };

    private static async Task SeedPsychometricsAsync(OpcbsDbContext context)
    {
        if (await context.PsychometricTests.AnyAsync())
            return;

        // 1. Seed PHQ-9
        var phq9 = new PsychometricTest
        {
            Title = "Patient Health Questionnaire (PHQ-9)",
            Description = "A 9-question clinical scale that helps assess the severity of depression symptoms over the past 2 weeks.",
            TestType = "PHQ9"
        };
        context.PsychometricTests.Add(phq9);

        var phq9Questions = new[]
        {
            "Little interest or pleasure in doing things.",
            "Feeling down, depressed, or hopeless.",
            "Trouble falling or staying asleep, or sleeping too much.",
            "Feeling tired or having little energy.",
            "Poor appetite or overeating.",
            "Feeling bad about yourself — or that you are a failure or have let yourself or your family down.",
            "Trouble concentrating on things, such as reading the newspaper or watching television.",
            "Moving or speaking so slowly that other people could have noticed. Or the opposite — being so fidgety or restless that you have been moving around a lot more than usual.",
            "Thoughts that you would be better off dead, or of hurting yourself in some way."
        };

        for (int i = 0; i < phq9Questions.Length; i++)
        {
            context.PsychometricQuestions.Add(new PsychometricQuestion
            {
                Test = phq9,
                QuestionText = phq9Questions[i],
                QuestionNumber = i + 1,
                Category = "Depression"
            });
        }

        // 2. Seed DASS-21
        var dass21 = new PsychometricTest
        {
            Title = "Depression, Anxiety and Stress Scale (DASS-21)",
            Description = "A 21-item assessment measuring the emotional states of depression, anxiety, and stress.",
            TestType = "DASS21"
        };
        context.PsychometricTests.Add(dass21);

        var dass21Questions = new[]
        {
            new { Text = "I found it hard to wind down.", Cat = "Stress" },
            new { Text = "I was aware of dryness of my mouth.", Cat = "Anxiety" },
            new { Text = "I couldn't seem to experience any positive feeling at all.", Cat = "Depression" },
            new { Text = "I experienced breathing difficulty (e.g., excessively rapid breathing, breathlessness in the absence of physical exertion).", Cat = "Anxiety" },
            new { Text = "I found it difficult to work up the initiative to do things.", Cat = "Depression" },
            new { Text = "I tended to over-react to situations.", Cat = "Stress" },
            new { Text = "I experienced trembling (e.g., in the hands).", Cat = "Anxiety" },
            new { Text = "I felt that I was using a lot of nervous energy.", Cat = "Stress" },
            new { Text = "I was worried about situations in which I might panic and make a fool of myself.", Cat = "Anxiety" },
            new { Text = "I felt that I had nothing to look forward to.", Cat = "Depression" },
            new { Text = "I found myself getting agitated.", Cat = "Stress" },
            new { Text = "I found it difficult to relax.", Cat = "Stress" },
            new { Text = "I felt down-hearted and blue.", Cat = "Depression" },
            new { Text = "I was intolerant of anything that kept me from getting on with what I was doing.", Cat = "Stress" },
            new { Text = "I felt I was close to panic.", Cat = "Anxiety" },
            new { Text = "I was unable to become enthusiastic about anything.", Cat = "Depression" },
            new { Text = "I felt I wasn't worth much as a person.", Cat = "Depression" },
            new { Text = "I felt that I was rather touchy.", Cat = "Stress" },
            new { Text = "I was aware of the action of my heart in the absence of physical exertion (e.g., sense of heart rate increase, heart missing a beat).", Cat = "Anxiety" },
            new { Text = "I felt scared without any good reason.", Cat = "Anxiety" },
            new { Text = "I felt that life was meaningless.", Cat = "Depression" }
        };

        for (int i = 0; i < dass21Questions.Length; i++)
        {
            context.PsychometricQuestions.Add(new PsychometricQuestion
            {
                Test = dass21,
                QuestionText = dass21Questions[i].Text,
                QuestionNumber = i + 1,
                Category = dass21Questions[i].Cat
            });
        }

        // ═══════════════════════════════════════════════
        // 16. AUDIT LOGS (Compliance & Security Records)
        // ═══════════════════════════════════════════════
        var hasAuditLogs = await context.AuditLogs.AnyAsync();
        if (!hasAuditLogs)
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@opcbs.com");
            var adminId = adminUser?.Id;

            context.AuditLogs.AddRange(
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "SystemConfig",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Create,
                    ActionDescription = "Initial system security settings and consultation fee baselines established",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                },
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "Specialization",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Create,
                    ActionDescription = "Core clinical psychological specializations seeded into platform",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                },
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "ServicePackage",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Create,
                    ActionDescription = "Doctor tier subscription packages configured (Standard, Pro, Enterprise)",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                },
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "PsychometricTest",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Create,
                    ActionDescription = "Published standardized diagnostic instruments: PHQ-9 & DASS-21",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                },
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "DoctorProfile",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Update,
                    ActionDescription = "Approved clinical practitioner license and credentials for Dr. Sarah Jenkins",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                },
                new AuditLog
                {
                    UserId = adminId,
                    EntityName = "User",
                    EntityId = Guid.NewGuid(),
                    Action = AuditAction.Update,
                    ActionDescription = "System Administrator performed system health verification and security audit",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPCBS/1.0"
                }
            );
        }

        await context.SaveChangesAsync();
    }
}
