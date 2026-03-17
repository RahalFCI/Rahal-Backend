# Refactoring Guide: From Inheritance to Profile Tables

## Changes Summary

This document outlines the refactoring from Table-Per-Concrete (TPC) inheritance model to a normalized profile table structure.

## What Has Been Changed

### 1. **Domain Entities** ✅
- `User.cs`: Changed from abstract class with inherited properties to concrete class containing only common properties
  - Added `UserType` column (UserRoleEnum)
  - Added navigation properties to profile tables (ExplorerProfile, VendorProfile, AdminProfile)
  - Removed abstract keyword

- `ExplorerProfile.cs`: **NEW** - Separate table for Explorer-specific data
  - Contains: Gender, BirthDate, Bio, CountryCode, XP, Level, IsPublic, IsPremium, PlanTierId
  - One-to-one relationship with User

- `VendorProfile.cs`: **NEW** - Separate table for Vendor-specific data
  - Contains: CountryCode, Address, AddressUrl, WorkingHours (JSON), CategoryId, IsApproved
  - One-to-one relationship with User

- `AdminProfile.cs`: **NEW** - Separate table for Admin-specific data
  - Contains: Department, CreatedAt
  - One-to-one relationship with User
  - Extensible for future admin-specific properties

### 2. **DbContext** ✅
- Updated `UsersDbContext` to use profile tables
- Removed: `DbSet<Explorer>`, `DbSet<Vendor>`, `DbSet<Admin>`
- Added: `DbSet<ExplorerProfile>`, `DbSet<VendorProfile>`, `DbSet<AdminProfile>`
- Removed TPC mapping strategy
- Added comprehensive one-to-one relationship configurations
- Added proper indexes and constraints

## What Still Needs to Be Changed (❌ = ACTION REQUIRED)

### 3. **Old Entity Classes** ❌ NEEDS UPDATE
Files that still use inheritance:
- `Domain/Entities/Explorer.cs` - Still inherits from User
- `Domain/Entities/Vendor.cs` - Still inherits from User
- `Domain/Entities/Admin.cs` - Still inherits from User

**Action Required:**
Delete these files or make them obsolete. Replace all references with:
- `User` entity
- `User.ExplorerProfile`, `User.VendorProfile`, `User.AdminProfile`

### 4. **AuthService<TUser>** ❌ NEEDS UPDATE
**Current Issue:** Generic type TUser still expects Explorer/Vendor/Admin classes

**What needs to change:**
```csharp
// OLD
public class AuthService<TUser> : IAuthService<TUser> where TUser : User
{
    // Methods expect TUser to be specific type
}

// NEW - Should be
public class AuthService : IAuthService
{
    // Work directly with User class
    // Use UserType to determine profile type
}
```

**Affected Methods:**
- `RegisterAsync(User user, string Password)` - ✅ Already uses User
- `LoginAsync(AuthRequestDto)` - ✅ Already uses User
- `LogoutAsync()` - ✅ Already uses User

**Changes Required:**
1. Remove generic type parameter from AuthService
2. Register single instance in DependencyInjection (not per user type)
3. Update IAuthService interface to not be generic

### 5. **UserService<TUser, TDto, TSummary>** ❌ NEEDS UPDATE
**Current Issue:** Generic constraints still reference old inheritance model

**What needs to change:**
The generic approach won't work well with profile tables. Need to:

```csharp
// Create specialized services
public class ExplorerService : IUserService<ExplorerDto, ExplorerSummaryDto>
{
    // Work with User + ExplorerProfile
}

public class VendorService : IUserService<VendorDto, VendorSummaryDto>
{
    // Work with User + VendorProfile
}

public class AdminService : IUserService<AdminDto, AdminSummaryDto>
{
    // Work with User + AdminProfile
}
```

OR keep generic but adjust:
```csharp
public interface IUserService<TDto, TSummary>
{
    UserRoleEnum UserType { get; } // Specify which type this service handles
    Task<ApiResponse<TDto>> GetById(Guid id, CancellationToken cancellationToken);
    // ...
}
```

**Methods that need updates:**
- `GetAllUsers()` - Need to filter by UserType
- `GetById()` - Need to load correct profile
- `UpdateUser()` - Need to update correct profile
- `DeleteUser()` - Need to cascade to profile

### 6. **IAuthService & IUserService Interfaces** ❌ NEEDS UPDATE
- Remove generic type constraints that reference old classes
- Update method signatures if needed

### 7. **Controllers** ❌ NEEDS UPDATE
**Current Issue:** Controllers injected with `IAuthService<Explorer>`, `IUserService<Explorer, ExplorerDto, ExplorerSummaryDto>`

**Changes Required:**
```csharp
// OLD
public class ExplorerController
{
    private readonly IAuthService<Explorer> _authService;
    private readonly IUserService<Explorer, ExplorerDto, ExplorerSummaryDto> _userService;
}

// NEW
public class ExplorerController
{
    private readonly IAuthService _authService;
    private readonly IUserService<ExplorerDto, ExplorerSummaryDto> _userService;
}
```

All three controllers (ExplorerController, VendorController, AdminController) need updates.

### 8. **Mappers & DTOs** ❌ NEEDS UPDATE
**Current Issue:** Mappers use inheritance hierarchy which no longer exists

**What needs to change:**

1. **Mapping Extension Methods** need to handle new structure:
```csharp
// OLD
public static Explorer ToEntity(this RegisterExplorerDto dto) => new() { ... };
public static ExplorerDto ToDto(this Explorer user) => new() { ... };

// NEW
public static User ToEntity(this RegisterExplorerDto dto) { ... }
public static ExplorerProfile CreateExplorerProfile(this RegisterExplorerDto dto) { ... }
public static ExplorerDto ToDto(this User user, ExplorerProfile profile) { ... }
```

2. **IUserMapper** interface needs adjustment:
```csharp
// Approach 1: Remove inheritance references
public interface IUserMapper<TDto, TSummary>
{
    TDto ToDto(User user, ExplorerProfile profile); // or similar
    TSummary ToSummary(User user, ExplorerProfile profile);
}

// Approach 2: Keep as-is but implement differently
```

3. **DTOs** can stay mostly the same, but:
   - `BaseUserDto` already correct
   - Remove references to inherited Entity types
   - Update mappings to load profiles

### 9. **DependencyInjection Registration** ❌ NEEDS UPDATE
**Current Code:**
```csharp
services.AddScoped<IAuthService<Explorer>, AuthService<Explorer>>();
services.AddScoped<IAuthService<Vendor>, AuthService<Vendor>>();
services.AddScoped<IAuthService<Admin>, AuthService<Admin>>();
services.AddScoped<IUserService<Explorer, ExplorerDto, ExplorerSummaryDto>, ...>();
// etc.
```

**Needs to become:**
```csharp
// Single instances
services.AddScoped<IAuthService, AuthService>();

// Or type-specific services
services.AddScoped<IExplorerService, ExplorerService>();
services.AddScoped<IVendorService, VendorService>();
services.AddScoped<IAdminService, AdminService>();
```

### 10. **Database Migration** ❌ NEEDS CREATION
**Required Steps:**
1. Delete old tables (Explorers, Vendors, Admins)
2. Create new tables (ExplorerProfiles, VendorProfiles, AdminProfiles)
3. Add UserType column to AspNetUsers
4. Migrate existing data (if any)

**Migration Command:**
```bash
dotnet ef migrations add RefactorToProfileTables -p Users.Infrastructure -s Rahal.Api
dotnet ef database update
```

## Files That Need Deletion

- `Domain/Entities/Explorer.cs`
- `Domain/Entities/Vendor.cs`
- `Domain/Entities/Admin.cs`

These should be replaced with Profile classes which have already been created.

## Implementation Order

1. ✅ Update Domain Entities (User, Profile classes)
2. ✅ Update DbContext
3. ❌ Delete old inheritance classes (Explorer, Vendor, Admin)
4. ❌ Update AuthService (remove generics or keep single instance)
5. ❌ Update UserService (handle profiles)
6. ❌ Update Mappers (handle User + Profile mapping)
7. ❌ Update Controllers (DI changes)
8. ❌ Update DependencyInjection registration
9. ❌ Create and run database migration
10. ❌ Test all endpoints

## Key Considerations

### Database Queries
Profile data loading will require explicit Include():
```csharp
var user = await _userManager.Users
    .Include(u => u.ExplorerProfile)
    .FirstOrDefaultAsync(u => u.Id == id);
```

### Null Checking
Since profiles are optional, always check:
```csharp
if (user.ExplorerProfile is null)
    // Handle missing profile
```

### Type Determination
Always use UserType:
```csharp
var profile = user.UserType switch
{
    UserRoleEnum.Explorer => user.ExplorerProfile,
    UserRoleEnum.Vendor => user.VendorProfile,
    UserRoleEnum.Admin => user.AdminProfile,
    _ => throw new InvalidOperationException()
};
```

## Questions for User

Once you review this document, please clarify:

1. **Service Architecture**: Should I:
   - Create separate services per user type (ExplorerService, VendorService, AdminService)?
   - Or keep generic service with UserType filtering?

2. **Controller Registration**: Should each controller register its own service or share one?

3. **Migration Data**: Do you have existing users that need migration?

4. **Feature Additions**: Do you need service-specific features (methods unique to each type)?
