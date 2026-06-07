using Gamification.Application.DTOs.VendorCategory;
using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.VendorCategories
{
    public record GetAllVendorCategoriesQuery() : IRequest<ApiResponse<IEnumerable<GetVendorCategoryDto>>>;

}
