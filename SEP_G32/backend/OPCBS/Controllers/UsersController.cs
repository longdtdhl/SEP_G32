using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// User profile management - GET/PUT /api/v1/users
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UpdateUserProfileDto> _updateProfileValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

    public UsersController(
        IUserService userService,
        IValidator<UpdateUserProfileDto> updateProfileValidator,
        IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _userService = userService;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    /// <summary>GET /api/v1/users/profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _userService.GetProfileAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>PUT /api/v1/users/profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
    {
        var validation = await _updateProfileValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage));

        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _userService.UpdateProfileAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/users/change-password</summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var validation = await _changePasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage));

        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _userService.ChangePasswordAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
