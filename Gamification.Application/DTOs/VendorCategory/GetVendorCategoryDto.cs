using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.VendorCategory
{
    public record GetVendorCategoryDto(
        Guid Id,
        string Name
    );
}
