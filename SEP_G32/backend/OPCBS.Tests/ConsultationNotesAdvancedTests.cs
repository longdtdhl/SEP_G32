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

public class ConsultationNotesAdvancedTests
{
    private readonly Mock<IRepository<ConsultationNote>> _recordRepo = new();
    private readonly Mock<IRepository<Appointment>> _apptRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepo = new();
    private readonly Mock<IRepository<PatientRecord>> _patientRecordRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<TreatmentPackage>> _packageRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly ConsultationNoteService _service;

    public ConsultationNotesAdvancedTests()
    {
        _service = new ConsultationNoteService(
            _recordRepo.Object,
            _apptRepo.Object,
            _doctorRepo.Object,
            _patientRepo.Object,
            _patientRecordRepo.Object,
            _userRepo.Object,
            _packageRepo.Object,
            _notificationService.Object,
            _uow.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task GetByDoctorAsync_ReturnsDoctorNotes()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = Guid.NewGuid(), DoctorId = doctorProfileId, Doctor = doc };
        var notes = new List<ConsultationNote>
        {
            new() { Id = Guid.NewGuid(), DoctorId = doctorProfileId, PatientRecordId = pRecord.Id, ConsultationSummary = "Session 1", Doctor = doc, PatientRecord = pRecord }
        };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notes);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doc.User });
        _patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { pRecord });
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        _mapper.Setup(m => m.Map<List<ConsultationNoteDto>>(It.IsAny<List<ConsultationNote>>()))
            .Returns(new List<ConsultationNoteDto> { new() { Id = notes[0].Id, ConsultationSummary = "Session 1" } });

        var result = await _service.GetByDoctorAsync(doctorUserId, 1, 10, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetByDoctorAsync_DoctorNotFound_ReturnsError()
    {
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.GetByDoctorAsync(Guid.NewGuid(), 1, 10, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetByPatientAsync_ReturnsVisibleNotesOnly()
    {
        var patientUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = Guid.NewGuid(), PatientId = patientProfileId, Doctor = doc };

        var notes = new List<ConsultationNote>
        {
            new() { Id = Guid.NewGuid(), PatientRecordId = pRecord.Id, Visibility = NoteVisibility.PatientVisible, ConsultationSummary = "Shared Notes", Doctor = doc, PatientRecord = pRecord }
        };

        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { pRecord });
        _recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notes);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, doc.User });
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        _mapper.Setup(m => m.Map<List<ConsultationNoteDto>>(It.IsAny<List<ConsultationNote>>()))
            .Returns(new List<ConsultationNoteDto> { new() { Id = notes[0].Id, ConsultationSummary = "Shared Notes" } });

        var result = await _service.GetByPatientAsync(patientUserId, 1, 10, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetByPatientAsync_PatientNotFound_ReturnsError()
    {
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());

        var result = await _service.GetByPatientAsync(Guid.NewGuid(), 1, 10, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingNote_ReturnsNoteDto()
    {
        var noteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = userId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = Guid.NewGuid(), Doctor = doc };
        var note = new ConsultationNote { Id = noteId, DoctorId = doc.Id, PatientRecordId = pRecord.Id, ConsultationSummary = "Detailed note", Doctor = doc, PatientRecord = pRecord };

        _recordRepo.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { pRecord });
        _recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ConsultationNote> { note });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doc.User });
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        _mapper.Setup(m => m.Map<ConsultationNoteDto>(note)).Returns(new ConsultationNoteDto { Id = noteId, ConsultationSummary = "Detailed note" });

        var result = await _service.GetByIdAsync(noteId, userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsError()
    {
        _recordRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ConsultationNote?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetByAppointmentAsync_ValidDoctor_ReturnsNotes()
    {
        var apptId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = Guid.NewGuid(), DoctorId = doctorProfileId, Doctor = doc };
        var notes = new List<ConsultationNote>
        {
            new() { Id = Guid.NewGuid(), AppointmentId = apptId, DoctorId = doctorProfileId, PatientRecordId = pRecord.Id, ConsultationSummary = "Appt Note", Doctor = doc, PatientRecord = pRecord }
        };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _recordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notes);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _patientRecordRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientRecord> { pRecord });
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { doc.User });
        _apptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        _packageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<TreatmentPackage>());
        _mapper.Setup(m => m.Map<List<ConsultationNoteDto>>(It.IsAny<List<ConsultationNote>>()))
            .Returns(new List<ConsultationNoteDto> { new() { Id = notes[0].Id, ConsultationSummary = "Appt Note" } });

        var result = await _service.GetByAppointmentAsync(apptId, doctorUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task CreateAsync_DoctorNotFound_ReturnsError()
    {
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var dto = new CreateConsultationNoteDto { PatientRecordId = Guid.NewGuid(), ConsultationSummary = "Summary" };
        var result = await _service.CreateAsync(Guid.NewGuid(), dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_PatientRecordNotFound_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _patientRecordRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PatientRecord?)null);

        var dto = new CreateConsultationNoteDto { PatientRecordId = Guid.NewGuid(), ConsultationSummary = "Summary" };
        var result = await _service.CreateAsync(doctorUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesNoteAndSaves()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patientRecordId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = patientRecordId, DoctorId = doctorProfileId, Doctor = doc };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _patientRecordRepo.Setup(r => r.GetByIdAsync(patientRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(pRecord);
        _mapper.Setup(m => m.Map<ConsultationNoteDto>(It.IsAny<ConsultationNote>()))
            .Returns(new ConsultationNoteDto { Id = Guid.NewGuid(), ConsultationSummary = "Weekly CBT sessions" });

        var dto = new CreateConsultationNoteDto
        {
            PatientRecordId = patientRecordId,
            ConsultationSummary = "Weekly CBT sessions",
            Diagnosis = "Adjustment Disorder",
            TherapyPlan = "Weekly CBT sessions",
            ConsultationDate = DateTime.UtcNow
        };

        var result = await _service.CreateAsync(doctorUserId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        _recordRepo.Verify(r => r.AddAsync(It.IsAny<ConsultationNote>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoctorNotFound_ReturnsError()
    {
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var dto = new UpdateConsultationNoteDto { ConsultationSummary = "Summary", Diagnosis = "Test" };
        var result = await _service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAsync_NoteNotFound_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _recordRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ConsultationNote?)null);

        var dto = new UpdateConsultationNoteDto { ConsultationSummary = "Summary", Diagnosis = "Test" };
        var result = await _service.UpdateAsync(Guid.NewGuid(), doctorUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAsync_ConfirmedNote_PreventsModification()
    {
        var noteId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var pRecord = new PatientRecord { Id = Guid.NewGuid(), Doctor = doc };
        var note = new ConsultationNote { Id = noteId, DoctorId = doctorProfileId, PatientRecordId = pRecord.Id, ConsultationSummary = "Old", IsPatientConfirmed = true, Doctor = doc, PatientRecord = pRecord };

        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _recordRepo.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var dto = new UpdateConsultationNoteDto { ConsultationSummary = "New Summary", Diagnosis = "New Diagnosis" };
        var result = await _service.UpdateAsync(noteId, doctorUserId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ConfirmByPatientAsync_PatientNotFound_ReturnsError()
    {
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());

        var result = await _service.ConfirmByPatientAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ConfirmByPatientAsync_NoteNotFound_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var patient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            UserId = patientUserId,
            User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } }
        };

        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _recordRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ConsultationNote?)null);

        var result = await _service.ConfirmByPatientAsync(Guid.NewGuid(), patientUserId, CancellationToken.None);

        Assert.False(result.Success);
    }
}
