using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;

namespace OPCBS.Tests;

public class ScheduleNoteSlotTests
{
    [Fact]
    public async Task CreateNoteAsync_WithOwnedSlot_LinksAndNormalizesSlotTime()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var appointmentRepo = new Mock<IRepository<Appointment>>();
        var noteRepo = new Mock<IRepository<ScheduleNote>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctor = CreateDoctor();
        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctor.Id,
            DoctorProfile = doctor,
            SlotDate = new DateOnly(2026, 8, 25),
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 30)
        };
        ScheduleNote? savedNote = null;

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { doctor });
        slotRepo.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);
        noteRepo.Setup(r => r.AddAsync(It.IsAny<ScheduleNote>(), It.IsAny<CancellationToken>()))
            .Callback<ScheduleNote, CancellationToken>((note, _) => savedNote = note)
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new ScheduleService(
            scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object,
            dayOffRepo.Object, appointmentRepo.Object, unitOfWork.Object, mapper.Object,
            scheduleNoteRepo: noteRepo.Object);

        var result = await service.CreateNoteAsync(doctor.UserId, new CreateScheduleNoteDto
        {
            AppointmentSlotId = slot.Id,
            Date = "2000-01-01",
            StartTime = "01:00",
            EndTime = "02:00",
            Title = "Preparation",
            Content = "Review patient history."
        });

        Assert.True(result.Success);
        Assert.NotNull(savedNote);
        Assert.Equal(slot.Id, savedNote.AppointmentSlotId);
        Assert.Equal(slot.SlotDate, savedNote.NoteDate);
        Assert.Equal(slot.StartTime, savedNote.StartTime);
        Assert.Equal(slot.EndTime, savedNote.EndTime);
        Assert.Equal(slot.Id, result.Data?.AppointmentSlotId);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_ReturnsLinkedNoteCountAndCompatibilityFlag()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var appointmentRepo = new Mock<IRepository<Appointment>>();
        var noteRepo = new Mock<IRepository<ScheduleNote>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctor = CreateDoctor();
        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctor.Id,
            DoctorProfile = doctor,
            SlotDate = new DateOnly(2026, 8, 25),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Status = AppointmentSlotStatus.Available,
            Notes = "Legacy slot note"
        };
        var linkedNotes = new[]
        {
            CreateLinkedNote(doctor.Id, slot, "First"),
            CreateLinkedNote(doctor.Id, slot, "Second"),
            new ScheduleNote
            {
                Id = Guid.NewGuid(),
                DoctorProfileId = doctor.Id,
                NoteDate = slot.SlotDate,
                Title = "Unlinked",
                Content = "Must not be counted."
            }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor });
        slotRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { slot });
        dayOffRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<DoctorDayOff>());
        appointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Appointment>());
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor.User });
        noteRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(linkedNotes);

        var service = new ScheduleService(
            scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object,
            dayOffRepo.Object, appointmentRepo.Object, unitOfWork.Object, mapper.Object,
            scheduleNoteRepo: noteRepo.Object);

        var result = await service.GetCalendarEventsAsync(
            doctor.UserId,
            slot.SlotDate.ToDateTime(TimeOnly.MinValue),
            slot.SlotDate.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var calendarEvent = Assert.Single(result.Data!);
        Assert.True(calendarEvent.HasNote);
        Assert.True(calendarEvent.HasNotes);
        Assert.Equal(3, calendarEvent.NoteCount);
    }

    [Fact]
    public async Task GetNotesAsync_WithAppointmentSlotId_ReturnsOnlyThatSlotsNotes()
    {
        var scheduleRepo = new Mock<IRepository<Schedule>>();
        var slotRepo = new Mock<IRepository<AppointmentSlot>>();
        var doctorRepo = new Mock<IRepository<DoctorProfile>>();
        var userRepo = new Mock<IRepository<User>>();
        var dayOffRepo = new Mock<IRepository<DoctorDayOff>>();
        var appointmentRepo = new Mock<IRepository<Appointment>>();
        var noteRepo = new Mock<IRepository<ScheduleNote>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();

        var doctor = CreateDoctor();
        var requestedSlotId = Guid.NewGuid();
        var otherSlotId = Guid.NewGuid();
        var notes = new[]
        {
            new ScheduleNote
            {
                Id = Guid.NewGuid(), DoctorProfileId = doctor.Id, AppointmentSlotId = requestedSlotId,
                NoteDate = new DateOnly(2026, 8, 25), Title = "Requested", Content = "Requested note"
            },
            new ScheduleNote
            {
                Id = Guid.NewGuid(), DoctorProfileId = doctor.Id, AppointmentSlotId = otherSlotId,
                NoteDate = new DateOnly(2026, 8, 25), Title = "Other", Content = "Other note"
            }
        };

        doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor });
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { doctor.User });
        noteRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notes);

        var service = new ScheduleService(
            scheduleRepo.Object, slotRepo.Object, doctorRepo.Object, userRepo.Object,
            dayOffRepo.Object, appointmentRepo.Object, unitOfWork.Object, mapper.Object,
            scheduleNoteRepo: noteRepo.Object);

        var result = await service.GetNotesAsync(doctor.UserId, appointmentSlotId: requestedSlotId);

        var note = Assert.Single(result.Data!);
        Assert.Equal(requestedSlotId, note.AppointmentSlotId);
        Assert.Equal("Requested", note.Title);
    }

    private static DoctorProfile CreateDoctor()
    {
        var userId = Guid.NewGuid();
        return new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "doctor@example.com",
                FullName = "Dr. Schedule",
                PhoneNumber = "0123456789",
                PasswordHash = "hash",
                Role = new Role { Name = "Doctor" }
            }
        };
    }

    private static ScheduleNote CreateLinkedNote(Guid doctorId, AppointmentSlot slot, string title) => new()
    {
        Id = Guid.NewGuid(),
        DoctorProfileId = doctorId,
        AppointmentSlotId = slot.Id,
        NoteDate = slot.SlotDate,
        StartTime = slot.StartTime,
        EndTime = slot.EndTime,
        Title = title,
        Content = title + " content"
    };
}
