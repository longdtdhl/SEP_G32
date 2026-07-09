using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Patient.Psychometrics;

public class ResultModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public ResultModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    public PsychometricSubmissionDto? Submission { get; set; }
    public string? Error { get; set; }

    public int DepressionScore { get; set; }
    public int AnxietyScore { get; set; }
    public int StressScore { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid submissionId)
    {
        var (data, error) = await _psychService.GetSubmissionByIdAsync(submissionId);
        if (data == null)
        {
            Error = error ?? "Không tìm thấy kết quả trắc nghiệm.";
            return Page();
        }

        Submission = data;

        if (data.TestType == "DASS21")
        {
            try
            {
                using var doc = JsonDocument.Parse(data.ScoreDataJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("Depression", out var dep)) DepressionScore = dep.GetInt32();
                if (root.TryGetProperty("Anxiety", out var anx)) AnxietyScore = anx.GetInt32();
                if (root.TryGetProperty("Stress", out var str)) StressScore = str.GetInt32();
            }
            catch
            {
                // Ignore parse failures
            }
        }

        return Page();
    }
}
