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
    public async Task ConsultationRecordService_GetByPatientAsync_WithPatientProfileId_ReturnsRecords()
    {
        var recordRepo = new Mock<IRepository<ConsultationRecord>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var uow = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var patientId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var record = new ConsultationRecord
        {
            Id = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PatientId = patientId,
            ConsultationSummary = "summary",
            Appointment = new Appointment { Id = Guid.NewGuid(), BookingCode = "BK-1", AppointmentSlot = new AppointmentSlot { Id = Guid.NewGuid(), DoctorProfileId = Guid.NewGuid(), SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), Status = AppointmentSlotStatus.Available, DoctorProfile = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } } }, Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } }, Patient = new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } },
            Doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Email = "d@test.com", FullName = "Doctor", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Doctor" } } },
            Patient = new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } }
        };

        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new() { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConsultationRecord> { record });
        mapper.Setup(m => m.Map<List<ConsultationRecordDto>>(It.IsAny<List<ConsultationRecord>>()))
            .Returns(new List<ConsultationRecordDto> { new() { Id = record.Id, AppointmentId = record.AppointmentId, DoctorId = record.DoctorId, DoctorName = "Doctor", PatientId = patientId, PatientName = "Patient", ConsultationSummary = record.ConsultationSummary } });

        var service = new ConsultationRecordService(recordRepo.Object, apptRepo.Object, doctorRepo.Object, patientRepo.Object, uow.Object, mapper.Object);

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
        mapper.Setup(m => m.Map<List<TreatmentPackageDto>>(It.IsAny<List<TreatmentPackage>>()))
            .Returns(new List<TreatmentPackageDto> { new() { Id = package.Id, Name = package.Name, DoctorName = "Doctor", PatientName = "Patient", Status = "Assigned" } });

        var service = new TreatmentPackageService(packageRepo.Object, doctorRepo.Object, patientRepo.Object, uow.Object, mapper.Object);

        var result = await service.GetByPatientAsync(patientId, 1, 10, default);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
    }
}
