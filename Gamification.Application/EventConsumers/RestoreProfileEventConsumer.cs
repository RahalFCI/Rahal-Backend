using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
using MassTransit;
using MediatR;
using Shared.Application.Events.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.EventConsumers
{
    public class RestoreProfileEventConsumer : IConsumer<RestoreProfileEvent>
    {
        private readonly IMediator _mediator;

        public RestoreProfileEventConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<RestoreProfileEvent> context)
        {
            var cancellationToken = context.CancellationToken;

            var userId = context.Message.UserId;
            var role = context.Message.Role;

            if (role == "Explorer")
            {  
                await _mediator.Send(new RestoreDeletedExplorerProfileCommand(userId), cancellationToken);
            }
            else if (role == "Vendor")
            {
                await _mediator.Send(new RestoreDeletedVendorProfileCommand(userId), cancellationToken);
            }
        }
    }
}
