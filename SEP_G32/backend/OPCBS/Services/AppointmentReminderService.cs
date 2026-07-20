using OPCBS.Application.Interfaces;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Enums;

namespace OPCBS.Services;

/// <summary>
/// Background service that checks every 5 minutes for upcoming appointments
/// and sends reminder notifications 1 hour before the appointment.
/// </summary>
public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public AppointmentReminderService(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentReminderService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync(stoppingToken);
                await CheckAndSendFollowUpRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AppointmentReminderService");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("AppointmentReminderService stopped.");
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var apptRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.Appointment>>();
        var slotRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.AppointmentSlot>>();
        var doctorRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.DoctorProfile>>();
        var patientRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.PatientProfile>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.User>>();
        var notifRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.Notification>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var reminderWindowStart = now.AddMinutes(55);
        var reminderWindowEnd = now.AddMinutes(65);

        var allAppts = await apptRepo.GetAllAsync(ct);
        var allSlots = await slotRepo.GetAllAsync(ct);
        var allNotifs = await notifRepo.GetAllAsync(ct);

        var slotDict = allSlots.ToDictionary(s => s.Id, s => s);

        // Find appointments that are Approved and start within 55-65 minutes from now
        var upcomingAppts = allAppts.Where(a =>
            a.Status == AppointmentStatus.Approved &&
            !a.IsDeleted &&
            slotDict.TryGetValue(a.AppointmentSlotId, out var slot) &&
            slot.SlotDate.ToDateTime(slot.StartTime) >= reminderWindowStart &&
            slot.SlotDate.ToDateTime(slot.StartTime) <= reminderWindowEnd
        ).ToList();

        if (!upcomingAppts.Any()) return;

        var allDoctors = await doctorRepo.GetAllAsync(ct);
        var allPatients = await patientRepo.GetAllAsync(ct);
        var allUsers = await userRepo.GetAllAsync(ct);
        var userDict = allUsers.ToDictionary(u => u.Id, u => u.FullName);
        var doctorUserMap = allDoctors.ToDictionary(d => d.Id, d => d.UserId);
        var patientUserMap = allPatients.ToDictionary(p => p.Id, p => p.UserId);

        foreach (var appt in upcomingAppts)
        {
            if (!slotDict.TryGetValue(appt.AppointmentSlotId, out var slot)) continue;

            // Check if reminder already sent (avoid duplicates)
            var alreadySent = allNotifs.Any(n =>
                n.RelatedEntityId == appt.Id &&
                n.RelatedEntityType == "AppointmentReminder" &&
                n.Type == NotificationType.Reminder);
            if (alreadySent) continue;

            var doctorName = "bác sĩ";
            if (doctorUserMap.TryGetValue(appt.DoctorId, out var docUserId) && userDict.TryGetValue(docUserId, out var dName))
                doctorName = dName;

            var patientName = appt.GuestName ?? "Bệnh nhân";
            Guid? patientUserId = null;
            if (appt.PatientId.HasValue && patientUserMap.TryGetValue(appt.PatientId.Value, out var patUserId))
            {
                patientUserId = patUserId;
                if (userDict.TryGetValue(patUserId, out var pName))
                    patientName = pName;
            }

            var timeStr = slot.StartTime.ToString("HH\\:mm");
            var dateStr = slot.SlotDate.ToString("dd/MM/yyyy");

            // Notify patient
            if (patientUserId.HasValue)
            {
                try
                {
                    await notificationService.CreateNotificationAsync(
                        patientUserId.Value,
                        "⏰ Nhắc nhở buổi hẹn",
                        $"Buổi tư vấn với BS {doctorName} sẽ bắt đầu lúc {timeStr} ngày {dateStr} (còn khoảng 1 tiếng nữa). Hãy chuẩn bị nhé!",
                        NotificationType.Reminder,
                        appt.Id,
                        "AppointmentReminder",
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send patient reminder for appointment {ApptId}", appt.Id);
                }
            }

            // Notify doctor
            if (doctorUserMap.TryGetValue(appt.DoctorId, out var doctorUserId))
            {
                try
                {
                    await notificationService.CreateNotificationAsync(
                        doctorUserId,
                        "⏰ Nhắc nhở buổi hẹn",
                        $"Bạn có buổi tư vấn với {patientName} lúc {timeStr} ngày {dateStr} (còn khoảng 1 tiếng nữa).",
                        NotificationType.Reminder,
                        appt.Id,
                        "AppointmentReminder",
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send doctor reminder for appointment {ApptId}", appt.Id);
                }
            }

            _logger.LogInformation("Sent reminders for appointment {ApptId}", appt.Id);
        }
    }

    /// <summary>
    /// Checks ConsultationNotes with NextAppointmentRecommendedDate and sends
    /// follow-up reminders 1 day before the recommended date.
    /// </summary>
    private async Task CheckAndSendFollowUpRemindersAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var noteRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.ConsultationNote>>();
        var doctorRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.DoctorProfile>>();
        var patientRecordRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.PatientRecord>>();
        var patientRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.PatientProfile>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.User>>();
        var notifRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.Notification>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;
        var tomorrowStart = now.Date.AddDays(1);
        var tomorrowEnd = tomorrowStart.AddDays(1);

        var allNotes = await noteRepo.GetAllAsync(ct);
        var allNotifs = await notifRepo.GetAllAsync(ct);

        // Find notes with follow-up date = tomorrow
        var followUpNotes = allNotes.Where(n =>
            !n.IsDeleted &&
            n.NextAppointmentRecommendedDate.HasValue &&
            n.NextAppointmentRecommendedDate.Value.Date >= tomorrowStart &&
            n.NextAppointmentRecommendedDate.Value.Date < tomorrowEnd
        ).ToList();

        if (!followUpNotes.Any()) return;

        var allDoctors = await doctorRepo.GetAllAsync(ct);
        var allPatientRecords = await patientRecordRepo.GetAllAsync(ct);
        var allPatients = await patientRepo.GetAllAsync(ct);
        var allUsers = await userRepo.GetAllAsync(ct);
        var userDict = allUsers.ToDictionary(u => u.Id, u => u);
        var doctorUserMap = allDoctors.ToDictionary(d => d.Id, d => d.UserId);

        foreach (var note in followUpNotes)
        {
            // Check if follow-up reminder already sent (dedup)
            var alreadySent = allNotifs.Any(n =>
                n.RelatedEntityId == note.Id &&
                n.RelatedEntityType == "FollowUpReminder" &&
                n.Type == NotificationType.Reminder);
            if (alreadySent) continue;

            // Get patient user
            var patientRecord = allPatientRecords.FirstOrDefault(pr => pr.Id == note.PatientRecordId);
            if (patientRecord?.PatientId == null) continue;

            var patient = allPatients.FirstOrDefault(p => p.Id == patientRecord.PatientId.Value);
            if (patient == null) continue;

            if (!userDict.TryGetValue(patient.UserId, out var patientUser)) continue;

            // Get doctor name
            var doctorName = "your doctor";
            if (doctorUserMap.TryGetValue(note.DoctorId, out var docUserId) && userDict.TryGetValue(docUserId, out var docUser))
                doctorName = docUser.FullName;

            var dateStr = note.NextAppointmentRecommendedDate!.Value.ToString("dd/MM/yyyy");

            // Send in-app notification
            try
            {
                await notificationService.CreateNotificationAsync(
                    patient.UserId,
                    "🔔 Nhắc nhở tái khám",
                    $"Bạn có lịch tái khám với BS {doctorName} vào ngày {dateStr}. Hãy đặt lịch hẹn ngay nhé!",
                    NotificationType.Reminder,
                    note.Id,
                    "FollowUpReminder",
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send follow-up notification for note {NoteId}", note.Id);
            }

            // Send email
            try
            {
                if (!string.IsNullOrEmpty(patientUser.Email))
                {
                    await emailService.SendFollowUpReminderEmailAsync(
                        patientUser.Email,
                        patientUser.FullName ?? "Patient",
                        doctorName,
                        dateStr,
                        ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send follow-up email for note {NoteId}", note.Id);
            }

            _logger.LogInformation("Sent follow-up reminder for consultation note {NoteId}", note.Id);
        }
    }
}
