using VolunteerAppSecurity.DTOs;
using FluentValidation;

namespace VolunteerAppSecurity.ValidatorsDTO
{
    public class EmailDTOValidator : AbstractValidator<EmailDTO>
    {
        public EmailDTOValidator()
        {
            RuleFor(x => x.To)
                .NotEmpty()
                .WithMessage("This field is required")
                .EmailAddress();
        }
    }
}
