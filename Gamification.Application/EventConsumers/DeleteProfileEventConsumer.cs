using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
using Gamification.Application.Interfaces;
using MassTransit;
using MediatR;
using Shared.Application.Events.Profiles;
using Shared.Domain.Events.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.EventConsumers
{
    public class DeleteProfileEventConsumer : IConsumer<DeleteProfileEvent>
    {
        private readonly IMediator _mediator;

        public DeleteProfileEventConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DeleteProfileEvent> context)
        {
            var cancellationToken = context.CancellationToken;

            var userId = context.Message.UserId;
            var IsPermanent = context.Message.IsPermanent;
            var role = context.Message.Role;

            if(role == "Explorer")
            {
                if (IsPermanent) 
                    await _mediator.Send(new PermanentDeleteExplorerProfileWithUserStatsOrchestrator(userId), cancellationToken);
                else
                    await _mediator.Send(new DeleteExplorerProfileWithUserStatsOrchestrator(userId), cancellationToken);
            }
            else if(role == "Vendor")
            {
                if (IsPermanent)
                    await _mediator.Send(new PermanentDeleteVendorProfileOrchestrator(userId), cancellationToken);
                else
                    await _mediator.Send(new DeleteVendorProfileCommand(userId), cancellationToken);
            }
        }
    }
}
