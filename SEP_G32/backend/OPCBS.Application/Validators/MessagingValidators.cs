using FluentValidation;
using OPCBS.Application.DTOs.Messaging;

namespace OPCBS.Application.Validators;

/// <summary>Validator for SendMessageDto</summary>
public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.AttachmentUrl))
            .WithMessage("Message must have content or an attachment.");

        When(x => !string.IsNullOrWhiteSpace(x.AttachmentUrl), () =>
        {
            RuleFor(x => x.AttachmentUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Attachment URL must be a valid URL.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Content), () =>
        {
            RuleFor(x => x.Content)
                .MaximumLength(4000)
                .WithMessage("Message content cannot exceed 4000 characters.");
        });
    }
}
