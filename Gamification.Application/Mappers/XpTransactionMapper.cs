using Gamification.Application.DTOs.XpTransaction;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class XpTransactionMapper
    {
        public static GetXpTransactionDto ToGetDto(XpTransaction xpTransaction)
        {
            return new GetXpTransactionDto
            {
                Id = xpTransaction.Id,
                ExplorerId = xpTransaction.ExplorerProfileId,
                Amount = xpTransaction.Amount,
                SourceType = xpTransaction.Source.ToString(),
                ReferenceId = xpTransaction.ReferenceId,
                CreatedAt = xpTransaction.CreatedAt
            };
        }

        public static XpTransaction ToEntity(CreateXpTransactionDto dto)
        {
            return new XpTransaction
            {
                ExplorerProfileId = dto.ExplorerId,
                Amount = dto.Amount,
                Source = Enum.Parse<Gamification.Domain.Enums.XpSourceType>(dto.SourceType),
                ReferenceId = dto.ReferenceId
            };
        }

        public static IEnumerable<GetXpTransactionDto> ToGetDtos(IEnumerable<XpTransaction> xpTransactions)
        {
            return xpTransactions.Select(ToGetDto);
        }
    }
}
