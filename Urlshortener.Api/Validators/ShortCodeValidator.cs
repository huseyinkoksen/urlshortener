using FluentValidation;

namespace UrlShortener.Api.Validators
{
    public class ShortCodeValidator : AbstractValidator<string>
    {
        public ShortCodeValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage("Short code is required.")
                .Length(6).WithMessage("Short code must be 6 characters.")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("Short code must be alphanumeric.");
        }
    }
} 