using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Events.Profiles
{
    public record DeleteProfileEvent
    {
        public Guid UserId { get; init; }
        public required string Role { get; init; }
        public bool IsPermanent { get; init; }
    }
}
