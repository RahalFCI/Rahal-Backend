using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence;

namespace Gamification.Infrastructure.Persistence
{
    public class GamificationUnitOfWork : UnitOfWork<GamificationDbContext>, IGamificationUnitOfWork
    {
        public GamificationUnitOfWork(GamificationDbContext context, ILogger<UnitOfWork<GamificationDbContext>> logger) : base(context, logger)
        {
        }
    }
}
