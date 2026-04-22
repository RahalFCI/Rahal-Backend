using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories
{

    public class EmailVerificationRepository : GenericRepository<EmailVerificationToken>, IEmailVerificationRepository
    {
        public EmailVerificationRepository(UsersDbContext dbContext) : base(dbContext)
        {
        }
    }
}
