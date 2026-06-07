using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.DTOs.VendorCategory;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.VendorCategories
{
    public record CreateVendorCategoryCommand(string CategoryName) : IRequest<ApiResponse<GetVendorCategoryDto>>;
}
