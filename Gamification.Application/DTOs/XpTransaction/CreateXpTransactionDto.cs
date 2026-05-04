namespace Gamification.Application.DTOs.XpTransaction
{
    public class CreateXpTransactionDto
    {
        public Guid ExplorerId { get; set; }
        public int Amount { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public Guid ReferenceId { get; set; }
    }
}
