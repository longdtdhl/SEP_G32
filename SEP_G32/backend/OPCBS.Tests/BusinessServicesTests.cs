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
        var service = new ConsultationNoteService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, patientRecordRepo.Object, userRepo.Object, notifService.Object, uow.Object, mapper.Object);

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
        var service = new TreatmentPackageService(packageRepo.Object, doctorRepo.Object, patientRepo.Object, userRepo.Object, notifService.Object, uow.Object, mapper.Object);

        var result = await service.GetByPatientAsync(patientId, 1, 10, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
    }
}
