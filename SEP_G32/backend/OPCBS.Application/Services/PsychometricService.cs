using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OPCBS.Application.DTOs.Psychometric;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class PsychometricService : IPsychometricService
{
    private readonly IRepository<PsychometricTest> _testRepo;
    private readonly IRepository<PsychometricQuestion> _questionRepo;
    private readonly IRepository<PsychometricSubmission> _submissionRepo;
    private readonly IRepository<PsychometricAnswer> _answerRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<TreatmentCase>? _caseRepo;
    private readonly IUnitOfWork _uow;

    public PsychometricService(
        IRepository<PsychometricTest> testRepo,
        IRepository<PsychometricQuestion> questionRepo,
        IRepository<PsychometricSubmission> submissionRepo,
        IRepository<PsychometricAnswer> answerRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        IRepository<TreatmentCase>? caseRepo = null)
    {
        _testRepo = testRepo;
        _questionRepo = questionRepo;
        _submissionRepo = submissionRepo;
        _answerRepo = answerRepo;
        _patientRepo = patientRepo;
        _appointmentRepo = appointmentRepo;
        _userRepo = userRepo;
        _uow = uow;
        _caseRepo = caseRepo;
    }

    public async Task<ApiResponse<List<PsychometricTestDto>>> GetTestsAsync(CancellationToken ct = default)
    {
        var tests = await _testRepo.GetAllAsync(ct);
        var questions = await _questionRepo.GetAllAsync(ct);
        var submissions = await _submissionRepo.GetAllAsync(ct);
        var users = await _userRepo.GetAllAsync(ct);
        var userDict = users.ToDictionary(u => u.Id, u => u.FullName);

        var qCounts = questions.Where(q => !q.IsDeleted).GroupBy(q => q.TestId).ToDictionary(g => g.Key, g => g.Count());
        var sCounts = submissions.Where(s => !s.IsDeleted).GroupBy(s => s.TestId).ToDictionary(g => g.Key, g => g.Count());

        // The public library contains system templates only. Doctor-owned assessments
        // are returned by GetDoctorAssessmentsOverviewAsync for their owner.
        var dtos = tests.Where(t => !t.IsDeleted && t.IsActive && !t.DoctorId.HasValue).Select(t => new PsychometricTestDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            TestType = t.TestType,
            Category = !string.IsNullOrWhiteSpace(t.Category) ? t.Category : (t.TestType == "PHQ9" ? "Depression" : (t.TestType == "DASS21" ? "Depression & Anxiety" : "General Wellbeing")),
            Purpose = !string.IsNullOrWhiteSpace(t.Purpose) ? t.Purpose : (t.TestType == "PHQ9" ? "Depression Screening" : (t.TestType == "DASS21" ? "Depression, Anxiety & Stress" : "Psychological Assessment")),
            SourceUrl = !string.IsNullOrWhiteSpace(t.SourceUrl) ? t.SourceUrl : (t.TestType == "DASS21" || t.Title.Contains("DASS") ? "http://www2.psy.unsw.edu.au/dass/" : null),
            DoctorId = t.DoctorId,
            DoctorName = t.DoctorId.HasValue && userDict.TryGetValue(t.DoctorId.Value, out var dName) ? dName : null,
            ScoreRangesJson = t.ScoreRangesJson,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            QuestionCount = qCounts.GetValueOrDefault(t.Id, 0),
            SubmissionCount = sCounts.GetValueOrDefault(t.Id, 0)
        }).OrderBy(t => t.Title).ToList();

        return ApiResponse<List<PsychometricTestDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<PsychometricTestDetailDto>> GetTestByIdAsync(
        Guid testId,
        Guid? requestingUserId = null,
        bool canManageSystemTemplates = false,
        CancellationToken ct = default)
    {
        var test = await _testRepo.GetByIdAsync(testId, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<PsychometricTestDetailDto>.ErrorResponse("Psychometric test not found.");

        if (!await CanAccessTestAsync(test, requestingUserId, canManageSystemTemplates, ct))
            return ApiResponse<PsychometricTestDetailDto>.ErrorResponse("You do not have access to this assessment template.");

        var questions = (await _questionRepo.GetAllAsync(ct))
            .Where(q => q.TestId == testId && !q.IsDeleted)
            .OrderBy(q => q.QuestionNumber)
            .Select(q => new PsychometricQuestionDto
            {
                Id = q.Id,
                TestId = q.TestId,
                QuestionText = q.QuestionText,
                QuestionNumber = q.QuestionNumber,
                Category = q.Category,
                QuestionType = !string.IsNullOrWhiteSpace(q.QuestionType) ? q.QuestionType : "Rating1To5",
                OptionsJson = q.OptionsJson
            }).ToList();

        var submissions = await _submissionRepo.GetAllAsync(ct);
        int submissionCount = submissions.Count(s => s.TestId == testId && !s.IsDeleted);

        string? doctorName = null;
        if (test.DoctorId.HasValue)
        {
            var docUser = await _userRepo.GetByIdAsync(test.DoctorId.Value, ct);
            doctorName = docUser?.FullName;
        }

        var dto = new PsychometricTestDetailDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            TestType = test.TestType,
            Category = !string.IsNullOrWhiteSpace(test.Category) ? test.Category : (test.TestType == "PHQ9" ? "Depression" : (test.TestType == "DASS21" ? "Depression & Anxiety" : "General Wellbeing")),
            Purpose = !string.IsNullOrWhiteSpace(test.Purpose) ? test.Purpose : (test.TestType == "PHQ9" ? "Depression Screening" : (test.TestType == "DASS21" ? "Depression, Anxiety & Stress" : "Psychological Assessment")),
            SourceUrl = !string.IsNullOrWhiteSpace(test.SourceUrl) ? test.SourceUrl : (test.TestType == "DASS21" || test.Title.Contains("DASS") ? "http://www2.psy.unsw.edu.au/dass/" : null),
            DoctorId = test.DoctorId,
            DoctorName = doctorName,
            ScoreRangesJson = test.ScoreRangesJson,
            IsActive = test.IsActive,
            CreatedAt = test.CreatedAt,
            QuestionCount = questions.Count,
            SubmissionCount = submissionCount,
            Questions = questions
        };

        return ApiResponse<PsychometricTestDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PsychometricTestDto>> CreateTestAsync(CreatePsychometricTestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<PsychometricTestDto>.ErrorResponse("Test title is required.");

        if (string.IsNullOrWhiteSpace(dto.TestType))
            return ApiResponse<PsychometricTestDto>.ErrorResponse("Test code / type is required.");

        if (dto.Questions == null || !dto.Questions.Any(q => !string.IsNullOrWhiteSpace(q.QuestionText)))
            return ApiResponse<PsychometricTestDto>.ErrorResponse("At least one question is required.");

        var test = new PsychometricTest
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            TestType = dto.TestType.Trim().ToUpper(),
            Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category.Trim() : "General Wellbeing",
            Purpose = dto.Purpose?.Trim(),
            SourceUrl = !string.IsNullOrWhiteSpace(dto.SourceUrl) ? dto.SourceUrl.Trim() : (dto.TestType.Trim().ToUpper() == "DASS21" || dto.Title.Contains("DASS", StringComparison.OrdinalIgnoreCase) ? "http://www2.psy.unsw.edu.au/dass/" : null),
            DoctorId = dto.DoctorId,
            ScoreRangesJson = dto.ScoreRangesJson ?? "[]"
        };

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _testRepo.AddAsync(test, ct);
            await _uow.SaveChangesAsync(ct);

            int qNum = 1;
            foreach (var q in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                    continue;

                var questionEntity = new PsychometricQuestion
                {
                    TestId = test.Id,
                    QuestionText = q.QuestionText.Trim(),
                    QuestionNumber = q.QuestionNumber > 0 ? q.QuestionNumber : qNum++,
                    Category = string.IsNullOrWhiteSpace(q.Category) ? test.Category : q.Category.Trim(),
                    QuestionType = !string.IsNullOrWhiteSpace(q.QuestionType) ? q.QuestionType : "Rating1To5",
                    OptionsJson = q.OptionsJson,
                    Test = test
                };
                await _questionRepo.AddAsync(questionEntity, ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            return ApiResponse<PsychometricTestDto>.ErrorResponse($"Failed to create assessment: {ex.Message}");
        }

        var resultDto = new PsychometricTestDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            TestType = test.TestType,
            Category = test.Category,
            Purpose = test.Purpose,
            SourceUrl = test.SourceUrl,
            DoctorId = test.DoctorId,
            CreatedAt = test.CreatedAt,
            QuestionCount = dto.Questions.Count(q => !string.IsNullOrWhiteSpace(q.QuestionText)),
            SubmissionCount = 0
        };

        return ApiResponse<PsychometricTestDto>.SuccessResponse(resultDto);
    }

    public async Task<ApiResponse<PsychometricTestDto>> CreateCustomTestAsync(CreatePsychometricTestDto dto, Guid doctorUserId, CancellationToken ct = default)
    {
        dto.DoctorId = doctorUserId;
        if (string.IsNullOrWhiteSpace(dto.TestType) || dto.TestType == "CUSTOM")
        {
            dto.TestType = "CUSTOM_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
        if (string.IsNullOrWhiteSpace(dto.Category))
        {
            dto.Category = "General Wellbeing";
        }
        if (string.IsNullOrWhiteSpace(dto.ScoreRangesJson))
        {
            dto.ScoreRangesJson = "[]";
        }
        return await CreateTestAsync(dto, ct);
    }

    public async Task<ApiResponse<PsychometricTestDto>> CloneCustomTestAsync(
        Guid sourceTestId,
        UpdatePsychometricTestDto dto,
        Guid doctorUserId,
        CancellationToken ct = default)
    {
        var source = await _testRepo.GetByIdAsync(sourceTestId, ct);
        if (source == null || source.IsDeleted || !source.IsActive)
            return ApiResponse<PsychometricTestDto>.ErrorResponse("Assessment template not found.");

        if (source.DoctorId.HasValue && source.DoctorId.Value != doctorUserId)
            return ApiResponse<PsychometricTestDto>.ErrorResponse("You cannot customize another doctor's assessment template.");

        var createDto = new CreatePsychometricTestDto
        {
            Title = dto.Title,
            Description = dto.Description,
            TestType = string.IsNullOrWhiteSpace(dto.TestType) || !dto.TestType.StartsWith("CUSTOM", StringComparison.OrdinalIgnoreCase)
                ? "CUSTOM_" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
                : dto.TestType,
            Category = dto.Category,
            Purpose = dto.Purpose,
            SourceUrl = dto.SourceUrl ?? source.SourceUrl,
            DoctorId = doctorUserId,
            ScoreRangesJson = dto.ScoreRangesJson ?? source.ScoreRangesJson ?? "[]",
            Questions = dto.Questions
        };

        return await CreateCustomTestAsync(createDto, doctorUserId, ct);
    }

    public async Task<ApiResponse<PsychometricTestDto>> UpdateTestAsync(
        Guid id,
        UpdatePsychometricTestDto dto,
        Guid requestingUserId,
        bool canManageSystemTemplates = false,
        CancellationToken ct = default)
    {
        var test = await _testRepo.GetByIdAsync(id, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<PsychometricTestDto>.ErrorResponse("Psychometric test not found.");

        if (test.DoctorId.HasValue)
        {
            if (test.DoctorId.Value != requestingUserId)
                return ApiResponse<PsychometricTestDto>.ErrorResponse("You can only edit your own custom assessments.");
        }
        else if (!canManageSystemTemplates)
        {
            return ApiResponse<PsychometricTestDto>.ErrorResponse("System templates are read-only. Create a private customized copy instead.");
        }

        var hasSubmissions = (await _submissionRepo.GetAllAsync(ct)).Any(s => s.TestId == id && !s.IsDeleted);
        if (hasSubmissions)
            return ApiResponse<PsychometricTestDto>.ErrorResponse("This assessment has patient submissions and is immutable. Create a new private version instead.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<PsychometricTestDto>.ErrorResponse("Test title is required.");

        test.Title = dto.Title.Trim();
        test.Description = dto.Description?.Trim();
        test.TestType = dto.TestType.Trim().ToUpper();
        test.Category = dto.Category?.Trim();
        test.Purpose = dto.Purpose?.Trim();
        test.SourceUrl = !string.IsNullOrWhiteSpace(dto.SourceUrl) ? dto.SourceUrl.Trim() : (dto.TestType.Trim().ToUpper() == "DASS21" || dto.Title.Contains("DASS", StringComparison.OrdinalIgnoreCase) ? "http://www2.psy.unsw.edu.au/dass/" : null);
        test.ScoreRangesJson = dto.ScoreRangesJson;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            _testRepo.Update(test);

            var existingQuestions = (await _questionRepo.GetAllAsync(ct)).Where(q => q.TestId == id && !q.IsDeleted).ToList();
            foreach (var eq in existingQuestions)
            {
                eq.IsDeleted = true;
                _questionRepo.Update(eq);
            }

            int qNum = 1;
            foreach (var q in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                    continue;

                var questionEntity = new PsychometricQuestion
                {
                    TestId = test.Id,
                    QuestionText = q.QuestionText.Trim(),
                    QuestionNumber = q.QuestionNumber > 0 ? q.QuestionNumber : qNum++,
                    Category = string.IsNullOrWhiteSpace(q.Category) ? null : q.Category.Trim(),
                    QuestionType = !string.IsNullOrWhiteSpace(q.QuestionType) ? q.QuestionType : "Rating1To5",
                    OptionsJson = q.OptionsJson,
                    Test = test
                };
                await _questionRepo.AddAsync(questionEntity, ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var resultDto = new PsychometricTestDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            TestType = test.TestType,
            Category = test.Category,
            Purpose = test.Purpose,
            DoctorId = test.DoctorId,
            CreatedAt = test.CreatedAt,
            QuestionCount = dto.Questions.Count(q => !string.IsNullOrWhiteSpace(q.QuestionText)),
            SubmissionCount = 0
        };

        return ApiResponse<PsychometricTestDto>.SuccessResponse(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteTestAsync(
        Guid id,
        Guid requestingUserId,
        bool canManageSystemTemplates = false,
        CancellationToken ct = default)
    {
        var test = await _testRepo.GetByIdAsync(id, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Psychometric test not found.");

        if (test.DoctorId.HasValue)
        {
            if (test.DoctorId.Value != requestingUserId)
                return ApiResponse<bool>.ErrorResponse("You can only delete your own custom assessments.");
        }
        else if (!canManageSystemTemplates)
        {
            return ApiResponse<bool>.ErrorResponse("System templates cannot be deleted by doctors.");
        }

        if ((await _submissionRepo.GetAllAsync(ct)).Any(s => s.TestId == id && !s.IsDeleted))
            return ApiResponse<bool>.ErrorResponse("This assessment has patient submissions and cannot be deleted. Deactivate or create a new version instead.");

        test.IsDeleted = true;
        _testRepo.Update(test);

        var questions = (await _questionRepo.GetAllAsync(ct)).Where(q => q.TestId == id && !q.IsDeleted).ToList();
        foreach (var q in questions)
        {
            q.IsDeleted = true;
            _questionRepo.Update(q);
        }

        await _uow.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Test deleted successfully.");
    }

    public async Task<ApiResponse<List<PsychometricQuestionDto>>> GetQuestionsAsync(
        Guid testId,
        Guid? requestingUserId = null,
        bool canManageSystemTemplates = false,
        CancellationToken ct = default)
    {
        var test = await _testRepo.GetByIdAsync(testId, ct);
        if (test == null || test.IsDeleted ||
            !await CanAccessTestAsync(test, requestingUserId, canManageSystemTemplates, ct))
            return ApiResponse<List<PsychometricQuestionDto>>.ErrorResponse("Assessment questions are not available.");

        var questions = await _questionRepo.GetAllAsync(ct);
        var filtered = questions
            .Where(q => q.TestId == testId && !q.IsDeleted)
            .OrderBy(q => q.QuestionNumber)
            .Select(q => new PsychometricQuestionDto
            {
                Id = q.Id,
                TestId = q.TestId,
                QuestionText = q.QuestionText,
                QuestionNumber = q.QuestionNumber,
                Category = q.Category,
                QuestionType = !string.IsNullOrWhiteSpace(q.QuestionType) ? q.QuestionType : "Rating1To5",
                OptionsJson = q.OptionsJson
            }).ToList();

        return ApiResponse<List<PsychometricQuestionDto>>.SuccessResponse(filtered);
    }

    private async Task<bool> CanAccessTestAsync(
        PsychometricTest test,
        Guid? requestingUserId,
        bool canManageSystemTemplates,
        CancellationToken ct)
    {
        // System templates are the shared, read-only assessment library.
        if (!test.DoctorId.HasValue)
            return true;

        if (!requestingUserId.HasValue)
            return false;

        // A private template is visible to its creator only. Manager permissions apply
        // to system templates and must not expose one doctor's clinical content to another.
        if (test.DoctorId.Value == requestingUserId.Value)
            return true;

        var patients = await _patientRepo.GetAllAsync(ct);
        var patient = patients.FirstOrDefault(p =>
            !p.IsDeleted && p.UserId == requestingUserId.Value);
        if (patient == null)
            return false;

        // Patients may read the exact private assessment that was assigned to them.
        var submissions = await _submissionRepo.GetAllAsync(ct);
        return submissions.Any(s =>
            !s.IsDeleted &&
            s.TestId == test.Id &&
            s.PatientId == patient.Id);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> AssignAssessmentAsync(AssignAssessmentDto dto, Guid doctorUserId, CancellationToken ct = default)
    {
        var test = await _testRepo.GetByIdAsync(dto.TestId, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Assessment template not found.");

        if (test.DoctorId.HasValue && test.DoctorId.Value != doctorUserId)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("You can only assign system templates or your own custom assessments.");

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.Id == dto.PatientId || p.UserId == dto.PatientId);
        if (patient == null)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Patient not found.");

        if (dto.TreatmentCaseId.HasValue && _caseRepo != null)
        {
            var treatmentCase = await _caseRepo.GetByIdAsync(dto.TreatmentCaseId.Value, ct);
            if (treatmentCase == null || treatmentCase.IsDeleted)
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Treatment case not found.");

            if (treatmentCase.Status is not (OPCBS.Domain.Enums.TreatmentCaseStatus.Active or OPCBS.Domain.Enums.TreatmentCaseStatus.OnHold) ||
                (treatmentCase.ExpectedEndDate.HasValue && treatmentCase.ExpectedEndDate.Value < DateTime.UtcNow))
            {
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("This treatment program is no longer active. Historical information remains available in read-only mode.");
            }
        }

        var submission = new PsychometricSubmission
        {
            TestId = test.Id,
            PatientId = patient.Id,
            TreatmentCaseId = dto.TreatmentCaseId,
            AssignedByDoctorId = doctorUserId,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(7),
            Status = "Assigned",
            DoctorNotes = dto.DoctorNote,
            TotalScore = 0,
            ScoreDataJson = "{}",
            Interpretation = "Assigned by Doctor (Pending Completion)",
            Test = test,
            Patient = patient
        };

        await _submissionRepo.AddAsync(submission, ct);
        await _uow.SaveChangesAsync(ct);

        var patientUser = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var docUser = await _userRepo.GetByIdAsync(doctorUserId, ct);

        var resultDto = new PsychometricSubmissionDto
        {
            Id = submission.Id,
            TestId = test.Id,
            TestTitle = test.Title,
            TestType = test.TestType,
            Category = test.Category,
            Purpose = test.Purpose,
            PatientId = patient.Id,
            PatientName = patientUser?.FullName,
            TreatmentCaseId = submission.TreatmentCaseId,
            AssignedByDoctorId = doctorUserId,
            AssignedByDoctorName = docUser?.FullName,
            DueDate = submission.DueDate,
            Status = submission.Status,
            DoctorNotes = submission.DoctorNotes,
            SubmittedAt = submission.CreatedAt,
            TotalScore = 0,
            ScoreDataJson = submission.ScoreDataJson,
            Interpretation = submission.Interpretation
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(resultDto, "Assessment assigned to patient successfully.");
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> SaveDoctorNoteAsync(Guid submissionId, string? doctorNotes, Guid doctorUserId, CancellationToken ct = default)
    {
        var submission = await _submissionRepo.GetByIdAsync(submissionId, ct);
        if (submission == null || submission.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Assessment submission not found.");

        submission.DoctorNotes = doctorNotes?.Trim();
        _submissionRepo.Update(submission);
        await _uow.SaveChangesAsync(ct);

        return await GetSubmissionByIdAsync(submissionId, doctorUserId, ct);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> SubmitTestAsync(SubmitTestDto dto, Guid patientUserId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy hồ sơ bệnh nhân.");

        var test = await _testRepo.GetByIdAsync(dto.TestId, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy bài trắc nghiệm.");

        PsychometricSubmission? assignedSubmission = null;
        if (dto.SubmissionId.HasValue)
        {
            assignedSubmission = await _submissionRepo.GetByIdAsync(dto.SubmissionId.Value, ct);
            if (assignedSubmission == null ||
                assignedSubmission.IsDeleted ||
                assignedSubmission.PatientId != patient.Id ||
                assignedSubmission.TestId != dto.TestId)
            {
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse(
                    "This assessment assignment is invalid or does not belong to the current patient.");
            }

            if (string.Equals(assignedSubmission.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse(
                    "This assigned assessment has already been completed.");
            }
        }

        if (test.DoctorId.HasValue && assignedSubmission == null)
        {
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse(
                "This private assessment can only be completed through a valid doctor assignment.");
        }

        var questions = (await _questionRepo.GetAllAsync(ct)).Where(q => q.TestId == dto.TestId && !q.IsDeleted).ToList();
        if (dto.Answers == null || dto.Answers.Count != questions.Count)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Số lượng câu trả lời không khớp với số câu hỏi.");

        var questionIds = questions.Select(q => q.Id).ToHashSet();
        foreach (var ans in dto.Answers)
        {
            if (!questionIds.Contains(ans.QuestionId))
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse($"Câu hỏi ID {ans.QuestionId} không thuộc bài trắc nghiệm này.");
            if (ans.Score < 0 || ans.Score > 5)
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Điểm số câu trả lời không hợp lệ.");
        }

        int totalScore = 0;
        int maxScore = 0;
        string scoreDataJson = "";
        string interpretation = "";
        string severity = "Bình thường";

        if (test.TestType == "PHQ9")
        {
            totalScore = dto.Answers.Sum(a => a.Score);
            maxScore = 27;
            interpretation = GetPhq9Interpretation(totalScore);
            severity = interpretation;
            scoreDataJson = JsonSerializer.Serialize(new { TotalScore = totalScore, MaxScore = maxScore, Severity = severity });
        }
        else if (test.TestType == "GAD7")
        {
            totalScore = dto.Answers.Sum(a => a.Score);
            maxScore = 21;
            interpretation = GetGad7Interpretation(totalScore);
            severity = GetGad7Severity(totalScore);
            scoreDataJson = JsonSerializer.Serialize(new { TotalScore = totalScore, MaxScore = maxScore, Severity = severity });
        }
        else if (test.TestType == "DASS21")
        {
            var qDict = questions.ToDictionary(q => q.Id, q => q);
            var depressionRaw = dto.Answers.Where(a => qDict.TryGetValue(a.QuestionId, out var q) && q.Category == "Depression").Sum(a => a.Score);
            var anxietyRaw = dto.Answers.Where(a => qDict.TryGetValue(a.QuestionId, out var q) && q.Category == "Anxiety").Sum(a => a.Score);
            var stressRaw = dto.Answers.Where(a => qDict.TryGetValue(a.QuestionId, out var q) && q.Category == "Stress").Sum(a => a.Score);

            int depressionScore = depressionRaw * 2;
            int anxietyScore = anxietyRaw * 2;
            int stressScore = stressRaw * 2;

            totalScore = depressionScore + anxietyScore + stressScore;
            maxScore = 126;

            var depInt = GetDassDepressionInterpretation(depressionScore);
            var anxInt = GetDassAnxietyInterpretation(anxietyScore);
            var strInt = GetDassStressInterpretation(stressScore);

            interpretation = $"Trầm cảm: {depInt}, Lo âu: {anxInt}, Căng thẳng: {strInt}";
            severity = depInt;
            scoreDataJson = JsonSerializer.Serialize(new
            {
                Depression = depressionScore,
                Anxiety = anxietyScore,
                Stress = stressScore,
                TotalScore = totalScore,
                MaxScore = maxScore,
                Severity = severity
            });
        }
        else
        {
            totalScore = dto.Answers.Sum(a => a.Score);
            maxScore = questions.Count * 5;
            double pct = maxScore > 0 ? (double)totalScore / maxScore : 0;
            if (pct <= 0.25) { severity = "Minimal"; interpretation = "Minimal symptoms based on assessment score."; }
            else if (pct <= 0.50) { severity = "Mild"; interpretation = "Mild symptoms based on assessment score."; }
            else if (pct <= 0.75) { severity = "Moderate"; interpretation = "Moderate symptoms based on assessment score."; }
            else { severity = "High"; interpretation = "High symptoms based on assessment score."; }

            scoreDataJson = JsonSerializer.Serialize(new
            {
                TotalScore = totalScore,
                MaxScore = maxScore,
                Severity = severity
            });
        }

        // If patient is completing an existing assigned assessment
        PsychometricSubmission submission;
        if (assignedSubmission != null)
        {
            assignedSubmission.TotalScore = totalScore;
            assignedSubmission.ScoreDataJson = scoreDataJson;
            assignedSubmission.Interpretation = interpretation;
            assignedSubmission.Status = "Completed";
            assignedSubmission.AppointmentId = dto.AppointmentId ?? assignedSubmission.AppointmentId;
            assignedSubmission.TreatmentCaseId = dto.TreatmentCaseId ?? assignedSubmission.TreatmentCaseId;
            _submissionRepo.Update(assignedSubmission);
            submission = assignedSubmission;
        }
        else
        {
            submission = new PsychometricSubmission
            {
                TestId = dto.TestId,
                PatientId = patient.Id,
                AppointmentId = dto.AppointmentId,
                TreatmentCaseId = dto.TreatmentCaseId,
                TotalScore = totalScore,
                ScoreDataJson = scoreDataJson,
                Interpretation = interpretation,
                Status = "Completed",
                Test = test,
                Patient = patient
            };
            await _submissionRepo.AddAsync(submission, ct);
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.SaveChangesAsync(ct);

            foreach (var ans in dto.Answers)
            {
                var q = questions.First(x => x.Id == ans.QuestionId);
                var answerEntity = new PsychometricAnswer
                {
                    SubmissionId = submission.Id,
                    QuestionId = ans.QuestionId,
                    Score = ans.Score,
                    Submission = submission,
                    Question = q
                };
                await _answerRepo.AddAsync(answerEntity, ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var patientUser = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var resultDto = new PsychometricSubmissionDto
        {
            Id = submission.Id,
            TestId = submission.TestId,
            TestTitle = test.Title,
            TestType = test.TestType,
            Category = test.Category,
            Purpose = test.Purpose,
            PatientId = submission.PatientId,
            PatientName = patientUser?.FullName,
            AppointmentId = submission.AppointmentId,
            TreatmentCaseId = submission.TreatmentCaseId,
            SubmittedAt = submission.CreatedAt,
            TotalScore = submission.TotalScore,
            MaxScore = maxScore,
            ScoreDataJson = submission.ScoreDataJson,
            Interpretation = submission.Interpretation,
            SeverityLevel = severity,
            Status = submission.Status
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(resultDto);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByIdAsync(Guid submissionId, Guid userId, CancellationToken ct = default)
    {
        var submission = await _submissionRepo.GetByIdAsync(submissionId, ct);
        if (submission == null || submission.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Submission not found.");

        var test = await _testRepo.GetByIdAsync(submission.TestId, ct);
        var patient = await _patientRepo.GetByIdAsync(submission.PatientId, ct);
        var patientUser = patient != null ? await _userRepo.GetByIdAsync(patient.UserId, ct) : null;

        string? doctorName = null;
        if (submission.AssignedByDoctorId.HasValue)
        {
            var docUser = await _userRepo.GetByIdAsync(submission.AssignedByDoctorId.Value, ct);
            doctorName = docUser?.FullName;
        }

        string? caseTitle = null;
        if (submission.TreatmentCaseId.HasValue && _caseRepo != null)
        {
            var tc = await _caseRepo.GetByIdAsync(submission.TreatmentCaseId.Value, ct);
            caseTitle = tc?.CaseName;
        }

        // Fetch question answers
        var answers = (await _answerRepo.GetAllAsync(ct)).Where(a => a.SubmissionId == submissionId && !a.IsDeleted).ToList();
        // Historical answers can reference questions that were soft-deleted when a test
        // template was edited. Keep those questions available for immutable reports.
        var testQuestions = (await _questionRepo.GetAllAsync(ct))
            .Where(q => q.TestId == submission.TestId)
            .ToList();
        var questions = testQuestions
            .GroupBy(q => q.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var answerDetails = answers.Select(a =>
        {
            questions.TryGetValue(a.QuestionId, out var q);
            return new PsychometricAnswerDetailDto
            {
                QuestionId = a.QuestionId,
                QuestionNumber = q?.QuestionNumber ?? 0,
                QuestionText = q?.QuestionText ?? string.Empty,
                Category = q?.Category,
                QuestionType = q?.QuestionType ?? "Rating1To5",
                Score = a.Score
            };
        }).OrderBy(a => a.QuestionNumber).ToList();

        int maxScore = 27;
        string severity = "Moderate";
        if (test != null)
        {
            if (test.TestType == "PHQ9") { maxScore = 27; severity = GetPhq9Severity(submission.TotalScore); }
            else if (test.TestType == "GAD7") { maxScore = 21; severity = GetGad7Severity(submission.TotalScore); }
            else if (test.TestType == "DASS21") { maxScore = 126; severity = GetDassDepressionInterpretation(submission.TotalScore / 3); }
            else
            {
                var activeQuestionCount = testQuestions.Count(q => !q.IsDeleted);
                var reportQuestionCount = answerDetails.Count > 0 ? answerDetails.Count : activeQuestionCount;
                maxScore = (reportQuestionCount > 0 ? reportQuestionCount : 5) * 5;
                severity = GetGenericSeverity(submission.TotalScore, maxScore);
            }
        }

        var dto = new PsychometricSubmissionDto
        {
            Id = submission.Id,
            TestId = submission.TestId,
            TestTitle = test?.Title ?? "Assessment Result",
            TestType = test?.TestType ?? "CUSTOM",
            Category = test?.Category ?? "Psychological Screening",
            Purpose = test?.Purpose ?? "Clinical Assessment",
            PatientId = submission.PatientId,
            PatientName = patientUser?.FullName ?? "Patient",
            AppointmentId = submission.AppointmentId,
            TreatmentCaseId = submission.TreatmentCaseId,
            TreatmentCaseTitle = caseTitle,
            AssignedByDoctorId = submission.AssignedByDoctorId,
            AssignedByDoctorName = doctorName,
            SubmittedAt = submission.CreatedAt,
            DueDate = submission.DueDate,
            Status = submission.Status,
            TotalScore = submission.TotalScore,
            MaxScore = maxScore,
            SeverityLevel = severity,
            DoctorNotes = submission.DoctorNotes,
            ScoreDataJson = submission.ScoreDataJson,
            Interpretation = submission.Interpretation,
            Answers = answerDetails
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByAppointmentAsync(Guid appointmentId, Guid userId, CancellationToken ct = default)
    {
        var submissions = await _submissionRepo.GetAllAsync(ct);
        var sub = submissions
            .Where(s => s.AppointmentId == appointmentId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();

        if (sub == null)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Assessment submission not found for this appointment.");

        return await GetSubmissionByIdAsync(sub.Id, userId, ct);
    }

    public async Task<ApiResponse<List<PsychometricSubmissionDto>>> GetPatientSubmissionsAsync(Guid patientUserId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId || p.Id == patientUserId);
        if (patient == null)
            return ApiResponse<List<PsychometricSubmissionDto>>.ErrorResponse("Patient profile not found.");

        var submissions = await _submissionRepo.GetAllAsync(ct);
        var filtered = submissions.Where(s => s.PatientId == patient.Id && !s.IsDeleted).OrderBy(s => s.CreatedAt).ToList();

        var tests = await _testRepo.GetAllAsync(ct);
        var testDict = tests.ToDictionary(t => t.Id, t => t);

        var patientUser = await _userRepo.GetByIdAsync(patient.UserId, ct);
        var patientName = patientUser?.FullName ?? "Patient";

        var dtos = new List<PsychometricSubmissionDto>();
        for (int i = 0; i < filtered.Count; i++)
        {
            var s = filtered[i];
            var prev = filtered.Take(i).LastOrDefault(x => x.TestId == s.TestId && x.Status == "Completed");
            int? prevScore = prev?.TotalScore;
            int? change = prevScore.HasValue ? s.TotalScore - prevScore.Value : null;

            testDict.TryGetValue(s.TestId, out var t);

            dtos.Add(new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = t?.Title ?? "Assessment Result",
                TestType = t?.TestType ?? "Screening",
                Category = t?.Category,
                Purpose = t?.Purpose,
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = s.TreatmentCaseId,
                SubmittedAt = s.CreatedAt,
                DueDate = s.DueDate,
                Status = s.Status,
                TotalScore = s.TotalScore,
                PreviousScore = prevScore,
                ScoreChange = change,
                DoctorNotes = s.DoctorNotes,
                ScoreDataJson = s.ScoreDataJson,
                Interpretation = s.Interpretation
            });
        }

        return ApiResponse<List<PsychometricSubmissionDto>>.SuccessResponse(dtos.OrderByDescending(s => s.SubmittedAt).ToList());
    }

    public async Task<ApiResponse<List<PsychometricSubmissionDto>>> GetSubmissionsByCaseIdAsync(Guid caseId, Guid requestingUserId, CancellationToken ct = default)
    {
        if (_caseRepo == null)
            return ApiResponse<List<PsychometricSubmissionDto>>.SuccessResponse(new List<PsychometricSubmissionDto>());

        var tc = await _caseRepo.GetByIdAsync(caseId, ct);
        if (tc == null || tc.IsDeleted)
            return ApiResponse<List<PsychometricSubmissionDto>>.ErrorResponse("Treatment case not found.");

        var submissions = await _submissionRepo.GetAllAsync(ct);
        var filtered = submissions.Where(s => !s.IsDeleted && (
            s.TreatmentCaseId == caseId ||
            (!s.TreatmentCaseId.HasValue && s.PatientId == tc.PatientId && s.CreatedAt >= tc.StartDate && (tc.ActualEndDate == null || s.CreatedAt <= tc.ActualEndDate))
        )).OrderBy(s => s.CreatedAt).ToList();

        var tests = await _testRepo.GetAllAsync(ct);
        var testDict = tests.ToDictionary(t => t.Id, t => t);

        var patientUser = tc.Patient != null ? await _userRepo.GetByIdAsync(tc.Patient.UserId, ct) : null;
        var patientName = patientUser?.FullName ?? "Patient";

        var dtos = new List<PsychometricSubmissionDto>();
        for (int i = 0; i < filtered.Count; i++)
        {
            var s = filtered[i];
            var prev = filtered.Take(i).LastOrDefault(x => x.TestId == s.TestId && x.Status == "Completed");
            int? prevScore = prev?.TotalScore;
            int? change = prevScore.HasValue ? s.TotalScore - prevScore.Value : null;

            testDict.TryGetValue(s.TestId, out var t);

            dtos.Add(new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = t?.Title ?? "Assessment Result",
                TestType = t?.TestType ?? "Screening",
                Category = t?.Category,
                Purpose = t?.Purpose,
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = caseId,
                SubmittedAt = s.CreatedAt,
                DueDate = s.DueDate,
                Status = s.Status,
                TotalScore = s.TotalScore,
                PreviousScore = prevScore,
                ScoreChange = change,
                DoctorNotes = s.DoctorNotes,
                ScoreDataJson = s.ScoreDataJson,
                Interpretation = s.Interpretation
            });
        }

        return ApiResponse<List<PsychometricSubmissionDto>>.SuccessResponse(dtos.OrderByDescending(s => s.SubmittedAt).ToList());
    }

    public async Task<ApiResponse<List<PsychometricSubmissionDto>>> GetAllSubmissionsAsync(Guid? testId = null, CancellationToken ct = default)
    {
        var submissions = await _submissionRepo.GetAllAsync(ct);
        var query = submissions.Where(s => !s.IsDeleted);
        if (testId.HasValue && testId.Value != Guid.Empty)
        {
            query = query.Where(s => s.TestId == testId.Value);
        }
        var filtered = query.OrderByDescending(s => s.CreatedAt).ToList();

        var tests = await _testRepo.GetAllAsync(ct);
        var testDict = tests.ToDictionary(t => t.Id, t => t);

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patientDict = allPatients.ToDictionary(p => p.Id, p => p);

        var allUsers = await _userRepo.GetAllAsync(ct);
        var userDict = allUsers.ToDictionary(u => u.Id, u => u);

        var dtos = filtered.Select(s =>
        {
            string patientName = "Patient";
            if (patientDict.TryGetValue(s.PatientId, out var p) && userDict.TryGetValue(p.UserId, out var u))
            {
                patientName = u.FullName ?? "Patient";
            }

            testDict.TryGetValue(s.TestId, out var t);

            return new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = t?.Title ?? "Assessment Result",
                TestType = t?.TestType ?? "Screening",
                Category = t?.Category,
                Purpose = t?.Purpose,
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = s.TreatmentCaseId,
                SubmittedAt = s.CreatedAt,
                DueDate = s.DueDate,
                Status = s.Status,
                TotalScore = s.TotalScore,
                DoctorNotes = s.DoctorNotes,
                ScoreDataJson = s.ScoreDataJson,
                Interpretation = s.Interpretation
            };
        }).ToList();

        return ApiResponse<List<PsychometricSubmissionDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<AssessmentHistoryItemDto>>> GetAssessmentHistoryAsync(Guid submissionId, CancellationToken ct = default)
    {
        var targetSub = await _submissionRepo.GetByIdAsync(submissionId, ct);
        if (targetSub == null || targetSub.IsDeleted)
            return ApiResponse<List<AssessmentHistoryItemDto>>.ErrorResponse("Submission not found.");

        var allSubs = await _submissionRepo.GetAllAsync(ct);
        var patientSubs = allSubs
            .Where(s => s.PatientId == targetSub.PatientId && s.TestId == targetSub.TestId && !s.IsDeleted && s.Status == "Completed")
            .OrderBy(s => s.CreatedAt)
            .ToList();

        var test = await _testRepo.GetByIdAsync(targetSub.TestId, ct);
        var questions = (await _questionRepo.GetAllAsync(ct)).Where(q => q.TestId == targetSub.TestId && !q.IsDeleted).ToList();
        int maxScore = 27;
        if (test != null)
        {
            if (test.TestType == "PHQ9") maxScore = 27;
            else if (test.TestType == "GAD7") maxScore = 21;
            else if (test.TestType == "DASS21") maxScore = 126;
            else maxScore = (questions.Count > 0 ? questions.Count : 5) * 5;
        }

        var history = new List<AssessmentHistoryItemDto>();
        for (int i = 0; i < patientSubs.Count; i++)
        {
            var s = patientSubs[i];
            string sev = "Moderate";
            if (test?.TestType == "PHQ9") sev = GetPhq9Severity(s.TotalScore);
            else if (test?.TestType == "GAD7") sev = GetGad7Severity(s.TotalScore);
            else sev = GetGenericSeverity(s.TotalScore, maxScore);

            string changeText = "Baseline";
            string changeClass = "text-muted";

            if (i > 0)
            {
                var prev = patientSubs[i - 1];
                int diff = s.TotalScore - prev.TotalScore;
                if (diff < 0)
                {
                    changeText = $"↓ {Math.Abs(diff)} Improvement";
                    changeClass = "text-success";
                }
                else if (diff > 0)
                {
                    changeText = $"↑ +{diff} Elevated";
                    changeClass = "text-danger";
                }
                else
                {
                    changeText = "— No Change";
                    changeClass = "text-muted";
                }
            }

            bool isCurrent = s.Id == submissionId;

            history.Add(new AssessmentHistoryItemDto
            {
                SubmissionId = s.Id,
                Date = s.CreatedAt,
                Score = s.TotalScore,
                MaxScore = maxScore,
                Severity = sev,
                ChangeText = changeText,
                ChangeClass = changeClass,
                IsCurrent = isCurrent
            });
        }

        // Return latest first
        return ApiResponse<List<AssessmentHistoryItemDto>>.SuccessResponse(history.OrderByDescending(h => h.Date).ToList());
    }

    public async Task<ApiResponse<DoctorAssessmentsOverviewDto>> GetDoctorAssessmentsOverviewAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var allSubs = await _submissionRepo.GetAllAsync(ct);
        var activeSubs = allSubs
            .Where(s => !s.IsDeleted && s.AssignedByDoctorId == doctorUserId)
            .ToList();

        var tests = await _testRepo.GetAllAsync(ct);
        var activeTests = tests.Where(t => !t.IsDeleted && t.IsActive).ToList();

        var questions = await _questionRepo.GetAllAsync(ct);
        var qCounts = questions.Where(q => !q.IsDeleted).GroupBy(q => q.TestId).ToDictionary(g => g.Key, g => g.Count());
        var sCounts = activeSubs.GroupBy(s => s.TestId).ToDictionary(g => g.Key, g => g.Count());

        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patientDict = allPatients.ToDictionary(p => p.Id, p => p);

        var allUsers = await _userRepo.GetAllAsync(ct);
        var userDict = allUsers.ToDictionary(u => u.Id, u => u);

        int totalAssigned = activeSubs.Count(s => s.Status == "Assigned");
        int totalCompleted = activeSubs.Count(s => s.Status == "Completed");
        int totalPending = activeSubs.Count(s => s.Status == "Assigned" || s.Status == "InProgress");
        int patientsAssessed = activeSubs.Where(s => s.Status == "Completed").Select(s => s.PatientId).Distinct().Count();

        var testDict = activeTests.ToDictionary(t => t.Id, t => t);

        var recentList = activeSubs.OrderByDescending(s => s.CreatedAt).Take(15).Select(s =>
        {
            string patientName = "Patient";
            if (patientDict.TryGetValue(s.PatientId, out var p) && userDict.TryGetValue(p.UserId, out var u))
            {
                patientName = u.FullName ?? "Patient";
            }

            testDict.TryGetValue(s.TestId, out var t);

            int maxScore = 27;
            if (t?.TestType == "PHQ9") maxScore = 27;
            else if (t?.TestType == "GAD7") maxScore = 21;
            else if (t?.TestType == "DASS21") maxScore = 126;
            else maxScore = qCounts.GetValueOrDefault(s.TestId, 5) * 5;

            return new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = t?.Title ?? "Assessment",
                TestType = t?.TestType ?? "CUSTOM",
                Category = t?.Category ?? "Screening",
                Purpose = t?.Purpose,
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = s.TreatmentCaseId,
                SubmittedAt = s.CreatedAt,
                DueDate = s.DueDate,
                Status = s.Status,
                TotalScore = s.TotalScore,
                MaxScore = maxScore,
                DoctorNotes = s.DoctorNotes,
                ScoreDataJson = s.ScoreDataJson,
                Interpretation = s.Interpretation
            };
        }).ToList();

        var sysTemplates = activeTests.Where(t => !t.DoctorId.HasValue).Select(t => new PsychometricTestDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            TestType = t.TestType,
            Category = !string.IsNullOrWhiteSpace(t.Category) ? t.Category : (t.TestType == "PHQ9" ? "Depression" : (t.TestType == "GAD7" ? "Anxiety" : "General Wellbeing")),
            Purpose = !string.IsNullOrWhiteSpace(t.Purpose) ? t.Purpose : (t.TestType == "PHQ9" ? "Depression Screening" : (t.TestType == "GAD7" ? "Anxiety Screening" : "General Assessment")),
            SourceUrl = !string.IsNullOrWhiteSpace(t.SourceUrl) ? t.SourceUrl : (t.TestType == "DASS21" || t.Title.Contains("DASS") ? "http://www2.psy.unsw.edu.au/dass/" : null),
            DoctorId = null,
            ScoreRangesJson = t.ScoreRangesJson,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            QuestionCount = qCounts.GetValueOrDefault(t.Id, 0),
            SubmissionCount = sCounts.GetValueOrDefault(t.Id, 0)
        }).OrderBy(t => t.Title).ToList();

        var myAssessments = activeTests.Where(t => t.DoctorId == doctorUserId).Select(t => new PsychometricTestDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            TestType = t.TestType,
            Category = t.Category ?? "Custom Assessment",
            Purpose = t.Purpose ?? "Doctor Check-in",
            SourceUrl = !string.IsNullOrWhiteSpace(t.SourceUrl) ? t.SourceUrl : (t.TestType == "DASS21" || t.Title.Contains("DASS") ? "http://www2.psy.unsw.edu.au/dass/" : null),
            DoctorId = t.DoctorId,
            DoctorName = userDict.TryGetValue(doctorUserId, out var docU) ? docU.FullName : "Doctor",
            ScoreRangesJson = t.ScoreRangesJson,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            QuestionCount = qCounts.GetValueOrDefault(t.Id, 0),
            SubmissionCount = sCounts.GetValueOrDefault(t.Id, 0)
        }).OrderBy(t => t.Title).ToList();

        var result = new DoctorAssessmentsOverviewDto
        {
            TotalAssigned = totalAssigned,
            TotalCompleted = totalCompleted,
            TotalPending = totalPending,
            PatientsAssessedCount = patientsAssessed,
            RecentAssessments = recentList,
            SystemTemplates = sysTemplates,
            MyAssessments = myAssessments
        };

        return ApiResponse<DoctorAssessmentsOverviewDto>.SuccessResponse(result);
    }

    #region Diagnostic Interpretations & Severities

    private string GetPhq9Interpretation(int score)
    {
        if (score <= 4) return "Bình thường / Không trầm cảm";
        if (score <= 9) return "Trầm cảm nhẹ";
        if (score <= 14) return "Trầm cảm vừa";
        if (score <= 19) return "Trầm cảm trung bình nặng";
        return "Trầm cảm nặng";
    }

    private string GetPhq9Severity(int score)
    {
        if (score <= 4) return "Minimal";
        if (score <= 9) return "Mild";
        if (score <= 14) return "Moderate";
        if (score <= 19) return "Moderately Severe";
        return "Severe";
    }

    private string GetGad7Interpretation(int score)
    {
        if (score <= 4) return "Minimal anxiety symptoms based on assessment score.";
        if (score <= 9) return "Mild anxiety symptoms based on assessment score.";
        if (score <= 14) return "Moderate anxiety symptoms based on assessment score.";
        return "Severe anxiety symptoms based on assessment score.";
    }

    private string GetGad7Severity(int score)
    {
        if (score <= 4) return "Minimal";
        if (score <= 9) return "Mild";
        if (score <= 14) return "Moderate";
        return "Severe";
    }

    private string GetDassDepressionInterpretation(int score)
    {
        if (score <= 9) return "Bình thường";
        if (score <= 13) return "Nhẹ";
        if (score <= 20) return "Vừa";
        if (score <= 27) return "Nặng";
        return "Rất nặng";
    }

    private string GetDassAnxietyInterpretation(int score)
    {
        if (score <= 7) return "Bình thường";
        if (score <= 9) return "Nhẹ";
        if (score <= 14) return "Vừa";
        if (score <= 19) return "Nặng";
        return "Rất nặng";
    }

    private string GetDassStressInterpretation(int score)
    {
        if (score <= 14) return "Bình thường";
        if (score <= 18) return "Nhẹ";
        if (score <= 25) return "Vừa";
        if (score <= 33) return "Nặng";
        return "Rất nặng";
    }

    private string GetGenericSeverity(int score, int maxScore)
    {
        if (maxScore <= 0) return "Normal";
        double pct = (double)score / maxScore;
        if (pct <= 0.25) return "Minimal";
        if (pct <= 0.50) return "Mild";
        if (pct <= 0.75) return "Moderate";
        return "High";
    }

    #endregion
}
