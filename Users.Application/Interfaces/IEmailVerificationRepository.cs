using Shared.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Repository interface for EmailVerificationToken entity
    /// Inherits all standard repository operations from IGenericRepository
    /// </summary>
    public interface IEmailVerificationRepository : IGenericRepository<EmailVerificationToken>
    {
    }
}
