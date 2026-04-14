using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities._Common;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{

    public class EmailVerificationToken : BaseAuditableEntity
    {

        public required Guid UserId { get; set; }
 
        public required User User { get; set; }

        public required string CodeHash { get; set; }

        public required DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public int AttemptCount { get; set; } = 0;

        public const int MaxAttempts = 5;
    }
}
