using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

/// <summary>
/// Service implementation for Treatment Case management.
/// Handles creation from package template, session tracking, goal management,
/// progress calculation, and timeline aggregation.
/// </summary>
public class TreatmentCaseService : ITreatmentCaseService
{
    private readonly IRepository<TreatmentCase> _caseRepo;
    private readonly IRepository<TreatmentSession> _sessionRepo;
    private readonly IRepository<TreatmentGoal> _goalRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<TherapyAssignment> _assignmentRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<ConsultationNote> _noteRepo;
    private readonly IRepository<EmotionJournal> _journalRepo;
    private readonly IRepository<PsychometricSubmission> _psychRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public TreatmentCaseService(
        IRepository<TreatmentCase> caseRepo,
        IRepository<TreatmentSession> sessionRepo,
        IRepository<TreatmentGoal> goalRepo,
        IRepository<TreatmentPackage> packageRepo,
        IRepository<TherapyAssignment> assignmentRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<ConsultationNote> noteRepo,
        IRepository<EmotionJournal> journalRepo,
        IRepository<PsychometricSubmission> psychRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _caseRepo = caseRepo;
        _sessionRepo = sessionRepo;
        _goalRepo = goalRepo;
        _packageRepo = packageRepo;
        _assignmentRepo = assignmentRepo;
        _appointmentRepo = appointmentRepo;
        _noteRepo = noteRepo;
        _journalRepo = journalRepo;
        _psychRepo = psychRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    // ==================== Treatment Case CRUD ====================

    public async Task<ApiResponse<TreatmentCaseDto>> CreateFromPackageAsync(CreateTreatmentCaseDto dto, CancellationToken ct)
    {
        var package = await _packageRepo.GetByIdAsync(dto.TreatmentPackageId, ct);
        if (package == null || package.IsDeleted)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment package not found.");

        if (package.Status != TreatmentPackageStatus.Active)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment package must be Active to create a case.");

        // Check if there's already an active case for this doctor-patient-package combo
        var allCases = await _caseRepo.GetAllAsync(ct);
        var existingActive = allCases.FirstOrDefault(c =>
            c.DoctorId == dto.DoctorId &&
            c.PatientId == dto.PatientId &&
            c.TreatmentPackageId == dto.TreatmentPackageId &&
            c.Status == TreatmentCaseStatus.Active &&
            !c.IsDeleted);

        if (existingActive != null)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("An active treatment case already exists for this patient with this package.");

        var entity = new TreatmentCase
        {
            TreatmentPackageId = dto.TreatmentPackageId,
            DoctorId = dto.DoctorId,
            PatientId = dto.PatientId,
            CaseName = package.Name,
            CaseDescription = package.Description,
            PrimaryConcern = dto.PrimaryConcern,
            TotalSessions = package.SessionQuantity,
            RemainingSessions = package.SessionQuantity,
            StartDate = DateTime.UtcNow,
            ExpectedEndDate = DateTime.UtcNow.AddDays(package.ValidityDays),
            Status = TreatmentCaseStatus.Active,
            TreatmentPackage = package,
            Doctor = package.Doctor,
            Patient = package.Patient!
        };

        await _caseRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct), "Treatment case created successfully.");
    }

    public async Task<ApiResponse<TreatmentCaseDto>> GetByIdAsync(Guid caseId, CancellationToken ct)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TreatmentCaseDto>.ErrorResponse("Treatment case not found.");

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct));
    }

    public async Task<ApiResponse<List<TreatmentCaseListDto>>> GetByDoctorAsync(Guid doctorUserId, CancellationToken ct)
    {
        var all = await _caseRepo.GetAllAsync(ct);
        var cases = all
            .Where(c => c.DoctorId == doctorUserId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var result = new List<TreatmentCaseListDto>();
        foreach (var c in cases)
            result.Add(await MapToListDtoAsync(c, ct));

        return ApiResponse<List<TreatmentCaseListDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<List<TreatmentCaseListDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct)
    {
        var all = await _caseRepo.GetAllAsync(ct);
        var cases = all
            .Where(c => c.PatientId == patientUserId && !c.IsDeleted)
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

        if (dto.CaseDescription != null) entity.CaseDescription = dto.CaseDescription;
        if (dto.PrimaryConcern != null) entity.PrimaryConcern = dto.PrimaryConcern;
        if (dto.Status.HasValue) entity.Status = (TreatmentCaseStatus)dto.Status.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        _caseRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentCaseDto>.SuccessResponse(await MapToCaseDtoAsync(entity, ct), "Treatment case updated successfully.");
    }

    public async Task<ApiResponse> CloseAsync(Guid caseId, CloseTreatmentCaseDto dto, CancellationToken ct)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse.ErrorResponse("Treatment case not found.");

        if (entity.Status != TreatmentCaseStatus.Active && entity.Status != TreatmentCaseStatus.OnHold)
            return ApiResponse.ErrorResponse("Only active or on-hold cases can be closed.");

        entity.Status = (TreatmentCaseStatus)dto.CloseStatus;
        entity.ClosureNote = dto.ClosureNote;
        entity.ActualEndDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        // If completed, set progress to 100%
        if (entity.Status == TreatmentCaseStatus.Completed)
            entity.OverallProgressPercent = 100;

        _caseRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Treatment case closed successfully.");
    }

    // ==================== Sessions ====================

    public async Task<ApiResponse<TreatmentSessionDto>> CreateSessionAsync(CreateSessionDto dto, CancellationToken ct)
    {
        var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId, ct);
        if (treatmentCase == null || treatmentCase.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Treatment case not found.");

        if (treatmentCase.Status != TreatmentCaseStatus.Active)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Cannot add sessions to a non-active case.");

        if (treatmentCase.RemainingSessions <= 0)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("No remaining sessions in this treatment case.");

        // Determine session number
        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var existingSessions = allSessions.Where(s => s.TreatmentCaseId == dto.TreatmentCaseId && !s.IsDeleted).ToList();
        var sessionNumber = existingSessions.Count + 1;

        var session = new TreatmentSession
        {
            TreatmentCaseId = dto.TreatmentCaseId,
            AppointmentId = dto.AppointmentId,
            SessionNumber = sessionNumber,
            Status = TreatmentSessionStatus.Scheduled,
            TreatmentCase = treatmentCase
        };

        await _sessionRepo.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), "Session created successfully.");
    }

    public async Task<ApiResponse<TreatmentSessionDto>> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.IsDeleted)
            return ApiResponse<TreatmentSessionDto>.ErrorResponse("Session not found.");

        session.SessionSummary = dto.SessionSummary;
        session.TherapistNotes = dto.TherapistNotes;
        session.PatientFeedback = dto.PatientFeedback;
        session.HomeworkAssigned = dto.HomeworkAssigned;
        session.MoodBefore = dto.MoodBefore;
        session.MoodAfter = dto.MoodAfter;
        session.Status = TreatmentSessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;

        _sessionRepo.Update(session);

        // Update parent case counters
        var treatmentCase = await _caseRepo.GetByIdAsync(session.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            treatmentCase.CompletedSessions++;
            treatmentCase.RemainingSessions = Math.Max(0, treatmentCase.RemainingSessions - 1);
            RecalculateProgress(treatmentCase, ct);
            treatmentCase.UpdatedAt = DateTime.UtcNow;
            _caseRepo.Update(treatmentCase);

            // Auto-close if all sessions completed
            if (treatmentCase.RemainingSessions <= 0)
            {
                treatmentCase.Status = TreatmentCaseStatus.Completed;
                treatmentCase.ActualEndDate = DateTime.UtcNow;
                treatmentCase.OverallProgressPercent = 100;
            }
        }

        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentSessionDto>.SuccessResponse(MapToSessionDto(session), "Session completed successfully.");
    }

    public async Task<ApiResponse<List<TreatmentSessionDto>>> GetSessionsByCaseAsync(Guid caseId, CancellationToken ct)
    {
        var all = await _sessionRepo.GetAllAsync(ct);
        var sessions = all
            .Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted)
            .OrderBy(s => s.SessionNumber)
            .Select(MapToSessionDto)
            .ToList();

        return ApiResponse<List<TreatmentSessionDto>>.SuccessResponse(sessions);
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
            Title = dto.Title,
            Description = dto.Description,
            Priority = (GoalPriority)dto.Priority,
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

        if (dto.Description != null) goal.Description = dto.Description;
        if (dto.Priority.HasValue) goal.Priority = (GoalPriority)dto.Priority.Value;
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
        if (dto.DoctorNotes != null) goal.DoctorNotes = dto.DoctorNotes;

        goal.UpdatedAt = DateTime.UtcNow;
        _goalRepo.Update(goal);

        // Recalculate case progress
        var treatmentCase = await _caseRepo.GetByIdAsync(goal.TreatmentCaseId, ct);
        if (treatmentCase != null)
        {
            RecalculateProgress(treatmentCase, ct);
            _caseRepo.Update(treatmentCase);
        }

        await _uow.SaveChangesAsync(ct);

        return ApiResponse<TreatmentGoalDto>.SuccessResponse(MapToGoalDto(goal), "Goal updated successfully.");
    }

    public async Task<ApiResponse<List<TreatmentGoalDto>>> GetGoalsByCaseAsync(Guid caseId, CancellationToken ct)
    {
        var all = await _goalRepo.GetAllAsync(ct);
        var goals = all
            .Where(g => g.TreatmentCaseId == caseId && !g.IsDeleted)
            .OrderByDescending(g => g.Priority)
            .ThenBy(g => g.CreatedAt)
            .Select(MapToGoalDto)
            .ToList();

        return ApiResponse<List<TreatmentGoalDto>>.SuccessResponse(goals);
    }

    // ==================== Progress & Timeline ====================

    public async Task<ApiResponse<TreatmentProgressDto>> GetProgressAsync(Guid caseId, CancellationToken ct)
    {
        var entity = await _caseRepo.GetByIdAsync(caseId, ct);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<TreatmentProgressDto>.ErrorResponse("Treatment case not found.");

        var allSessions = await _sessionRepo.GetAllAsync(ct);
        var sessions = allSessions.Where(s => s.TreatmentCaseId == caseId && !s.IsDeleted).ToList();
        var completedSessions = sessions.Count(s => s.Status == TreatmentSessionStatus.Completed);

        var allGoals = await _goalRepo.GetAllAsync(ct);
        var goals = allGoals.Where(g => g.TreatmentCaseId == caseId && !g.IsDeleted).ToList();
        var achievedGoals = goals.Count(g => g.Status == GoalStatus.Achieved);

        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        var assignments = allAssignments.Where(a => a.TreatmentCaseId == caseId && !a.IsDeleted).ToList();
        var completedAssignments = assignments.Count(a => a.Status >= 1); // Submitted or Reviewed

        var sessionProgress = entity.TotalSessions > 0 ? (completedSessions * 100 / entity.TotalSessions) : 0;
        var goalProgress = goals.Count > 0 ? (achievedGoals * 100 / goals.Count) : 0;
        var assignmentProgress = assignments.Count > 0 ? (completedAssignments * 100 / assignments.Count) : 0;

        // Mood trend from completed sessions
        var moodTrend = sessions
            .Where(s => s.Status == TreatmentSessionStatus.Completed && (s.MoodBefore.HasValue || s.MoodAfter.HasValue))
            .OrderBy(s => s.SessionNumber)
            .TakeLast(10)
            .Select(s => new MoodTrendItem
            {
                SessionNumber = s.SessionNumber,
                MoodBefore = s.MoodBefore,
                MoodAfter = s.MoodAfter,
                Date = s.UpdatedAt ?? s.CreatedAt
            })
            .ToList();

        var daysElapsed = (DateTime.UtcNow - entity.StartDate).Days;
        int? daysRemaining = entity.ExpectedEndDate.HasValue
            ? Math.Max(0, (entity.ExpectedEndDate.Value - DateTime.UtcNow).Days)
            : null;

        var progress = new TreatmentProgressDto
        {
            CaseId = entity.Id,
            CaseName = entity.CaseName,
            OverallProgressPercent = entity.OverallProgressPercent,
            TotalSessions = entity.TotalSessions,
            CompletedSessions = completedSessions,
            SessionProgressPercent = sessionProgress,
            TotalGoals = goals.Count,
            AchievedGoals = achievedGoals,
            GoalProgressPercent = goalProgress,
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

    public async Task<ApiResponse<List<TreatmentTimelineDto>>> GetTimelineAsync(Guid caseId, CancellationToken ct)
    {
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
                Title = $"Session #{s.SessionNumber}",
                Description = s.SessionSummary ?? "Scheduled",
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
                Description = $"Progress: {g.ProgressPercent}%",
                Status = g.Status.ToString(),
                IconCss = "bi-bullseye"
            });
        }

        // Assignments (Homework)
        var allAssignments = await _assignmentRepo.GetAllAsync(ct);
        foreach (var a in allAssignments.Where(a => a.TreatmentCaseId == caseId && !a.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = a.Id,
                EventDate = a.SubmittedAt ?? a.CreatedAt,
                EventType = "Assignment",
                Title = a.Title,
                Description = a.Description,
                Status = a.Status switch { 0 => "Pending", 1 => "Submitted", 2 => "Reviewed", _ => "Unknown" },
                IconCss = "bi-journal-check"
            });
        }

        // Emotion Journals
        var allJournals = await _journalRepo.GetAllAsync(ct);
        foreach (var j in allJournals.Where(j => j.TreatmentCaseId == caseId && !j.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = j.Id,
                EventDate = j.CreatedAt,
                EventType = "Mood",
                Title = j.Title,
                Description = $"Mood: {j.MoodScale}/5 | Stress: {j.StressScale}/5",
                Status = j.MoodScale >= 4 ? "Positive" : j.MoodScale <= 2 ? "Negative" : "Neutral",
                IconCss = "bi-emoji-smile"
            });
        }

        // Psychometric Submissions
        var allPsych = await _psychRepo.GetAllAsync(ct);
        foreach (var p in allPsych.Where(p => p.TreatmentCaseId == caseId && !p.IsDeleted))
        {
            timeline.Add(new TreatmentTimelineDto
            {
                Id = p.Id,
                EventDate = p.CreatedAt,
                EventType = "Assessment",
                Title = $"Assessment (Score: {p.TotalScore})",
                Description = p.Interpretation,
                Status = "Completed",
                IconCss = "bi-clipboard2-pulse"
            });
        }

        // Sort by date descending
        var sorted = timeline.OrderByDescending(t => t.EventDate).ToList();
        return ApiResponse<List<TreatmentTimelineDto>>.SuccessResponse(sorted);
    }

    // ==================== Private Helpers ====================

    private void RecalculateProgress(TreatmentCase treatmentCase, CancellationToken ct)
    {
        // Formula: 40% sessions + 40% goals + 20% homework
        var sessionWeight = 40;
        var goalWeight = 40;
        var homeworkWeight = 20;

        var sessionPercent = treatmentCase.TotalSessions > 0
            ? (treatmentCase.CompletedSessions * 100 / treatmentCase.TotalSessions)
            : 0;

        // For goals and homework, we use the available data from entity navigation
        // This is a simplified calculation - full version would query repos
        treatmentCase.OverallProgressPercent = Math.Min(100,
            (sessionPercent * sessionWeight / 100));

        treatmentCase.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        return user?.FullName;
    }

    private async Task<TreatmentCaseDto> MapToCaseDtoAsync(TreatmentCase entity, CancellationToken ct)
    {
        // Get aggregated counts
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
            DoctorName = await GetUserNameAsync(entity.DoctorId, ct),
            PatientName = await GetUserNameAsync(entity.PatientId, ct),
            GoalCount = goals.Count,
            AchievedGoalCount = goals.Count(g => g.Status == GoalStatus.Achieved),
            AssignmentCount = assignments.Count,
            CompletedAssignmentCount = assignments.Count(a => a.Status >= 1)
        };
    }

    private async Task<TreatmentCaseListDto> MapToListDtoAsync(TreatmentCase entity, CancellationToken ct)
    {
        return new TreatmentCaseListDto
        {
            Id = entity.Id,
            CaseName = entity.CaseName,
            PatientName = await GetUserNameAsync(entity.PatientId, ct),
            DoctorName = await GetUserNameAsync(entity.DoctorId, ct),
            TotalSessions = entity.TotalSessions,
            CompletedSessions = entity.CompletedSessions,
            OverallProgressPercent = entity.OverallProgressPercent,
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
            SessionSummary = s.SessionSummary,
            TherapistNotes = s.TherapistNotes,
            PatientFeedback = s.PatientFeedback,
            HomeworkAssigned = s.HomeworkAssigned,
            MoodBefore = s.MoodBefore,
            MoodAfter = s.MoodAfter,
            Status = (int)s.Status,
            CreatedAt = s.CreatedAt
        };
    }

    private static TreatmentGoalDto MapToGoalDto(TreatmentGoal g)
    {
        return new TreatmentGoalDto
        {
            Id = g.Id,
            TreatmentCaseId = g.TreatmentCaseId,
            Title = g.Title,
            Description = g.Description,
            Priority = (int)g.Priority,
            Status = (int)g.Status,
            ProgressPercent = g.ProgressPercent,
            TargetDate = g.TargetDate,
            AchievedDate = g.AchievedDate,
            DoctorNotes = g.DoctorNotes,
            CreatedAt = g.CreatedAt
        };
    }
}
