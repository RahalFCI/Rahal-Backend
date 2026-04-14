using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for EmailVerificationToken entity
    /// Provides data access operations for email verification tokens
    /// </summary>
    public class EmailVerificationRepository : GenericRepository<EmailVerificationToken>, IEmailVerificationRepository
    {
        public EmailVerificationRepository(UsersDbContext dbContext) : base(dbContext)
        {
        }
    }
}
