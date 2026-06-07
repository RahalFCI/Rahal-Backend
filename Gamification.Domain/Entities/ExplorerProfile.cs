using Gamification.Domain.Enums;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Gamification.Domain.Entities
{
    public class ExplorerProfile : BaseEntity
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ProfilePictureURL { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public GenderEnum Gender { get; set; }

        public DateOnly BirthDate { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;

        public int Level { get; set; } = 1;

        public bool IsPublic { get; set; } = true;

        public bool IsPremium { get; set; } = false;

        public Guid? PlanTierId { get; set; } = Guid.Empty;

        public UserStats? Stats { get; set; }

        public IEnumerable<ExplorerAchievement> ExplorerAchievements { get; set; } = new List<ExplorerAchievement>();
        public IEnumerable<XpTransaction> XpTransactions { get; set; } = new List<XpTransaction>();
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.DayOfYear > today.DayOfYear)
                    age--;
                return age;
            }
        }
    }
}
