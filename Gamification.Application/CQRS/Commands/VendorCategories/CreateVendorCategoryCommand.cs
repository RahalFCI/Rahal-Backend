using Gamification.Application.DTOs.AchievementCriteriaType;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.VendorCategories
{
    public record CreateVendorCategoryCommand(string CategoryName) : IRequest<ApiResponse<string>>;
    {
    }
}
