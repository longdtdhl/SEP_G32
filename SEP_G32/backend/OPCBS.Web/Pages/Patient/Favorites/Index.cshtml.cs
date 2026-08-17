using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Favorites;

[Authorize(Roles = "Patient")]
public class IndexModel : PageModel
{
    private readonly IFavoriteApiService _favoriteService;
    private readonly JwtCookieService _jwt;

    public IndexModel(IFavoriteApiService favoriteService, JwtCookieService jwt)
    {
        _favoriteService = favoriteService;
        _jwt = jwt;
    }

    public List<FavoriteDoctorWebDto> Favorites { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _favoriteService.GetFavoritesAsync();
        if (error != null)
            ErrorMessage = error;
        else
            Favorites = data;
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid doctorId)
    {
        await _favoriteService.RemoveFavoriteAsync(doctorId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetCheckAsync(Guid doctorId)
    {
        var (isFav, _) = await _favoriteService.IsFavoriteAsync(doctorId);
        return new JsonResult(new { isFavorite = isFav });
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid doctorId)
    {
        var (isFav, _) = await _favoriteService.IsFavoriteAsync(doctorId);
        if (isFav)
            await _favoriteService.RemoveFavoriteAsync(doctorId);
        else
            await _favoriteService.AddFavoriteAsync(doctorId);
        return new JsonResult(new { success = true, isFavorite = !isFav });
    }
}
