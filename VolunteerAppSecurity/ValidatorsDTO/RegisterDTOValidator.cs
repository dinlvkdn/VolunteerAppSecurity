using FluentValidation;
using VolunteerAppSecurity.DTOs;

namespace VolunteerAppSecurity.ValidatorsDTO
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
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
