using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Interfaces
{
    public interface IProfileChecker
    {
        Task<bool> HasProfileAsync(Guid userId, string? role);
    }
}
