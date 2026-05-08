using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Commands
{
    public class PermenantDeleteXpTransactionCommandHandler : IRequestHandler<PermenantDeleteXpTransactionCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<XpTransaction> _repository;
        private readonly ILogger<PermenantDeleteXpTransactionCommandHandler> _logger;

        public PermenantDeleteXpTransactionCommandHandler(
            IGenericRepository<XpTransaction> repository,
            ILogger<PermenantDeleteXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting XP transaction {TransactionId}", request.Id);

            var transaction = await _repository.GetTable().Where(t => t.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (transaction is null)
            {
                _logger.LogWarning("XP transaction {TransactionId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(transaction);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("XP transaction {TransactionId} deleted", request.Id);
            return ApiResponse<string>.Success("XP transaction deleted successfully");
        }
    }
}
