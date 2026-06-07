using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.VendorCategories
{
    public record DeleteVendorCategoryCommand(Guid CategoryId) : IRequest<ApiResponse<string>>;
}
