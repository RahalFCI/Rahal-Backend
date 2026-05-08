using Gamification.Domain.Enums;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Gamification.Domain.Entities
{
    public class ExplorerProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        public GenderEnum Gender { get; set; }

        public DateOnly BirthDate { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;

        public int AvailableXp { get; set; } = 0;

        public int CumulativeXp { get; set; } = 0;

        public int Level { get; set; } = 1;

        public int Streak { get; set; } = 0;

        public bool IsPublic { get; set; } = true;

        public bool IsPremium { get; set; } = false;

        public Guid PlanTierId { get; set; } = Guid.Empty;

        public UserStats? Stats { get; set; }
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
