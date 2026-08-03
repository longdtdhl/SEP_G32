using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Tests;

public class BusinessServicesTests
{
    [Fact]
    public async Task ConsultationNoteservice_GetByPatientAsync_WithPatientProfileId_ReturnsRecords()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var patientId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var record = new ConsultationNote
        {
            Id = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientRecordId = Guid.NewGuid(),
            ConsultationSummary = "summary",
            Appointment = new Appointment { Id = Guid.NewGuid(), BookingCode = "BK-1", AppointmentSlot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = Guid.NewGuid(), SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), Status = AppointmentSlotStatus.Available, DoctorProfile = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } } }, Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } }, Patient = new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } },
            Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } },
            PatientRecord = new PatientRecord { Id = Guid.NewGuid(), Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } } }
        };

        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConsultationNote> { record });
        mapper.Setup(m => m.Map<List<ConsultationNoteDto>>(It.IsAny<List<ConsultationNote>>()))
            .Returns(new List<ConsultationNoteDto> { new() { Id = record.Id, AppointmentId = record.AppointmentId, DoctorId = record.DoctorId, DoctorName = "Doctor", PatientRecordId = record.PatientRecordId, PatientName = "Patient", ConsultationSummary = record.ConsultationSummary } });

        var userRepo = new Mock<IRepository<User>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PatientRecord 
            { 
                Id = record.PatientRecordId, 
                PatientId = patientId,
                Doctor = new DoctorProfile 
                { 
                    Id = Guid.NewGuid(), 
                    UserId = Guid.NewGuid(), 
                    User = new User 
                    { 
                        Id = Guid.NewGuid(), 
                        Email = "d@test.com", 
                        FullName = "Doctor", 
                        PhoneNumber = "123", 
                        PasswordHash = "hash", 
                        RoleId = Guid.NewGuid(), 
                        Role = new Role { Name = "Doctor" } 
                    } 
                }
            });
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var pkgRepo = new Mock<IRepository<TreatmentPackage>>();
        pkgRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, pkgRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var result = await service.GetByPatientAsync(patientId, 1, 10, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task TreatmentPackageService_GetByPatientAsync_WithPatientProfileId_ReturnsPackages()
    {
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var patientId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var package = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientId = patientId,
            Name = "Package",
            Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } },
            Patient = new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } }
        };

        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TreatmentPackage> { package });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { package.Doctor.User, package.Patient.User });
        mapper.Setup(m => m.Map<List<TreatmentPackageDto>>(It.IsAny<List<TreatmentPackage>>()))
            .Returns(new List<TreatmentPackageDto> { new() { Id = package.Id, Name = package.Name, DoctorName = "Doctor", PatientName = "Patient", Status = "Assigned" } });

        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var caseRepo = new Mock<IRepository<TreatmentCase>>();
        caseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());
        var service = new TreatmentPackageService(packageRepo.Object, doctorRepo.Object, patientRepo.Object, userRepo.Object, caseRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var result = await service.GetByPatientAsync(patientId, 1, 10, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
    }

    // ──────────────────────────────────────────────
    // CONSULTATION NOTE SERVICE CREATE TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DoctorNotFound_ReturnsError()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        var result = await service.CreateAsync(Guid.NewGuid(), new CreateConsultationNoteDto { PatientRecordId = Guid.NewGuid(), ConsultationSummary = "Summary" }, default);

        Assert.False(result.Success);
        Assert.Contains("Doctor not found", result.Message);
    }

    [Fact]
    public async Task CreateAsync_PatientRecordNotFound_ReturnsError()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Doctor", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } };
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        patientRecordRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PatientRecord?)null);

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        var result = await service.CreateAsync(doctorUserId, new CreateConsultationNoteDto { PatientRecordId = Guid.NewGuid(), ConsultationSummary = "Summary" }, default);

        Assert.False(result.Success);
        Assert.Contains("Patient record not found", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Success_NoAppointment()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Doctor", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } };
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });

        var patientRecordId = Guid.NewGuid();
        var patientRecord = new PatientRecord { Id = patientRecordId, PatientId = Guid.NewGuid(), Doctor = doctor };
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(patientRecord);

        var dto = new CreateConsultationNoteDto
        {
            PatientRecordId = patientRecordId,
            ConsultationSummary = "Test Summary"
        };

        mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>()))
            .Returns(new ConsultationNoteDto { Id = Guid.NewGuid(), PatientRecordId = patientRecordId, ConsultationSummary = "Test Summary" });

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        var result = await service.CreateAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Summary", result.Data!.ConsultationSummary);
        recordRepo.Verify(r => r.AddAsync(It.IsAny<ConsultationNote>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Success_WithAppointmentAndNotification()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Doctor", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } };
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });

        var patientRecordId = Guid.NewGuid();
        var patientRecord = new PatientRecord { Id = patientRecordId, PatientId = Guid.NewGuid(), Doctor = doctor };
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(patientRecord);

        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var appointment = new Appointment { Id = appointmentId, BookingCode = "BK-1", PatientId = patientId, DoctorId = doctor.Id, AppointmentSlotId = Guid.NewGuid(), AppointmentSlot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = doctor.Id, SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), Status = AppointmentSlotStatus.Booked, DoctorProfile = doctor }, Doctor = doctor };
        apptRepo.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var patient = new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "2", PasswordHash = "y", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doctor.User, patient.User });

        var dto = new CreateConsultationNoteDto
        {
            PatientRecordId = patientRecordId,
            AppointmentId = appointmentId,
            ConsultationSummary = "Test Summary"
        };

        mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>()))
            .Returns(new ConsultationNoteDto { Id = Guid.NewGuid(), PatientRecordId = patientRecordId, ConsultationSummary = "Test Summary" });

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        var result = await service.CreateAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        notifService.Verify(n => n.CreateNotificationAsync(patientUserId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Diagnosis Check", "Rec Check", "Follow Up Check", "Plan Check")]
    [InlineData("Diagnosis 2", null, "Follow Up 2", null)]
    public async Task CreateAsync_DTOFieldsSavedCorrectly(string diagnosis, string recommendation, string followUp, string therapyPlan)
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Doctor", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } };
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });

        var patientRecordId = Guid.NewGuid();
        var patientRecord = new PatientRecord { Id = patientRecordId, PatientId = Guid.NewGuid(), Doctor = doctor };
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(patientRecord);

        var dto = new CreateConsultationNoteDto
        {
            PatientRecordId = patientRecordId,
            ConsultationSummary = "Summary",
            Diagnosis = diagnosis,
            Recommendation = recommendation,
            FollowUpNotes = followUp,
            TherapyPlan = therapyPlan,
            NextAppointmentRecommendedDate = DateTime.UtcNow.AddDays(7)
        };

        ConsultationNote? savedNote = null;
        recordRepo.Setup(r => r.AddAsync(It.IsAny<ConsultationNote>(), It.IsAny<CancellationToken>()))
            .Callback<ConsultationNote, CancellationToken>((n, c) => savedNote = n)
            .Returns(Task.CompletedTask);

        mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>()))
            .Returns(new ConsultationNoteDto { Id = Guid.NewGuid(), PatientRecordId = patientRecordId, ConsultationSummary = "Summary" });

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        await service.CreateAsync(doctorUserId, dto, default);

        Assert.NotNull(savedNote);
        Assert.Equal(diagnosis, savedNote!.Diagnosis);
        Assert.Equal(recommendation, savedNote.Recommendation);
        Assert.Equal(followUp, savedNote.FollowUpNotes);
        Assert.Equal(therapyPlan, savedNote.TherapyPlan);
        Assert.NotNull(savedNote.NextAppointmentRecommendedDate);
    }

    [Fact]
    public async Task CreateAsync_DbSaveFails_ThrowsException()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var notifService = new Mock<OPCBS.Application.Interfaces.Services.INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Doctor", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } };
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });

        var patientRecordId = Guid.NewGuid();
        var patientRecord = new PatientRecord { Id = patientRecordId, PatientId = Guid.NewGuid(), Doctor = doctor };
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(patientRecord);

        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Save Error"));

        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, new Mock<IRepository<TreatmentPackage>>().Object, notifService.Object, uow.Object, mapper.Object);
        var dto = new CreateConsultationNoteDto { PatientRecordId = patientRecordId, ConsultationSummary = "Summary" };

        await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(doctorUserId, dto, default));
    }

    [Fact]
    public async Task DoctorVerificationService_SubmitVerification_SavesCertificateUrlAndSubmittedStatus()
    {
        var verRepo = new Mock<IRepository<VerificationRequest>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUserId,
            VerificationStatus = VerificationStatus.Draft,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest>());
        userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.User);

        VerificationRequest? savedRequest = null;
        verRepo.Setup(r => r.AddAsync(It.IsAny<VerificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<VerificationRequest, CancellationToken>((req, _) => savedRequest = req)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(verRepo.Object, doctorRepo.Object, userRepo.Object, uow.Object, mapper.Object);
        var dto = new SubmitVerificationDto
        {
            LicenseNumber = "LIC-12345",
            Specialization = "Clinical Psychologist",
            ExperienceYears = 8,
            CertificateUrl = "/uploads/verifications/cert123.pdf"
        };

        var result = await service.SubmitVerificationAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        Assert.NotNull(savedRequest);
        Assert.Equal(VerificationStatus.Submitted, savedRequest!.Status);
        Assert.Equal("/uploads/verifications/cert123.pdf", savedRequest.CertificateUrl);
        Assert.Equal(VerificationStatus.Submitted, doctor.VerificationStatus);
        Assert.Equal("/uploads/verifications/cert123.pdf", result.Data!.CertificateUrl);
    }

    [Fact]
    public async Task DoctorVerificationService_GetVerificationStatus_NoRequest_ReturnsDraftStatus()
    {
        var verRepo = new Mock<IRepository<VerificationRequest>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUserId,
            VerificationStatus = VerificationStatus.Draft,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest>());
        userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.User);

        var service = new VerificationService(verRepo.Object, doctorRepo.Object, userRepo.Object, uow.Object, mapper.Object);

        var result = await service.GetVerificationStatusAsync(doctorUserId, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Draft", result.Data!.Status);
        Assert.Equal("Dr. Test", result.Data.DoctorName);
    }

    [Fact]
    public async Task ConsultationNoteService_UpdateAsync_WhenConfirmed_ReturnsError()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        var notifService = new Mock<INotificationService>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        var note = new ConsultationNote
        {
            Id = recordId,
            DoctorId = doctorId,
            PatientRecordId = Guid.NewGuid(),
            ConsultationSummary = "Original summary",
            Diagnosis = "Original diagnosis",
            IsPatientConfirmed = true, // ALREADY CONFIRMED BY PATIENT
            PatientConfirmedAt = DateTime.UtcNow,
            Doctor = doctor,
            PatientRecord = new PatientRecord { Id = Guid.NewGuid(), DoctorId = doctorId, Doctor = doctor }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        recordRepo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var service = new ConsultationNoteService(
            recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object,
            patientRecordRepo.Object, userRepo.Object, packageRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var updateDto = new UpdateConsultationNoteDto
        {
            Diagnosis = "New Diagnosis Attempt",
            ConsultationSummary = "New Notes Attempt"
        };

        var result = await service.UpdateAsync(recordId, doctorUserId, updateDto, default);

        Assert.False(result.Success);
        Assert.Contains("confirmed by the patient and can no longer be edited", result.Message);
    }

    [Fact]
    public async Task ConsultationNoteService_UpdateAsync_WhenUnconfirmed_UpdatesRecord()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        var notifService = new Mock<INotificationService>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        var note = new ConsultationNote
        {
            Id = recordId,
            DoctorId = doctorId,
            PatientRecordId = Guid.NewGuid(),
            ConsultationSummary = "Original summary",
            Diagnosis = "Original diagnosis",
            IsPatientConfirmed = false, // UNCONFIRMED
            Doctor = doctor,
            PatientRecord = new PatientRecord { Id = Guid.NewGuid(), DoctorId = doctorId, Doctor = doctor }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        recordRepo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote> { note });
        patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord>());
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>())).Returns((ConsultationNote src) => new ConsultationNoteDto { Id = src.Id, Diagnosis = src.Diagnosis });

        var service = new ConsultationNoteService(
            recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object,
            patientRecordRepo.Object, userRepo.Object, packageRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var updateDto = new UpdateConsultationNoteDto
        {
            Diagnosis = "Updated Diagnosis",
            ConsultationSummary = "Updated Notes"
        };

        var result = await service.UpdateAsync(recordId, doctorUserId, updateDto, default);

        Assert.True(result.Success);
        Assert.Equal("Updated Diagnosis", note.Diagnosis);
        Assert.Equal("Updated Notes", note.ConsultationSummary);
        Assert.Equal(doctorUserId, note.LastEditedByDoctorId != null ? doctorUserId : (Guid?)null);
    }

    [Fact]
    public async Task ConsultationNoteService_ConfirmByPatientAsync_WhenAuthorized_ConfirmsRecord()
    {
        var recordRepo = new Mock<IRepository<ConsultationNote>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientRecordRepo = new Mock<IRepository<PatientRecord>>();
        var userRepo = new Mock<IRepository<User>>();
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        var notifService = new Mock<INotificationService>();

        var patientUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var patientRecordId = Guid.NewGuid();

        var patient = new PatientProfile
        {
            Id = patientId,
            UserId = patientUserId,
            User = new User { Id = patientUserId, Email = "patient@test.com", FullName = "Patient Name", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } }
        };

        var patientRecord = new PatientRecord
        {
            Id = patientRecordId,
            PatientId = patientId,
            DoctorId = Guid.NewGuid(),
            Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doc", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } }
        };

        var note = new ConsultationNote
        {
            Id = recordId,
            DoctorId = patientRecord.DoctorId,
            PatientRecordId = patientRecordId,
            ConsultationSummary = "Summary",
            IsPatientConfirmed = false,
            Doctor = patientRecord.Doctor,
            PatientRecord = patientRecord
        };

        recordRepo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(patientRecord);
        recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote> { note });
        patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { patientRecord });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { patient.User });
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>())).Returns((ConsultationNote src) => new ConsultationNoteDto { Id = src.Id, IsPatientConfirmed = src.IsPatientConfirmed });

        var service = new ConsultationNoteService(
            recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object,
            patientRecordRepo.Object, userRepo.Object, packageRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var result = await service.ConfirmByPatientAsync(recordId, patientUserId, default);

        Assert.True(result.Success);
        Assert.True(note.IsPatientConfirmed);
        Assert.NotNull(note.PatientConfirmedAt);
        Assert.Equal(patientUserId, note.PatientConfirmedById);
    }
}

