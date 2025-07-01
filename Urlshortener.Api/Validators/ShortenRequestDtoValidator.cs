using FluentValidation;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Validators
{
    public class ShortenRequestDtoValidator : AbstractValidator<ShortenRequestDto>
    {
        public ShortenRequestDtoValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("OriginalUrl is required.")
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("OriginalUrl must be a valid absolute URL.");
        }
    }
} 