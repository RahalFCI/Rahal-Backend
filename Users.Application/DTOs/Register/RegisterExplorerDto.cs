using Shared.Domain.Enums;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Register
{
    public record RegisterExplorerDto(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        DateOnly BirthDate,
        GenderEnum Gender,
        string Bio,
        string CountryCode,
        bool IsPublic)
    {
        public RegisterExplorerDto() : this(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            default,
            default,
            string.Empty,
            string.Empty,
            true)
        {
        }

        public BaseRegisterDto ToBaseRegisterDto() => new(
            Name,
            Email,
            Password,
            ConfirmPassword,
            PhoneNumber,
            UserRoleEnum.Explorer);
    }
}
