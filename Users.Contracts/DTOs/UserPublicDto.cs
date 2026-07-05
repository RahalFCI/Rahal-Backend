using System;

namespace Users.Contracts.DTOs
{
    public class UserPublicDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
