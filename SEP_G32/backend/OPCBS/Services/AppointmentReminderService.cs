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
                await CheckPendingCompletionConfirmationsAsync(stoppingToken);
                await CheckGuestBookingConfirmationsAsync(stoppingToken);
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

    /// <summary>
    /// Reminds a patient one day after a doctor requests completion confirmation.
    /// Unresolved requests lock the patient account after seven days, as approved by policy.
    /// </summary>
    private async Task CheckPendingCompletionConfirmationsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var confirmationRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.AppointmentCompletionConfirmation>>();
        var appointmentRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.Appointment>>();
        var historyRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.AppointmentHistory>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.User>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var confirmations = (await confirmationRepo.GetAllAsync(ct))
            .Where(c => !c.IsDeleted && c.Status == AppointmentCompletionConfirmationStatus.Pending)
            .ToList();
        if (confirmations.Count == 0) return;

        var users = await userRepo.GetAllAsync(ct);
        foreach (var confirmation in confirmations)
        {
            var patient = users.FirstOrDefault(u => u.Id == confirmation.PatientUserId && !u.IsDeleted);
            if (patient == null) continue;

            if (!confirmation.ReminderSentAt.HasValue && now >= confirmation.ReminderDueAt && now < confirmation.EscalationDueAt)
            {
                try
                {
                    await notificationService.CreateNotificationAsync(patient.Id, "Completion confirmation reminder",
                        "Customer Support is waiting for you to confirm your consultation note. Please review it today to avoid policy action.",
                        NotificationType.Reminder, confirmation.AppointmentId, "AppointmentCompletionConfirmation", ct);
                    if (!string.IsNullOrWhiteSpace(patient.Email))
                        await emailService.SendEmailAsync(patient.Email, "OPCBS - Confirmation reminder", "<p>Please confirm your consultation note. Customer Support will review an unresolved request after seven days.</p>", ct);
                    confirmation.ReminderSentAt = now;
                    confirmation.UpdatedAt = now;
                    confirmationRepo.Update(confirmation);
                    await uow.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not send completion reminder for appointment {AppointmentId}", confirmation.AppointmentId);
                }
            }

            if (now < confirmation.EscalationDueAt) continue;

            var appointment = await appointmentRepo.GetByIdAsync(confirmation.AppointmentId, ct);
            confirmation.Status = AppointmentCompletionConfirmationStatus.ExpiredAndAccountLocked;
            confirmation.LockedAt = now;
            confirmation.UpdatedAt = now;
            patient.Status = UserStatus.Locked;
            userRepo.Update(patient);
            confirmationRepo.Update(confirmation);
            if (appointment != null && appointment.Status == AppointmentStatus.AwaitingPatientConfirmation)
            {
                var previousStatus = appointment.Status;
                appointment.Status = AppointmentStatus.NoShow;
                appointment.UpdatedAt = now;
                appointmentRepo.Update(appointment);
                await historyRepo.AddAsync(new Domain.Entities.AppointmentHistory
                {
                    AppointmentId = appointment.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = AppointmentStatus.NoShow,
                    Reason = "Patient did not confirm completion request within seven days.",
                    Appointment = appointment
                }, ct);
            }
            await uow.SaveChangesAsync(ct);
            try
            {
                await notificationService.CreateNotificationAsync(patient.Id, "Account locked for policy review",
                    "Your account was locked because a consultation completion request was not confirmed within seven days. Contact Customer Support by email to request reactivation.",
                    NotificationType.System, confirmation.AppointmentId, "AppointmentCompletionConfirmation", ct);
                if (!string.IsNullOrWhiteSpace(patient.Email))
                    await emailService.SendEmailAsync(patient.Email, "OPCBS - Account locked", "<p>Your account was locked after a consultation completion confirmation remained unresolved for seven days. Contact Customer Support by email to request reactivation.</p>", ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send account-lock notice for appointment {AppointmentId}", confirmation.AppointmentId);
            }
        }
    }

    /// <summary>Handles daily guest-email confirmation reminders and releases unconfirmed slots 24 hours before start.</summary>
    private async Task CheckGuestBookingConfirmationsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var appointmentRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.Appointment>>();
        var slotRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.AppointmentSlot>>();
        var historyRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.AppointmentHistory>>();
        var doctorRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.DoctorProfile>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<Domain.Entities.User>>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        var appointments = (await appointmentRepo.GetAllAsync(ct))
            .Where(a => !a.IsDeleted && a.Status == AppointmentStatus.AwaitingGuestConfirmation && !string.IsNullOrWhiteSpace(a.GuestEmail))
            .ToList();
        if (appointments.Count == 0) return;
        var doctors = await doctorRepo.GetAllAsync(ct);
        var users = await userRepo.GetAllAsync(ct);

        foreach (var appointment in appointments)
        {
            var slot = await slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
            if (slot == null) continue;
            var appointmentStart = slot.SlotDate.ToDateTime(slot.StartTime);
            if (appointmentStart <= now.AddDays(1))
            {
                var previousStatus = appointment.Status;
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancelledAt = now;
                appointment.CancellationReason = "Guest did not confirm booking by email at least 24 hours before the appointment.";
                appointment.GuestConfirmationTokenHash = null;
                appointment.UpdatedAt = now;
                slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
                slot.Status = slot.CurrentBookings < slot.MaxPatients ? AppointmentSlotStatus.Available : AppointmentSlotStatus.Booked;
                appointmentRepo.Update(appointment);
                slotRepo.Update(slot);
                await historyRepo.AddAsync(new Domain.Entities.AppointmentHistory
                {
                    AppointmentId = appointment.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = AppointmentStatus.Cancelled,
                    Reason = appointment.CancellationReason,
                    Appointment = appointment
                }, ct);
                await uow.SaveChangesAsync(ct);
                try
                {
                    await emailService.SendEmailAsync(appointment.GuestEmail!, "OPCBS - Unconfirmed booking cancelled",
                        $"<p>Your booking <strong>{System.Net.WebUtility.HtmlEncode(appointment.BookingCode)}</strong> was cancelled because it was not confirmed at least 24 hours before the appointment. The slot is now available again.</p>", ct);
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not send guest cancellation email for {AppointmentId}", appointment.Id); }
                continue;
            }

            if (appointment.GuestConfirmationLastSentAt?.Date >= now.Date) continue;
            var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            appointment.GuestConfirmationTokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
            appointment.GuestConfirmationLastSentAt = now;
            appointment.GuestConfirmationSendCount++;
            appointment.UpdatedAt = now;
            appointmentRepo.Update(appointment);
            await uow.SaveChangesAsync(ct);
            var doctor = doctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
            var doctorName = doctor == null ? "your doctor" : users.FirstOrDefault(u => u.Id == doctor.UserId)?.FullName ?? "your doctor";
            var confirmUrl = $"http://localhost:5044/Appointment/GuestConfirm?token={Uri.EscapeDataString(token)}";
            try
            {
                await emailService.SendEmailAsync(appointment.GuestEmail!, "OPCBS - Reminder to confirm your appointment",
                    $"<p>Please confirm your appointment with <strong>{System.Net.WebUtility.HtmlEncode(doctorName)}</strong>.</p><p><a href='{confirmUrl}'>Confirm appointment</a></p><p>Bookings not confirmed at least 24 hours before the appointment are cancelled automatically.</p>", ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not send guest confirmation reminder for {AppointmentId}", appointment.Id); }
        }
    }
}
