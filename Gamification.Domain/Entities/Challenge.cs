using Gamification.Domain.Enums;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class Challenge : BaseEntity
    {
        public Guid PlaceId { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ChallengeDifficulty Difficulty { get; set; }
        public ChallengeType Type { get; set; }
        public int MinimumLevelRequired { get; set; }
        public int XpReward{ get; set; }
        public IEnumerable<CheckInChallenge> CheckInChallenges { get; set; } = new List<CheckInChallenge>();
    }
}
