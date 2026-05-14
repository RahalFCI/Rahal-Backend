using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Utils
{
    public class ProfileChecker : IProfileChecker
    {
        private readonly IMediator _mediator;

        public ProfileChecker(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> HasProfileAsync(Guid userId, string? role)
        {
            return role switch
            {
                "Explorer" => (await _mediator.Send(new GetExplorerProfileByUserIdQuery(userId))).IsSuccess,
                "Vendor" => (await _mediator.Send(new GetVendorProfileByUserIdQuery(userId))).IsSuccess,
                "Admin" => true, // admins have no profile requirement
                _ => false
            };
        }
    }
}
