using Gamification.Application.CQRS.Commands.ExplorerProfiles;
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
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public DeleteProfileEventConsumer(IGamificationUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DeleteProfileEvent> context)
        {
            var cancellationToken = context.CancellationToken;

            var userId = context.Message.UserId;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // TODO: call delete ProfileOrchestrator

            await _unitOfWork.CommitTransactionAsync(cancellationToken);



        }
    }
}
