using FluentValidation;
using VolunteerAppSecurity.DTOs;

namespace VolunteerAppSecurity.ValidatorsDTO
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(u => u.Email)
            .NotEmpty()
            .WithMessage("This field is required")
            .EmailAddress();

            RuleFor(u => u.Password)
                .NotEmpty()
                .WithMessage("This field is required");
        }
    }
}
