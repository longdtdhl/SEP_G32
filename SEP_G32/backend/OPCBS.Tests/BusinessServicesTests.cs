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
        var unassignedTemplate = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = package.DoctorId,
            PatientId = null,
            Name = "Unassigned template",
            Status = TreatmentPackageStatus.Created,
            Doctor = package.Doctor
        };

        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TreatmentPackage> { package, unassignedTemplate });
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
        mapper.Verify(m => m.Map<List<TreatmentPackageDto>>(It.Is<List<TreatmentPackage>>(items =>
            items.Count == 1 && items[0].Id == package.Id)), Times.Once);
    }

    // ──────────────────────────────────────────────
    // CONSULTATION NOTE SERVICE CREATE TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task TreatmentPackageService_AcceptAssignedPackage_CreatesCaseWithProfileIds()
    {
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var caseRepo = new Mock<IRepository<TreatmentCase>>();
        var notificationService = new Mock<INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorProfileId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var package = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorProfileId,
            PatientId = patientProfileId,
            Doctor = null!,
            Name = "CBT for Anxiety",
            SessionQuantity = 10,
            ValidityDays = 90,
            Status = TreatmentPackageStatus.Assigned
        };

        packageRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DoctorProfile> { new() { Id = doctorProfileId, UserId = Guid.NewGuid(), User = null! } });
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientProfileId, UserId = patientUserId, User = null! } });
        caseRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentCase>());

        var service = new TreatmentPackageService(packageRepo.Object, doctorRepo.Object, patientRepo.Object,
            userRepo.Object, caseRepo.Object, notificationService.Object, uow.Object, mapper.Object);

        var result = await service.AcceptPackageAsync(package.Id, patientUserId, default);

        Assert.True(result.Success);
        Assert.Equal(TreatmentPackageStatus.Active, package.Status);
        caseRepo.Verify(r => r.AddAsync(It.Is<TreatmentCase>(c =>
            c.TreatmentPackageId == package.Id &&
            c.DoctorId == doctorProfileId &&
            c.PatientId == patientProfileId &&
            c.TotalSessions == 10), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TreatmentPackageService_AcceptPackageAssignedToAnotherPatient_IsRejected()
    {
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var package = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            Doctor = null!,
            Name = "Private package",
            Status = TreatmentPackageStatus.Assigned
        };

        packageRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientProfileId, UserId = patientUserId, User = null! } });

        var caseRepo = new Mock<IRepository<TreatmentCase>>();
        var uow = new Mock<IUnitOfWork>();
        var service = new TreatmentPackageService(packageRepo.Object, new Mock<IRepository<DoctorProfile>>().Object,
            patientRepo.Object, new Mock<IRepository<User>>().Object, caseRepo.Object,
            new Mock<INotificationService>().Object, uow.Object, new Mock<IMapper>().Object);

        var result = await service.AcceptPackageAsync(package.Id, patientUserId, default);

        Assert.False(result.Success);
        Assert.Contains("Not authorized", result.Message);
        caseRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentCase>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TreatmentPackageService_RejectAssignedPackage_UpdatesStatusAndReason()
    {
        var packageRepo = new Mock<IRepository<TreatmentPackage>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var package = new TreatmentPackage
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientId = patientProfileId,
            Doctor = null!,
            Name = "Package",
            Status = TreatmentPackageStatus.Assigned
        };

        packageRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientProfileId, UserId = patientUserId, User = null! } });

        var uow = new Mock<IUnitOfWork>();
        var service = new TreatmentPackageService(packageRepo.Object, new Mock<IRepository<DoctorProfile>>().Object,
            patientRepo.Object, new Mock<IRepository<User>>().Object, new Mock<IRepository<TreatmentCase>>().Object,
            new Mock<INotificationService>().Object, uow.Object, new Mock<IMapper>().Object);

        var result = await service.RejectPackageAsync(package.Id, patientUserId, "Schedule does not fit", default);

        Assert.True(result.Success);
        Assert.Equal(TreatmentPackageStatus.Rejected, package.Status);
        Assert.Equal("Schedule does not fit", package.RejectionReason);
        packageRepo.Verify(r => r.Update(package), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

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
    public async Task DoctorVerificationService_SubmitVerification_PendingRequest_UpdatesInPlaceAndResetsReviewState()
    {
        var verRepo = new Mock<IRepository<VerificationRequest>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            VerificationStatus = VerificationStatus.Submitted,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        var existingReq = new VerificationRequest
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            Status = VerificationStatus.Submitted,
            CertificateUrl = "https://res.cloudinary.com/old.pdf",
            CertificateFileName = "old.pdf",
            ReviewedAt = DateTime.UtcNow.AddDays(-1),
            ReviewedBy = Guid.NewGuid(),
            RejectionReason = "Previous reason",
            DoctorProfile = doctor
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest> { existingReq });
        doctorRepo.Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.User);

        VerificationRequest? updatedReq = null;
        verRepo.Setup(r => r.Update(It.IsAny<VerificationRequest>()))
            .Callback<VerificationRequest>(req => updatedReq = req);

        var service = new VerificationService(verRepo.Object, doctorRepo.Object, userRepo.Object, uow.Object, mapper.Object);
        var dto = new SubmitVerificationDto
        {
            CertificateUrl = "https://res.cloudinary.com/new.pdf",
            CertificateFileName = "new.pdf",
            CertificateContentType = "application/pdf"
        };

        var result = await service.SubmitVerificationAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        Assert.NotNull(updatedReq);
        Assert.Equal(existingReq.Id, updatedReq!.Id);
        Assert.Equal(VerificationStatus.Submitted, updatedReq.Status);
        Assert.Equal("https://res.cloudinary.com/new.pdf", updatedReq.CertificateUrl);
        Assert.Null(updatedReq.ReviewedAt);
        Assert.Null(updatedReq.ReviewedBy);
        Assert.Null(updatedReq.RejectionReason);
    }

    [Fact]
    public async Task DoctorVerificationService_SubmitVerification_RejectedRequest_CreatesNewRequestPreservingHistory()
    {
        var verRepo = new Mock<IRepository<VerificationRequest>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            VerificationStatus = VerificationStatus.Rejected,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        var rejectedReq = new VerificationRequest
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            Status = VerificationStatus.Rejected,
            CertificateUrl = "https://res.cloudinary.com/rejected.pdf",
            RejectionReason = "Illegible text",
            DoctorProfile = doctor
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest> { rejectedReq });
        doctorRepo.Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.User);

        VerificationRequest? newAddedReq = null;
        verRepo.Setup(r => r.AddAsync(It.IsAny<VerificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<VerificationRequest, CancellationToken>((req, _) => newAddedReq = req)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(verRepo.Object, doctorRepo.Object, userRepo.Object, uow.Object, mapper.Object);
        var dto = new SubmitVerificationDto
        {
            CertificateUrl = "https://res.cloudinary.com/resubmitted.pdf",
            CertificateFileName = "resubmitted.pdf"
        };

        var result = await service.SubmitVerificationAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        Assert.NotNull(newAddedReq);
        Assert.NotEqual(rejectedReq.Id, newAddedReq!.Id);
        Assert.Equal(VerificationStatus.Submitted, newAddedReq.Status);
        Assert.Equal("https://res.cloudinary.com/resubmitted.pdf", newAddedReq.CertificateUrl);
        Assert.Equal(VerificationStatus.Submitted, doctor.VerificationStatus);
    }

    [Fact]
    public async Task DoctorVerificationService_SubmitVerification_ApprovedRequest_CreatesNewRequestPreservingApprovedHistory()
    {
        var verRepo = new Mock<IRepository<VerificationRequest>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            VerificationStatus = VerificationStatus.Approved,
            User = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr. Test", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } }
        };

        var approvedReq = new VerificationRequest
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            Status = VerificationStatus.Approved,
            CertificateUrl = "https://res.cloudinary.com/approved.pdf",
            CertificateFileName = "approved.pdf",
            ReviewedAt = DateTime.UtcNow.AddMonths(-6),
            DoctorProfile = doctor
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest> { approvedReq });
        doctorRepo.Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.User);

        VerificationRequest? newAddedReq = null;
        verRepo.Setup(r => r.AddAsync(It.IsAny<VerificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<VerificationRequest, CancellationToken>((req, _) => newAddedReq = req)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(verRepo.Object, doctorRepo.Object, userRepo.Object, uow.Object, mapper.Object);
        var dto = new SubmitVerificationDto
        {
            CertificateUrl = "https://res.cloudinary.com/updated_cert.pdf",
            CertificateFileName = "updated_cert.pdf"
        };

        var result = await service.SubmitVerificationAsync(doctorUserId, dto, default);

        Assert.True(result.Success);
        Assert.NotNull(newAddedReq);
        Assert.NotEqual(approvedReq.Id, newAddedReq!.Id);
        Assert.Equal(VerificationStatus.Submitted, newAddedReq.Status);
        Assert.Equal("https://res.cloudinary.com/updated_cert.pdf", newAddedReq.CertificateUrl);
        Assert.Equal(VerificationStatus.Submitted, doctor.VerificationStatus);
        Assert.Equal("https://res.cloudinary.com/approved.pdf", result.Data!.PreviousApprovedCertificateUrl);
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

    [Fact]
    public async Task ScheduleService_GetCalendarEventsAsync_ReturnsFormattedCalendarEvents()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            UserId = doctorUserId,
            User = new User
            {
                Id = doctorUserId,
                FullName = "Dr. Test",
                Email = "dr.test@example.com",
                PasswordHash = "hash",
                PhoneNumber = "0123456789",
                Role = new Role { Name = "Doctor" }
            },
            Biography = "Bio",
            LicenseNumber = "LIC123"
        };

        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Status = AppointmentSlotStatus.Available,
            DoctorProfile = doctor,
            Notes = "Sample Slot Note"
        };

        var dayOff = new DoctorDayOff
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today,
            Reason = "Vacation",
            DoctorProfile = doctor
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { slot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff> { dayOff });
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doctor.User });

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object);

        var result = await service.GetCalendarEventsAsync(doctorUserId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(2), default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, e => e.EventType == "Availability");
        Assert.Contains(result.Data, e => e.EventType == "DayOff");
    }

    [Fact]
    public async Task ScheduleService_CreateSlot_CustomDurations_Success()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "d@test.com", FullName = "Doc", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot>());
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff>());

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object);
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)).ToString("yyyy-MM-dd");

        // 1. 30 Minutes
        var res30 = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = futureDate, StartTime = "08:00", EndTime = "08:30" });
        Assert.True(res30.Success);
        Assert.Equal("08:00", res30.Data!.StartTime);
        Assert.Equal("08:30", res30.Data!.EndTime);

        // 2. 75 Minutes (1 hour 15 min)
        var res75 = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = futureDate, StartTime = "09:00", EndTime = "10:15" });
        Assert.True(res75.Success);
        Assert.Equal("09:00", res75.Data!.StartTime);
        Assert.Equal("10:15", res75.Data!.EndTime);

        // 3. 3 Hours
        var res3h = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = futureDate, StartTime = "13:00", EndTime = "16:00" });
        Assert.True(res3h.Success);
        Assert.Equal("13:00", res3h.Data!.StartTime);
        Assert.Equal("16:00", res3h.Data!.EndTime);
    }

    [Fact]
    public async Task ScheduleService_CreateSlot_Validations_ReturnsErrors()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "d@test.com", FullName = "Doc", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };

        var testDateOnly = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var testDateStr = testDateOnly.ToString("yyyy-MM-dd");
        var dayOffDateTime = DateTime.Today.AddDays(14);
        var dayOffStr = dayOffDateTime.ToString("yyyy-MM-dd");

        var existingSlot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDateOnly,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(9, 0),
            Status = AppointmentSlotStatus.Available
        };

        var dayOff = new DoctorDayOff
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            StartDate = dayOffDateTime.Date,
            EndDate = dayOffDateTime.Date
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { existingSlot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff> { dayOff });

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object);

        // 1. StartTime >= EndTime
        var resInvalidTime = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = testDateStr, StartTime = "09:00", EndTime = "08:00" });
        Assert.False(resInvalidTime.Success);
        Assert.Contains("Start time must be before end time", resInvalidTime.Message);

        // 2. Overlapping Slot
        var resOverlap = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = testDateStr, StartTime = "08:30", EndTime = "09:30" });
        Assert.False(resOverlap.Success);
        Assert.Contains("overlaps with an existing slot", resOverlap.Message);

        // 3. Day Off
        var resDayOff = await service.CreateSlotAsync(doctorUserId, new CreateSlotDto { Date = dayOffStr, StartTime = "08:00", EndTime = "09:00" });
        Assert.False(resDayOff.Success);
        Assert.Contains("day off", resDayOff.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScheduleService_GetCalendarEventsAsync_OutsideHoursAndNoDuplicateBookedEvents()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "d@test.com", FullName = "Doc", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };
        var testDate = new DateOnly(2026, 8, 10);
        var slotId1 = Guid.NewGuid();
        var slotId2 = Guid.NewGuid();

        var earlySlot = new AppointmentSlot
        {
            Id = slotId1,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDate,
            StartTime = new TimeOnly(5, 0),
            EndTime = new TimeOnly(6, 0),
            Status = AppointmentSlotStatus.Available
        };

        var bookedSlot = new AppointmentSlot
        {
            Id = slotId2,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDate,
            StartTime = new TimeOnly(23, 0),
            EndTime = new TimeOnly(23, 30),
            Status = AppointmentSlotStatus.Booked
        };

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "B123",
            DoctorId = doctorId,
            Doctor = doctor,
            PatientId = Guid.NewGuid(),
            AppointmentSlotId = slotId2,
            AppointmentSlot = bookedSlot,
            AppointmentDate = new DateTime(2026, 8, 10),
            Status = AppointmentStatus.Approved
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { earlySlot, bookedSlot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff>());
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object);

        var start = new DateTime(2026, 8, 10, 0, 0, 0);
        var end = new DateTime(2026, 8, 11, 0, 0, 0);
        var result = await service.GetCalendarEventsAsync(doctorUserId, start, end, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // Early slot (05:00-06:00) is included
        Assert.Contains(result.Data, e => e.SlotId == slotId1 && e.Start.Contains("T05:00:00"));

        // Booked slot with appointment renders ONLY 1 Appointment event, no duplicate Booked slot event
        Assert.Single(result.Data.Where(e => e.SlotId == slotId2));
        Assert.Equal("Appointment", result.Data.First(e => e.SlotId == slotId2).EventType);
    }

    [Fact]
    public async Task ScheduleService_GenerateWeeklyScheduleAsync_SuccessAndIdempotent()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr Weekly", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot>());
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff>());

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object);

        var today = DateTime.Today;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        var nextMonday = today.AddDays(daysUntilMonday);

        var config = new WeeklyScheduleConfigDto
        {
            WorkingDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            TimeRanges = new List<WeeklyScheduleRangeDto>
            {
                new WeeklyScheduleRangeDto { StartTime = "08:00", EndTime = "10:00" }
            },
            SlotDurationMinutes = 60,
            BreakTimeMinutes = 0,
            StartDate = nextMonday.ToString("yyyy-MM-dd"), // Future Monday
            WeeksToApply = 2
        };

        var result = await service.GenerateWeeklyScheduleAsync(doctorUserId, config);

        Assert.True(result.Success);
        // 2 weeks * 2 days/week (Mon, Wed) * 2 slots/day (08-09, 09-10) = 8 slots
        Assert.Equal(8, result.Data);

        // Run second time (idempotency check)
        var existingGeneratedSlots = new List<AppointmentSlot>();
        slotRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<AppointmentSlot>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<AppointmentSlot>, CancellationToken>((slots, ct) => existingGeneratedSlots.AddRange(slots));

        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingGeneratedSlots);

        var result2 = await service.GenerateWeeklyScheduleAsync(doctorUserId, config);
        Assert.True(result2.Success);
    }

    [Fact]
    public async Task ScheduleService_GetCalendarEvents_ResolvesRegisteredPatientAndGuestNames_DeduplicatesBlockedSlots()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr Tester", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };

        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var patUser = new User { Id = patientUserId, Email = "pat@test.com", FullName = "Nguyen Van A", PhoneNumber = "456", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } };
        var patientProfile = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = patUser };

        var testDate = new DateOnly(2026, 8, 12);
        var slotId1 = Guid.NewGuid();
        var slot1 = new AppointmentSlot
        {
            Id = slotId1,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Status = AppointmentSlotStatus.Blocked // Incorrectly set to Blocked in DB
        };

        var appt1 = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "APT-REG01",
            DoctorId = doctorId,
            Doctor = doctor,
            PatientId = patientProfileId, // Points to PatientProfile.Id
            AppointmentSlotId = slotId1,
            AppointmentSlot = slot1,
            AppointmentDate = new DateTime(2026, 8, 12, 10, 0, 0),
            Status = AppointmentStatus.Approved
        };

        var slotId2 = Guid.NewGuid();
        var slot2 = new AppointmentSlot
        {
            Id = slotId2,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDate,
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 0),
            Status = AppointmentSlotStatus.Booked
        };

        var appt2 = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "APT-GUEST01",
            DoctorId = doctorId,
            Doctor = doctor,
            PatientId = null,
            GuestName = "Tran Guest B",
            AppointmentSlotId = slotId2,
            AppointmentSlot = slot2,
            AppointmentDate = new DateTime(2026, 8, 12, 14, 0, 0),
            Status = AppointmentStatus.Approved
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { slot1, slot2 });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff>());
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt1, appt2 });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser, patUser });
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patientProfile });

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object, patientRepo: patientRepo.Object);

        var start = new DateTime(2026, 8, 12, 0, 0, 0);
        var end = new DateTime(2026, 8, 13, 0, 0, 0);
        var result = await service.GetCalendarEventsAsync(doctorUserId, start, end, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // 1. Registered patient name resolved as FullName "Nguyen Van A"
        var ev1 = result.Data.FirstOrDefault(e => e.AppointmentId == appt1.Id);
        Assert.NotNull(ev1);
        Assert.Equal("Nguyen Van A", ev1.PatientName);
        Assert.Equal("APT-REG01", ev1.BookingCode);

        // 2. Guest patient name resolved as GuestName "Tran Guest B"
        var ev2 = result.Data.FirstOrDefault(e => e.AppointmentId == appt2.Id);
        Assert.NotNull(ev2);
        Assert.Equal("Tran Guest B", ev2.PatientName);

        // 3. Exactly 2 total calendar events (the Blocked slot1 and Booked slot2 are suppressed because active appointments exist for both)
        Assert.Equal(2, result.Data.Count);
        Assert.DoesNotContain(result.Data, e => e.EventType == "Blocked" && e.SlotId == slotId1);
    }

    [Fact]
    public async Task ScheduleService_GetCalendarEvents_CancelledAppointmentDoesNotSuppressAvailableSlot()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var docUser = new User { Id = doctorUserId, Email = "doc@test.com", FullName = "Dr Tester", PhoneNumber = "123", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorId, UserId = doctorUserId, User = docUser };

        var futureDate = DateTime.UtcNow.AddDays(1);
        var testDate = DateOnly.FromDateTime(futureDate);
        var slotId = Guid.NewGuid();
        var slot = new AppointmentSlot
        {
            Id = slotId,
            DoctorProfileId = doctorId,
            DoctorProfile = doctor,
            SlotDate = testDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Status = AppointmentSlotStatus.Available
        };

        var cancelledAppt = new Appointment
        {
            Id = Guid.NewGuid(),
            BookingCode = "APT-CANCELLED",
            DoctorId = doctorId,
            Doctor = doctor,
            PatientId = Guid.NewGuid(),
            AppointmentSlotId = slotId,
            AppointmentSlot = slot,
            AppointmentDate = futureDate.Date.AddHours(9),
            Status = AppointmentStatus.Cancelled
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AppointmentSlot> { slot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorDayOff>());
        apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { cancelledAppt });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { docUser });
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());

        var service = new ScheduleService(scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object, dayOffRepo.Object, apptRepo.Object, uow.Object, mapper.Object, patientRepo: patientRepo.Object);

        var start = futureDate.Date;
        var end = futureDate.Date.AddDays(1);
        var result = await service.GetCalendarEventsAsync(doctorUserId, start, end, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // Cancelled appointment is NOT returned, and the Available slot IS returned
        Assert.Single(result.Data);
        var ev = result.Data.First();
        Assert.Equal("Availability", ev.EventType);
        Assert.Equal(slotId, ev.SlotId);
    }
}

