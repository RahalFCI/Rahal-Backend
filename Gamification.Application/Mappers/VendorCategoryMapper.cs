using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.DTOs.VendorCategory;
using Gamification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Mappers
{

    public static class VendorCategoryMapper
    {
        public static GetVendorCategoryDto ToGetDto(VendorCategory Category)
        {
            return new GetVendorCategoryDto(Category.Id, Category.CategoryName);
        }

        public static IEnumerable<GetVendorCategoryDto> ToGetDtos(IEnumerable<VendorCategory?> categories)
        {
            return categories.Where(a => a != null).Select(a => ToGetDto(a!));
        }
    }
}
