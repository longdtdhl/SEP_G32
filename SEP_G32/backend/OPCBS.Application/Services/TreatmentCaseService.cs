using Microsoft.Extensions.Logging;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

/// <summary>
/// Service implementation for Treatment Case management.
/// Handles creation with package snapshot, session tracking, schedule generation,
/// goal management & progress history, homework, mood tracking, and timeline aggregation.
/// </summary>
public class TreatmentCaseService : ITreatmentCaseService
{
    private readonly IRepository<TreatmentCase> _caseRepo;
    private readonly IRepository<TreatmentSession> _sessionRepo;
    private readonly IRepository<TreatmentGoal> _goalRepo;
    private readonly IRepository<TreatmentGoalProgress> _goalProgressRepo;
    private readonly IRepository<TreatmentSessionGoal> _sessionGoalRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<TherapyAssignment> _assignmentRepo;
    private readonly IRepository<MoodEntry> _moodRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<AppointmentSlot> _slotRepo;
    private readonly IRepository<ConsultationNote> _noteRepo;
    private readonly IRepository<EmotionJournal> _journalRepo;
    private readonly IRepository<PsychometricSubmission> _psychRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<AppointmentHistory> _appointmentHistoryRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TreatmentCaseService> _logger;

    public TreatmentCaseService(
        IRepository<TreatmentCase> caseRepo,
        IRepository<TreatmentSession> sessionRepo,
        IRepository<TreatmentGoal> goalRepo,
        IRepository<TreatmentGoalProgress> goalProgressRepo,
        IRepository<TreatmentSessionGoal> sessionGoalRepo,
        IRepository<TreatmentPackage> packageRepo,
        IRepository<TherapyAssignment> assignmentRepo,
        IRepository<MoodEntry> moodRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<AppointmentSlot> slotRepo,
        IRepository<ConsultationNote> noteRepo,
        IRepository<EmotionJournal> journalRepo,
        IRepository<PsychometricSubmission> psychRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<User> userRepo,
        IRepository<AppointmentHistory> appointmentHistoryRepo,
        IUnitOfWork uow,
        ILogger<TreatmentCaseService> logger)
    {
        _caseRepo = caseRepo;
        _sessionRepo = sessionRepo;
        _goalRepo = goalRepo;
        _goalProgressRepo = goalProgressRepo;
        _sessionGoalRepo = sessionGoalRepo;
        _packageRepo = packageRepo;
        _assignmentRepo = assignmentRepo;
        _moodRepo = moodRepo;
        _appointmentRepo = appointmentRepo;
        _slotRepo = slotRepo;
        _noteRepo = noteRepo;
        _journalRepo = journalRepo;
        _psychRepo = psychRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _userRepo = userRepo;
        _appointmentHistoryRepo = appointmentHistoryRepo;
        _uow = uow;
        _logger = logger;
    }

    // ==================== Treatment Case CRUD ====================

    public async Task<ApiResponse<TreatmentCaseDto>> CreateFromPackageAsync(CreateTreatmentCaseDto dto, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(dto.TreatmentPackageId, ct);
        if (package == null || package.IsDeleted)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment package not found.");

        if (package.Status != TreatmentPackageStatus.Active && package.Status != TreatmentPackageStatus.Created && package.Status != TreatmentPackageStatus.Assigned)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment package must be active/assigned to create a case.");

        var allCases = await _caseRepo.GetAllAsync(ct);
        var existingActive = allCases.FirstOrDefault(c =>
            c.DoctorId == dto.DoctorId &&
            c.PatientId == dto.PatientId &&
            c.TreatmentPackageId == dto.TreatmentPackageId &&
            c.Status == TreatmentCaseStatus.Active &&
            !c.IsDeleted);

        if (existingActive != null)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("An active treatment case already exists for this patient with this package.");

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == dto.DoctorId || d.UserId == dto.DoctorId);
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.Id == dto.PatientId || p.UserId == dto.PatientId);

        if (doctor == null || patient == null)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Doctor or Patient record not found.");

        var entity = new TreatmentCase
        {
            TreatmentPackageId = package.Id,
            DoctorId = doctor.UserId,
            PatientId = patient.UserId,
            CaseName = package.Name,
            CaseDescription = package.Description,
            PrimaryConcern = dto.PrimaryConcern ?? package.TargetOutcome,
            
            // Snapshot fields
            PackageNameSnapshot = package.Name,
            PackageDescriptionSnapshot = package.Description,
            TotalSessionsSnapshot = package.SessionQuantity,
            DurationDaysSnapshot = package.ValidityDays,
            RecommendedSessionsPerWeekSnapshot = package.RecommendedSessionsPerWeek,
            PriceSnapshot = package.Price,
            TargetOutcomesSnapshot = package.TargetOutcome,
            RecommendedExercisesSnapshot = package.RecommendedExercises,
            PatientGuidanceSnapshot = package.Instructions,

            TotalSessions = package.SessionQuantity,
            RemainingSessions = package.SessionQuantity,
            StartDate = DateTime.UtcNow,
            ExpectedEndDate = DateTime.UtcNow.AddDays(package.ValidityDays > 0 ? package.ValidityDays : 90),
            Status = TreatmentCaseStatus.Active,
            TreatmentPackage = package,
            Doctor = doctor,
            Patient = patient
        };

        await _caseRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct), "Treatment case created successfully.");
    }

    private async Task<bool> ValidateUserAccessToCaseAsync(TreatmentCase treatmentCase, Guid? requestingUserId, CancellationToken ct)
    {
        if (!requestingUserId.HasValue || requestingUserId.Value == Guid.Empty)
            return true;

        var userId = requestingUserId.Value;

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == userId || d.Id == userId);
        if (doctor != null && treatmentCase.DoctorId == doctor.Id)
            return true;

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId || p.Id == userId);
        if (patient != null && treatmentCase.PatientId == patient.Id)
            return true;

        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Id == userId);
        if (user != null && user.Role?.Name == "Admin")
            return true;

        return false;
    }

    public async Task<ApiResponse<TreatmentCaseDto>> GetByIdAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(entity, requestingUserId, ct))
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Access denied. You do not have permission to view this treatment case.");

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct));
    }

    public async Task<ApiResponse<List<TreatmentCaseListDto>>> GetByDoctorAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null)
            return ApiResponse<List<TreatmentCaseListDto>>.SuccessResponse(new List<TreatmentCaseListDto>());

        var all = await _caseRepo.GetAllAsync(ct);
        var cases = all
            .Where(c => !c.IsDeleted && (c.DoctorId == doctor.Id || c.DoctorId == doctor.UserId))
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var result = new List<TreatmentCaseListDto>();
        foreach (var c in cases)
            result.Add(await MapToListDtoAsync(c, ct));

        return ApiResponse<List<TreatmentCaseListDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<List<TreatmentCaseListDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<List<TreatmentCaseListDto>>.SuccessResponse(new List<TreatmentCaseListDto>());

        var all = await _caseRepo.GetAllAsync(ct);
        var cases = all
            .Where(c => !c.IsDeleted && (c.PatientId == patient.Id || c.PatientId == patient.UserId))
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var result = new List<TreatmentCaseListDto>();
        foreach (var c in cases)
            result.Add(await MapToListDtoAsync(c, ct));

        return ApiResponse<List<TreatmentCaseListDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<TreatmentCaseDto>> UpdateAsync(Guid caseId, UpdateTreatmentCaseDto dto, CancellationToken ct)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment case not found.");

        if (dto.CaseName != null) entity.CaseName = dto.CaseName;
        if (dto.CaseDescription != null) entity.CaseDescription = dto.CaseDescription;
        if (dto.PrimaryConcern != null) entity.PrimaryConcern = dto.PrimaryConcern;
        if (dto.Status.HasValue) entity.Status = (TreatmentCaseStatus)dto.Status.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        _caseRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct), "Treatment case updated successfully.");
    }

    public async Task<ApiResponse> CloseAsync(Guid caseId, CloseTreatmentCaseDto dto, CancellationToken ct = default)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse.ErrorResponse("Treatment case not found.");

        if (entity.Status != TreatmentCaseStatus.Active && entity.Status != TreatmentCaseStatus.OnHold)
            return ApiResponse.ErrorResponse("Only active or on-hold cases can be closed.");

        var newStatus = (TreatmentCaseStatus)dto.CloseStatus;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            entity.Status = newStatus;
            entity.ClosureNote = dto.ClosureNote;
            entity.ActualEndDate = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            if (newStatus == TreatmentCaseStatus.Completed)
            {
                entity.OverallProgressPercent = 100;
            }
            else if (newStatus == TreatmentCaseStatus.Terminated || newStatus == TreatmentCaseStatus.Cancelled)
            {
                var allSessions = await _sessionRepo.GetAllAsync(ct);
                var uncompletedSessions = allSessions.Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted && s.Status != TreatmentSessionStatus.Completed).ToList();
                foreach (var session in uncompletedSessions)
                {
                    session.Status = TreatmentSessionStatus.Cancelled;
                    session.UpdatedAt = DateTime.UtcNow;
                    _sessionRepo.Update(session);

                    if (session.AppointmentId.HasValue)
                    {
                        var appt = await _appointmentRepo.GetByIdAsync(session.AppointmentId.Value, ct);
                        if (appt != null && appt.Status != AppointmentStatus.Completed && appt.Status != AppointmentStatus.Cancelled)
                        {
                            appt.Status = AppointmentStatus.Cancelled;
                            appt.CancelledAt = DateTime.UtcNow;
                            appt.CancellationReason = $"Treatment case closed ({newStatus}).";
                            appt.UpdatedAt = DateTime.UtcNow;
                            _appointmentRepo.Update(appt);

                            var slot = await _slotRepo.GetByIdAsync(appt.AppointmentSlotId, ct);
                            if (slot != null)
                            {
                                slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
                                if (slot.CurrentBookings < slot.MaxPatients)
                                    slot.Status = AppointmentSlotStatus.Available;
                                slot.UpdatedAt = DateTime.UtcNow;
                                _slotRepo.Update(slot);
                            }
                        }
                    }
                }
            }

            _caseRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return ApiResponse.SuccessResponse("Treatment case closed successfully.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Failed to close treatment case {CaseId}", caseId);
            return ApiResponse.ErrorResponse("Failed to close treatment case due to database transaction error.");
        }
    }

    // ==================== Schedule Generation ====================

    public async Task<ApiResponse<List<TreatmentSessionDto>>> GenerateScheduleAsync(GenerateScheduleDto dto, Guid doctorUserId, CancellationToken ct)
    {
        // ── Input validation ────────────────────────────────────────────
        if (dto.DaysOfWeek == null || !dto.DaysOfWeek.Any())
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("At least one day of the week must be selected.");

        if (!TimeOnly.TryParse(dto.StartTime, out var startTime))
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Invalid start time format. Use HH:mm (e.g. 09:00).");

        if (startTime < new TimeOnly(7, 0) || startTime > new TimeOnly(23, 0))
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Start time must be between 07:00 and 23:00.");

        var durationMinutes = dto.DurationMinutes > 0 ? dto.DurationMinutes : 60;
        if (durationMinutes < 15 || durationMinutes > 240)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Duration must be between 15 and 240 minutes.");

        var endTime = startTime.AddMinutes(durationMinutes);
        if (endTime <= startTime)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Session end time must be after start time and cannot cross midnight.");

        var startDate = dto.StartDate?.Date ?? DateTime.Today.AddDays(1);
        if (startDate.Date < DateTime.Today)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Start date cannot be in the past.");

        // ── Load treatment case ─────────────────────────────────────────
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Treatment case not found.");

        if (treatmentCase.Status != TreatmentCaseStatus.Active)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Can only generate schedule for active treatment cases.");

        // ── Ownership check ─────────────────────────────────────────────
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);
        if (doctorProfile == null)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Doctor profile not found.");

        if (treatmentCase.DoctorId != doctorProfile.Id && treatmentCase.DoctorId != doctorUserId)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("You do not have permission to generate schedule for this treatment case.");

        // ── Resolve patient ─────────────────────────────────────────────
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patientProfile = allPatients.FirstOrDefault(p => p.UserId == treatmentCase.PatientId || p.Id == treatmentCase.PatientId);
        if (patientProfile == null)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Patient profile not found for this treatment case.");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var patientUser = allUsers.FirstOrDefault(u => u.Id == patientProfile.UserId);
        var patientName = patientUser?.FullName ?? "Patient";

        // ── Load existing sessions ──────────────────────────────────────
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var existingSessions = allSessions
            .Where(s => s.TreatmentCaseId == dto.TreatmentCaseId && !s.IsDeleted)
            .ToList();

        // ── Clear future sessions if requested ──────────────────────────
        if (dto.ClearExistingFutureSessions)
        {
            var futureUncompleted = existingSessions.Where(s =>
                s.Status == TreatmentSessionStatus.Scheduled ||
                s.Status == TreatmentSessionStatus.Planned).ToList();

            foreach (var s in futureUncompleted)
            {
                s.Status = TreatmentSessionStatus.Cancelled;
                s.UpdatedAt = DateTime.UtcNow;
                _sessionRepo.Update(s);

                if (s.AppointmentId.HasValue)
                {
                    var linkedAppt = await _appointmentRepo.GetByIdAsync(s.AppointmentId.Value, ct);
                    if (linkedAppt != null && linkedAppt.Status != AppointmentStatus.Completed)
                    {
                        linkedAppt.Status = AppointmentStatus.Cancelled;
                        linkedAppt.CancelledAt = DateTime.UtcNow;
                        linkedAppt.CancellationReason = "Schedule regenerated by doctor.";
                        linkedAppt.UpdatedAt = DateTime.UtcNow;
                        _appointmentRepo.Update(linkedAppt);

                        await _appointmentHistoryRepo.AddAsync(new AppointmentHistory
                        {
                            AppointmentId = linkedAppt.Id,
                            PreviousStatus = AppointmentStatus.Approved,
                            NewStatus = AppointmentStatus.Cancelled,
                            Reason = "Schedule regenerated by doctor.",
                            Appointment = linkedAppt
                        }, ct);
                    }
                }
            }

            await _uow.SaveChangesAsync(ct);

            existingSessions = existingSessions
                .Where(s => s.Status != TreatmentSessionStatus.Cancelled)
                .ToList();
        }

        // ── Calculate sessions needed ───────────────────────────────────
        var completedCount = existingSessions.Count(s => s.Status == TreatmentSessionStatus.Completed);
        var activeCount = existingSessions.Count(s =>
            s.Status == TreatmentSessionStatus.Scheduled ||
            s.Status == TreatmentSessionStatus.InProgress);
        var plannedSessions = existingSessions
            .Where(s => s.Status == TreatmentSessionStatus.Planned)
            .OrderBy(s => s.SessionNumber)
            .ToList();

        var totalNeeded = treatmentCase.TotalSessions;
        var sessionsToSchedule = Math.Max(0, totalNeeded - completedCount - activeCount);

        if (sessionsToSchedule <= 0)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse(
                $"All {totalNeeded} sessions are already completed or scheduled. No new sessions needed.");

        // ── Load slots and appointments ─────────────────────────────────
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var allAppointments = await _appointmentRepo.GetAllAsync(ct);

        var maxSessionNumber = existingSessions.Any()
            ? existingSessions.Max(s => s.SessionNumber)
            : 0;

        var currentDate = startDate;
        var maxDate = dto.TotalWeeks.HasValue && dto.TotalWeeks.Value > 0
            ? startDate.AddDays(dto.TotalWeeks.Value * 7)
            : startDate.AddYears(1);

        var createdSessions = new List<TreatmentSession>();
        var plannedQueue = new Queue<TreatmentSession>(plannedSessions);
        var skippedDates = new List<string>();

        // ── Begin transaction for staged persistence ────────────────────
        await _uow.BeginTransactionAsync(ct);
        try
        {
            while (createdSessions.Count < sessionsToSchedule && currentDate < maxDate)
            {
                if (!dto.DaysOfWeek.Contains(currentDate.DayOfWeek))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var slotDate = DateOnly.FromDateTime(currentDate);
                var slotStartDateTime = currentDate.Date.Add(startTime.ToTimeSpan());

                // Check for overlapping booked slots on this date
                var overlappingBookedSlots = allSlots.Where(s =>
                    s.DoctorProfileId == doctorProfile.Id &&
                    s.SlotDate == slotDate &&
                    !s.IsDeleted &&
                    s.Status == AppointmentSlotStatus.Booked &&
                    s.StartTime < endTime && s.EndTime > startTime).ToList();

                if (overlappingBookedSlots.Any())
                {
                    skippedDates.Add(currentDate.ToString("MMM dd (ddd)") + " — slot conflict");
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Check for overlapping active appointments
                var overlappingAppointments = allAppointments.Where(a =>
                    a.DoctorId == doctorProfile.Id &&
                    a.AppointmentDate.HasValue &&
                    a.AppointmentDate.Value.Date == currentDate.Date &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Rejected).ToList();

                var hasActiveOverlap = overlappingAppointments.Any(a =>
                {
                    var apptSlot = allSlots.FirstOrDefault(s => s.Id == a.AppointmentSlotId);
                    return apptSlot != null &&
                           apptSlot.StartTime < endTime && apptSlot.EndTime > startTime;
                });

                if (hasActiveOverlap)
                {
                    skippedDates.Add(currentDate.ToString("MMM dd (ddd)") + " — appointment conflict");
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Check unique index: DoctorProfileId + SlotDate + StartTime
                var duplicateSlot = allSlots.Any(s =>
                    s.DoctorProfileId == doctorProfile.Id &&
                    s.SlotDate == slotDate &&
                    s.StartTime == startTime &&
                    !s.IsDeleted);

                if (duplicateSlot)
                {
                    skippedDates.Add(currentDate.ToString("MMM dd (ddd)") + " — slot already exists at this time");
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // ── STAGE 1: Create AppointmentSlot and save ────────────
                var slotNotes = $"Treatment session for {patientName}: {treatmentCase.CaseName}";
                var slot = new AppointmentSlot
                {
                    DoctorProfileId = doctorProfile.Id,
                    SlotDate = slotDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = AppointmentSlotStatus.Booked,
                    CurrentBookings = 1,
                    MaxPatients = 1,
                    Notes = slotNotes,
                    DoctorProfile = doctorProfile
                };
                await _slotRepo.AddAsync(slot, ct);
                await _uow.SaveChangesAsync(ct);

                // ── STAGE 2: Create/reuse TreatmentSession (Planned, no AppointmentId) and save ──
                TreatmentSession session;
                if (plannedQueue.Count > 0)
                {
                    session = plannedQueue.Dequeue();
                    session.PlannedStartTime = slotStartDateTime;
                    session.PlannedEndTime = currentDate.Date.Add(endTime.ToTimeSpan());
                    session.UpdatedAt = DateTime.UtcNow;
                    _sessionRepo.Update(session);
                }
                else
                {
                    maxSessionNumber++;
                    session = new TreatmentSession
                    {
                        TreatmentCaseId = treatmentCase.Id,
                        AppointmentId = null,
                        SessionNumber = maxSessionNumber,
                        Title = $"Session {maxSessionNumber}: {treatmentCase.CaseName}",
                        Description = $"Planned session {maxSessionNumber} of {treatmentCase.TotalSessions}",
                        PlannedStartTime = slotStartDateTime,
                        PlannedEndTime = currentDate.Date.Add(endTime.ToTimeSpan()),
                        Status = TreatmentSessionStatus.Planned,
                        TreatmentCase = treatmentCase
                    };
                    await _sessionRepo.AddAsync(session, ct);
                }
                await _uow.SaveChangesAsync(ct);

                // ── STAGE 3: Create Appointment with TreatmentSessionId and save ──
                var bookingCode = $"TC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                var appointment = new Appointment
                {
                    BookingCode = bookingCode,
                    AppointmentSlotId = slot.Id,
                    DoctorId = doctorProfile.Id,
                    PatientId = patientProfile.Id,
                    Status = AppointmentStatus.Approved,
                    ApprovedAt = DateTime.UtcNow,
                    TreatmentPackageId = treatmentCase.TreatmentPackageId,
                    TreatmentCaseId = treatmentCase.Id,
                    TreatmentSessionId = session.Id,
                    AppointmentDate = slotStartDateTime,
                    Notes = slotNotes,
                    AppointmentSlot = slot,
                    Doctor = doctorProfile,
                    Patient = patientProfile
                };
                await _appointmentRepo.AddAsync(appointment, ct);
                await _uow.SaveChangesAsync(ct);

                // ── STAGE 4: Link session back to appointment, set Scheduled ──
                session.AppointmentId = appointment.Id;
                session.Status = TreatmentSessionStatus.Scheduled;
                session.UpdatedAt = DateTime.UtcNow;
                _sessionRepo.Update(session);
                await _uow.SaveChangesAsync(ct);

                // Add appointment history
                await _appointmentHistoryRepo.AddAsync(new AppointmentHistory
                {
                    AppointmentId = appointment.Id,
                    PreviousStatus = null,
                    NewStatus = AppointmentStatus.Approved,
                    Reason = $"Auto-generated for treatment session {session.SessionNumber}.",
                    Appointment = appointment
                }, ct);

                createdSessions.Add(session);

                // Refresh in-memory slot/appointment collections
                allSlots = await _slotRepo.GetAllAsync(ct);
                allAppointments = await _appointmentRepo.GetAllAsync(ct);

                currentDate = currentDate.AddDays(1);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex,
                "GenerateScheduleAsync failed. CaseId={CaseId}, DoctorUserId={DoctorUserId}, " +
                "StartDate={StartDate}, StartTime={StartTime}, EndTime={EndTime}, " +
                "SessionsCreatedBeforeFailure={Count}",
                dto.TreatmentCaseId, doctorUserId, startDate, dto.StartTime, endTime,
                createdSessions.Count);
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse(
                "Failed to generate schedule due to a database conflict. Please try again or contact support.");
        }

        if (!createdSessions.Any())
        {
            var skipSummary = skippedDates.Any()
                ? " Skipped dates: " + string.Join(", ", skippedDates.Take(5))
                : "";
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse(
                $"No available doctor slots were found for the selected recurrence.{skipSummary} Update Schedule Management or select different days and times.");
        }

        var dtos = createdSessions.Select(MapToSessionDto).ToList();
        var skipInfo = skippedDates.Any()
            ? $" ({skippedDates.Count} date(s) skipped due to conflicts)"
            : "";
        return ApiResponse<List<TreatmentSessionDto>>.SuccessResponse(dtos,
            $"Generated {dtos.Count} treatment sessions and appointments.{skipInfo}");
    }



    // ==================== Sessions ====================

    public async Task<ApiResponse<TreatmentSessionDto>> CreateSessionAsync(CreateSessionDto dto, CancellationToken ct)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Treatment case not found.");

        if (treatmentCase.Status != TreatmentCaseStatus.Active)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Cannot add sessions to a non-active case.");

        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var existingSessions = allSessions.Where(s => s.TreatmentCaseId == dto.TreatmentCaseId && !s.IsDeleted).ToList();
        var sessionNumber = existingSessions.Any() ? existingSessions.Max(s => s.SessionNumber) + 1 : 1;

        if (existingSessions.Any(s => s.SessionNumber == sessionNumber))
            return ApiResponse<TreatmentSessionDto>.ErrorResponse($"Session number {sessionNumber} already exists for this treatment case.");

        var session = new TreatmentSession
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            AppointmentId = dto.AppointmentId,
            SessionNumber = sessionNumber,
            Title = dto.Title ?? $"Session {sessionNumber}",
            Description = dto.Description,
            PlannedStartTime = dto.PlannedStartTime,
            PlannedEndTime = dto.PlannedEndTime,
            Status = dto.AppointmentId.HasValue ? TreatmentSessionStatus.Scheduled : TreatmentSessionStatus.Planned,
            TreatmentCase = treatmentCase
        };

        await _sessionRepo.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), "Session created successfully.");
    }

    public async Task<ApiResponse<TreatmentSessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionDto dto, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Session not found.");

        if (dto.Title != null) session.Title = dto.Title;
        if (dto.Description != null) session.Description = dto.Description;
        if (dto.PlannedStartTime.HasValue) session.PlannedStartTime = dto.PlannedStartTime;
        if (dto.PlannedEndTime.HasValue) session.PlannedEndTime = dto.PlannedEndTime;

        session.UpdatedAt = DateTime.UtcNow;
        _sessionRepo.Update(session);

        // Update session-goal links
        if (dto.LinkedGoalIds != null)
        {
            var allLinks = await _sessionGoalRepo.GetAllAsync(ct);
            var existingLinks = allLinks.Where(sg => sg.TreatmentSessionId == sessionId).ToList();
            foreach (var link in existingLinks)
                _sessionGoalRepo.Delete(link);

            foreach (var goalId in dto.LinkedGoalIds)
            {
                var goal = await _goalRepo.GetByIdAsync(goalId, ct);
                if (goal != null)
                {
                    await _sessionGoalRepo.AddAsync(new TreatmentSessionGoal
                    {
                        TreatmentSessionId = sessionId,
                        TreatmentGoalId = goalId,
                        TreatmentSession = session,
                        TreatmentGoal = goal
                    }, ct);
                }
            }
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), "Session updated successfully.");
    }

    public async Task<ApiResponse> DeleteSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.IsDeleted)
            return ApiResponse.ErrorResponse("Session not found.");

        if (session.Status == TreatmentSessionStatus.Completed)
            return ApiResponse.ErrorResponse("Cannot delete a completed session.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (session.AppointmentId.HasValue)
            {
                var appt = await _appointmentRepo.GetByIdAsync(session.AppointmentId.Value, ct);
                if (appt != null && appt.Status != AppointmentStatus.Completed && appt.Status != AppointmentStatus.Cancelled)
                {
                    appt.Status = AppointmentStatus.Cancelled;
                    appt.CancelledAt = DateTime.UtcNow;
                    appt.CancellationReason = "Session deleted by doctor.";
                    appt.UpdatedAt = DateTime.UtcNow;
                    _appointmentRepo.Update(appt);

                    var slot = await _slotRepo.GetByIdAsync(appt.AppointmentSlotId, ct);
                    if (slot != null)
                    {
                        slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - 1);
                        if (slot.CurrentBookings < slot.MaxPatients)
                            slot.Status = AppointmentSlotStatus.Available;
                        slot.UpdatedAt = DateTime.UtcNow;
                        _slotRepo.Update(slot);
                    }
                }
            }

            session.IsDeleted = true;
            session.Status = TreatmentSessionStatus.Cancelled;
            session.UpdatedAt = DateTime.UtcNow;
            _sessionRepo.Update(session);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return ApiResponse.SuccessResponse("Session deleted successfully.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
            return ApiResponse.ErrorResponse("Failed to delete session due to database transaction error.");
        }
    }

    public async Task<ApiResponse> ReorderSessionsAsync(ReorderSessionsDto dto, CancellationToken ct)
    {
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var sessions = allSessions.Where(s => s.TreatmentCaseId == dto.TreatmentCaseId && !s.IsDeleted).ToList();

        for (int i = 0; i < dto.SessionIdsInOrder.Count; i++)
        {
            var id = dto.SessionIdsInOrder[i];
            var s = sessions.FirstOrDefault(x => x.Id == id);
            if (s != null)
            {
                s.SessionNumber = i + 1;
                s.UpdatedAt = DateTime.UtcNow;
                _sessionRepo.Update(s);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Sessions reordered successfully.");
    }

    public async Task<ApiResponse<TreatmentSessionDto>> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Session not found.");

        if (session.Status == TreatmentSessionStatus.Planned || session.Status == TreatmentSessionStatus.Cancelled)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Cannot complete a planned or cancelled session.");

        if (!session.AppointmentId.HasValue)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Cannot complete a session without a scheduled appointment.");

        var linkedAppt = await _appointmentRepo.GetByIdAsync(session.AppointmentId.Value, ct);
        if (linkedAppt == null || linkedAppt.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Linked appointment not found.");

        if (session.PlannedStartTime.HasValue && session.PlannedStartTime.Value > DateTime.UtcNow.AddMinutes(5))
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Cannot complete a future session before its scheduled time.");

        // Idempotency guard: track whether this is a first-time completion
        var wasAlreadyCompleted = session.Status == TreatmentSessionStatus.Completed;

        if (dto.Title != null) session.Title = dto.Title;
        session.SessionSummary = dto.SessionSummary;
        session.DoctorClinicalAssessment = dto.DoctorClinicalAssessment ?? dto.TherapistNotes;
        session.PatientFriendlySummary = dto.PatientFriendlySummary ?? dto.SessionSummary;
        session.DoctorPrivateNotes = dto.DoctorPrivateNotes ?? dto.TherapistNotes;
        session.TherapistNotes = dto.DoctorPrivateNotes ?? dto.TherapistNotes;
        session.PatientFeedback = dto.PatientFeedback;
        session.HomeworkAssigned = dto.HomeworkAssigned;
        session.MoodBefore = dto.MoodBefore;
        session.MoodAfter = dto.MoodAfter;
        session.Status = TreatmentSessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;

        _sessionRepo.Update(session);

        // Update linked Appointment status to Completed
        if (session.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(session.AppointmentId.Value, ct);
            if (appointment != null && appointment.Status != AppointmentStatus.Completed)
            {
                appointment.Status = AppointmentStatus.Completed;
                appointment.CompletedAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;
                _appointmentRepo.Update(appointment);
            }
        }

        // Link goals if provided
        if (dto.LinkedGoalIds != null && dto.LinkedGoalIds.Any())
        {
            var allLinks = await _sessionGoalRepo.GetAllAsync(ct);
            var existingLinks = allLinks.Where(sg => sg.TreatmentSessionId == sessionId).ToList();
            foreach (var link in existingLinks)
                _sessionGoalRepo.Delete(link);

            foreach (var goalId in dto.LinkedGoalIds)
            {
                var goal = await _goalRepo.GetByIdAsync(goalId, ct);
                if (goal != null)
                {
                    await _sessionGoalRepo.AddAsync(new TreatmentSessionGoal
                    {
                        TreatmentSessionId = sessionId,
                        TreatmentGoalId = goalId,
                        TreatmentSession = session,
                        TreatmentGoal = goal
                    }, ct);
                }
            }
        }

        // Update parent case counters using count-based recalculation (idempotent)
        var treatmentCase = await _caseRepo.GetByIdAsync(session.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            // Count-based recalculation prevents double-counting on repeated calls
            var allSessions = await _sessionRepo.GetAllAsync(ct);
            var caseSessions = allSessions.Where(s => s.TreatmentCaseId == treatmentCase.Id && !s.IsDeleted).ToList();
            treatmentCase.CompletedSessions = caseSessions.Count(s => s.Status == TreatmentSessionStatus.Completed);
            treatmentCase.RemainingSessions = Math.Max(0, treatmentCase.TotalSessions - treatmentCase.CompletedSessions);

            await RecalculateProgressAsync(treatmentCase, ct);
            treatmentCase.UpdatedAt = DateTime.UtcNow;
            _caseRepo.Update(treatmentCase);

            if (treatmentCase.RemainingSessions <= 0)
            {
                treatmentCase.Status = TreatmentCaseStatus.Completed;
                treatmentCase.ActualEndDate = DateTime.UtcNow;
                treatmentCase.OverallProgressPercent = 100;
            }
        }

        await _uow.SaveChangesAsync(ct);
        var message = wasAlreadyCompleted ? "Session updated (already completed)." : "Session completed successfully.";
        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), message);
    }

    public async Task<ApiResponse<List<TreatmentSessionDto>>> GetSessionsByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(tc, requestingUserId, ct))
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse("Access denied. You do not have permission to view sessions for this treatment case.");

        var all = await _sessionRepo.GetAllAsync(ct);
        var sessions = all
            .Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted)
            .OrderBy(s => s.SessionNumber)
            .ToList();

        var dtos = new List<TreatmentSessionDto>();
        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var allSessionGoals = await _sessionGoalRepo.GetAllAsync(ct);
        var allGoals = await _goalRepo.GetAllAsync(ct);

        foreach (var s in sessions)
        {
            var dto = MapToSessionDto(s);
            var sessionHomework = allAssignments.Where(a => a.TreatmentSessionId == s.Id && !a.IsDeleted).ToList();
            dto.HomeworkList = sessionHomework.Select(MapToHomeworkDto).ToList();

            var linkedGoalIds = allSessionGoals.Where(sg => sg.TreatmentSessionId == s.Id).Select(sg => sg.TreatmentGoalId).ToHashSet();
            var linkedGoals = allGoals.Where(g => linkedGoalIds.Contains(g.Id) && !g.IsDeleted).ToList();
            dto.LinkedGoals = linkedGoals.Select(MapToGoalDto).ToList();

            dtos.Add(dto);
        }

        return ApiResponse<List<TreatmentSessionDto>>.SuccessResponse(dtos);
    }

    // ==================== Goals ====================

    public async Task<ApiResponse<TreatmentGoalDto>> CreateGoalAsync(CreateGoalDto dto, CancellationToken ct)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Treatment case not found.");

        var goal = new TreatmentGoal
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            CreatedByDoctorId = treatmentCase.DoctorId,
            Title = dto.Title,
            Description = dto.Description,
            Category = (GoalCategory)dto.Category,
            Priority = (GoalPriority)dto.Priority,
            TargetValue = dto.TargetValue,
            CurrentValue = dto.CurrentValue,
            Unit = dto.Unit,
            TargetDate = dto.TargetDate,
            Status = GoalStatus.NotStarted,
            TreatmentCase = treatmentCase
        };

        await _goalRepo.AddAsync(goal, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentGoalDto>.SuccessResponse(MapToGoalDto(goal), "Treatment goal created successfully.");
    }

    public async Task<ApiResponse<TreatmentGoalDto>> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, CancellationToken ct)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal not found.");

        if (dto.Title != null) goal.Title = dto.Title;
        if (dto.Description != null) goal.Description = dto.Description;
        if (dto.Category.HasValue) goal.Category = (GoalCategory)dto.Category.Value;
        if (dto.Priority.HasValue) goal.Priority = (GoalPriority)dto.Priority.Value;
        if (dto.CurrentValue.HasValue) goal.CurrentValue = dto.CurrentValue.Value;
        if (dto.TargetValue.HasValue) goal.TargetValue = dto.TargetValue.Value;
        if (dto.Unit != null) goal.Unit = dto.Unit;
        if (dto.DoctorNotes != null) goal.DoctorNotes = dto.DoctorNotes;

        if (dto.Status.HasValue)
        {
            goal.Status = (GoalStatus)dto.Status.Value;
            if (goal.Status == GoalStatus.Achieved)
            {
                goal.AchievedDate = DateTime.UtcNow;
                goal.ProgressPercent = 100;
            }
        }
        if (dto.ProgressPercent.HasValue) goal.ProgressPercent = dto.ProgressPercent.Value;

        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            await RecalculateProgressAsync(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentGoalDto>.SuccessResponse(MapToGoalDto(goal), "Goal updated successfully.");
    }

    public async Task<ApiResponse<List<TreatmentGoalDto>>> GetGoalsByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<TreatmentGoalDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(tc, requestingUserId, ct))
            return ApiResponse<List<TreatmentGoalDto>>.ErrorResponse("Access denied. You do not have permission to view goals for this treatment case.");

        var all = await _goalRepo.GetAllAsync(ct);
        var goals = all
            .Where(g => g.TreatmentCaseId == caseId && !g.IsDeleted)
            .OrderByDescending(g => g.Priority)
            .ThenBy(g => g.CreatedAt)
            .ToList();

        var allProgress = await _goalProgressRepo.GetAllAsync(ct);
        var dtos = new List<TreatmentGoalDto>();

        foreach (var g in goals)
        {
            var dto = MapToGoalDto(g);
            var history = allProgress.Where(p => p.GoalId == g.Id && !p.IsDeleted).OrderByDescending(p => p.RecordedAt).ToList();
            dto.ProgressHistory = history.Select(MapToGoalProgressDto).ToList();
            dtos.Add(dto);
        }

        return ApiResponse<List<TreatmentGoalDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<TreatmentGoalProgressDto>> RecordGoalProgressAsync(CreateGoalProgressDto dto, CancellationToken ct)
    {
        var goal = await _goalRepo.GetByIdAsync(dto.GoalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Goal not found.");

        goal.ProgressPercent = dto.ProgressPercent;
        if (dto.CurrentValue.HasValue) goal.CurrentValue = dto.CurrentValue.Value;
        if (dto.ProgressPercent >= 100)
        {
            goal.Status = GoalStatus.Achieved;
            goal.AchievedDate = DateTime.UtcNow;
        }
        else if (goal.Status == GoalStatus.NotStarted && dto.ProgressPercent > 0)
        {
            goal.Status = GoalStatus.InProgress;
        }

        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        var progressRecord = new TreatmentGoalProgress
        {
            GoalId = dto.GoalId,
            TreatmentSessionId = dto.TreatmentSessionId,
            ProgressPercent = dto.ProgressPercent,
            CurrentValue = dto.CurrentValue,
            DoctorComment = dto.DoctorComment,
            RecordedAt = DateTime.UtcNow,
            Goal = goal
        };

        await _goalProgressRepo.AddAsync(progressRecord, ct);

        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            await RecalculateProgressAsync(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentGoalProgressDto>.SuccessResponse(MapToGoalProgressDto(progressRecord), "Goal progress recorded successfully.");
    }

    public async Task<ApiResponse<List<TreatmentGoalProgressDto>>> GetGoalProgressHistoryAsync(Guid goalId, CancellationToken ct)
    {
        var all = await _goalProgressRepo.GetAllAsync(ct);
        var history = all
            .Where(p => p.GoalId == goalId && !p.IsDeleted)
            .OrderByDescending(p => p.RecordedAt)
            .Select(MapToGoalProgressDto)
            .ToList();

        return ApiResponse<List<TreatmentGoalProgressDto>>.SuccessResponse(history);
    }

    // ==================== Homework / Therapy Assignments ====================

    public async Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto, CancellationToken ct)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<HomeworkDto>.ErrorResponse("Treatment case not found.");

        if (!dto.TreatmentSessionId.HasValue)
            return ApiResponse<HomeworkDto>.ErrorResponse("Homework must be linked to a treatment session.");

        var session = await _sessionRepo.GetByIdAsync(dto.TreatmentSessionId.Value, ct);
        if (session == null || session.IsDeleted || session.TreatmentCaseId != dto.TreatmentCaseId)
            return ApiResponse<HomeworkDto>.ErrorResponse("Select a valid session from this treatment case.");

        if (string.IsNullOrWhiteSpace(dto.DetailedInstructions))
            return ApiResponse<HomeworkDto>.ErrorResponse("Homework instructions and completion conditions are required.");

        if (!dto.DueDate.HasValue)
            return ApiResponse<HomeworkDto>.ErrorResponse("A due date is required for homework.");

        var assignment = new TherapyAssignment
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            TreatmentSessionId = dto.TreatmentSessionId,
            Title = dto.Title,
            Description = dto.Description,
            DetailedInstructions = dto.DetailedInstructions,
            ResourceUrl = dto.ResourceUrl,
            DueDate = dto.DueDate,
            Status = HomeworkStatus.Assigned
        };

        await _assignmentRepo.AddAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<HomeworkDto>.SuccessResponse(MapToHomeworkDto(assignment), "Homework assigned successfully.");
    }

    public async Task<ApiResponse<HomeworkDto>> SubmitHomeworkAsync(Guid homeworkId, SubmitHomeworkDto dto, CancellationToken ct)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(homeworkId, ct);
        if (assignment == null || assignment.IsDeleted)
            return ApiResponse<HomeworkDto>.ErrorResponse("Homework assignment not found.");

        if (string.IsNullOrWhiteSpace(dto.PatientSubmission) && string.IsNullOrWhiteSpace(dto.PatientSubmissionUrl))
            return ApiResponse<HomeworkDto>.ErrorResponse("Provide a written reflection or an evidence link before submitting homework.");

        assignment.PatientSubmission = dto.PatientSubmission;
        assignment.PatientSubmissionUrl = dto.PatientSubmissionUrl;
        assignment.SubmittedAt = DateTime.UtcNow;
        assignment.Status = HomeworkStatus.Submitted;
        assignment.UpdatedAt = DateTime.UtcNow;

        _assignmentRepo.Update(assignment);

        if (assignment.TreatmentCaseId.HasValue)
        {
            var treatmentCase = await _caseRepo.GetByIdAsync(assignment.TreatmentCaseId.Value, ct);
            if (treatmentCase != null)
            {
                await RecalculateProgressAsync(treatmentCase, ct);
                _caseRepo.Update(treatmentCase);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<HomeworkDto>.SuccessResponse(MapToHomeworkDto(assignment), "Homework submitted successfully.");
    }

    public async Task<ApiResponse<HomeworkDto>> ReviewHomeworkAsync(Guid homeworkId, ReviewHomeworkDto dto, CancellationToken ct)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(homeworkId, ct);
        if (assignment == null || assignment.IsDeleted)
            return ApiResponse<HomeworkDto>.ErrorResponse("Homework assignment not found.");

        assignment.DoctorFeedback = dto.DoctorFeedback;
        assignment.FeedbackAt = DateTime.UtcNow;
        assignment.Status = HomeworkStatus.Reviewed;
        assignment.UpdatedAt = DateTime.UtcNow;

        _assignmentRepo.Update(assignment);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<HomeworkDto>.SuccessResponse(MapToHomeworkDto(assignment), "Homework reviewed successfully.");
    }

    public async Task<ApiResponse<List<HomeworkDto>>> GetHomeworkByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<HomeworkDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(tc, requestingUserId, ct))
            return ApiResponse<List<HomeworkDto>>.ErrorResponse("Access denied. You do not have permission to view homework for this treatment case.");

        var all = await _assignmentRepo.GetAllAsync(ct);
        var homeworkList = all
            .Where(a => a.TreatmentCaseId == caseId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(MapToHomeworkDto)
            .ToList();

        return ApiResponse<List<HomeworkDto>>.SuccessResponse(homeworkList);
    }

    // ==================== Mood Tracking ====================

    public async Task<ApiResponse<MoodEntryDto>> AddMoodEntryAsync(Guid patientUserId, CreateMoodEntryDto dto, CancellationToken ct)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<MoodEntryDto>.ErrorResponse("Treatment case not found.");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<MoodEntryDto>.ErrorResponse("Patient profile not found.");

        var moodEntry = new MoodEntry
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            PatientId = patient.Id,
            MoodScore = dto.MoodScore,
            AnxietyScore = dto.AnxietyScore,
            StressScore = dto.StressScore,
            SleepQualityScore = dto.SleepQualityScore,
            DepressionScore = dto.DepressionScore,
            RelationshipScore = dto.RelationshipScore,
            Note = dto.Note,
            RecordedAt = DateTime.UtcNow,
            TreatmentCase = treatmentCase,
            Patient = patient
        };

        await _moodRepo.AddAsync(moodEntry, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<MoodEntryDto>.SuccessResponse(MapToMoodEntryDto(moodEntry), "Mood entry recorded successfully.");
    }

    public async Task<ApiResponse<List<MoodEntryDto>>> GetMoodEntriesAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<MoodEntryDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(tc, requestingUserId, ct))
            return ApiResponse<List<MoodEntryDto>>.ErrorResponse("Access denied. You do not have permission to view mood entries for this treatment case.");

        var all = await _moodRepo.GetAllAsync(ct);
        var entries = all
            .Where(m => m.TreatmentCaseId == caseId && !m.IsDeleted)
            .OrderByDescending(m => m.RecordedAt)
            .Select(MapToMoodEntryDto)
            .ToList();

        return ApiResponse<List<MoodEntryDto>>.SuccessResponse(entries);
    }

    // ==================== Progress & Timeline ====================

    public async Task<ApiResponse> RefreshProgressAsync(Guid caseId, CancellationToken ct = default)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(caseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse.ErrorResponse("Treatment case not found.");

        var sessions = (await _sessionRepo.GetAllAsync(ct))
            .Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted)
            .ToList();

        treatmentCase.CompletedSessions = sessions.Count(s => s.Status == TreatmentSessionStatus.Completed);
        treatmentCase.RemainingSessions = Math.Max(0, treatmentCase.TotalSessions - treatmentCase.CompletedSessions);
        await RecalculateProgressAsync(treatmentCase, ct);

        if (treatmentCase.TotalSessions > 0 && treatmentCase.CompletedSessions >= treatmentCase.TotalSessions)
        {
            treatmentCase.Status = TreatmentCaseStatus.Completed;
            treatmentCase.ActualEndDate ??= DateTime.UtcNow;
            treatmentCase.OverallProgressPercent = 100;
        }

        _caseRepo.Update(treatmentCase);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Treatment case progress refreshed.");
    }

    public async Task<ApiResponse<TreatmentProgressDto>> GetProgressAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TreatmentProgressDto>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(entity, requestingUserId, ct))
            return ApiResponse<TreatmentProgressDto>.ErrorResponse("Access denied. You do not have permission to view progress for this treatment case.");

        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var sessions = allSessions.Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted).ToList();
        var completedSessions = sessions.Count(s => s.Status == TreatmentSessionStatus.Completed);

        var allGoals = await _goalRepo.GetAllAsync(ct);
        var goals = allGoals.Where(g => g.TreatmentCaseId == caseId && !g.IsDeleted).ToList();
        var achievedGoals = goals.Count(g => g.Status == GoalStatus.Achieved);
        var avgGoalProgress = goals.Count > 0 ? goals.Average(g => g.ProgressPercent) : 0;

        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var assignments = allAssignments.Where(a => a.TreatmentCaseId == caseId && !a.IsDeleted).ToList();
        var completedAssignments = assignments.Count(a => a.Status == HomeworkStatus.Submitted || a.Status == HomeworkStatus.Reviewed);

        var sessionProgress = entity.TotalSessions > 0 ? (completedSessions * 100 / entity.TotalSessions) : 0;
        var goalProgress = (int)Math.Round(avgGoalProgress);
        var assignmentProgress = assignments.Count > 0 ? (completedAssignments * 100 / assignments.Count) : 0;
        var calculatedOverallProgress = goals.Count > 0 && assignments.Count > 0
            ? (int)Math.Round(sessionProgress * 0.50 + goalProgress * 0.35 + assignmentProgress * 0.15)
            : goals.Count > 0
                ? (int)Math.Round(sessionProgress * 0.60 + goalProgress * 0.40)
                : sessionProgress;

        var allMoods = await _moodRepo.GetAllAsync(ct);
        var moodEntries = allMoods.Where(m => m.TreatmentCaseId == caseId && !m.IsDeleted).OrderBy(m => m.RecordedAt).ToList();

        var moodTrend = moodEntries.TakeLast(10).Select(m => new MoodTrendItem
        {
            MoodScore = m.MoodScore,
            Date = m.RecordedAt
        }).ToList();

        var daysElapsed = (DateTime.UtcNow - entity.StartDate).Days;
        int? daysRemaining = entity.ExpectedEndDate.HasValue
            ? Math.Max(0, (entity.ExpectedEndDate.Value - DateTime.UtcNow).Days)
            : null;

        var progress = new TreatmentProgressDto
        {
            CaseId = entity.Id,
            CaseName = entity.CaseName,
            OverallProgressPercent = entity.Status == TreatmentCaseStatus.Completed
                ? 100
                : Math.Clamp(calculatedOverallProgress, 0, 100),
            TotalSessions = entity.TotalSessions,
            CompletedSessions = completedSessions,
            SessionProgressPercent = sessionProgress,
            TotalGoals = goals.Count,
            AchievedGoals = achievedGoals,
            GoalProgressPercent = goalProgress,
            AverageGoalProgressPercent = avgGoalProgress,
            TotalAssignments = assignments.Count,
            CompletedAssignments = completedAssignments,
            AssignmentProgressPercent = assignmentProgress,
            MoodTrend = moodTrend,
            Status = (int)entity.Status,
            StartDate = entity.StartDate,
            ExpectedEndDate = entity.ExpectedEndDate,
            DaysElapsed = daysElapsed,
            DaysRemaining = daysRemaining
        };

        return ApiResponse<TreatmentProgressDto>.SuccessResponse(progress);
    }

    public async Task<ApiResponse<List<TreatmentTimelineDto>>> GetTimelineAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<TreatmentTimelineDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(tc, requestingUserId, ct))
            return ApiResponse<List<TreatmentTimelineDto>>.ErrorResponse("Access denied. You do not have permission to view timeline for this treatment case.");

        var timeline = new List<TreatmentTimelineDto>();

        // Sessions
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        foreach (var s in allSessions.Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = s.Id,
                EventDate = s.UpdatedAt ?? s.CreatedAt,
                EventType = "Session",
                Title = s.Title ?? $"Session #{s.SessionNumber}",
                Description = s.PatientFriendlySummary ?? s.SessionSummary ?? "Scheduled",
                Status = s.Status.ToString(),
                IconCss = "bi-camera-video"
            });
        }

        // Goals
        var allGoals = await _goalRepo.GetAllAsync(ct);
        foreach (var g in allGoals.Where(g => g.TreatmentCaseId == caseId && !g.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = g.Id,
                EventDate = g.AchievedDate ?? g.UpdatedAt ?? g.CreatedAt,
                EventType = "Goal",
                Title = g.Title,
                Description = $"Category: {g.Category} | Progress: {g.ProgressPercent}%",
                Status = g.Status.ToString(),
                IconCss = "bi-bullseye"
            });
        }

        // Homework / Assignments
        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        foreach (var a in allAssignments.Where(a => a.TreatmentCaseId == caseId && !a.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = a.Id,
                EventDate = a.SubmittedAt ?? a.CreatedAt,
                EventType = "Homework",
                Title = a.Title,
                Description = a.Description,
                Status = a.Status.ToString(),
                IconCss = "bi-journal-check"
            });
        }

        // Mood entries
        var allMoods = await _moodRepo.GetAllAsync(ct);
        foreach (var m in allMoods.Where(m => m.TreatmentCaseId == caseId && !m.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = m.Id,
                EventDate = m.RecordedAt,
                EventType = "Mood",
                Title = $"Mood Check-in: {m.MoodScore}/10",
                Description = m.Note ?? $"Stress: {m.StressScore ?? 0}/10 | Anxiety: {m.AnxietyScore ?? 0}/10",
                Status = m.MoodScore >= 7 ? "Positive" : m.MoodScore <= 4 ? "Negative" : "Neutral",
                IconCss = "bi-emoji-smile"
            });
        }

        // Sort by date descending
        var sorted = timeline.OrderByDescending(t => t.EventDate).ToList();
        return ApiResponse<List<TreatmentTimelineDto>>.SuccessResponse(sorted);
    }

    // ==================== Private Helpers ====================

    private async Task RecalculateProgressAsync(TreatmentCase treatmentCase, CancellationToken ct)
    {
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var sessions = allSessions.Where(s => s.TreatmentCaseId == treatmentCase.Id && !s.IsDeleted).ToList();
        var sessionPercent = treatmentCase.TotalSessions > 0
            ? (sessions.Count(s => s.Status == TreatmentSessionStatus.Completed) * 100 / treatmentCase.TotalSessions)
            : 0;

        var allGoals = await _goalRepo.GetAllAsync(ct);
        var goals = allGoals.Where(g => g.TreatmentCaseId == treatmentCase.Id && !g.IsDeleted).ToList();
        var goalPercent = goals.Count > 0 ? (int)Math.Round(goals.Average(g => g.ProgressPercent)) : 0;

        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var assignments = allAssignments.Where(a => a.TreatmentCaseId == treatmentCase.Id && !a.IsDeleted).ToList();
        var homeworkPercent = assignments.Count > 0
            ? (assignments.Count(a => a.Status == HomeworkStatus.Submitted || a.Status == HomeworkStatus.Reviewed) * 100 / assignments.Count)
            : 0;

        // Weighted progress formula: 50% Sessions + 35% Goals + 15% Homework
        int overall;
        if (goals.Count > 0 && assignments.Count > 0)
        {
            overall = (int)Math.Round(sessionPercent * 0.50 + goalPercent * 0.35 + homeworkPercent * 0.15);
        }
        else if (goals.Count > 0)
        {
            overall = (int)Math.Round(sessionPercent * 0.60 + goalPercent * 0.40);
        }
        else
        {
            overall = sessionPercent;
        }

        treatmentCase.OverallProgressPercent = Math.Min(100, Math.Max(0, overall));
        treatmentCase.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user != null)
            return user.FullName;

        // Older treatment cases may store a profile ID rather than the user ID.
        var doctor = (await _doctorRepo.GetAllAsync(ct)).FirstOrDefault(d => d.Id == userId);
        if (doctor != null)
            return (await _userRepo.GetByIdAsync(doctor.UserId, ct))?.FullName;

        var patient = (await _patientRepo.GetAllAsync(ct)).FirstOrDefault(p => p.Id == userId);
        return patient == null ? null : (await _userRepo.GetByIdAsync(patient.UserId, ct))?.FullName;
    }

    private async Task<string?> GetPackageNameAsync(Guid packageId, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(packageId, ct);
        return package?.Name;
    }

    private async Task<TreatmentCaseDto> MapToCaseDtoAsync(TreatmentCase entity, CancellationToken ct)
    {
        var allGoals = await _goalRepo.GetAllAsync(ct);
        var goals = allGoals.Where(g => g.TreatmentCaseId == entity.Id && !g.IsDeleted).ToList();

        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var assignments = allAssignments.Where(a => a.TreatmentCaseId == entity.Id && !a.IsDeleted).ToList();

        return new TreatmentCaseDto
        {
            Id = entity.Id,
            TreatmentPackageId = entity.TreatmentPackageId,
            DoctorId = entity.DoctorId,
            PatientId = entity.PatientId,
            CaseName = entity.CaseName,
            CaseDescription = entity.CaseDescription,
            PrimaryConcern = entity.PrimaryConcern,

            PackageNameSnapshot = entity.PackageNameSnapshot ?? entity.CaseName,
            PackageDescriptionSnapshot = entity.PackageDescriptionSnapshot ?? entity.CaseDescription,
            TotalSessionsSnapshot = entity.TotalSessionsSnapshot > 0 ? entity.TotalSessionsSnapshot : entity.TotalSessions,
            DurationDaysSnapshot = entity.DurationDaysSnapshot,
            RecommendedSessionsPerWeekSnapshot = entity.RecommendedSessionsPerWeekSnapshot,
            PriceSnapshot = entity.PriceSnapshot,
            TargetOutcomesSnapshot = entity.TargetOutcomesSnapshot,
            RecommendedExercisesSnapshot = entity.RecommendedExercisesSnapshot,
            PatientGuidanceSnapshot = entity.PatientGuidanceSnapshot,

            TotalSessions = entity.TotalSessions,
            CompletedSessions = entity.CompletedSessions,
            RemainingSessions = entity.RemainingSessions,
            StartDate = entity.StartDate,
            ExpectedEndDate = entity.ExpectedEndDate,
            ActualEndDate = entity.ActualEndDate,
            Status = (int)entity.Status,
            ClosureNote = entity.ClosureNote,
            OverallProgressPercent = entity.OverallProgressPercent,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DoctorName = await GetUserNameAsync(entity.DoctorId, ct),
            PatientName = await GetUserNameAsync(entity.PatientId, ct),
            PackageName = await GetPackageNameAsync(entity.TreatmentPackageId, ct),
            GoalCount = goals.Count,
            AchievedGoalCount = goals.Count(g => g.Status == GoalStatus.Achieved),
            TotalHomeworkAssigned = assignments.Count,
            HomeworkSubmittedCount = assignments.Count(a => a.Status == HomeworkStatus.Submitted),
            HomeworkReviewedCount = assignments.Count(a => a.Status == HomeworkStatus.Reviewed),
            HomeworkOverdueCount = assignments.Count(a => a.Status == HomeworkStatus.Assigned && a.DueDate.HasValue && a.DueDate.Value < DateTime.UtcNow),
            AssignmentCount = assignments.Count,
            CompletedAssignmentCount = assignments.Count(a => a.Status == HomeworkStatus.Submitted || a.Status == HomeworkStatus.Reviewed)
        };
    }

    private async Task<TreatmentCaseListDto> MapToListDtoAsync(TreatmentCase entity, CancellationToken ct)
    {
        // List cards must use the same live aggregation as the details page, rather than stale case counters.
        var progress = (await GetProgressAsync(entity.Id, null, ct)).Data;

        return new TreatmentCaseListDto
        {
            Id = entity.Id,
            CaseName = entity.CaseName,
            PackageNameSnapshot = entity.PackageNameSnapshot,
            PatientName = await GetUserNameAsync(entity.PatientId, ct),
            DoctorName = await GetUserNameAsync(entity.DoctorId, ct),
            TotalSessions = progress?.TotalSessions ?? entity.TotalSessions,
            CompletedSessions = progress?.CompletedSessions ?? entity.CompletedSessions,
            OverallProgressPercent = progress?.OverallProgressPercent ?? entity.OverallProgressPercent,
            Status = (int)entity.Status,
            StartDate = entity.StartDate,
            CreatedAt = entity.CreatedAt
        };
    }

    private static TreatmentSessionDto MapToSessionDto(TreatmentSession s)
    {
        return new TreatmentSessionDto
        {
            Id = s.Id,
            TreatmentCaseId = s.TreatmentCaseId,
            AppointmentId = s.AppointmentId,
            SessionNumber = s.SessionNumber,
            Title = s.Title ?? $"Session #{s.SessionNumber}",
            Description = s.Description,
            PlannedStartTime = s.PlannedStartTime,
            PlannedEndTime = s.PlannedEndTime,
            SessionSummary = s.SessionSummary,
            DoctorClinicalAssessment = s.DoctorClinicalAssessment ?? s.TherapistNotes,
            PatientFriendlySummary = s.PatientFriendlySummary ?? s.SessionSummary,
            DoctorPrivateNotes = s.DoctorPrivateNotes ?? s.TherapistNotes,
            TherapistNotes = s.DoctorPrivateNotes ?? s.TherapistNotes,
            PatientFeedback = s.PatientFeedback,
            HomeworkAssigned = s.HomeworkAssigned,
            MoodBefore = s.MoodBefore,
            MoodAfter = s.MoodAfter,
            Status = (int)s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            AppointmentDate = s.Appointment?.AppointmentDate ?? s.PlannedStartTime,
            BookingCode = s.Appointment?.BookingCode
        };
    }

    private static TreatmentGoalDto MapToGoalDto(TreatmentGoal g)
    {
        return new TreatmentGoalDto
        {
            Id = g.Id,
            TreatmentCaseId = g.TreatmentCaseId,
            CreatedByDoctorId = g.CreatedByDoctorId,
            Title = g.Title,
            Description = g.Description,
            Category = (int)g.Category,
            CategoryText = g.Category.ToString(),
            Priority = (int)g.Priority,
            Status = (int)g.Status,
            ProgressPercent = g.ProgressPercent,
            TargetValue = g.TargetValue,
            CurrentValue = g.CurrentValue,
            Unit = g.Unit,
            TargetDate = g.TargetDate,
            AchievedDate = g.AchievedDate,
            DoctorNotes = g.DoctorNotes,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        };
    }

    private static TreatmentGoalProgressDto MapToGoalProgressDto(TreatmentGoalProgress p)
    {
        return new TreatmentGoalProgressDto
        {
            Id = p.Id,
            GoalId = p.GoalId,
            TreatmentSessionId = p.TreatmentSessionId,
            ProgressPercent = p.ProgressPercent,
            CurrentValue = p.CurrentValue,
            DoctorComment = p.DoctorComment,
            RecordedAt = p.RecordedAt
        };
    }

    private static HomeworkDto MapToHomeworkDto(TherapyAssignment a)
    {
        return new HomeworkDto
        {
            Id = a.Id,
            TreatmentCaseId = a.TreatmentCaseId ?? Guid.Empty,
            TreatmentSessionId = a.TreatmentSessionId,
            SessionNumber = a.TreatmentSession?.SessionNumber,
            Title = a.Title,
            Description = a.Description,
            DetailedInstructions = a.DetailedInstructions,
            ResourceUrl = a.ResourceUrl,
            DueDate = a.DueDate,
            Status = (int)a.Status,
            PatientSubmission = a.PatientSubmission,
            PatientSubmissionUrl = a.PatientSubmissionUrl,
            SubmittedAt = a.SubmittedAt,
            DoctorFeedback = a.DoctorFeedback,
            FeedbackAt = a.FeedbackAt,
            CreatedAt = a.CreatedAt
        };
    }

    private static MoodEntryDto MapToMoodEntryDto(MoodEntry m)
    {
        return new MoodEntryDto
        {
            Id = m.Id,
            TreatmentCaseId = m.TreatmentCaseId,
            PatientId = m.PatientId,
            MoodScore = m.MoodScore,
            AnxietyScore = m.AnxietyScore,
            StressScore = m.StressScore,
            SleepQualityScore = m.SleepQualityScore,
            DepressionScore = m.DepressionScore,
            RelationshipScore = m.RelationshipScore,
            Note = m.Note,
            RecordedAt = m.RecordedAt
        };
    }
}
