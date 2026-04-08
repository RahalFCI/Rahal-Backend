using FluentValidation;
using Users.Application.DTOs.EmailVerification;

namespace Users.Application.Validators
{
    public class VerifyOtpValidator : AbstractValidator<VerifyOtpRequest>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("OTP must contain only digits");
        }
    }

    public class ResendOtpValidator : AbstractValidator<ResendOtpRequest>
    {
        public ResendOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }
}
