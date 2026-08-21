using System;
using System.Collections.Generic;

namespace OPCBS.Web.DTOs;

public class PsychometricTestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public bool IsSystemTemplate => !DoctorId.HasValue;
    public string? ScoreRangesJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
}

public class CreatePsychometricQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public string? OptionsJson { get; set; }
}

public class CreatePsychometricTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = "CUSTOM";
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? ScoreRangesJson { get; set; }
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class UpdatePsychometricTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public string? ScoreRangesJson { get; set; }
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricTestDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public bool IsSystemTemplate => !DoctorId.HasValue;
    public string? ScoreRangesJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
    public List<PsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricQuestionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public string? OptionsJson { get; set; }
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public int Score { get; set; }
    public string? TextAnswer { get; set; }
}

public class SubmitTestDto
{
    public Guid TestId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? SubmissionId { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public class PsychometricAnswerDetailDto
{
    public Guid QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public int Score { get; set; }
    public string? TextAnswer { get; set; }
}

public class AssignAssessmentDto
{
    public Guid TestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? DoctorNote { get; set; }
}

public class SaveDoctorNoteDto
{
    public Guid SubmissionId { get; set; }
    public string? DoctorNotes { get; set; }
}

public class AssessmentHistoryItemDto
{
    public Guid SubmissionId { get; set; }
    public DateTime Date { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string ChangeText { get; set; } = string.Empty;
    public string ChangeClass { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class DoctorAssessmentsOverviewDto
{
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalPending { get; set; }
    public int PatientsAssessedCount { get; set; }
    public List<PsychometricSubmissionDto> RecentAssessments { get; set; } = new();
    public List<PsychometricTestDto> SystemTemplates { get; set; } = new();
    public List<PsychometricTestDto> MyAssessments { get; set; } = new();
}

public class PsychometricSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public string? TreatmentCaseTitle { get; set; }
    public Guid? AssignedByDoctorId { get; set; }
    public string? AssignedByDoctorName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Completed";
    public int TotalScore { get; set; }
    public int MaxScore { get; set; } = 27;
    public int CalculatedMaxScore => MaxScore > 0 ? MaxScore : (TestType == "PHQ9" ? 27 : (TestType == "GAD7" ? 21 : (TestType == "DASS21" ? 126 : 100)));
    public int? PreviousScore { get; set; }
    public int? ScoreChange { get; set; }
    public string ScoreDataJson { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
    public string? SeverityLevel { get; set; }
    public string? DoctorNotes { get; set; }
    public List<PsychometricAnswerDetailDto> Answers { get; set; } = new();

    public int DepressionScore
    {
        get
        {
            if (string.IsNullOrEmpty(ScoreDataJson)) return 0;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(ScoreDataJson);
                if (doc.RootElement.TryGetProperty("Depression", out var d)) return d.GetInt32();
            }
            catch { }
            return 0;
        }
    }

    public int AnxietyScore
    {
        get
        {
            if (string.IsNullOrEmpty(ScoreDataJson)) return 0;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(ScoreDataJson);
                if (doc.RootElement.TryGetProperty("Anxiety", out var a)) return a.GetInt32();
            }
            catch { }
            return 0;
        }
    }

    public int StressScore
    {
        get
        {
            if (string.IsNullOrEmpty(ScoreDataJson)) return 0;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(ScoreDataJson);
                if (doc.RootElement.TryGetProperty("Stress", out var s)) return s.GetInt32();
            }
            catch { }
            return 0;
        }
    }

public class SymptomBreakdownItemDto
{
    public string Domain { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public int Percent => MaxScore > 0 ? (int)Math.Min(100, Math.Round((double)Score * 100 / MaxScore)) : 0;
    public string BarColor { get; set; } = "#0d9488";
}

    public List<SymptomBreakdownItemDto> GetScoreBreakdown()
    {
        var list = new List<SymptomBreakdownItemDto>();
        if (TestType == "PHQ9" && Answers != null && Answers.Any())
        {
            var qMap = Answers.ToDictionary(a => a.QuestionNumber, a => a.Score);
            int mood = (qMap.GetValueOrDefault(1) + qMap.GetValueOrDefault(2));
            int sleep = qMap.GetValueOrDefault(3);
            int energy = qMap.GetValueOrDefault(4);
            int eatingConcentration = (qMap.GetValueOrDefault(5) + qMap.GetValueOrDefault(7));
            int other = (qMap.GetValueOrDefault(6) + qMap.GetValueOrDefault(8) + qMap.GetValueOrDefault(9));

            list.Add(new SymptomBreakdownItemDto { Domain = "Mood / Interest", Score = mood, MaxScore = 6, BarColor = mood >= 4 ? "#ef4444" : (mood >= 2 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Sleep Quality", Score = sleep, MaxScore = 3, BarColor = sleep >= 2 ? "#ef4444" : (sleep == 1 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Energy / Fatigue", Score = energy, MaxScore = 3, BarColor = energy >= 2 ? "#ef4444" : (energy == 1 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Appetite & Concentration", Score = eatingConcentration, MaxScore = 6, BarColor = eatingConcentration >= 4 ? "#ef4444" : (eatingConcentration >= 2 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Somatic & Self-Perception", Score = other, MaxScore = 9, BarColor = other >= 6 ? "#ef4444" : (other >= 3 ? "#eab308" : "#22c55e") });
            return list;
        }

        if (TestType == "DASS21")
        {
            list.Add(new SymptomBreakdownItemDto { Domain = "Depression (Dysphoria & Hopelessness)", Score = DepressionScore, MaxScore = 42, BarColor = GetDepressionSeverity().Bar });
            list.Add(new SymptomBreakdownItemDto { Domain = "Anxiety (Autonomic & Skeletal Arousal)", Score = AnxietyScore, MaxScore = 42, BarColor = GetAnxietySeverity().Bar });
            list.Add(new SymptomBreakdownItemDto { Domain = "Stress (Tension & Irritability)", Score = StressScore, MaxScore = 42, BarColor = GetStressSeverity().Bar });
            return list;
        }

        if (TestType == "GAD7" && Answers != null && Answers.Any())
        {
            var qMap = Answers.ToDictionary(a => a.QuestionNumber, a => a.Score);
            int worry = (qMap.GetValueOrDefault(1) + qMap.GetValueOrDefault(2) + qMap.GetValueOrDefault(3));
            int somatic = (qMap.GetValueOrDefault(4) + qMap.GetValueOrDefault(5));
            int fear = (qMap.GetValueOrDefault(6) + qMap.GetValueOrDefault(7));

            list.Add(new SymptomBreakdownItemDto { Domain = "Nervousness & Worry", Score = worry, MaxScore = 9, BarColor = worry >= 6 ? "#ef4444" : (worry >= 3 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Restlessness & Tension", Score = somatic, MaxScore = 6, BarColor = somatic >= 4 ? "#ef4444" : (somatic >= 2 ? "#eab308" : "#22c55e") });
            list.Add(new SymptomBreakdownItemDto { Domain = "Irritability & Dread", Score = fear, MaxScore = 6, BarColor = fear >= 4 ? "#ef4444" : (fear >= 2 ? "#eab308" : "#22c55e") });
            return list;
        }

        if (Answers != null && Answers.Any())
        {
            var catGroups = Answers.GroupBy(a => string.IsNullOrWhiteSpace(a.Category) ? "General Symptoms" : a.Category).ToList();
            foreach (var grp in catGroups)
            {
                int grpScore = grp.Sum(a => a.Score);
                int grpMax = grp.Count() * 5;
                double pct = grpMax > 0 ? (double)grpScore / grpMax : 0;
                string color = pct >= 0.7 ? "#ef4444" : (pct >= 0.4 ? "#eab308" : "#22c55e");
                list.Add(new SymptomBreakdownItemDto { Domain = grp.Key, Score = grpScore, MaxScore = grpMax, BarColor = color });
            }
        }
        return list;
    }

    public string GetEnglishInterpretation()
    {
        if (TestType == "PHQ9")
        {
            var s = TotalScore;
            if (s <= 4) return "Minimal depression. Symptoms suggest normal emotional fluctuations with no significant impairment.";
            if (s <= 9) return "Mild depressive symptoms. Patient may benefit from psychological monitoring and supportive counseling.";
            if (s <= 14) return "Moderate depressive disorder. Clinical intervention, cognitive behavioral therapy, and follow-up are recommended.";
            if (s <= 19) return "Moderately severe depression. Active clinical treatment plan and regular psychotherapy sessions are advised.";
            return "Severe depression. Comprehensive psychiatric evaluation and active clinical monitoring are strongly recommended.";
        }
        if (TestType == "GAD7")
        {
            var s = TotalScore;
            if (s <= 4) return "Minimal anxiety symptoms within normal clinical limits.";
            if (s <= 9) return "Mild anxiety symptoms. Patient may benefit from relaxation strategies and mindfulness exercises.";
            if (s <= 14) return "Moderate anxiety disorder. Clinical evaluation and structured therapy sessions are recommended.";
            return "Severe anxiety disorder. Active clinical management and frequent therapeutic support are advised.";
        }
        if (TestType == "DASS21")
        {
            var depSev = GetDepressionSeverity().En;
            var anxSev = GetAnxietySeverity().En;
            var strSev = GetStressSeverity().En;
            return $"Multi-dimensional assessment — Depression: {depSev} (Score: {DepressionScore}), Anxiety: {anxSev} (Score: {AnxietyScore}), Stress: {strSev} (Score: {StressScore}).";
        }

        if (!string.IsNullOrWhiteSpace(Interpretation) && !Interpretation.Any(c => c > 127))
        {
            return Interpretation;
        }

        var effSev = GetEffectiveSeverity().En;
        return $"Assessment completed with overall classification: {effSev} (Score: {TotalScore} / {MaxScore}).";
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetEffectiveSeverity()
    {
        if (TestType == "PHQ9") return GetPhq9Severity();
        if (TestType == "GAD7") return GetGad7Severity();
        if (TestType == "DASS21") return GetDepressionSeverity();

        string sev = SeverityLevel ?? "Moderate";
        return sev switch
        {
            "Minimal" => ("Minimal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e"),
            "Mild" => ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6"),
            "Moderate" => ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308"),
            "Moderately Severe" => ("Moderately Severe", "Moderately Severe", "#fff7ed", "#9a3412", "#fed7aa", "#f97316"),
            "Severe" or "High" => ("Severe", "Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444"),
            _ => ("Moderate", "Moderate", "#f8fafc", "#334155", "#e2e8f0", "#64748b")
        };
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetGad7Severity()
    {
        var s = TotalScore;
        if (s <= 4) return ("Minimal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e");
        if (s <= 9) return ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6");
        if (s <= 14) return ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308");
        return ("Severe", "Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444");
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetDepressionSeverity()
    {
        var s = DepressionScore > 0 ? DepressionScore : TotalScore;
        if (s <= 9) return ("Normal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e");
        if (s <= 13) return ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6");
        if (s <= 20) return ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308");
        if (s <= 27) return ("Severe", "Severe", "#fff7ed", "#9a3412", "#fed7aa", "#f97316");
        return ("Extremely Severe", "Extremely Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444");
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetAnxietySeverity()
    {
        var s = AnxietyScore;
        if (s <= 7) return ("Normal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e");
        if (s <= 9) return ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6");
        if (s <= 14) return ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308");
        if (s <= 19) return ("Severe", "Severe", "#fff7ed", "#9a3412", "#fed7aa", "#f97316");
        return ("Extremely Severe", "Extremely Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444");
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetStressSeverity()
    {
        var s = StressScore;
        if (s <= 14) return ("Normal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e");
        if (s <= 18) return ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6");
        if (s <= 25) return ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308");
        if (s <= 33) return ("Severe", "Severe", "#fff7ed", "#9a3412", "#fed7aa", "#f97316");
        return ("Extremely Severe", "Extremely Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444");
    }

    public (string En, string Vi, string Bg, string Text, string Border, string Bar) GetPhq9Severity()
    {
        var s = TotalScore;
        if (s <= 4) return ("Minimal", "Normal", "#f0fdf4", "#166534", "#bbf7d0", "#22c55e");
        if (s <= 9) return ("Mild", "Mild", "#eff6ff", "#1e40af", "#bfdbfe", "#3b82f6");
        if (s <= 14) return ("Moderate", "Moderate", "#fefce8", "#854d0e", "#fef08a", "#eab308");
        if (s <= 19) return ("Moderately Severe", "Moderately Severe", "#fff7ed", "#9a3412", "#fed7aa", "#f97316");
        return ("Severe", "Severe", "#fef2f2", "#991b1b", "#fecaca", "#ef4444");
    }
}
