using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // Standalone Consultation Notes list is deprecated.
        // Redirect doctors to Patient Records where Consultation History is centered.
        return RedirectToPage("/Doctor/Patients/Index");
    }
}
