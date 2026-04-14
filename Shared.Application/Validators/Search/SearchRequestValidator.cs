using FluentValidation;
using Shared.Application.DTOs.Search;

namespace Shared.Application.Validators.Search
{
    /// <summary>
    /// Validator for SearchRequestDto
    /// Validates search parameters before processing
    /// </summary>
    public class SearchRequestValidator : AbstractValidator<SearchRequestDto>
    {
        public SearchRequestValidator()
        {
            // Query validation
            RuleFor(x => x.Query)
                .NotEmpty()
                .WithMessage("Search query is required")
                .WithErrorCode("SEARCH_QUERY_REQUIRED")
                .MaximumLength(500)
                .WithMessage("Search query cannot exceed 500 characters")
                .WithErrorCode("SEARCH_QUERY_TOO_LONG");

            // Page validation
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0")
                .WithErrorCode("INVALID_PAGE_NUMBER");

            // PageSize validation
            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0")
                .WithErrorCode("INVALID_PAGE_SIZE")
                .LessThanOrEqualTo(1000)
                .WithMessage("PageSize cannot exceed 1000")
                .WithErrorCode("PAGE_SIZE_TOO_LARGE");

            // Filter validation (optional, but if provided should be reasonable length)
            RuleFor(x => x.Filter)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Filter))
                .WithMessage("Filter expression cannot exceed 1000 characters")
                .WithErrorCode("FILTER_TOO_LONG");

            // SortBy validation (optional, but if provided should be reasonable length)
            RuleFor(x => x.SortBy)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
                .WithMessage("Sort expression cannot exceed 500 characters")
                .WithErrorCode("SORT_TOO_LONG");
        }
    }
}
