using Shared.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{

    public interface IEmailVerificationRepository : IGenericRepository<EmailVerificationToken>
    {
    }
}
