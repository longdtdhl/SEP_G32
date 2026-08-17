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
    private readonly IRepository<GoalDetail> _goalDetailRepo;
    private readonly IRepository<GoalSuccessCriteria> _successCriteriaRepo;
    private readonly IRepository<SuccessCriteriaEvaluation> _criteriaEvaluationRepo;
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
    private readonly IRepository<DoctorDayOff>? _dayOffRepo;
    private readonly IRepository<Message>? _messageRepo;
    private readonly IRepository<Conversation>? _conversationRepo;
    private readonly IRepository<PsychometricTest>? _psychTestRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TreatmentCaseService> _logger;

    public TreatmentCaseService(
        IRepository<TreatmentCase> caseRepo,
        IRepository<TreatmentSession> sessionRepo,
        IRepository<TreatmentGoal> goalRepo,
        IRepository<GoalDetail> goalDetailRepo,
        IRepository<GoalSuccessCriteria> successCriteriaRepo,
        IRepository<SuccessCriteriaEvaluation> criteriaEvaluationRepo,
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
        ILogger<TreatmentCaseService> logger,
        IRepository<DoctorDayOff>? dayOffRepo = null,
        IRepository<Message>? messageRepo = null,
        IRepository<Conversation>? conversationRepo = null,
        IRepository<PsychometricTest>? psychTestRepo = null)
    {
        _caseRepo = caseRepo;
        _sessionRepo = sessionRepo;
        _goalRepo = goalRepo;
        _goalDetailRepo = goalDetailRepo;
        _successCriteriaRepo = successCriteriaRepo;
        _criteriaEvaluationRepo = criteriaEvaluationRepo;
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
        _dayOffRepo = dayOffRepo;
        _messageRepo = messageRepo;
        _conversationRepo = conversationRepo;
        _psychTestRepo = psychTestRepo;
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
        if (doctor != null && (treatmentCase.DoctorId == doctor.Id || treatmentCase.DoctorId == doctor.UserId))
            return true;

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == userId || p.Id == userId);
        if (patient != null && (treatmentCase.PatientId == patient.Id || treatmentCase.PatientId == patient.UserId))
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

    public async Task<ApiResponse<DoctorTreatmentDashboardDto>> GetDoctorDashboardAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var doctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null)
            return ApiResponse<DoctorTreatmentDashboardDto>.ErrorResponse("Doctor profile not found.");

        var cases = (await _caseRepo.GetAllAsync(ct))
            .Where(c => !c.IsDeleted && c.Status == TreatmentCaseStatus.Active &&
                        (c.DoctorId == doctor.UserId || c.DoctorId == doctor.Id))
            .ToList();

        var attentionCases = new List<TreatmentCaseRiskDto>();
        foreach (var treatmentCase in cases)
        {
            var risk = await BuildCaseRiskAsync(treatmentCase, ct);
            if (!string.Equals(risk.Level, "Low", StringComparison.OrdinalIgnoreCase))
                attentionCases.Add(risk);
        }

        var unreadCount = 0;
        if (_conversationRepo != null && _messageRepo != null)
        {
            var conversationIds = (await _conversationRepo.GetAllAsync(ct))
                .Where(c => !c.IsDeleted && (c.DoctorId == doctor.UserId || c.DoctorId == doctor.Id))
                .Select(c => c.Id)
                .ToHashSet();

            unreadCount = (await _messageRepo.GetAllAsync(ct))
                .Count(m => conversationIds.Contains(m.ConversationId) &&
                            m.SenderId != doctor.UserId && m.SenderId != doctor.Id && !m.IsRead);
        }

        var result = new DoctorTreatmentDashboardDto
        {
            ActiveCaseCount = cases.Count,
            HighRiskCaseCount = attentionCases.Count(r => r.Level == "High"),
            AttentionCaseCount = attentionCases.Count,
            UnreadMessageCount = unreadCount,
            AttentionCases = attentionCases
                .OrderByDescending(r => r.Level == "High")
                .ThenByDescending(r => r.Score)
                .ToList()
        };

        return ApiResponse<DoctorTreatmentDashboardDto>.SuccessResponse(result);
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

        await _uow.BeginTransactionAsync(ct);

        // ── Clear future sessions if requested ──────────────────────────
        if (dto.ClearExistingFutureSessions)
        {
            var futureUncompleted = existingSessions.Where(s =>
                s.Status == TreatmentSessionStatus.Scheduled ||
                s.Status == TreatmentSessionStatus.Planned).ToList();

            foreach (var s in futureUncompleted)
            {
                s.Status = TreatmentSessionStatus.Planned;
                s.PlannedStartTime = null;
                s.PlannedEndTime = null;
                s.UpdatedAt = DateTime.UtcNow;
                _sessionRepo.Update(s);

                if (s.AppointmentId.HasValue)
                {
                    var linkedAppt = await _appointmentRepo.GetByIdAsync(s.AppointmentId.Value, ct);
                    if (linkedAppt != null && linkedAppt.Status != AppointmentStatus.Completed)
                    {
                        var previousStatus = linkedAppt.Status;
                        linkedAppt.Status = AppointmentStatus.Cancelled;
                        linkedAppt.CancelledAt = DateTime.UtcNow;
                        linkedAppt.CancellationReason = "Schedule regenerated by doctor.";
                        linkedAppt.TreatmentSessionId = null;
                        linkedAppt.UpdatedAt = DateTime.UtcNow;
                        _appointmentRepo.Update(linkedAppt);

                        var linkedSlot = await _slotRepo.GetByIdAsync(linkedAppt.AppointmentSlotId, ct);
                        if (linkedSlot != null)
                        {
                            linkedSlot.CurrentBookings = Math.Max(0, linkedSlot.CurrentBookings - 1);
                            linkedSlot.Status = linkedSlot.CurrentBookings < linkedSlot.MaxPatients
                                ? AppointmentSlotStatus.Available
                                : AppointmentSlotStatus.Booked;
                            linkedSlot.UpdatedAt = DateTime.UtcNow;
                            _slotRepo.Update(linkedSlot);
                        }

                        await _appointmentHistoryRepo.AddAsync(new AppointmentHistory
                        {
                            AppointmentId = linkedAppt.Id,
                            PreviousStatus = previousStatus,
                            NewStatus = AppointmentStatus.Cancelled,
                            Reason = "Schedule regenerated by doctor.",
                            Appointment = linkedAppt
                        }, ct);
                    }

                    s.AppointmentId = null;
                }
            }

            await _uow.SaveChangesAsync(ct);

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
        {
            await _uow.RollbackTransactionAsync(ct);
            return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse(
                $"This treatment package has no sessions remaining. All {totalNeeded} package sessions are already completed or scheduled. Create or assign a new package before generating more treatment sessions.");
        }

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
        try
        {
            // Load days off for the doctor
            var allDaysOff = _dayOffRepo != null ? await _dayOffRepo.GetAllAsync(ct) : new List<DoctorDayOff>();
            var doctorDaysOff = allDaysOff.Where(d => (d.DoctorProfileId == doctorProfile.Id || d.DoctorProfileId == doctorUserId) && !d.IsDeleted).ToList();

            while (createdSessions.Count < sessionsToSchedule && currentDate < maxDate)
            {
                if (!dto.DaysOfWeek.Contains(currentDate.DayOfWeek))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var slotDate = DateOnly.FromDateTime(currentDate);
                var slotStartDateTime = currentDate.Date.Add(startTime.ToTimeSpan());

                // Check if date is a doctor day off
                var isDayOff = doctorDaysOff.Any(d => currentDate.Date >= d.StartDate.Date && currentDate.Date <= d.EndDate.Date);
                if (isDayOff)
                {
                    skippedDates.Add(currentDate.ToString("MMM dd (ddd)") + " — day off");
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Check for overlapping booked/blocked slots on this date
                var overlappingBookedSlots = allSlots.Where(s =>
                    s.DoctorProfileId == doctorProfile.Id &&
                    s.SlotDate == slotDate &&
                    !s.IsDeleted &&
                    (s.Status == AppointmentSlotStatus.Booked || s.Status == AppointmentSlotStatus.Blocked) &&
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

                var slotNotes = $"Treatment session for {patientName}: {treatmentCase.CaseName}";

                // ── STAGE 1: Check for existing Available slot to REUSE, or create new ──
                var existingAvailableSlot = allSlots.FirstOrDefault(s =>
                    s.DoctorProfileId == doctorProfile.Id &&
                    s.SlotDate == slotDate &&
                    s.StartTime == startTime &&
                    !s.IsDeleted &&
                    s.Status == AppointmentSlotStatus.Available);

                AppointmentSlot slot;
                if (existingAvailableSlot != null)
                {
                    // Reuse existing Available slot
                    slot = existingAvailableSlot;
                    slot.Status = AppointmentSlotStatus.Booked;
                    slot.CurrentBookings = 1;
                    slot.Notes = slotNotes;
                    slot.UpdatedAt = DateTime.UtcNow;
                    _slotRepo.Update(slot);
                    await _uow.SaveChangesAsync(ct);
                }
                else
                {
                    // Create new AppointmentSlot
                    slot = new AppointmentSlot
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
                }

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

            if (createdSessions.Count < sessionsToSchedule)
            {
                await _uow.RollbackTransactionAsync(ct);
                var missingCount = sessionsToSchedule - createdSessions.Count;
                var skipSummary = skippedDates.Any()
                    ? " Conflict reasons: " + string.Join(", ", skippedDates.Take(5))
                    : "";
                return ApiResponse<List<TreatmentSessionDto>>.ErrorResponse(
                    $"Insufficient available schedule slots for the requested recurrence. Only {createdSessions.Count} of {sessionsToSchedule} required sessions could be scheduled (missing {missingCount} session(s)).{skipSummary}");
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
        var activeSessionCount = existingSessions.Count(s => s.Status != TreatmentSessionStatus.Cancelled);
        if (activeSessionCount >= treatmentCase.TotalSessions)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse(
                $"This treatment package has no sessions remaining ({activeSessionCount}/{treatmentCase.TotalSessions} sessions already planned or completed). Create or assign a new package before adding more treatment sessions.");

        var sessionNumber = existingSessions.Any() ? existingSessions.Max(s => s.SessionNumber) + 1 : 1;

        if (existingSessions.Any(s => s.SessionNumber == sessionNumber))
            return ApiResponse<TreatmentSessionDto>.ErrorResponse($"Session number {sessionNumber} already exists for this treatment case.");

        Appointment? appointment = null;
        AppointmentSlot? appointmentSlot = null;
        if (dto.AppointmentId.HasValue)
        {
            appointment = await _appointmentRepo.GetByIdAsync(dto.AppointmentId.Value, ct);
            if (appointment == null || appointment.IsDeleted)
                return ApiResponse<TreatmentSessionDto>.ErrorResponse("Linked appointment not found.");
            if (appointment.TreatmentCaseId != treatmentCase.Id)
                return ApiResponse<TreatmentSessionDto>.ErrorResponse("The appointment does not belong to this treatment case.");
            if (appointment.TreatmentSessionId.HasValue)
                return ApiResponse<TreatmentSessionDto>.ErrorResponse("The appointment is already linked to another treatment session.");
            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected or AppointmentStatus.NoShow)
                return ApiResponse<TreatmentSessionDto>.ErrorResponse("A cancelled, rejected, or no-show appointment cannot be linked to a new session.");

            appointmentSlot = await _slotRepo.GetByIdAsync(appointment.AppointmentSlotId, ct);
            if (appointmentSlot == null || appointmentSlot.IsDeleted)
                return ApiResponse<TreatmentSessionDto>.ErrorResponse("Linked appointment slot not found.");
        }
        else if (dto.PlannedStartTime.HasValue)
        {
            var doctor = await _doctorRepo.GetByIdAsync(treatmentCase.DoctorId, ct);
            if (doctor == null)
            {
                var allDocs = await _doctorRepo.GetAllAsync(ct);
                doctor = allDocs.FirstOrDefault(d => d.Id == treatmentCase.DoctorId || d.UserId == treatmentCase.DoctorId);
            }

            var patient = await _patientRepo.GetByIdAsync(treatmentCase.PatientId, ct);
            if (patient == null)
            {
                var allPats = await _patientRepo.GetAllAsync(ct);
                patient = allPats.FirstOrDefault(p => p.Id == treatmentCase.PatientId || p.UserId == treatmentCase.PatientId);
            }

            var slotDate = DateOnly.FromDateTime(dto.PlannedStartTime.Value);
            var slotStart = TimeOnly.FromDateTime(dto.PlannedStartTime.Value);
            var slotEnd = dto.PlannedEndTime.HasValue
                ? TimeOnly.FromDateTime(dto.PlannedEndTime.Value)
                : slotStart.AddMinutes(60);

            var allDoctorSlots = await _slotRepo.GetAllAsync(ct);
            var docProfileId = doctor?.Id ?? treatmentCase.DoctorId;
            appointmentSlot = allDoctorSlots.FirstOrDefault(s =>
                s.DoctorProfileId == docProfileId &&
                s.SlotDate == slotDate &&
                s.StartTime == slotStart &&
                s.EndTime == slotEnd &&
                !s.IsDeleted);

            if (appointmentSlot == null)
            {
                appointmentSlot = new AppointmentSlot
                {
                    DoctorProfileId = docProfileId,
                    SlotDate = slotDate,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    MaxPatients = 1,
                    CurrentBookings = 1,
                    Status = AppointmentSlotStatus.Booked,
                    DoctorProfile = doctor!
                };
                await _slotRepo.AddAsync(appointmentSlot, ct);
            }
            else
            {
                appointmentSlot.CurrentBookings++;
                appointmentSlot.Status = AppointmentSlotStatus.Booked;
                _slotRepo.Update(appointmentSlot);
            }

            var bookingCode = $"OPCBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            appointment = new Appointment
            {
                BookingCode = bookingCode,
                AppointmentSlotId = appointmentSlot.Id,
                DoctorId = docProfileId,
                PatientId = patient?.Id ?? treatmentCase.PatientId,
                TreatmentCaseId = treatmentCase.Id,
                TreatmentPackageId = treatmentCase.TreatmentPackageId != Guid.Empty ? treatmentCase.TreatmentPackageId : null,
                AppointmentDate = dto.PlannedStartTime.Value,
                Status = AppointmentStatus.Approved,
                ApprovedAt = DateTime.UtcNow,
                AppointmentSlot = appointmentSlot,
                Doctor = doctor!,
                Patient = patient
            };
            await _appointmentRepo.AddAsync(appointment, ct);
        }

        var sessionPlannedStart = appointmentSlot?.SlotDate.ToDateTime(appointmentSlot.StartTime) ?? dto.PlannedStartTime;
        var sessionPlannedEnd = appointmentSlot?.SlotDate.ToDateTime(appointmentSlot.EndTime) ?? dto.PlannedEndTime;

        var session = new TreatmentSession
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            AppointmentId = appointment?.Id ?? dto.AppointmentId,
            SessionNumber = sessionNumber,
            Title = dto.Title ?? $"Session {sessionNumber}",
            Description = dto.Description,
            PlannedStartTime = sessionPlannedStart,
            PlannedEndTime = sessionPlannedEnd,
            Status = appointment == null ? TreatmentSessionStatus.Planned : appointment.Status switch
            {
                AppointmentStatus.InProgress or AppointmentStatus.AwaitingPatientConfirmation or
                    AppointmentStatus.AwaitingGuestCompletionConfirmation or AppointmentStatus.CompletionDisputed
                    => TreatmentSessionStatus.InProgress,
                AppointmentStatus.Completed => TreatmentSessionStatus.Completed,
                _ => TreatmentSessionStatus.Scheduled
            },
            TreatmentCase = treatmentCase
        };

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _sessionRepo.AddAsync(session, ct);
            if (appointment != null)
            {
                appointment.TreatmentSessionId = session.Id;
                appointment.UpdatedAt = DateTime.UtcNow;
                _appointmentRepo.Update(appointment);
            }
            await _uow.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Failed to create treatment session for case {CaseId}", dto.TreatmentCaseId);
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Failed to create session due to a scheduling conflict.");
        }

        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), "Session created successfully.");
    }

    public async Task<ApiResponse<TreatmentSessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionDto dto, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Session not found.");

        if (session.AppointmentId.HasValue && (dto.PlannedStartTime.HasValue || dto.PlannedEndTime.HasValue))
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Reschedule the linked appointment to change this session's planned time.");

        if (dto.Title != null) session.Title = dto.Title;
        if (dto.Description != null) session.Description = dto.Description;
        if (dto.PlannedStartTime.HasValue) session.PlannedStartTime = dto.PlannedStartTime;
        if (dto.PlannedEndTime.HasValue) session.PlannedEndTime = dto.PlannedEndTime;

        session.UpdatedAt = DateTime.UtcNow;
        _sessionRepo.Update(session);

        if (dto.LinkedGoalDetailIds != null || dto.LinkedGoalIds != null)
        {
            var detailIds = await ResolveGoalDetailIdsAsync(
                session.TreatmentCaseId, dto.LinkedGoalDetailIds, dto.LinkedGoalIds, ct);
            await ReplaceSessionGoalLinksAsync(session, detailIds, ct);
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
                    var previousStatus = appt.Status;
                    appt.Status = AppointmentStatus.Cancelled;
                    appt.CancelledAt = DateTime.UtcNow;
                    appt.CancellationReason = "Session deleted by doctor.";
                    appt.UpdatedAt = DateTime.UtcNow;
                    appt.TreatmentSessionId = null;
                    _appointmentRepo.Update(appt);

                    await _appointmentHistoryRepo.AddAsync(new AppointmentHistory
                    {
                        AppointmentId = appt.Id,
                        PreviousStatus = previousStatus,
                        NewStatus = AppointmentStatus.Cancelled,
                        Reason = appt.CancellationReason,
                        ChangedByRole = "Doctor",
                        Appointment = appt
                    }, ct);

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

        if (dto.SessionIdsInOrder.Count != sessions.Count ||
            dto.SessionIdsInOrder.Distinct().Count() != dto.SessionIdsInOrder.Count ||
            dto.SessionIdsInOrder.Any(id => sessions.All(s => s.Id != id)))
            return ApiResponse.ErrorResponse("The reorder request must contain every active session exactly once.");

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

        if ((linkedAppt.TreatmentSessionId.HasValue && linkedAppt.TreatmentSessionId != session.Id) ||
            (linkedAppt.TreatmentCaseId.HasValue && linkedAppt.TreatmentCaseId != session.TreatmentCaseId))
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Appointment and treatment session links are inconsistent. Reschedule this session before continuing.");

        // Repair legacy one-way links while preserving compatibility with existing data.
        linkedAppt.TreatmentSessionId = session.Id;
        linkedAppt.TreatmentCaseId = session.TreatmentCaseId;
        _appointmentRepo.Update(linkedAppt);

        if (linkedAppt.Status != AppointmentStatus.Completed)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("The session can only be completed after the patient or guest confirms appointment completion.");

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

        if (dto.LinkedGoalDetailIds != null || dto.LinkedGoalIds != null)
        {
            var detailIds = await ResolveGoalDetailIdsAsync(
                session.TreatmentCaseId, dto.LinkedGoalDetailIds, dto.LinkedGoalIds, ct);
            await ReplaceSessionGoalLinksAsync(session, detailIds, ct);
        }

        await SynchronizeGoalsForSessionAsync(session, null, ct);

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

        var allAppts = await _appointmentRepo.GetAllAsync(ct);
        var allSlots = await _slotRepo.GetAllAsync(ct);
        var slotDict = allSlots.GroupBy(sl => sl.Id).ToDictionary(g => g.Key, g => g.First());

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.Id == tc.PatientId || p.UserId == tc.PatientId);
        var patientIds = new HashSet<Guid> { tc.PatientId };
        if (patient != null)
        {
            patientIds.Add(patient.Id);
            patientIds.Add(patient.UserId);
        }

        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = allDoctors.FirstOrDefault(d => d.Id == tc.DoctorId || d.UserId == tc.DoctorId);
        var doctorIds = new HashSet<Guid> { tc.DoctorId };
        if (doctor != null)
        {
            doctorIds.Add(doctor.Id);
            doctorIds.Add(doctor.UserId);
        }

        var caseAppts = allAppts.Where(a => !a.IsDeleted && (
            a.TreatmentCaseId == caseId ||
            (tc.TreatmentPackageId != Guid.Empty && a.TreatmentPackageId == tc.TreatmentPackageId) ||
            (a.TreatmentSessionId.HasValue && sessions.Any(s => s.Id == a.TreatmentSessionId.Value)) ||
            (sessions.Any(s => s.AppointmentId == a.Id)) ||
            (a.PatientId.HasValue && patientIds.Contains(a.PatientId.Value) && doctorIds.Contains(a.DoctorId) && (
                tc.TreatmentPackageId != Guid.Empty ? (a.TreatmentPackageId == tc.TreatmentPackageId || a.TreatmentPackageId == null) : true
            ))
        )).ToList();

        bool hasChanges = false;

        // 1. Link any existing sessions that don't have AppointmentId yet to available unlinked appointments
        var linkedApptIds = sessions.Where(s => s.AppointmentId.HasValue).Select(s => s.AppointmentId!.Value).ToHashSet();
        foreach (var s in sessions)
        {
            if (!s.AppointmentId.HasValue)
            {
                var candidateAppt = caseAppts.FirstOrDefault(a => !linkedApptIds.Contains(a.Id) && (!a.TreatmentSessionId.HasValue || a.TreatmentSessionId == s.Id));
                if (candidateAppt != null)
                {
                    s.AppointmentId = candidateAppt.Id;
                    candidateAppt.TreatmentSessionId = s.Id;
                    candidateAppt.TreatmentCaseId = caseId;
                    _sessionRepo.Update(s);
                    _appointmentRepo.Update(candidateAppt);
                    linkedApptIds.Add(candidateAppt.Id);
                    hasChanges = true;
                }
                else if (s.PlannedStartTime.HasValue)
                {
                    var slotDate = DateOnly.FromDateTime(s.PlannedStartTime.Value);
                    var slotStart = TimeOnly.FromDateTime(s.PlannedStartTime.Value);
                    var slotEnd = s.PlannedEndTime.HasValue
                        ? TimeOnly.FromDateTime(s.PlannedEndTime.Value)
                        : slotStart.AddMinutes(60);

                    var docProfileId = doctor?.Id ?? tc.DoctorId;
                    var apptSlot = allSlots.FirstOrDefault(sl =>
                        sl.DoctorProfileId == docProfileId &&
                        sl.SlotDate == slotDate &&
                        sl.StartTime == slotStart &&
                        sl.EndTime == slotEnd &&
                        !sl.IsDeleted);

                    if (apptSlot == null)
                    {
                        apptSlot = new AppointmentSlot
                        {
                            DoctorProfileId = docProfileId,
                            SlotDate = slotDate,
                            StartTime = slotStart,
                            EndTime = slotEnd,
                            MaxPatients = 1,
                            CurrentBookings = 1,
                            Status = AppointmentSlotStatus.Booked,
                            DoctorProfile = doctor!
                        };
                        await _slotRepo.AddAsync(apptSlot, ct);
                        slotDict[apptSlot.Id] = apptSlot;
                    }
                    else
                    {
                        apptSlot.CurrentBookings++;
                        apptSlot.Status = AppointmentSlotStatus.Booked;
                        _slotRepo.Update(apptSlot);
                    }

                    var bookingCode = $"OPCBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                    var newAppt = new Appointment
                    {
                        BookingCode = bookingCode,
                        AppointmentSlotId = apptSlot.Id,
                        DoctorId = docProfileId,
                        PatientId = patient?.Id ?? tc.PatientId,
                        TreatmentCaseId = caseId,
                        TreatmentPackageId = tc.TreatmentPackageId != Guid.Empty ? tc.TreatmentPackageId : null,
                        TreatmentSessionId = s.Id,
                        AppointmentDate = s.PlannedStartTime.Value,
                        Status = s.Status == TreatmentSessionStatus.Completed ? AppointmentStatus.Completed
                               : s.Status == TreatmentSessionStatus.Cancelled ? AppointmentStatus.Cancelled
                               : AppointmentStatus.Approved,
                        ApprovedAt = DateTime.UtcNow,
                        AppointmentSlot = apptSlot,
                        Doctor = doctor!,
                        Patient = patient
                    };
                    await _appointmentRepo.AddAsync(newAppt, ct);
                    s.AppointmentId = newAppt.Id;
                    _sessionRepo.Update(s);
                    caseAppts.Add(newAppt);
                    linkedApptIds.Add(newAppt.Id);
                    hasChanges = true;
                }
            }
        }

        // 2. For every appointment belonging to the case that does not have a session, create its session
        foreach (var appt in caseAppts)
        {
            if (!sessions.Any(s => s.AppointmentId == appt.Id || (appt.TreatmentSessionId.HasValue && s.Id == appt.TreatmentSessionId.Value)))
            {
                DateTime? startTime = appt.AppointmentDate;
                DateTime? endTime = appt.AppointmentDate?.AddMinutes(60);
                if (slotDict.TryGetValue(appt.AppointmentSlotId, out var slot))
                {
                    startTime = slot.SlotDate.ToDateTime(slot.StartTime);
                    endTime = slot.SlotDate.ToDateTime(slot.EndTime);
                }

                var nextNumber = sessions.Count == 0 ? 1 : sessions.Max(s => s.SessionNumber) + 1;
                var newSession = new TreatmentSession
                {
                    TreatmentCaseId = caseId,
                    SessionNumber = nextNumber,
                    Title = $"Session {nextNumber}: {tc.CaseName}",
                    Description = $"Treatment session {nextNumber} of {tc.TotalSessions}",
                    PlannedStartTime = startTime,
                    PlannedEndTime = endTime,
                    Status = appt.Status == AppointmentStatus.Completed ? TreatmentSessionStatus.Completed
                           : appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.Rejected ? TreatmentSessionStatus.Cancelled
                           : TreatmentSessionStatus.Scheduled,
                    AppointmentId = appt.Id,
                    TreatmentCase = tc
                };
                await _sessionRepo.AddAsync(newSession, ct);
                
                appt.TreatmentCaseId = caseId;
                appt.TreatmentSessionId = newSession.Id;
                _appointmentRepo.Update(appt);
                
                sessions.Add(newSession);
                linkedApptIds.Add(appt.Id);
                hasChanges = true;
            }
        }

        // 3. Keep status, dates, and two-way keys in 100% sync between session and appointment
        foreach (var s in sessions)
        {
            if (s.AppointmentId.HasValue)
            {
                var appt = caseAppts.FirstOrDefault(a => a.Id == s.AppointmentId.Value) ?? allAppts.FirstOrDefault(a => a.Id == s.AppointmentId.Value);
                if (appt != null)
                {
                    if (appt.TreatmentCaseId != caseId || appt.TreatmentSessionId != s.Id)
                    {
                        appt.TreatmentCaseId = caseId;
                        appt.TreatmentSessionId = s.Id;
                        _appointmentRepo.Update(appt);
                        hasChanges = true;
                    }

                    if (slotDict.TryGetValue(appt.AppointmentSlotId, out var slot))
                    {
                        var start = slot.SlotDate.ToDateTime(slot.StartTime);
                        var end = slot.SlotDate.ToDateTime(slot.EndTime);
                        if (s.PlannedStartTime != start || s.PlannedEndTime != end)
                        {
                            s.PlannedStartTime = start;
                            s.PlannedEndTime = end;
                            _sessionRepo.Update(s);
                            hasChanges = true;
                        }
                    }
                    else if (appt.AppointmentDate.HasValue && (s.PlannedStartTime == null || s.PlannedStartTime != appt.AppointmentDate.Value))
                    {
                        s.PlannedStartTime = appt.AppointmentDate.Value;
                        s.PlannedEndTime = appt.AppointmentDate.Value.AddMinutes(60);
                        _sessionRepo.Update(s);
                        hasChanges = true;
                    }

                    if (appt.Status == AppointmentStatus.Completed && s.Status != TreatmentSessionStatus.Completed)
                    {
                        s.Status = TreatmentSessionStatus.Completed;
                        _sessionRepo.Update(s);
                        hasChanges = true;
                    }
                    else if ((appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.Rejected) && s.Status != TreatmentSessionStatus.Cancelled)
                    {
                        s.Status = TreatmentSessionStatus.Cancelled;
                        _sessionRepo.Update(s);
                        hasChanges = true;
                    }
                }
            }
        }

        if (hasChanges)
        {
            await _uow.SaveChangesAsync(ct);
        }

        var dtos = new List<TreatmentSessionDto>();
        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var allSessionGoals = await _sessionGoalRepo.GetAllAsync(ct);
        var allGoals = await _goalRepo.GetAllAsync(ct);
        var allDetails = await _goalDetailRepo.GetAllAsync(ct);

        foreach (var s in sessions.OrderBy(x => x.SessionNumber))
        {
            var dto = MapToSessionDto(s);
            if (s.AppointmentId.HasValue)
            {
                var appt = caseAppts.FirstOrDefault(a => a.Id == s.AppointmentId.Value) ?? allAppts.FirstOrDefault(a => a.Id == s.AppointmentId.Value);
                if (appt != null)
                {
                    dto.BookingCode = appt.BookingCode;
                    if (slotDict.TryGetValue(appt.AppointmentSlotId, out var slot))
                    {
                        var start = slot.SlotDate.ToDateTime(slot.StartTime);
                        var end = slot.SlotDate.ToDateTime(slot.EndTime);
                        dto.PlannedStartTime = start;
                        dto.PlannedEndTime = end;
                        dto.AppointmentDate = start;
                    }
                    else
                    {
                        dto.AppointmentDate = appt.AppointmentDate ?? s.PlannedStartTime;
                        dto.PlannedStartTime ??= appt.AppointmentDate;
                        dto.PlannedEndTime ??= appt.AppointmentDate?.AddMinutes(60);
                    }
                }
            }
            var sessionHomework = allAssignments.Where(a => a.TreatmentSessionId == s.Id && !a.IsDeleted).ToList();
            dto.HomeworkList = sessionHomework.Select(MapToHomeworkDto).ToList();

            var linkedDetailIds = allSessionGoals.Where(sg => sg.TreatmentSessionId == s.Id && !sg.IsDeleted).Select(sg => sg.GoalDetailId).ToHashSet();
            var linkedDetails = allDetails.Where(d => linkedDetailIds.Contains(d.Id) && !d.IsDeleted).ToList();
            var linkedGoalIds = linkedDetails.Select(d => d.GoalId).ToHashSet();
            var linkedGoals = allGoals.Where(g => linkedGoalIds.Contains(g.Id) && !g.IsDeleted).ToList();
            dto.LinkedGoals = linkedGoals.Select(MapToGoalDto).ToList();
            dto.LinkedGoalDetails = linkedDetails.Select(MapToGoalDetailDto).ToList();

            dtos.Add(dto);
        }

        return ApiResponse<List<TreatmentSessionDto>>.SuccessResponse(dtos);
    }

    // ==================== Goals ====================

    public async Task<ApiResponse<TreatmentGoalDto>> CreateGoalAsync(CreateGoalDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Treatment case not found.");
        if (doctorUserId.HasValue && !await ValidateUserAccessToCaseAsync(treatmentCase, doctorUserId, ct))
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Access denied.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal title is required.");

        if (!Enum.IsDefined(typeof(GoalCategory), dto.Category) || !Enum.IsDefined(typeof(GoalPriority), dto.Priority))
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal category or priority is invalid.");

        var currentGoals = await _goalRepo.GetAllAsync(ct);
        var nextOrder = currentGoals.Where(g => g.TreatmentCaseId == dto.TreatmentCaseId && !g.IsDeleted)
            .Select(g => g.OrderIndex).DefaultIfEmpty(-1).Max() + 1;

        var goal = new TreatmentGoal
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            TemplateId = dto.TemplateId,
            CreatedByDoctorId = treatmentCase.DoctorId,
            Title = dto.Title,
            Description = dto.Description,
            Category = (GoalCategory)dto.Category,
            Priority = (GoalPriority)dto.Priority,
            OrderIndex = dto.OrderIndex ?? nextOrder,
            TargetValue = dto.TargetValue,
            CurrentValue = dto.CurrentValue,
            Unit = dto.Unit,
            TargetDate = dto.TargetDate,
            StartDate = dto.StartDate,
            Status = GoalStatus.Draft,
            TreatmentCase = treatmentCase
        };

        await _goalRepo.AddAsync(goal, ct);

        foreach (var detailDto in dto.Details)
        {
            if (string.IsNullOrWhiteSpace(detailDto.Title))
                return ApiResponse<TreatmentGoalDto>.ErrorResponse("Every goal detail needs a title.");

            var detail = await CreateGoalDetailEntityAsync(goal, detailDto, ct);
            await _goalDetailRepo.AddAsync(detail, ct);
            await ReplaceSessionGoalLinksAsync(detail, detailDto.TreatmentSessionIds, ct);
        }

        foreach (var criteriaDto in dto.SuccessCriteria)
        {
            var criteriaValidation = ValidateCriteria(criteriaDto.CriteriaType, criteriaDto.DataSource, criteriaDto.Operator, criteriaDto.TargetValue, criteriaDto.Weight);
            if (criteriaValidation != null)
                return ApiResponse<TreatmentGoalDto>.ErrorResponse(criteriaValidation);

            await _successCriteriaRepo.AddAsync(new GoalSuccessCriteria
            {
                GoalId = goal.Id,
                CriteriaType = (GoalSuccessCriteriaType)criteriaDto.CriteriaType,
                DataSource = (GoalCriteriaDataSource)criteriaDto.DataSource,
                Operator = (GoalCriteriaOperator)criteriaDto.Operator,
                TargetValue = criteriaDto.TargetValue,
                Weight = criteriaDto.Weight,
                IsRequired = criteriaDto.IsRequired,
                Description = criteriaDto.Description,
                Goal = goal
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentGoalDto>.SuccessResponse(await BuildGoalDtoAsync(goal, ct), "Treatment goal created successfully.");
    }

    public async Task<ApiResponse<TreatmentGoalDto>> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal not found.");
        if (!await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<TreatmentGoalDto>.ErrorResponse("Access denied.");

        if (dto.Title != null) goal.Title = dto.Title;
        if (dto.Description != null) goal.Description = dto.Description;
        if (dto.Category.HasValue) goal.Category = (GoalCategory)dto.Category.Value;
        if (dto.Priority.HasValue) goal.Priority = (GoalPriority)dto.Priority.Value;
        if (dto.CurrentValue.HasValue) goal.CurrentValue = dto.CurrentValue.Value;
        if (dto.TargetValue.HasValue) goal.TargetValue = dto.TargetValue.Value;
        if (dto.Unit != null) goal.Unit = dto.Unit;
        if (dto.DoctorNotes != null) goal.DoctorNotes = dto.DoctorNotes;
        if (dto.StartDate.HasValue) goal.StartDate = dto.StartDate;
        if (dto.OrderIndex.HasValue) goal.OrderIndex = dto.OrderIndex.Value;

        if (dto.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(GoalStatus), dto.Status.Value))
                return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal status is invalid.");
            if ((GoalStatus)dto.Status.Value == GoalStatus.Achieved)
                return ApiResponse<TreatmentGoalDto>.ErrorResponse("Goal achievement is determined from required success criteria.");
            goal.Status = (GoalStatus)dto.Status.Value;
        }
        if (dto.ProgressPercent.HasValue)
        {
            if (dto.ProgressPercent is < 0 or > 100)
                return ApiResponse<TreatmentGoalDto>.ErrorResponse("Progress percentage must be between 0 and 100.");
            goal.ProgressPercent = dto.ProgressPercent.Value;
        }

        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            await RecalculateProgressAsync(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentGoalDto>.SuccessResponse(await BuildGoalDtoAsync(goal, ct), "Goal updated successfully.");
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

        var dtos = new List<TreatmentGoalDto>();
        foreach (var g in goals)
            dtos.Add(await BuildGoalDtoAsync(g, ct));

        return ApiResponse<List<TreatmentGoalDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<TreatmentGoalProgressDto>> RecordGoalProgressAsync(CreateGoalProgressDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var goal = await _goalRepo.GetByIdAsync(dto.GoalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Goal not found.");
        if (!await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Access denied.");

        if (dto.ProgressPercent is < 0 or > 100)
            return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Progress percentage must be between 0 and 100.");

        GoalDetail? detail = null;
        if (dto.GoalDetailId.HasValue)
        {
            detail = await _goalDetailRepo.GetByIdAsync(dto.GoalDetailId.Value, ct);
            if (detail == null || detail.IsDeleted || detail.GoalId != goal.Id)
                return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Goal detail does not belong to this goal.");
        }

        TreatmentSession? session = null;
        if (dto.TreatmentSessionId.HasValue)
        {
            session = await _sessionRepo.GetByIdAsync(dto.TreatmentSessionId.Value, ct);
            if (session == null || session.IsDeleted || session.TreatmentCaseId != goal.TreatmentCaseId)
                return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Treatment session does not belong to this treatment case.");
            if (detail != null && !await IsDetailLinkedToSessionAsync(detail.Id, session.Id, ct))
                return ApiResponse<TreatmentGoalProgressDto>.ErrorResponse("Link this goal detail to the treatment session before recording progress.");
        }

        if (detail != null)
        {
            detail.ProgressPercent = dto.ProgressPercent;
            detail.Status = dto.ProgressPercent >= 100 ? GoalDetailStatus.Completed :
                detail.Status == GoalDetailStatus.NotStarted && dto.ProgressPercent > 0 ? GoalDetailStatus.InProgress : detail.Status;
            detail.CompletedDate = dto.ProgressPercent >= 100 ? DateTime.UtcNow : null;
            detail.UpdatedAt = DateTime.UtcNow;
            _goalDetailRepo.Update(detail);
        }

        goal.ProgressPercent = await CalculateGoalProgressAsync(goal.Id, dto.ProgressPercent, ct);
        if (dto.CurrentValue.HasValue) goal.CurrentValue = dto.CurrentValue.Value;

        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        var progressRecord = new TreatmentGoalProgress
        {
            GoalId = dto.GoalId,
            TreatmentSessionId = dto.TreatmentSessionId,
            GoalDetailId = dto.GoalDetailId,
            ProgressPercent = dto.ProgressPercent,
            CurrentValue = dto.CurrentValue,
            DoctorComment = dto.DoctorComment,
            RecordedAt = DateTime.UtcNow,
            Goal = goal
        };

        await _goalProgressRepo.AddAsync(progressRecord, ct);

        await SynchronizeAndEvaluateGoalAsync(goal, session, doctorUserId, ct);

        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            await RecalculateProgressAsync(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }

        await _uow.SaveChangesAsync(ct);
        return ApiResponse<TreatmentGoalProgressDto>.SuccessResponse(MapToGoalProgressDto(progressRecord), "Goal progress recorded successfully.");
    }

    public async Task<ApiResponse<List<TreatmentGoalProgressDto>>> GetGoalProgressHistoryAsync(Guid goalId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<List<TreatmentGoalProgressDto>>.ErrorResponse("Goal not found.");
        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase == null || !await ValidateUserAccessToCaseAsync(treatmentCase, requestingUserId, ct))
            return ApiResponse<List<TreatmentGoalProgressDto>>.ErrorResponse("Access denied.");
        var all = await _goalProgressRepo.GetAllAsync(ct);
        var history = all
            .Where(p => p.GoalId == goalId && !p.IsDeleted)
            .OrderByDescending(p => p.RecordedAt)
            .Select(MapToGoalProgressDto)
            .ToList();

        return ApiResponse<List<TreatmentGoalProgressDto>>.SuccessResponse(history);
    }

    public async Task<ApiResponse<GoalDetailDto>> CreateGoalDetailAsync(Guid goalId, CreateGoalDetailDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<GoalDetailDto>.ErrorResponse("Goal not found.");
        if (!await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<GoalDetailDto>.ErrorResponse("Access denied.");
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<GoalDetailDto>.ErrorResponse("Goal detail title is required.");

        var detail = await CreateGoalDetailEntityAsync(goal, dto, ct);
        await _goalDetailRepo.AddAsync(detail, ct);
        await ReplaceSessionGoalLinksAsync(detail, dto.TreatmentSessionIds, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<GoalDetailDto>.SuccessResponse(await BuildGoalDetailDtoAsync(detail, ct), "Goal detail created successfully.");
    }

    public async Task<ApiResponse<GoalDetailDto>> UpdateGoalDetailAsync(Guid detailId, UpdateGoalDetailDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var detail = await _goalDetailRepo.GetByIdAsync(detailId, ct);
        if (detail == null || detail.IsDeleted)
            return ApiResponse<GoalDetailDto>.ErrorResponse("Goal detail not found.");
        var goal = await _goalRepo.GetByIdAsync(detail.GoalId, ct);
        if (goal == null || !await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<GoalDetailDto>.ErrorResponse("Access denied.");

        if (dto.Title != null) detail.Title = dto.Title;
        if (dto.Description != null) detail.Description = dto.Description;
        if (dto.Objective != null) detail.Objective = dto.Objective;
        if (dto.ExpectedOutcome != null) detail.ExpectedOutcome = dto.ExpectedOutcome;
        if (dto.OrderIndex.HasValue) detail.OrderIndex = dto.OrderIndex.Value;
        if (dto.EstimatedSessions.HasValue) detail.EstimatedSessions = dto.EstimatedSessions;
        if (dto.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(GoalDetailStatus), dto.Status.Value))
                return ApiResponse<GoalDetailDto>.ErrorResponse("Goal detail status is invalid.");
            detail.Status = (GoalDetailStatus)dto.Status.Value;
            detail.CompletedDate = detail.Status == GoalDetailStatus.Completed ? DateTime.UtcNow : null;
            detail.ProgressPercent = detail.Status == GoalDetailStatus.Completed ? 100 : detail.ProgressPercent;
        }
        detail.UpdatedAt = DateTime.UtcNow;
        _goalDetailRepo.Update(detail);
        if (dto.TreatmentSessionIds != null)
            await ReplaceSessionGoalLinksAsync(detail, dto.TreatmentSessionIds, ct);
        await SynchronizeAndEvaluateGoalAsync(goal, null, doctorUserId, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<GoalDetailDto>.SuccessResponse(await BuildGoalDetailDtoAsync(detail, ct), "Goal detail updated successfully.");
    }

    public async Task<ApiResponse> DeleteGoalDetailAsync(Guid detailId, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var detail = await _goalDetailRepo.GetByIdAsync(detailId, ct);
        if (detail == null || detail.IsDeleted)
            return ApiResponse.ErrorResponse("Goal detail not found.");
        var goal = await _goalRepo.GetByIdAsync(detail.GoalId, ct);
        if (goal == null || !await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse.ErrorResponse("Access denied.");
        detail.IsDeleted = true;
        detail.UpdatedAt = DateTime.UtcNow;
        _goalDetailRepo.Update(detail);
        foreach (var link in (await _sessionGoalRepo.GetAllAsync(ct)).Where(x => x.GoalDetailId == detailId && !x.IsDeleted))
            _sessionGoalRepo.Delete(link);
        await SynchronizeAndEvaluateGoalAsync(goal, null, doctorUserId, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Goal detail deleted successfully.");
    }

    public async Task<ApiResponse<GoalSuccessCriteriaDto>> CreateSuccessCriteriaAsync(Guid goalId, CreateGoalSuccessCriteriaDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, ct);
        if (goal == null || goal.IsDeleted)
            return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse("Goal not found.");
        if (!await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse("Access denied.");
        var validation = ValidateCriteria(dto.CriteriaType, dto.DataSource, dto.Operator, dto.TargetValue, dto.Weight);
        if (validation != null) return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse(validation);

        var criteria = new GoalSuccessCriteria
        {
            GoalId = goalId,
            CriteriaType = (GoalSuccessCriteriaType)dto.CriteriaType,
            DataSource = (GoalCriteriaDataSource)dto.DataSource,
            Operator = (GoalCriteriaOperator)dto.Operator,
            TargetValue = dto.TargetValue,
            Weight = dto.Weight,
            IsRequired = dto.IsRequired,
            Description = dto.Description,
            Goal = goal
        };
        await _successCriteriaRepo.AddAsync(criteria, ct);
        await SynchronizeAndEvaluateGoalAsync(goal, null, doctorUserId, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<GoalSuccessCriteriaDto>.SuccessResponse(await BuildCriteriaDtoAsync(criteria, ct), "Success criterion created successfully.");
    }

    public async Task<ApiResponse<GoalSuccessCriteriaDto>> UpdateSuccessCriteriaAsync(Guid criteriaId, UpdateGoalSuccessCriteriaDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var criteria = await _successCriteriaRepo.GetByIdAsync(criteriaId, ct);
        if (criteria == null || criteria.IsDeleted)
            return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse("Success criterion not found.");
        var goal = await _goalRepo.GetByIdAsync(criteria.GoalId, ct);
        if (goal == null || !await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse("Access denied.");

        var type = dto.CriteriaType ?? (int)criteria.CriteriaType;
        var source = dto.DataSource ?? (int)criteria.DataSource;
        var op = dto.Operator ?? (int)criteria.Operator;
        var target = dto.TargetValue ?? criteria.TargetValue;
        var weight = dto.Weight ?? criteria.Weight;
        var validation = ValidateCriteria(type, source, op, target, weight);
        if (validation != null) return ApiResponse<GoalSuccessCriteriaDto>.ErrorResponse(validation);
        criteria.CriteriaType = (GoalSuccessCriteriaType)type;
        criteria.DataSource = (GoalCriteriaDataSource)source;
        criteria.Operator = (GoalCriteriaOperator)op;
        criteria.TargetValue = target;
        criteria.Weight = weight;
        if (dto.IsRequired.HasValue) criteria.IsRequired = dto.IsRequired.Value;
        if (dto.Description != null) criteria.Description = dto.Description;
        criteria.UpdatedAt = DateTime.UtcNow;
        _successCriteriaRepo.Update(criteria);
        await SynchronizeAndEvaluateGoalAsync(goal, null, doctorUserId, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<GoalSuccessCriteriaDto>.SuccessResponse(await BuildCriteriaDtoAsync(criteria, ct), "Success criterion updated successfully.");
    }

    public async Task<ApiResponse> DeleteSuccessCriteriaAsync(Guid criteriaId, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var criteria = await _successCriteriaRepo.GetByIdAsync(criteriaId, ct);
        if (criteria == null || criteria.IsDeleted)
            return ApiResponse.ErrorResponse("Success criterion not found.");
        var goal = await _goalRepo.GetByIdAsync(criteria.GoalId, ct);
        if (goal == null || !await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse.ErrorResponse("Access denied.");

        criteria.IsDeleted = true;
        criteria.UpdatedAt = DateTime.UtcNow;
        _successCriteriaRepo.Update(criteria);
        await SynchronizeAndEvaluateGoalAsync(goal, null, doctorUserId, ct);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse.SuccessResponse("Success criterion deleted successfully.");
    }

    public async Task<ApiResponse<SuccessCriteriaEvaluationDto>> EvaluateSuccessCriteriaAsync(Guid criteriaId, CreateSuccessCriteriaEvaluationDto dto, Guid? doctorUserId = null, CancellationToken ct = default)
    {
        var criteria = await _successCriteriaRepo.GetByIdAsync(criteriaId, ct);
        if (criteria == null || criteria.IsDeleted)
            return ApiResponse<SuccessCriteriaEvaluationDto>.ErrorResponse("Success criterion not found.");
        var goal = await _goalRepo.GetByIdAsync(criteria.GoalId, ct);
        if (goal == null || !await CanDoctorManageGoalAsync(goal, doctorUserId, ct))
            return ApiResponse<SuccessCriteriaEvaluationDto>.ErrorResponse("Access denied.");
        if (!dto.CurrentValue.HasValue)
            return ApiResponse<SuccessCriteriaEvaluationDto>.ErrorResponse("Current value is required for a manual criterion evaluation.");

        TreatmentSession? session = null;
        if (dto.TreatmentSessionId.HasValue)
        {
            session = await _sessionRepo.GetByIdAsync(dto.TreatmentSessionId.Value, ct);
            if (session == null || session.IsDeleted || session.TreatmentCaseId != goal.TreatmentCaseId)
                return ApiResponse<SuccessCriteriaEvaluationDto>.ErrorResponse("Treatment session does not belong to this treatment case.");
        }

        criteria.CurrentValue = dto.CurrentValue;
        criteria.UpdatedAt = DateTime.UtcNow;
        _successCriteriaRepo.Update(criteria);
        var evaluation = await AddCriteriaEvaluationAsync(criteria, session?.Id, dto.CurrentValue, doctorUserId, ct);
        await SynchronizeAndEvaluateGoalAsync(goal, session, doctorUserId, ct, synchronizeAutomaticCriteria: false);
        await _uow.SaveChangesAsync(ct);
        return ApiResponse<SuccessCriteriaEvaluationDto>.SuccessResponse(MapToCriteriaEvaluationDto(evaluation), "Success criterion evaluated successfully.");
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

        await SynchronizeGoalsForHomeworkAsync(assignment, null, ct);

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
        await SynchronizeGoalsForHomeworkAsync(assignment, null, ct);

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

        var entries = new List<MoodEntryDto>();

        // 1. Fetch EmotionJournal entries (Mood Journey)
        if (_journalRepo != null)
        {
            var journals = await _journalRepo.GetAllAsync(ct);
            var caseJournals = journals.Where(j => !j.IsDeleted && (
                j.TreatmentCaseId == caseId ||
                (!j.TreatmentCaseId.HasValue && j.PatientId == tc.PatientId && j.CreatedAt >= tc.StartDate && (tc.ActualEndDate == null || j.CreatedAt <= tc.ActualEndDate))
            )).ToList();

            foreach (var j in caseJournals)
            {
                int moodScore = j.MoodScale <= 5 ? j.MoodScale * 2 : j.MoodScale;
                int stressScore = j.StressScale <= 5 ? j.StressScale * 2 : j.StressScale;

                entries.Add(new MoodEntryDto
                {
                    Id = j.Id,
                    TreatmentCaseId = caseId,
                    PatientId = j.PatientId,
                    MoodScore = moodScore,
                    StressScore = stressScore,
                    Note = !string.IsNullOrWhiteSpace(j.Title) ? (string.IsNullOrWhiteSpace(j.Content) ? j.Title : $"{j.Title}: {j.Content}") : j.Content,
                    RecordedAt = j.CreatedAt
                });
            }
        }

        // 2. Fetch legacy MoodEntry entries
        var allMoods = await _moodRepo.GetAllAsync(ct);
        var caseMoods = allMoods.Where(m => !m.IsDeleted && (
            m.TreatmentCaseId == caseId ||
            (m.TreatmentCaseId == Guid.Empty && m.PatientId == tc.PatientId && m.RecordedAt >= tc.StartDate && (tc.ActualEndDate == null || m.RecordedAt <= tc.ActualEndDate))
        )).ToList();

        foreach (var m in caseMoods)
        {
            if (!entries.Any(e => e.Id == m.Id))
            {
                entries.Add(MapToMoodEntryDto(m));
            }
        }

        entries = entries.OrderByDescending(m => m.RecordedAt).ToList();
        return ApiResponse<List<MoodEntryDto>>.SuccessResponse(entries);
    }

    public async Task<ApiResponse<TreatmentCaseRiskDto>> GetCaseRiskAsync(Guid caseId, Guid doctorUserId, CancellationToken ct = default)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(caseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<TreatmentCaseRiskDto>.ErrorResponse("Treatment case not found.");

        var doctors = await _doctorRepo.GetAllAsync(ct);
        var doctor = doctors.FirstOrDefault(d => d.UserId == doctorUserId || d.Id == doctorUserId);
        if (doctor == null || (treatmentCase.DoctorId != doctor.UserId && treatmentCase.DoctorId != doctor.Id))
            return ApiResponse<TreatmentCaseRiskDto>.ErrorResponse("Access denied. Only the treating doctor can view attention risk.");

        return ApiResponse<TreatmentCaseRiskDto>.SuccessResponse(await BuildCaseRiskAsync(treatmentCase, ct));
    }

    public async Task<ApiResponse<List<TreatmentCaseFileDto>>> GetPatientFilesAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(caseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<List<TreatmentCaseFileDto>>.ErrorResponse("Treatment case not found.");

        if (!await ValidateUserAccessToCaseAsync(treatmentCase, requestingUserId, ct))
            return ApiResponse<List<TreatmentCaseFileDto>>.ErrorResponse("Access denied. You do not have permission to view patient files for this treatment case.");

        var files = (await _assignmentRepo.GetAllAsync(ct))
            .Where(a => !a.IsDeleted && a.TreatmentCaseId == caseId &&
                        !string.IsNullOrWhiteSpace(a.PatientSubmissionUrl))
            .OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt)
            .Select(a => new TreatmentCaseFileDto
            {
                Id = a.Id,
                TreatmentCaseId = caseId,
                TreatmentSessionId = a.TreatmentSessionId,
                HomeworkId = a.Id,
                FileName = GetFileName(a.PatientSubmissionUrl!, a.Title),
                FileUrl = a.PatientSubmissionUrl!,
                HomeworkTitle = a.Title,
                UploadedAt = a.SubmittedAt ?? a.CreatedAt
            })
            .ToList();

        return ApiResponse<List<TreatmentCaseFileDto>>.SuccessResponse(files);
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

        var moodResult = await GetMoodEntriesAsync(caseId, requestingUserId, ct);
        var moodEntries = moodResult.Success && moodResult.Data != null ? moodResult.Data : new List<MoodEntryDto>();

        var moodTrend = moodEntries.OrderBy(m => m.RecordedAt).TakeLast(10).Select(m => new MoodTrendItem
        {
            MoodScore = m.MoodScore,
            AnxietyScore = m.AnxietyScore,
            StressScore = m.StressScore,
            SleepQualityScore = m.SleepQualityScore,
            Note = m.Note,
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

    private async Task<GoalDetail> CreateGoalDetailEntityAsync(TreatmentGoal goal, CreateGoalDetailDto dto, CancellationToken ct)
    {
        var allDetails = await _goalDetailRepo.GetAllAsync(ct);
        var nextOrder = allDetails.Where(d => d.GoalId == goal.Id && !d.IsDeleted)
            .Select(d => d.OrderIndex).DefaultIfEmpty(-1).Max() + 1;
        return new GoalDetail
        {
            GoalId = goal.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Objective = dto.Objective,
            ExpectedOutcome = dto.ExpectedOutcome,
            OrderIndex = dto.OrderIndex ?? nextOrder,
            EstimatedSessions = dto.EstimatedSessions,
            Goal = goal
        };
    }

    private async Task ReplaceSessionGoalLinksAsync(TreatmentSession session, IEnumerable<Guid> detailIds, CancellationToken ct)
    {
        var requestedIds = detailIds.Distinct().ToList();
        var details = (await _goalDetailRepo.GetAllAsync(ct))
            .Where(d => requestedIds.Contains(d.Id) && !d.IsDeleted)
            .ToList();
        var caseGoalIds = (await _goalRepo.GetAllAsync(ct))
            .Where(g => g.TreatmentCaseId == session.TreatmentCaseId && !g.IsDeleted)
            .Select(g => g.Id).ToHashSet();
        if (details.Count != requestedIds.Count || details.Any(d => !caseGoalIds.Contains(d.GoalId)))
            throw new InvalidOperationException("Goal detail selection is not valid for this treatment case.");

        var existingLinks = (await _sessionGoalRepo.GetAllAsync(ct))
            .Where(link => link.TreatmentSessionId == session.Id && !link.IsDeleted)
            .ToList();
        foreach (var link in existingLinks)
            _sessionGoalRepo.Delete(link);

        for (var index = 0; index < details.Count; index++)
        {
            await _sessionGoalRepo.AddAsync(new TreatmentSessionGoal
            {
                TreatmentSessionId = session.Id,
                GoalDetailId = details[index].Id,
                OrderIndex = index,
                TreatmentSession = session,
                GoalDetail = details[index]
            }, ct);
        }
    }

    private async Task ReplaceSessionGoalLinksAsync(GoalDetail detail, IEnumerable<Guid> sessionIds, CancellationToken ct)
    {
        var requestedIds = sessionIds.Distinct().ToList();
        var sessions = (await _sessionRepo.GetAllAsync(ct))
            .Where(s => requestedIds.Contains(s.Id) && !s.IsDeleted)
            .ToList();
        var goal = await _goalRepo.GetByIdAsync(detail.GoalId, ct) ?? detail.Goal;
        if (goal == null || sessions.Count != requestedIds.Count || sessions.Any(s => s.TreatmentCaseId != goal.TreatmentCaseId))
            throw new InvalidOperationException("Treatment session selection is not valid for this goal detail.");

        var existingLinks = (await _sessionGoalRepo.GetAllAsync(ct))
            .Where(link => link.GoalDetailId == detail.Id && !link.IsDeleted)
            .ToList();
        foreach (var link in existingLinks)
            _sessionGoalRepo.Delete(link);

        for (var index = 0; index < sessions.Count; index++)
        {
            await _sessionGoalRepo.AddAsync(new TreatmentSessionGoal
            {
                TreatmentSessionId = sessions[index].Id,
                GoalDetailId = detail.Id,
                OrderIndex = index,
                TreatmentSession = sessions[index],
                GoalDetail = detail
            }, ct);
        }
    }

    private async Task<List<Guid>> ResolveGoalDetailIdsAsync(Guid treatmentCaseId, IEnumerable<Guid>? detailIds, IEnumerable<Guid>? legacyGoalIds, CancellationToken ct)
    {
        var requested = detailIds?.Distinct().ToList() ?? new List<Guid>();
        if (legacyGoalIds != null)
        {
            var legacyIds = legacyGoalIds.Distinct().ToHashSet();
            var goals = (await _goalRepo.GetAllAsync(ct))
                .Where(g => g.TreatmentCaseId == treatmentCaseId && legacyIds.Contains(g.Id) && !g.IsDeleted)
                .Select(g => g.Id).ToHashSet();
            requested.AddRange((await _goalDetailRepo.GetAllAsync(ct))
                .Where(d => goals.Contains(d.GoalId) && !d.IsDeleted)
                .Select(d => d.Id));
        }
        return requested.Distinct().ToList();
    }

    private async Task<bool> IsDetailLinkedToSessionAsync(Guid detailId, Guid sessionId, CancellationToken ct) =>
        (await _sessionGoalRepo.GetAllAsync(ct)).Any(link =>
            !link.IsDeleted && link.GoalDetailId == detailId && link.TreatmentSessionId == sessionId);

    private async Task SynchronizeGoalsForHomeworkAsync(TherapyAssignment assignment, Guid? evaluatedBy, CancellationToken ct)
    {
        if (!assignment.TreatmentSessionId.HasValue)
            return;
        var session = await _sessionRepo.GetByIdAsync(assignment.TreatmentSessionId.Value, ct);
        if (session != null && !session.IsDeleted)
            await SynchronizeGoalsForSessionAsync(session, evaluatedBy, ct);
    }

    private async Task SynchronizeGoalsForSessionAsync(TreatmentSession session, Guid? evaluatedBy, CancellationToken ct)
    {
        var links = (await _sessionGoalRepo.GetAllAsync(ct) ?? Enumerable.Empty<TreatmentSessionGoal>())
            .Where(link => link.TreatmentSessionId == session.Id && !link.IsDeleted).ToList();
        if (links.Count == 0)
            return;

        var detailIds = links.Select(link => link.GoalDetailId).ToHashSet();
        var goalIds = (await _goalDetailRepo.GetAllAsync(ct) ?? Enumerable.Empty<GoalDetail>())
            .Where(detail => detailIds.Contains(detail.Id) && !detail.IsDeleted)
            .Select(detail => detail.GoalId).Distinct().ToHashSet();
        var goals = (await _goalRepo.GetAllAsync(ct) ?? Enumerable.Empty<TreatmentGoal>())
            .Where(goal => goalIds.Contains(goal.Id) && !goal.IsDeleted).ToList();
        foreach (var goal in goals)
            await SynchronizeAndEvaluateGoalAsync(goal, session, evaluatedBy, ct);
    }

    private async Task<bool> CanDoctorManageGoalAsync(TreatmentGoal goal, Guid? doctorUserId, CancellationToken ct)
    {
        if (!doctorUserId.HasValue || doctorUserId.Value == Guid.Empty)
            return true;
        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return false;
        var doctor = (await _doctorRepo.GetAllAsync(ct))
            .FirstOrDefault(d => d.UserId == doctorUserId.Value || d.Id == doctorUserId.Value);
        return doctor != null && (treatmentCase.DoctorId == doctor.Id || treatmentCase.DoctorId == doctor.UserId);
    }

    private static string? ValidateCriteria(int criteriaType, int dataSource, int comparisonOperator, decimal? targetValue, decimal weight)
    {
        if (!Enum.IsDefined(typeof(GoalSuccessCriteriaType), criteriaType) ||
            !Enum.IsDefined(typeof(GoalCriteriaDataSource), dataSource) ||
            !Enum.IsDefined(typeof(GoalCriteriaOperator), comparisonOperator))
            return "Success criterion type, data source, or operator is invalid.";
        if (!targetValue.HasValue)
            return "Success criterion target value is required.";
        if (weight <= 0)
            return "Success criterion weight must be greater than zero.";
        return null;
    }

    private async Task<int> CalculateGoalProgressAsync(Guid goalId, int fallbackProgress, CancellationToken ct)
    {
        var details = (await _goalDetailRepo.GetAllAsync(ct)).Where(d => d.GoalId == goalId && !d.IsDeleted).ToList();
        return details.Count == 0 ? fallbackProgress : (int)Math.Round(details.Average(d => d.ProgressPercent));
    }

    private async Task SynchronizeAndEvaluateGoalAsync(TreatmentGoal goal, TreatmentSession? session, Guid? evaluatedBy, CancellationToken ct, bool synchronizeAutomaticCriteria = true)
    {
        goal.ProgressPercent = await CalculateGoalProgressAsync(goal.Id, goal.ProgressPercent, ct);
        var criteria = (await _successCriteriaRepo.GetAllAsync(ct)).Where(c => c.GoalId == goal.Id && !c.IsDeleted).ToList();

        if (synchronizeAutomaticCriteria)
        {
            var detailIds = (await _goalDetailRepo.GetAllAsync(ct)).Where(d => d.GoalId == goal.Id && !d.IsDeleted).Select(d => d.Id).ToHashSet();
            var linkedSessionIds = (await _sessionGoalRepo.GetAllAsync(ct))
                .Where(link => detailIds.Contains(link.GoalDetailId) && !link.IsDeleted)
                .Select(link => link.TreatmentSessionId).Distinct().ToHashSet();
            var sessions = (await _sessionRepo.GetAllAsync(ct)).Where(s => linkedSessionIds.Contains(s.Id) && !s.IsDeleted).ToList();
            var assignments = (await _assignmentRepo.GetAllAsync(ct))
                .Where(a => a.TreatmentSessionId.HasValue && linkedSessionIds.Contains(a.TreatmentSessionId.Value) && !a.IsDeleted).ToList();

            foreach (var criterion in criteria.Where(c => c.DataSource is GoalCriteriaDataSource.GoalProgress or GoalCriteriaDataSource.Homework or GoalCriteriaDataSource.Attendance))
            {
                decimal value = criterion.DataSource switch
                {
                    GoalCriteriaDataSource.GoalProgress => goal.ProgressPercent,
                    GoalCriteriaDataSource.Homework => assignments.Count == 0 ? 0 : Math.Round(assignments.Count(a => a.Status is HomeworkStatus.Submitted or HomeworkStatus.Reviewed) * 100m / assignments.Count, 2),
                    GoalCriteriaDataSource.Attendance => sessions.Count == 0 ? 0 : Math.Round(sessions.Count(s => s.Status == TreatmentSessionStatus.Completed) * 100m / sessions.Count, 2),
                    _ => 0
                };
                criterion.CurrentValue = value;
                criterion.UpdatedAt = DateTime.UtcNow;
                _successCriteriaRepo.Update(criterion);
                await AddCriteriaEvaluationAsync(criterion, session?.Id, value, evaluatedBy, ct);
            }
        }

        var requiredCriteria = criteria.Where(c => c.IsRequired).ToList();
        var allRequiredCriteriaPassed = requiredCriteria.Count > 0 && requiredCriteria.All(IsCriterionPassed);
        if (goal.Status != GoalStatus.Cancelled && goal.Status != GoalStatus.OnHold)
        {
            if (allRequiredCriteriaPassed)
            {
                goal.Status = GoalStatus.Achieved;
                goal.AchievedDate ??= DateTime.UtcNow;
            }
            else if (goal.ProgressPercent > 0 || criteria.Any(c => c.CurrentValue.HasValue))
            {
                goal.Status = GoalStatus.InProgress;
                goal.AchievedDate = null;
            }
            else if (goal.Status != GoalStatus.Draft)
            {
                goal.Status = GoalStatus.NotStarted;
            }
        }
        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            await RecalculateProgressAsync(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }
    }

    private static bool IsCriterionPassed(GoalSuccessCriteria criterion)
    {
        if (!criterion.CurrentValue.HasValue || !criterion.TargetValue.HasValue)
            return false;
        return criterion.Operator switch
        {
            GoalCriteriaOperator.GreaterThan => criterion.CurrentValue > criterion.TargetValue,
            GoalCriteriaOperator.GreaterThanOrEqual => criterion.CurrentValue >= criterion.TargetValue,
            GoalCriteriaOperator.LessThan => criterion.CurrentValue < criterion.TargetValue,
            GoalCriteriaOperator.LessThanOrEqual => criterion.CurrentValue <= criterion.TargetValue,
            GoalCriteriaOperator.Equal => criterion.CurrentValue == criterion.TargetValue,
            _ => false
        };
    }

    private async Task<SuccessCriteriaEvaluation> AddCriteriaEvaluationAsync(GoalSuccessCriteria criterion, Guid? sessionId, decimal? currentValue, Guid? evaluatedBy, CancellationToken ct)
    {
        var evaluation = new SuccessCriteriaEvaluation
        {
            SuccessCriteriaId = criterion.Id,
            TreatmentSessionId = sessionId,
            CurrentValue = currentValue,
            IsPassed = IsCriterionPassed(criterion),
            EvaluatedAt = DateTime.UtcNow,
            EvaluatedBy = evaluatedBy,
            SuccessCriteria = criterion
        };
        await _criteriaEvaluationRepo.AddAsync(evaluation, ct);
        return evaluation;
    }

    private async Task<TreatmentGoalDto> BuildGoalDtoAsync(TreatmentGoal goal, CancellationToken ct)
    {
        var dto = MapToGoalDto(goal);
        var details = (await _goalDetailRepo.GetAllAsync(ct)).Where(d => d.GoalId == goal.Id && !d.IsDeleted).OrderBy(d => d.OrderIndex).ToList();
        dto.Details = (await Task.WhenAll(details.Select(d => BuildGoalDetailDtoAsync(d, ct)))).ToList();
        var criteria = (await _successCriteriaRepo.GetAllAsync(ct)).Where(c => c.GoalId == goal.Id && !c.IsDeleted).ToList();
        dto.SuccessCriteria = (await Task.WhenAll(criteria.Select(c => BuildCriteriaDtoAsync(c, ct)))).ToList();
        dto.ProgressHistory = (await _goalProgressRepo.GetAllAsync(ct)).Where(p => p.GoalId == goal.Id && !p.IsDeleted)
            .OrderByDescending(p => p.RecordedAt).Select(MapToGoalProgressDto).ToList();
        return dto;
    }

    private async Task<GoalDetailDto> BuildGoalDetailDtoAsync(GoalDetail detail, CancellationToken ct)
    {
        var links = (await _sessionGoalRepo.GetAllAsync(ct)).Where(l => l.GoalDetailId == detail.Id && !l.IsDeleted).OrderBy(l => l.OrderIndex).ToList();
        return new GoalDetailDto
        {
            Id = detail.Id, GoalId = detail.GoalId, Title = detail.Title, Description = detail.Description,
            Objective = detail.Objective, ExpectedOutcome = detail.ExpectedOutcome, OrderIndex = detail.OrderIndex,
            ProgressPercent = detail.ProgressPercent, Status = (int)detail.Status, EstimatedSessions = detail.EstimatedSessions,
            CompletedDate = detail.CompletedDate, Sessions = links.Select(MapToSessionGoalDto).ToList()
        };
    }

    private async Task<GoalSuccessCriteriaDto> BuildCriteriaDtoAsync(GoalSuccessCriteria criteria, CancellationToken ct)
    {
        var evaluations = (await _criteriaEvaluationRepo.GetAllAsync(ct))
            .Where(e => e.SuccessCriteriaId == criteria.Id).OrderByDescending(e => e.EvaluatedAt).ToList();
        return new GoalSuccessCriteriaDto
        {
            Id = criteria.Id, GoalId = criteria.GoalId, CriteriaType = (int)criteria.CriteriaType,
            DataSource = (int)criteria.DataSource, Operator = (int)criteria.Operator, TargetValue = criteria.TargetValue,
            CurrentValue = criteria.CurrentValue, Weight = criteria.Weight, IsRequired = criteria.IsRequired,
            Description = criteria.Description, Evaluations = evaluations.Select(MapToCriteriaEvaluationDto).ToList()
        };
    }

    private static GoalDetailDto MapToGoalDetailDto(GoalDetail detail) => new()
    {
        Id = detail.Id, GoalId = detail.GoalId, Title = detail.Title, Description = detail.Description,
        Objective = detail.Objective, ExpectedOutcome = detail.ExpectedOutcome, OrderIndex = detail.OrderIndex,
        ProgressPercent = detail.ProgressPercent, Status = (int)detail.Status, EstimatedSessions = detail.EstimatedSessions,
        CompletedDate = detail.CompletedDate
    };

    private static TreatmentSessionGoalDto MapToSessionGoalDto(TreatmentSessionGoal link) => new()
    {
        Id = link.Id, TreatmentSessionId = link.TreatmentSessionId, GoalDetailId = link.GoalDetailId,
        OrderIndex = link.OrderIndex, PlannedActivity = link.PlannedActivity
    };

    private static SuccessCriteriaEvaluationDto MapToCriteriaEvaluationDto(SuccessCriteriaEvaluation evaluation) => new()
    {
        Id = evaluation.Id, SuccessCriteriaId = evaluation.SuccessCriteriaId,
        TreatmentSessionId = evaluation.TreatmentSessionId, CurrentValue = evaluation.CurrentValue,
        IsPassed = evaluation.IsPassed, EvaluatedAt = evaluation.EvaluatedAt, EvaluatedBy = evaluation.EvaluatedBy
    };

    private async Task CompleteCaseAndPackageAsync(TreatmentCase treatmentCase, CancellationToken ct)
    {
        treatmentCase.Status = TreatmentCaseStatus.Completed;
        treatmentCase.ActualEndDate ??= DateTime.UtcNow;
        treatmentCase.OverallProgressPercent = 100;

        var package = await _packageRepo.GetByIdAsync(treatmentCase.TreatmentPackageId, ct);
        if (package != null && !package.IsDeleted &&
            package.Status is TreatmentPackageStatus.Active or TreatmentPackageStatus.Accepted)
        {
            package.Status = TreatmentPackageStatus.Completed;
            package.RemainingSessions = 0;
            package.UpdatedAt = DateTime.UtcNow;
            _packageRepo.Update(package);
        }
    }

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
        if (treatmentCase.OverallProgressPercent >= 100 &&
            treatmentCase.Status is TreatmentCaseStatus.Active or TreatmentCaseStatus.OnHold)
        {
            await CompleteCaseAndPackageAsync(treatmentCase, ct);
        }
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

    private async Task<Guid> GetCanonicalPatientUserIdAsync(Guid patientId, CancellationToken ct)
    {
        var patient = (await _patientRepo.GetAllAsync(ct))
            .FirstOrDefault(p => p.Id == patientId || p.UserId == patientId);
        return patient?.UserId ?? patientId;
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
            PatientId = await GetCanonicalPatientUserIdAsync(entity.PatientId, ct),
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
            TreatmentPackageId = entity.TreatmentPackageId,
            DoctorId = entity.DoctorId,
            PatientId = await GetCanonicalPatientUserIdAsync(entity.PatientId, ct),
            CaseName = entity.CaseName,
            PackageNameSnapshot = entity.PackageNameSnapshot,
            PackageName = await GetPackageNameAsync(entity.TreatmentPackageId, ct),
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

    private async Task<TreatmentCaseRiskDto> BuildCaseRiskAsync(TreatmentCase treatmentCase, CancellationToken ct)
    {
        var patient = (await _patientRepo.GetAllAsync(ct))
            .FirstOrDefault(p => p.UserId == treatmentCase.PatientId || p.Id == treatmentCase.PatientId);
        var patientIds = new HashSet<Guid> { treatmentCase.PatientId };
        if (patient != null)
        {
            patientIds.Add(patient.Id);
            patientIds.Add(patient.UserId);
        }

        var result = new TreatmentCaseRiskDto
        {
            TreatmentCaseId = treatmentCase.Id,
            CaseName = treatmentCase.CaseName,
            PatientName = await GetUserNameAsync(treatmentCase.PatientId, ct)
        };

        var moodPoints = new List<(DateTime RecordedAt, decimal Score)>();
        var journalEntries = await _journalRepo.GetAllAsync(ct);
        moodPoints.AddRange(journalEntries
            .Where(j => !j.IsDeleted && j.IsShared &&
                        (j.TreatmentCaseId == treatmentCase.Id ||
                         (!j.TreatmentCaseId.HasValue && patientIds.Contains(j.PatientId) && j.CreatedAt >= treatmentCase.StartDate)))
            .Select(j => (j.CreatedAt, (decimal)(j.MoodScale <= 5 ? j.MoodScale * 2 : j.MoodScale))));

        var moodEntries = await _moodRepo.GetAllAsync(ct);
        moodPoints.AddRange(moodEntries
            .Where(m => !m.IsDeleted &&
                        (m.TreatmentCaseId == treatmentCase.Id ||
                         (m.TreatmentCaseId == Guid.Empty && patientIds.Contains(m.PatientId) && m.RecordedAt >= treatmentCase.StartDate)))
            .Select(m => (m.RecordedAt, (decimal)m.MoodScore)));

        var orderedMoods = moodPoints.OrderByDescending(m => m.RecordedAt).ToList();
        var recentMoods = orderedMoods.Take(7).Select(m => m.Score).ToList();
        var previousMoods = orderedMoods.Skip(7).Take(7).Select(m => m.Score).ToList();
        result.RecentAverageMood = recentMoods.Count > 0 ? Math.Round(recentMoods.Average(), 1) : null;
        result.PreviousAverageMood = previousMoods.Count > 0 ? Math.Round(previousMoods.Average(), 1) : null;

        if (result.RecentAverageMood is <= 3)
        {
            result.Factors.Add(new TreatmentRiskFactorDto
            {
                Code = "low_mood",
                Label = "Low recent mood",
                Severity = "High",
                Detail = $"Average mood is {result.RecentAverageMood:0.0}/10 across the most recent entries."
            });
        }
        else if (result.RecentAverageMood is <= 5)
        {
            result.Factors.Add(new TreatmentRiskFactorDto
            {
                Code = "low_mood",
                Label = "Low recent mood",
                Severity = "Medium",
                Detail = $"Average mood is {result.RecentAverageMood:0.0}/10 across the most recent entries."
            });
        }

        if (result.RecentAverageMood.HasValue && result.PreviousAverageMood.HasValue &&
            result.PreviousAverageMood.Value - result.RecentAverageMood.Value >= 2)
        {
            result.Factors.Add(new TreatmentRiskFactorDto
            {
                Code = "mood_declining",
                Label = "Mood trend declining",
                Severity = "Medium",
                Detail = $"Recent average declined from {result.PreviousAverageMood:0.0}/10 to {result.RecentAverageMood:0.0}/10."
            });
        }

        var sessions = (await _sessionRepo.GetAllAsync(ct))
            .Where(s => !s.IsDeleted && s.TreatmentCaseId == treatmentCase.Id)
            .ToList();
        var sessionIds = sessions.Select(s => s.Id).ToHashSet();
        var appointments = (await _appointmentRepo.GetAllAsync(ct))
            .Where(a => !a.IsDeleted && (a.TreatmentCaseId == treatmentCase.Id ||
                                         (a.TreatmentSessionId.HasValue && sessionIds.Contains(a.TreatmentSessionId.Value))))
            .OrderByDescending(a => a.AppointmentDate ?? a.CreatedAt)
            .ToList();

        result.ConsecutiveNoShows = appointments.TakeWhile(a => a.Status == AppointmentStatus.NoShow).Count();
        if (result.ConsecutiveNoShows >= 3)
        {
            result.Factors.Add(new TreatmentRiskFactorDto
            {
                Code = "repeated_no_show",
                Label = "Repeated missed appointments",
                Severity = "High",
                Detail = $"{result.ConsecutiveNoShows} consecutive treatment appointments were marked as no-show."
            });
        }
        else if (result.ConsecutiveNoShows >= 2)
        {
            result.Factors.Add(new TreatmentRiskFactorDto
            {
                Code = "repeated_no_show",
                Label = "Repeated missed appointments",
                Severity = "Medium",
                Detail = $"{result.ConsecutiveNoShows} consecutive treatment appointments were marked as no-show."
            });
        }

        var assessments = (await _psychRepo.GetAllAsync(ct))
            .Where(s => !s.IsDeleted &&
                        (s.TreatmentCaseId == treatmentCase.Id ||
                         (!s.TreatmentCaseId.HasValue && patientIds.Contains(s.PatientId) && s.CreatedAt >= treatmentCase.StartDate)))
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
        var latestAssessment = assessments.FirstOrDefault();
        if (latestAssessment != null)
        {
            var tests = _psychTestRepo == null
                ? new Dictionary<Guid, PsychometricTest>()
                : (await _psychTestRepo.GetAllAsync(ct)).Where(t => !t.IsDeleted).ToDictionary(t => t.Id);
            var testName = tests.TryGetValue(latestAssessment.TestId, out var test)
                ? test.TestType
                : "Psychometric assessment";
            result.LatestAssessment = $"{testName}: {latestAssessment.Interpretation}";
            result.LatestAssessmentAt = latestAssessment.CreatedAt;

            var interpretation = latestAssessment.Interpretation ?? string.Empty;
            var severity = interpretation.Contains("severe", StringComparison.OrdinalIgnoreCase) ||
                           interpretation.Contains("high", StringComparison.OrdinalIgnoreCase)
                ? "High"
                : interpretation.Contains("moderate", StringComparison.OrdinalIgnoreCase)
                    ? "Medium"
                    : null;
            if (severity != null)
            {
                result.Factors.Add(new TreatmentRiskFactorDto
                {
                    Code = "assessment_attention",
                    Label = "Assessment requires attention",
                    Severity = severity,
                    Detail = result.LatestAssessment
                });
            }
        }

        result.Score = result.Factors.Sum(f => f.Severity == "High" ? 50 : 25);
        result.Level = result.Factors.Any(f => f.Severity == "High")
            ? "High"
            : result.Factors.Any() ? "Medium" : "Low";
        return result;
    }

    private static string GetFileName(string fileUrl, string fallback)
    {
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            var name = Path.GetFileName(Uri.UnescapeDataString(uri.LocalPath));
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return string.IsNullOrWhiteSpace(fallback) ? "Homework submission" : fallback;
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
            TemplateId = g.TemplateId,
            CreatedByDoctorId = g.CreatedByDoctorId,
            Title = g.Title,
            Description = g.Description,
            Category = (int)g.Category,
            CategoryText = g.Category.ToString(),
            Priority = (int)g.Priority,
            OrderIndex = g.OrderIndex,
            Status = (int)g.Status,
            ProgressPercent = g.ProgressPercent,
            TargetValue = g.TargetValue,
            CurrentValue = g.CurrentValue,
            Unit = g.Unit,
            TargetDate = g.TargetDate,
            AchievedDate = g.AchievedDate,
            StartDate = g.StartDate,
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
            GoalDetailId = p.GoalDetailId,
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
