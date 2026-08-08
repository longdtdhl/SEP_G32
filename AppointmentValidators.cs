using FluentValidation;
using OPCBS.Application.DTOs.Appointments;

namespace OPCBS.Application.Validators;

/// <summary>
/// Validator for creating appointments
/// </summary>
public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentDtoValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required");

        RuleFor(x => x.AppointmentSlotId)
            .NotEmpty().WithMessage("Appointment slot is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Guest booking validation: when GuestName is provided, require all guest fields
        RuleFor(x => x.GuestName)
            .MaximumLength(255).WithMessage("Guest name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.GuestName));

        RuleFor(x => x.GuestEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.GuestEmail));

        RuleFor(x => x.GuestPhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.GuestPhoneNumber));
    }
}


/// <summary>
/// Validator for canceling appointments
/// </summary>
public class CancelAppointmentDtoValidator : AbstractValidator<CancelAppointmentDto>
{
    public CancelAppointmentDtoValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Cancellation reason cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

/// <summary>
/// Validator for rejecting appointments
/// </summary>
public class RejectAppointmentDtoValidator : AbstractValidator<RejectAppointmentDto>
{
    public RejectAppointmentDtoValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

/// <summary>
/// Validator for tracking appointments
/// </summary>
public class TrackAppointmentDtoValidator : AbstractValidator<TrackAppointmentDto>
{
    public TrackAppointmentDtoValidator()
    {
        RuleFor(x => x.BookingCode)
            .NotEmpty().WithMessage("Booking code is required")
            .MaximumLength(50).WithMessage("Booking code cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
