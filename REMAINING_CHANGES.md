# Remaining Changes Required - Implementation Instructions

## 1. **Update IUserMapper Interface**
File: `Users.Application/Interfaces/IUserMapper.cs`

```csharp
using System;
using Users.Application.DTOs._Common;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    public interface IUserMapper<TDto, TSummary>
        where TDto : BaseUserDto
        where TSummary : BaseUserSummaryDto
    {
        TSummary ToSummary(User user);
        TDto ToDto(User user);
    }
}
```

**Changes:**
- Remove generic TUser constraint
- Work directly with User class
- Profile data loaded from User.ExplorerProfile/VendorProfile/AdminProfile

## 2. **Update Mapper Implementations**

### ExplorerMapper
```csharp
public class ExplorerMapper : IUserMapper<ExplorerDto, ExplorerSummaryDto>
{
    public ExplorerDto ToDto(User user)
    {
        if (user.ExplorerProfile == null)
            throw new InvalidOperationException("Explorer profile not found");

        return new ExplorerDto(
            Id: user.Id,
            Name: user.DisplayName,
            Email: user.Email!,
            PhoneNumber: user.PhoneNumber!,
            BirthDate: user.ExplorerProfile.BirthDate,
            gender: user.ExplorerProfile.Gender,
            Bio: user.ExplorerProfile.Bio,
            CountryCode: user.ExplorerProfile.CountryCode,
            AvailableXp: user.ExplorerProfile.AvailableXp,
            CumlativeXp: user.ExplorerProfile.CumulativeXp,
            Level: user.ExplorerProfile.Level,
            IsPublic: user.ExplorerProfile.IsPublic,
            IsPremium: user.ExplorerProfile.IsPremium,
            ProfilePictureUrl: user.ProfilePictureURL,
            Role: user.UserType
        );
    }

    public ExplorerSummaryDto ToSummary(User user)
    {
        if (user.ExplorerProfile == null)
            throw new InvalidOperationException("Explorer profile not found");

        return new ExplorerSummaryDto(
            Id: user.Id,
            Name: user.DisplayName,
            ProfilePictureUrl: user.ProfilePictureURL,
            Role: user.UserType,
            Bio: user.ExplorerProfile.Bio,
            CumlativeXp: user.ExplorerProfile.CumulativeXp,
            Level: user.ExplorerProfile.Level,
            IsPublic: user.ExplorerProfile.IsPublic,
            IsPremium: user.ExplorerProfile.IsPremium
        );
    }
}
```

### Similar changes for VendorMapper and AdminMapper

## 3. **Update Mapping Extensions (MappingExtension.cs)**

The extension methods need to handle User + Profile mapping:

```csharp
// These should map DTO to separate User and Profile
public static User ToUserEntity(this RegisterExplorerDto dto) => new()
{
    DisplayName = dto.Name,
    Email = dto.Email,
    UserName = dto.Email,
    PhoneNumber = dto.PhoneNumber,
    ProfilePictureURL = dto.ProfilePictureUrl ?? string.Empty,
    UserType = UserRoleEnum.Explorer,
};

public static ExplorerProfile ToExplorerProfile(this RegisterExplorerDto dto, Guid userId) => new()
{
    UserId = userId,
    Gender = dto.Gender,
    BirthDate = dto.BirthDate,
    Bio = dto.Bio,
    CountryCode = dto.CountryCode,
    IsPublic = dto.IsPublic,
};
```

## 4. **Update Controllers - DependencyInjection**

### ExplorerController
```csharp
public class ExplorerController : CustomControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService<ExplorerDto, ExplorerSummaryDto> _userService;
    private readonly IUserFactory<RegisterExplorerDto, Explorer> _userFactory;

    public ExplorerController(
        IAuthService authService,
        IUserService<ExplorerDto, ExplorerSummaryDto> userService,
        IUserFactory<RegisterExplorerDto, Explorer> userFactory)
    {
        _authService = authService;
        _userService = userService;
        _userFactory = userFactory;
    }

    // ... rest of methods stay the same
}
```

**Changes:**
- Remove generic type from `IAuthService<Explorer>`
- Update `IUserService` to use new interface signature

### Same for VendorController and AdminController

## 5. **Update DependencyInjection Registration**

File: `Users.Infrastructure/DependencyInjection.cs`

Replace:
```csharp
// OLD
services.AddScoped<IAuthService<Explorer>, AuthService<Explorer>>();
services.AddScoped<IAuthService<Vendor>, AuthService<Vendor>>();
services.AddScoped<IAuthService<Admin>, AuthService<Admin>>();
services.AddScoped<IUserService<Explorer, ExplorerDto, ExplorerSummaryDto>, ...>();
// etc.
```

With:
```csharp
// NEW
// Single Auth Service
services.AddScoped<IAuthService, AuthService>();

// Separate User Services
services.AddScoped<IUserService<ExplorerDto, ExplorerSummaryDto>, ExplorerService>();
services.AddScoped<IUserService<VendorDto, VendorSummaryDto>, VendorService>();
services.AddScoped<IUserService<AdminDto, AdminSummaryDto>, AdminService>();

// Keep factories and mappers
services.AddScoped<IUserFactory<RegisterExplorerDto, User>, ExplorerUserFactory>();
// ... update all factory registrations to map to User instead of specific types
```

## 6. **Update User Factories**

The factories now create User + Profile separately:

```csharp
public class ExplorerUserFactory : IUserFactory<RegisterExplorerDto, User>
{
    public User CreateUser(RegisterExplorerDto dto)
    {
        var user = dto.ToUserEntity();
        user.ExplorerProfile = dto.ToExplorerProfile(user.Id);
        return user;
    }
}
```

## 7. **Update Mapper Registrations in DependencyInjection**

```csharp
services.AddScoped<IUserMapper<User, ExplorerDto, ExplorerSummaryDto>, ExplorerMapper>();
services.AddScoped<IUserMapper<User, VendorDto, VendorSummaryDto>, VendorMapper>();
services.AddScoped<IUserMapper<User, AdminDto, AdminSummaryDto>, AdminMapper>();
```

## 8. **Create Database Migration**

```bash
cd Users.Infrastructure
dotnet ef migrations add RefactorToProfileTables -s ../Rahal.Api
dotnet ef database update
```

## Critical Issues to Watch For

### Issue 1: Profile Not Eagerly Loaded
When loading User without including profile, you'll get null. Always use:
```csharp
await _userManager.Users
    .Include(u => u.ExplorerProfile)
    .FirstOrDefaultAsync(...)
```

### Issue 2: Null Profile Checks
Always verify profile exists before accessing it. The services above handle this.

### Issue 3: Foreign Key References
Factory registration needs updating. Old registrations reference deleted classes:
```csharp
// These need updating:
IUserFactory<RegisterExplorerDto, Explorer>  // <- Explorer doesn't exist
// Should be:
IUserFactory<RegisterExplorerDto, User>  // <- Now returns User
```

## Implementation Priority

1. **CRITICAL - DependencyInjection** - Without this, services won't wire up
2. **CRITICAL - Controllers** - Update DI injection signatures
3. **HIGH - Mappers** - Update all mapping logic
4. **HIGH - Mapping Extensions** - Update DTO->Entity mappings
5. **MEDIUM - User Factories** - Update to use new User + Profile approach
6. **LOW - Database Migration** - Only after code compiles

## Testing Checklist

After all changes:
- [ ] Solution compiles without errors
- [ ] All services wire up correctly
- [ ] Database migration applies successfully
- [ ] Can register new Explorer
- [ ] Can register new Vendor
- [ ] Can register new Admin
- [ ] Can login as each user type
- [ ] Can fetch user by ID
- [ ] Can update user profile
- [ ] Can update password
- [ ] Can delete user

## Token-Saving Implementation Approach

For remaining files, use multi_replace_string_in_file to batch updates and save tokens.
