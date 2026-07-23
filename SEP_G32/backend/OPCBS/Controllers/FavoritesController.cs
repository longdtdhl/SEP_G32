using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/favorites")]
[Authorize(Roles = RoleConstants.Patient)]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteDoctorService _favoriteService;

    public FavoritesController(IFavoriteDoctorService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/v1/favorites - Get all favorite doctors</summary>
    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var result = await _favoriteService.GetFavoritesAsync(GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/favorites/{doctorId} - Add doctor to favorites</summary>
    [HttpPost("{doctorId:guid}")]
    public async Task<IActionResult> AddFavorite(Guid doctorId)
    {
        var result = await _favoriteService.AddFavoriteAsync(GetUserId(), doctorId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/favorites/{doctorId} - Remove doctor from favorites</summary>
    [HttpDelete("{doctorId:guid}")]
    public async Task<IActionResult> RemoveFavorite(Guid doctorId)
    {
        var result = await _favoriteService.RemoveFavoriteAsync(GetUserId(), doctorId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/favorites/{doctorId}/check - Check if doctor is in favorites</summary>
    [HttpGet("{doctorId:guid}/check")]
    public async Task<IActionResult> IsFavorite(Guid doctorId)
    {
        var result = await _favoriteService.IsFavoriteAsync(GetUserId(), doctorId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
