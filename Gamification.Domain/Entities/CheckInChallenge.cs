using Gamification.Domain.Enums;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class CheckInChallenge : BaseEntity
    {
        public Guid ChallengeId { get; set; } = Guid.Empty;
        public virtual Challenge? Challenge { get; set; }
        public Guid CheckInId { get; set; } = Guid.Empty;
        public Guid ExplorerId { get; set; } = Guid.Empty;
        public string ProofUrl { get; set; } = string.Empty;
        public ChallengeValidationStatus ValidationStatus { get; set; }

    }
}
