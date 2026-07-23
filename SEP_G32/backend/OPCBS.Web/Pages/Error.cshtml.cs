using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages;

public class ErrorModel : PageModel
{
    public new int? StatusCode { get; set; }

    public void OnGet([FromQuery] int? code)
    {
        StatusCode = code ?? HttpContext.Response.StatusCode;
    }
}
