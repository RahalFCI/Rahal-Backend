using Microsoft.Extensions.Logging;
using Rewards.Application.Interfaces;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewards.Infrastructure.Persistence
{
    public class RewardsUnitOfWork : UnitOfWork<RewardsDbContext>, IRewardsUnitOfWork
    {
        public RewardsUnitOfWork(RewardsDbContext context, ILogger<UnitOfWork<RewardsDbContext>> logger) : base(context, logger)
        {
        }
    }
}
