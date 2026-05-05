using Shared.Domain.Entities;

namespace Gamification.Domain.Entities
{
    public class AdminProfile : BaseEntity
    {
        public Guid UserId { get; set; }
    }
}
