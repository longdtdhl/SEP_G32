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
        var dtos = tests.Where(t => !t.IsDeleted).Select(t => new PsychometricTestDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            TestType = t.TestType
        }).ToList();

        return ApiResponse<List<PsychometricTestDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<PsychometricQuestionDto>>> GetQuestionsAsync(Guid testId, CancellationToken ct = default)
    {
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
                Category = q.Category
            }).ToList();

        return ApiResponse<List<PsychometricQuestionDto>>.SuccessResponse(filtered);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> SubmitTestAsync(SubmitTestDto dto, Guid patientUserId, CancellationToken ct = default)
    {
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var patient = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        if (patient == null)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy hồ sơ bệnh nhân.");

        var test = await _testRepo.GetByIdAsync(dto.TestId, ct);
        if (test == null || test.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy bài trắc nghiệm.");

        var questions = (await _questionRepo.GetAllAsync(ct)).Where(q => q.TestId == dto.TestId && !q.IsDeleted).ToList();
        if (dto.Answers == null || dto.Answers.Count != questions.Count)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Số lượng câu trả lời không khớp với số câu hỏi.");

        var questionIds = questions.Select(q => q.Id).ToHashSet();
        foreach (var ans in dto.Answers)
        {
            if (!questionIds.Contains(ans.QuestionId))
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse($"Câu hỏi ID {ans.QuestionId} không thuộc bài trắc nghiệm này.");
            if (ans.Score < 0 || ans.Score > 3)
                return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Điểm số câu trả lời phải nằm trong khoảng từ 0 đến 3.");
        }

        int totalScore = 0;
        string scoreDataJson = "";
        string interpretation = "";

        if (test.TestType == "PHQ9")
        {
            totalScore = dto.Answers.Sum(a => a.Score);
            interpretation = GetPhq9Interpretation(totalScore);
            scoreDataJson = JsonSerializer.Serialize(new { TotalScore = totalScore });
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

            var depInt = GetDassDepressionInterpretation(depressionScore);
            var anxInt = GetDassAnxietyInterpretation(anxietyScore);
            var strInt = GetDassStressInterpretation(stressScore);

            interpretation = $"Trầm cảm: {depInt}, Lo âu: {anxInt}, Căng thẳng: {strInt}";
            scoreDataJson = JsonSerializer.Serialize(new
            {
                Depression = depressionScore,
                Anxiety = anxietyScore,
                Stress = stressScore
            });
        }
        else
        {
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Loại bài trắc nghiệm không được hỗ trợ.");
        }

        if (dto.AppointmentId.HasValue)
        {
            var allSubmissions = await _submissionRepo.GetAllAsync(ct);
            var existing = allSubmissions.FirstOrDefault(s => s.AppointmentId == dto.AppointmentId.Value && s.TestId == dto.TestId && !s.IsDeleted);
            if (existing != null)
            {
                existing.IsDeleted = true;
                _submissionRepo.Update(existing);
            }
        }

        var submission = new PsychometricSubmission
        {
            TestId = dto.TestId,
            PatientId = patient.Id,
            AppointmentId = dto.AppointmentId,
            TotalScore = totalScore,
            ScoreDataJson = scoreDataJson,
            Interpretation = interpretation,
            Test = test,
            Patient = patient
        };

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _submissionRepo.AddAsync(submission, ct);
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
            PatientId = submission.PatientId,
            PatientName = patientUser?.FullName,
            AppointmentId = submission.AppointmentId,
            SubmittedAt = submission.CreatedAt,
            TotalScore = submission.TotalScore,
            ScoreDataJson = submission.ScoreDataJson,
            Interpretation = submission.Interpretation
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(resultDto);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByAppointmentAsync(Guid appointmentId, Guid userId, CancellationToken ct = default)
    {
        var submissions = await _submissionRepo.GetAllAsync(ct);
        // An appointment could have multiple submissions (DASS-21 and PHQ-9), let's get the latest non-deleted one
        var sub = submissions
            .Where(s => s.AppointmentId == appointmentId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();

        if (sub == null)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy kết quả trắc nghiệm cho buổi hẹn này.");

        var test = await _testRepo.GetByIdAsync(sub.TestId, ct);
        var patient = await _patientRepo.GetByIdAsync(sub.PatientId, ct);
        var patientUser = patient != null ? await _userRepo.GetByIdAsync(patient.UserId, ct) : null;

        var dto = new PsychometricSubmissionDto
        {
            Id = sub.Id,
            TestId = sub.TestId,
            TestTitle = test?.Title ?? "",
            TestType = test?.TestType ?? "",
            PatientId = sub.PatientId,
            PatientName = patientUser?.FullName,
            AppointmentId = sub.AppointmentId,
            SubmittedAt = sub.CreatedAt,
            TotalScore = sub.TotalScore,
            ScoreDataJson = sub.ScoreDataJson,
            Interpretation = sub.Interpretation
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByIdAsync(Guid submissionId, Guid userId, CancellationToken ct = default)
    {
        var sub = await _submissionRepo.GetByIdAsync(submissionId, ct);
        if (sub == null || sub.IsDeleted)
            return ApiResponse<PsychometricSubmissionDto>.ErrorResponse("Không tìm thấy kết quả trắc nghiệm.");

        var test = await _testRepo.GetByIdAsync(sub.TestId, ct);
        var patient = await _patientRepo.GetByIdAsync(sub.PatientId, ct);
        var patientUser = patient != null ? await _userRepo.GetByIdAsync(patient.UserId, ct) : null;

        var dto = new PsychometricSubmissionDto
        {
            Id = sub.Id,
            TestId = sub.TestId,
            TestTitle = test?.Title ?? "",
            TestType = test?.TestType ?? "",
            PatientId = sub.PatientId,
            PatientName = patientUser?.FullName,
            AppointmentId = sub.AppointmentId,
            SubmittedAt = sub.CreatedAt,
            TotalScore = sub.TotalScore,
            ScoreDataJson = sub.ScoreDataJson,
            Interpretation = sub.Interpretation
        };

        return ApiResponse<PsychometricSubmissionDto>.SuccessResponse(dto);
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
            var prev = filtered.Take(i).LastOrDefault(x => x.TestId == s.TestId);
            int? prevScore = prev?.TotalScore;
            int? change = prevScore.HasValue ? s.TotalScore - prevScore.Value : null;

            dtos.Add(new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = testDict.TryGetValue(s.TestId, out var t) ? t.Title : "Assessment Result",
                TestType = testDict.TryGetValue(s.TestId, out var t2) ? t2.TestType : "Screening",
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = s.TreatmentCaseId,
                SubmittedAt = s.CreatedAt,
                TotalScore = s.TotalScore,
                PreviousScore = prevScore,
                ScoreChange = change,
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
            var prev = filtered.Take(i).LastOrDefault(x => x.TestId == s.TestId);
            int? prevScore = prev?.TotalScore;
            int? change = prevScore.HasValue ? s.TotalScore - prevScore.Value : null;

            dtos.Add(new PsychometricSubmissionDto
            {
                Id = s.Id,
                TestId = s.TestId,
                TestTitle = testDict.TryGetValue(s.TestId, out var t) ? t.Title : "Assessment Result",
                TestType = testDict.TryGetValue(s.TestId, out var t2) ? t2.TestType : "Screening",
                PatientId = s.PatientId,
                PatientName = patientName,
                AppointmentId = s.AppointmentId,
                TreatmentCaseId = caseId,
                SubmittedAt = s.CreatedAt,
                TotalScore = s.TotalScore,
                PreviousScore = prevScore,
                ScoreChange = change,
                ScoreDataJson = s.ScoreDataJson,
                Interpretation = s.Interpretation
            });
        }

        return ApiResponse<List<PsychometricSubmissionDto>>.SuccessResponse(dtos.OrderByDescending(s => s.SubmittedAt).ToList());
    }

    #region Diagnostic Interpretations

    private string GetPhq9Interpretation(int score)
    {
        if (score <= 4) return "Bình thường / Không trầm cảm";
        if (score <= 9) return "Trầm cảm nhẹ";
        if (score <= 14) return "Trầm cảm vừa";
        if (score <= 19) return "Trầm cảm trung bình nặng";
        return "Trầm cảm nặng";
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

    #endregion
}
