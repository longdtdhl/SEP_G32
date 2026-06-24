namespace OPCBS.Web.DTOs;

public record LoginRequestDto(string Email, string Password);
public record RegisterRequestDto(string Email, string Password, string FullName);
public record VerifyOtpRequestDto(string Email, string Otp);
public record AuthResponseDto(string Token);
