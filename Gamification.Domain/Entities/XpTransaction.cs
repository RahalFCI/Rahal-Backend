using Gamification.Domain.Enums;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace Gamification.Domain.Entities
{
    public class XpTransaction : BaseEntity
    {
        public Guid ExplorerProfileId { get; set; } = Guid.Empty;
        public ExplorerProfile? ExplorerProfile { get; set; }
        public int Amount { get; set; } = 0;
        public XpSourceType Source { get; set; }
        public Guid ReferenceId { get; set; } = Guid.Empty;
    }
}
