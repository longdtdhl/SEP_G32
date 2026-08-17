using Moq;
using OPCBS.Application.DTOs.Messaging;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class MessagingServiceTests
{
    private readonly Mock<IRepository<Conversation>> _mockConversationRepo = new();
    private readonly Mock<IRepository<Message>> _mockMessageRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _mockPatientRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _mockDoctorRepo = new();
    private readonly Mock<IRepository<Appointment>> _mockAppointmentRepo = new();
    private readonly Mock<IRepository<TreatmentPackage>> _mockPackageRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<INotificationService> _mockNotificationService = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly MessagingService _service;

    public MessagingServiceTests()
    {
        _service = new MessagingService(
            _mockConversationRepo.Object,
            _mockMessageRepo.Object,
            _mockPatientRepo.Object,
            _mockDoctorRepo.Object,
            _mockAppointmentRepo.Object,
            _mockPackageRepo.Object,
            _mockUserRepo.Object,
            _mockNotificationService.Object,
            _mockUow.Object);
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsUserConversations()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = dUser };

        var conv = new Conversation
        {
            Id = convId,
            PatientId = patientUserId,
            DoctorId = doctorUserId,
            Patient = patient,
            Doctor = doctor,
            CreatedAt = DateTime.UtcNow
        };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Conversation> { conv });
        _mockMessageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Message>());
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });

        var result = await _service.GetConversationsAsync(patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetMessagesAsync_UserIsParticipant_ReturnsMessages()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = dUser };

        var conv = new Conversation
        {
            Id = convId,
            PatientId = patientUserId,
            DoctorId = doctorUserId,
            Patient = patient,
            Doctor = doctor
        };

        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ConversationId = convId, SenderId = patientUserId, Content = "Hello doc", CreatedAt = DateTime.UtcNow, Conversation = conv }
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);
        _mockMessageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(messages);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });

        var result = await _service.GetMessagesAsync(patientUserId, convId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetMessagesAsync_UserNotParticipant_ReturnsError()
    {
        var unauthorizedUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = Guid.NewGuid(), Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = Guid.NewGuid(), Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), User = dUser };

        var conv = new Conversation
        {
            Id = convId,
            PatientId = pUser.Id,
            DoctorId = dUser.Id,
            Patient = patient,
            Doctor = doctor
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var result = await _service.GetMessagesAsync(unauthorizedUserId, convId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SendMessageAsync_ValidMessage_SendsAndUpdatesLastMessage()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = senderId, Email = "p@test.com", PasswordHash = "h", FullName = "Sender", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = recipientId, Email = "d@test.com", PasswordHash = "h", FullName = "Recipient", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = senderId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = recipientId, User = dUser };

        var conv = new Conversation
        {
            Id = convId,
            PatientId = senderId,
            DoctorId = recipientId,
            Status = ConversationStatus.Open,
            Patient = patient,
            Doctor = doctor
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);
        _mockUserRepo.Setup(r => r.GetByIdAsync(senderId, It.IsAny<CancellationToken>())).ReturnsAsync(pUser);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });

        var dto = new SendMessageDto { Content = "Can we reschedule?" };
        var result = await _service.SendMessageAsync(senderId, convId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Can we reschedule?", result.Data!.Content);
        _mockMessageRepo.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ClosedConversation_ReturnsError()
    {
        var senderId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = senderId, Email = "p@test.com", PasswordHash = "h", FullName = "Sender", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = Guid.NewGuid(), Email = "d@test.com", PasswordHash = "h", FullName = "Doc", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = senderId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), User = dUser };

        var conv = new Conversation
        {
            Id = convId,
            PatientId = senderId,
            DoctorId = dUser.Id,
            Status = ConversationStatus.Closed,
            Patient = patient,
            Doctor = doctor
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var dto = new SendMessageDto { Content = "Hello" };
        var result = await _service.SendMessageAsync(senderId, convId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidParticipant_MarksUnreadMessagesAsRead()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = userId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = otherUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = userId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = otherUserId, User = dUser };

        var conv = new Conversation { Id = convId, PatientId = userId, DoctorId = otherUserId, Patient = patient, Doctor = doctor };
        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ConversationId = convId, SenderId = otherUserId, IsRead = false, Conversation = conv }
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);
        _mockMessageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(messages);

        var result = await _service.MarkAsReadAsync(userId, convId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(messages[0].IsRead);
        _mockMessageRepo.Verify(r => r.UpdateRange(It.IsAny<IEnumerable<Message>>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsTotalUnreadForUser()
    {
        var userId = Guid.NewGuid();
        var docUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = userId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = docUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = userId, User = pUser };
        var doctor = new DoctorProfile { Id = Guid.NewGuid(), UserId = docUserId, User = dUser };

        var conv = new Conversation { Id = convId, PatientId = userId, DoctorId = docUserId, Patient = patient, Doctor = doctor };
        var convs = new List<Conversation> { conv };

        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ConversationId = convId, SenderId = docUserId, IsRead = false, Conversation = conv },
            new() { Id = Guid.NewGuid(), ConversationId = convId, SenderId = docUserId, IsRead = false, Conversation = conv },
            new() { Id = Guid.NewGuid(), ConversationId = convId, SenderId = userId, IsRead = false, Conversation = conv } // Sent by user themselves
        };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(convs);
        _mockMessageRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(messages);

        var result = await _service.GetUnreadCountAsync(userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data);
    }

    [Fact]
    public async Task CloseConversationAsync_Valid_ClosesConversation()
    {
        var convId = Guid.NewGuid();
        var pUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var conv = new Conversation
        {
            Id = convId,
            Status = ConversationStatus.Open,
            Patient = new PatientProfile { User = pUser },
            Doctor = new DoctorProfile { User = dUser }
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var result = await _service.CloseConversationAsync(convId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ConversationStatus.Closed, conv.Status);
        _mockConversationRepo.Verify(r => r.Update(conv), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseConversationAsync_NotFound_ReturnsError()
    {
        _mockConversationRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Conversation?)null);

        var result = await _service.CloseConversationAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetOrCreateConversationAsync_ExistingConversation_ReturnsIt()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };

        var existingConv = new Conversation
        {
            Id = convId,
            PatientId = patientUserId,
            DoctorId = doctorUserId,
            Patient = patient,
            Doctor = doctor,
            Status = ConversationStatus.Open
        };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Conversation> { existingConv });
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>
        {
            new() { Id = Guid.NewGuid(), BookingCode = "BK-1", PatientId = patientProfileId, DoctorId = doctorProfileId, Status = AppointmentStatus.Approved, Doctor = doctor, AppointmentSlot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doctor } }
        });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });

        var result = await _service.GetOrCreateConversationAsync(patientUserId, doctorUserId, null, null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(convId, result.Data!.Id);
        _mockConversationRepo.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateConversationAsync_NewValidRelationship_CreatesConversation()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Conversation>());
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>
        {
            new() { Id = Guid.NewGuid(), BookingCode = "BK-1", PatientId = patientProfileId, DoctorId = doctorProfileId, Status = AppointmentStatus.Approved, Doctor = doctor, AppointmentSlot = new AppointmentSlot { SlotDate = DateOnly.FromDateTime(DateTime.UtcNow), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), DoctorProfile = doctor } }
        });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });

        var result = await _service.GetOrCreateConversationAsync(patientUserId, doctorUserId, null, null, CancellationToken.None);

        Assert.True(result.Success);
        _mockConversationRepo.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateConversationAsync_NoCareRelationship_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = patientProfileId, UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Conversation>());
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockAppointmentRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());

        var result = await _service.GetOrCreateConversationAsync(patientUserId, doctorUserId, null, null, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetConversationAuditsAsync_ReturnsAllConversationsWithoutContent()
    {
        var pUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { User = pUser };
        var doctor = new DoctorProfile { User = dUser };

        var convs = new List<Conversation>
        {
            new() { Id = Guid.NewGuid(), PatientId = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Patient = patient, Doctor = doctor, CreatedAt = DateTime.UtcNow }
        };

        _mockConversationRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(convs);
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { pUser, dUser });

        var result = await _service.GetConversationAuditsAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task SendMessageAsync_EmptyContent_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var pUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var conv = new Conversation
        {
            Id = convId,
            PatientId = userId,
            DoctorId = Guid.NewGuid(),
            Patient = new PatientProfile { User = pUser },
            Doctor = new DoctorProfile { User = dUser }
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var dto = new SendMessageDto { Content = "   " };
        var result = await _service.SendMessageAsync(userId, convId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SendMessageAsync_NonParticipant_ReturnsError()
    {
        var unauthorizedUserId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var pUser = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } };
        var dUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var conv = new Conversation
        {
            Id = convId,
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            Patient = new PatientProfile { User = pUser },
            Doctor = new DoctorProfile { User = dUser }
        };

        _mockConversationRepo.Setup(r => r.GetByIdAsync(convId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var dto = new SendMessageDto { Content = "Hello" };
        var result = await _service.SendMessageAsync(unauthorizedUserId, convId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }
}
