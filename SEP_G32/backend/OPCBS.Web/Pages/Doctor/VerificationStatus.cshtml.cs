using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class VerificationStatusModel : PageModel
{
    private readonly IVerificationApiService _api;
    public VerificationStatusModel(IVerificationApiService api) => _api = api;
    public VerificationDto? Verification { get; set; }

    public async Task OnGetAsync()
    {
        var (data, _) = await _api.GetMyVerificationAsync();
        Verification = data;
    }
}
