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
/// Authentication endpoints - /api/v1/auth
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<VerifyOtpDto> _otpValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotValidator;
    private readonly IValidator<ResetPasswordDto> _resetValidator;
    private readonly IValidator<RegisterDoctorDto> _registerDoctorValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<VerifyOtpDto> otpValidator,
        IValidator<ForgotPasswordDto> forgotValidator,
        IValidator<ResetPasswordDto> resetValidator,
        IValidator<RegisterDoctorDto> registerDoctorValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _otpValidator = otpValidator;
        _forgotValidator = forgotValidator;
        _resetValidator = resetValidator;
        _registerDoctorValidator = registerDoctorValidator;
    }

    /// <summary>POST /api/v1/auth/register - Register patient account</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/auth/register-doctor - Register doctor account (not in spec but needed)</summary>
    [HttpPost("register-doctor")]
    public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto)
    {
        var validation = await _registerDoctorValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.RegisterDoctorAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/auth/verify-otp</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var validation = await _otpValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.VerifyOtpAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/auth/login</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>POST /api/v1/auth/google-login - Google OAuth (stub, configurable per SYS-05)</summary>
    [HttpPost("google-login")]
    public IActionResult GoogleLogin()
    {
        return BadRequest(ApiResponse.ErrorResponse("Google OAuth is not configured for this environment."));
    }

    /// <summary>POST /api/v1/auth/forgot-password</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var validation = await _forgotValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.ForgotPasswordAsync(dto);
        return Ok(result); // Always OK per security best practice
    }

    /// <summary>POST /api/v1/auth/reset-password</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var validation = await _resetValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.ResetPasswordAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/auth/refresh-token</summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(ApiResponse.ErrorResponse("Refresh token is required."));

        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>POST /api/v1/auth/logout</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _authService.LogoutAsync(userId.Value);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
