using FluentValidation;
using OPCBS.Application.DTOs.Auth;
using System.Text.RegularExpressions;

namespace OPCBS.Application.Validators;

/// <summary>
/// Validator for user registration
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(50).WithMessage("Password cannot exceed 50 characters")
            .Must(BeValidPassword).WithMessage("Password must contain uppercase, lowercase, and number");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Must(BeValidVietnamesePhoneNumber).WithMessage("Invalid Vietnamese phone number format");
    }

    private static bool BeValidPassword(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, "[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, "[a-z]");
        var hasNumber = Regex.IsMatch(password, "[0-9]");
        return hasUpperCase && hasLowerCase && hasNumber;
    }

    private static bool BeValidVietnamesePhoneNumber(string phoneNumber)
    {
        // Vietnamese phone format: 0xxx xxxxxxx or +84 xxx xxxxxxx
        var pattern = @"^(0\d{9,10}|(\+84|0084)\d{9,10})$";
        return Regex.IsMatch(phoneNumber, pattern);
    }
}

/// <summary>
/// Validator for OTP verification
/// </summary>
public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
{
    public VerifyOtpDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP code must be exactly 6 digits");
    }
}

/// <summary>
/// Validator for login
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

/// <summary>
/// Validator for forgot password
/// </summary>
public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

/// <summary>
/// Validator for reset password
/// </summary>
public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP code must be exactly 6 digits");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(50).WithMessage("Password cannot exceed 50 characters")
            .Must(BeValidPassword).WithMessage("Password must contain uppercase, lowercase, and number");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }

    private static bool BeValidPassword(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, "[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, "[a-z]");
        var hasNumber = Regex.IsMatch(password, "[0-9]");
        return hasUpperCase && hasLowerCase && hasNumber;
    }
}

/// <summary>
/// Validator for change password
/// </summary>
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(50).WithMessage("Password cannot exceed 50 characters")
            .Must(BeValidPassword).WithMessage("Password must contain uppercase, lowercase, and number");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }

    private static bool BeValidPassword(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, "[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, "[a-z]");
        var hasNumber = Regex.IsMatch(password, "[0-9]");
        return hasUpperCase && hasLowerCase && hasNumber;
    }
}

/// <summary>
/// Validator for doctor registration
/// </summary>
public class RegisterDoctorDtoValidator : AbstractValidator<RegisterDoctorDto>
{
    public RegisterDoctorDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(50).WithMessage("Password cannot exceed 50 characters")
            .Must(BeValidPassword).WithMessage("Password must contain uppercase, lowercase, and number");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Must(BeValidVietnamesePhoneNumber).WithMessage("Invalid Vietnamese phone number format");

        RuleFor(x => x.ProfessionalTitle)
            .NotEmpty().WithMessage("Professional title is required")
            .MaximumLength(255).WithMessage("Professional title cannot exceed 255 characters");

        RuleFor(x => x.Biography)
            .NotEmpty().WithMessage("Biography is required")
            .MaximumLength(5000).WithMessage("Biography cannot exceed 5000 characters");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years cannot be negative")
            .LessThanOrEqualTo(60).WithMessage("Experience years cannot exceed 60");

        RuleFor(x => x.SpecializationIds)
            .NotEmpty().WithMessage("At least one specialization is required");
    }

    private static bool BeValidPassword(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, "[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, "[a-z]");
        var hasNumber = Regex.IsMatch(password, "[0-9]");
        return hasUpperCase && hasLowerCase && hasNumber;
    }

    private static bool BeValidVietnamesePhoneNumber(string phoneNumber)
    {
        var pattern = @"^(0\d{9,10}|(\+84|0084)\d{9,10})$";
        return Regex.IsMatch(phoneNumber, pattern);
    }
}

/// <summary>
/// Validator for update doctor profile
/// </summary>
public class UpdateDoctorProfileDtoValidator : AbstractValidator<UpdateDoctorProfileDto>
{
    public UpdateDoctorProfileDtoValidator()
    {
        RuleFor(x => x.ProfessionalTitle)
            .MaximumLength(255).WithMessage("Professional title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.ProfessionalTitle));

        RuleFor(x => x.Biography)
            .MaximumLength(5000).WithMessage("Biography cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Biography));

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years cannot be negative")
            .LessThanOrEqualTo(60).WithMessage("Experience years cannot exceed 60")
            .When(x => x.ExperienceYears.HasValue);
    }
}

/// <summary>
/// Validator for update user profile
/// </summary>
public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidVietnamesePhoneNumber!).WithMessage("Invalid Vietnamese phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private static bool BeValidVietnamesePhoneNumber(string phoneNumber)
    {
        var pattern = @"^(0\d{9,10}|(\+84|0084)\d{9,10})$";
        return Regex.IsMatch(phoneNumber, pattern);
    }
}
