using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class PatientRecordServiceTests
{
    private readonly Mock<IRepository<PatientRecord>> _mockRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _mockDoctorRepo = new();
    private readonly Mock<IRepository<Appointment>> _mockApptRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _mockPatientRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<IRepository<Role>> _mockRoleRepo = new();
    private readonly Mock<IRepository<OtpVerification>> _mockOtpRepo = new();
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly PatientRecordService _service;

    public PatientRecordServiceTests()
    {
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord>());
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        _mockApptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _mockRoleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role>());

        _mockMapper.Setup(m => m.Map<PatientRecordDto>(It.IsAny<PatientRecord>()))
            .Returns((PatientRecord r) => new PatientRecordDto
            {
                Id = r.Id,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                GuestName = r.GuestName,
                PsychologicalHistory = r.PsychologicalHistory,
                CurrentSymptoms = r.CurrentSymptoms,
                StressFactors = r.StressFactors,
                GeneralNotes = r.GeneralNotes
            });

        _mockMapper.Setup(m => m.Map<List<PatientRecordDto>>(It.IsAny<object>()))
            .Returns((object src) =>
            {
                if (src is IEnumerable<PatientRecord> list)
                {
                    return list.Select(r => new PatientRecordDto
                    {
                        Id = r.Id,
                        PatientId = r.PatientId,
                        DoctorId = r.DoctorId,
                        GuestName = r.GuestName,
                        PsychologicalHistory = r.PsychologicalHistory,
                        CurrentSymptoms = r.CurrentSymptoms,
                        StressFactors = r.StressFactors,
                        GeneralNotes = r.GeneralNotes
                    }).ToList();
                }
                return new List<PatientRecordDto>();
            });

        _service = new PatientRecordService(
            _mockRepo.Object,
            _mockDoctorRepo.Object,
            _mockApptRepo.Object,
            _mockPatientRepo.Object,
            _mockUserRepo.Object,
            _mockRoleRepo.Object,
            _mockOtpRepo.Object,
            _mockEmailService.Object,
            _mockMapper.Object,
            _mockUow.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedRecords()
    {
        var records = new List<PatientRecord>
        {
            new() { Id = Guid.NewGuid(), PsychologicalHistory = "History 1", Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } }
        };

        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(records);

        var result = await _service.GetAllAsync(CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Equal("History 1", result[0].PsychologicalHistory);
    }

    [Fact]
    public async Task GetSystemPatientsAsync_ReturnsSystemOnly()
    {
        var patientId = Guid.NewGuid();
        var records = new List<PatientRecord>
        {
            new() { Id = Guid.NewGuid(), PatientId = patientId, Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } },
            new() { Id = Guid.NewGuid(), PatientId = null, Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } }
        };

        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(records);

        var result = await _service.GetSystemPatientsAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.False(result[0].IsGuest);
    }

    [Fact]
    public async Task GetGuestPatientsAsync_ReturnsGuestsOnly()
    {
        var records = new List<PatientRecord>
        {
            new() { Id = Guid.NewGuid(), PatientId = null, GuestName = "Guest John", Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } },
            new() { Id = Guid.NewGuid(), PatientId = Guid.NewGuid(), Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } }
        };

        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(records);

        var result = await _service.GetGuestPatientsAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.True(result[0].IsGuest);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRecord_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var record = new PatientRecord { Id = id, PsychologicalHistory = "Asthma", Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } };

        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Asthma", result.PsychologicalHistory);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PatientRecord?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedRecord_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PatientRecord?)null);

        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingPatient_ReturnsDto()
    {
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var record = new PatientRecord { Id = Guid.NewGuid(), PatientId = patientProfileId, PsychologicalHistory = "Anxiety", Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } };

        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { record });

        var result = await _service.GetByUserIdAsync(patientUserId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Anxiety", result.PsychologicalHistory);
    }

    [Fact]
    public async Task GetByUserIdAsync_PatientNotFound_ReturnsNull()
    {
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord>());

        var result = await _service.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CanDoctorAccessPatientAsync_DoctorAssignedViaAppointment_ReturnsTrue()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var patientRecordId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var record = new PatientRecord { Id = patientRecordId, PatientId = patientProfileId, DoctorId = doctorProfileId, Doctor = doc };
        var appt = new Appointment { Id = Guid.NewGuid(), BookingCode = "BK-1", DoctorId = doctorProfileId, PatientId = patientProfileId, Status = AppointmentStatus.Approved, Doctor = doc, AppointmentSlot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doc } };

        _mockRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockApptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment> { appt });

        var result = await _service.CanDoctorAccessPatientAsync(doctorUserId, patientRecordId, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task CanDoctorAccessPatientAsync_NoRelationship_ReturnsFalse()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockApptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var result = await _service.CanDoctorAccessPatientAsync(doctorUserId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_AddsRecord()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var dto = new CreatePatientRecordDto
        {
            PsychologicalHistory = "No known allergies",
            GeneralNotes = "Healthy"
        };

        var result = await _service.CreateAsync(doctorUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<PatientRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingRecord_UpdatesFields()
    {
        var id = Guid.NewGuid();
        var record = new PatientRecord { Id = id, PsychologicalHistory = "Old", Doctor = new DoctorProfile { User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } } };

        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var dto = new UpdatePatientRecordDto { PsychologicalHistory = "Updated", GeneralNotes = "Good" };
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Updated", record.PsychologicalHistory);
        Assert.Equal("Good", record.GeneralNotes);
        _mockRepo.Verify(r => r.Update(record), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsError()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PatientRecord?)null);

        var dto = new UpdatePatientRecordDto { PsychologicalHistory = "Updated" };
        var result = await _service.UpdateAsync(Guid.NewGuid(), dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetMyPatientsAsync_DoctorWithAssignedPatients_ReturnsPatients()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var record = new PatientRecord { Id = Guid.NewGuid(), DoctorId = doctorProfileId, PatientId = patientProfileId, Doctor = doc };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { record });

        var result = await _service.GetMyPatientsAsync(doctorUserId, CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetMyPatientsAsync_DoctorNotFound_ReturnsEmpty()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.GetMyPatientsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateBatchAsync_ValidInput_AddsMultipleRecords()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var dtos = new List<CreatePatientRecordDto>
        {
            new() { GuestName = "Patient 1", GuestPhone = "0901234567", GuestEmail = "p1@example.com", GeneralNotes = "Note 1" },
            new() { GuestName = "Patient 2", GuestPhone = "0912345678", GuestEmail = "p2@example.com", GeneralNotes = "Note 2" }
        };

        var result = await _service.CreateBatchAsync(doctorUserId, dtos, CancellationToken.None);

        Assert.True(result.Success);
        _mockRepo.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<PatientRecord>>(list => list.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBatchAsync_EmptyList_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var result = await _service.CreateBatchAsync(doctorUserId, new List<CreatePatientRecordDto>(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAsync_ValidDoctorAndRecord_MarksAsDeleted()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var recordId = Guid.NewGuid();
        var record = new PatientRecord { Id = recordId, DoctorId = doctorProfileId, Doctor = doc, GuestName = "Guest Test", IsDeleted = false };
        _mockRepo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _service.DeleteAsync(doctorUserId, recordId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(record.IsDeleted);
        _mockRepo.Verify(r => r.Update(record), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnauthorizedDoctor_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });

        var recordId = Guid.NewGuid();
        var otherDoctorId = Guid.NewGuid();
        var record = new PatientRecord { Id = recordId, DoctorId = otherDoctorId, Doctor = doc, GuestName = "Other Doctor Guest", IsDeleted = false };
        _mockRepo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _service.DeleteAsync(doctorUserId, recordId, CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(record.IsDeleted);
    }
}
