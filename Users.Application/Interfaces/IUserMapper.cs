 using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Generic interface for mapping between User + Profile and DTOs
    /// Each user type (Explorer, Vendor, Admin) has its own mapper implementation
    /// </summary>
    public interface IUserMapper<TDto, TSummary>
        where TDto : BaseUserDto
        where TSummary : BaseUserSummaryDto
    {
        /// <summary>
        /// Map User entity (with loaded profile) to summary DTO
        /// </summary>
        TSummary ToSummary(User user);

        /// <summary>
        /// Map User entity (with loaded profile) to full DTO
        /// </summary>
        TDto ToDto(User user);
    }
}

