using Shared.Application.Interfaces;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Interfaces
{
    public interface IGamificationRepository<TEntity>
    : IGenericRepository<TEntity>
    where TEntity : BaseEntity
    {
    }
}
