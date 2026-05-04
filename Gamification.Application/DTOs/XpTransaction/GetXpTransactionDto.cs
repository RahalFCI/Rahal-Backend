namespace Gamification.Application.DTOs.XpTransaction
{
    public class GetXpTransactionDto
    {
        public Guid Id { get; set; }
        public Guid ExplorerId { get; set; }
        public int Amount { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
