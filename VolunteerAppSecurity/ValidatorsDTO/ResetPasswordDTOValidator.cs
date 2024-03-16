using FluentValidation;
using VolunteerAppSecurity.DTOs;

namespace VolunteerAppSecurity.ValidatorsDTO
{
    public class ResetPasswordDTOValidator : AbstractValidator<ResetPasswordDTO>
    {
        public ResetPasswordDTOValidator()
        {
            RuleFor(x => x.Email)
              .NotEmpty()
              .WithMessage("This field is required")
              .EmailAddress();

            RuleFor(x => x.NewPassword)
              .NotEmpty()
              .WithMessage("This field is required")
              .EmailAddress();
        }
    }  
}
