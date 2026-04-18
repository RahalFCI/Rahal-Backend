# Caching Implementation Quick Start Guide

## Quick Reference for Priority 1 Implementation

### 1. Cache User Roles (30 minutes)

**File**: `Users.Application/Services/AuthService.cs`

```csharp
// After getting user, cache the roles
var rolesCacheKey = $"user:{user.Id}:roles";
var cachedRoles = await _cacheService.GetAsync<IList<string>>(rolesCacheKey);

if (cachedRoles == null)
{
    cachedRoles = await _userManager.GetRolesAsync(user);
    await _cacheService.SetAsync(rolesCacheKey, cachedRoles, TimeSpan.FromMinutes(30), cancellationToken);
}

var roles = cachedRoles;
```

**Invalidation**: On role assignment in AdminController:
```csharp
await _userManager.AddToRoleAsync(user, newRole);
await _cacheService.RemoveAsync($"user:{user.Id}:roles");
```

---

### 2. Cache User Profiles (2-3 hours)

**Files**: 
- `Users.Application/Services/ExplorerService.cs`
- `Users.Application/Services/VendorService.cs`
- `Users.Application/Services/AdminService.cs`

**Template for ExplorerService.GetById()**:

```csharp
public async Task<ApiResponse<ExplorerDto>> GetById(Guid id, CancellationToken cancellationToken = default)
{
    var cacheKey = $"user:{id}:profile:explorer:full";
    var cached = await _cacheService.GetAsync<ExplorerDto>(cacheKey);
    if (cached != null)
        return ApiResponse<ExplorerDto>.Success(cached);

    var user = await _userManager.Users
        .AsNoTracking()
        .Include(u => u.ExplorerProfile)
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    if(user?.ExplorerProfile == null)
        return ApiResponse<ExplorerDto>.Failure(ErrorCode.NotFound);

    var userDto = _mapper.ToDto(user);
    await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(15), cancellationToken);
    
    return ApiResponse<ExplorerDto>.Success(userDto);
}
```

**Invalidation**: In profile update handler:
```csharp
await _cacheService.RemoveAsync($"user:{userId}:profile:explorer:full");
await _cacheService.RemoveAsync("explorer:all:summaries");  // List cache
```

---

### 3. Cache Vendor Categories (1 hour)

**File**: `Users.Infrastructure/Persistence/UsersDbContext.cs` or new initialization service

```csharp
// At startup or in a seeding service
public async Task SeedCategoriesCache(IServiceProvider serviceProvider)
{
    var cacheService = serviceProvider.GetRequiredService<ICacheService>();
    var context = serviceProvider.GetRequiredService<UsersDbContext>();
    
    var categories = await context.VendorCategories
        .AsNoTracking()
        .ToListAsync();
    
    await cacheService.SetAsync("global:vendor:categories:all", categories, 
        TimeSpan.FromHours(24));
}

// Register in Program.cs:
// app.MapGet("/seed", SeedCategoriesCache).WithOpenApi();
```

---

### 4. Fix N+1 in GetAllUsers() (1 hour)

**Current Code** (Inefficient):
```csharp
public async Task<ApiResponse<IEnumerable<ExplorerSummaryDto>>> GetAllUsers(CancellationToken cancellationToken = default)
{
    var explorers = await _userManager.Users
        .Where(u => u.UserType == UserRoleEnum.Explorer)
        .Include(u => u.ExplorerProfile)
        .ToListAsync(cancellationToken);

    var summaries = explorers
        .Where(u => u.ExplorerProfile != null)
        .Select(u => _mapper.ToSummary(u))
        .Cast<ExplorerSummaryDto>()
        .ToList();

    return ApiResponse<IEnumerable<ExplorerSummaryDto>>.Success(summaries);
}
```

**Fixed Code** (with cache + optimization):
```csharp
public async Task<ApiResponse<IEnumerable<ExplorerSummaryDto>>> GetAllUsers(CancellationToken cancellationToken = default)
{
    const string cacheKey = "explorer:all:summaries";
    
    // Try cache
    var cached = await _cacheService.GetAsync<IEnumerable<ExplorerSummaryDto>>(cacheKey);
    if (cached != null)
        return ApiResponse<IEnumerable<ExplorerSummaryDto>>.Success(cached);

    // Single optimized query with AsNoTracking
    var explorers = await _userManager.Users
        .AsNoTracking()  // Important: read-only query
        .Where(u => u.UserType == UserRoleEnum.Explorer && u.ExplorerProfile != null)
        .Include(u => u.ExplorerProfile)
        .ToListAsync(cancellationToken);

    var summaries = explorers
        .Select(u => _mapper.ToSummary(u))
        .Cast<ExplorerSummaryDto>()
        .ToList();

    // Cache for 30 minutes
    await _cacheService.SetAsync(cacheKey, summaries, TimeSpan.FromMinutes(30), cancellationToken);

    return ApiResponse<IEnumerable<ExplorerSummaryDto>>.Success(summaries);
}
```

---

## Testing the Implementation

### Unit Test Example
```csharp
[TestClass]
public class UserProfileCachingTests
{
    private Mock<ICacheService> _mockCache;
    private ExplorerService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockCache = new Mock<ICacheService>();
        _service = new ExplorerService(_userManager, _mapper, _profileService, _mediator, _mockCache.Object, _logger);
    }

    [TestMethod]
    public async Task GetById_WithCachedProfile_ReturnsFromCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cacheKey = $"user:{userId}:profile:explorer:full";
        var cachedDto = new ExplorerDto { Id = userId };
        
        _mockCache.Setup(c => c.GetAsync<ExplorerDto>(cacheKey, null))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _service.GetById(userId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(cachedDto, result.Data);
        _mockCache.Verify(c => c.GetAsync<ExplorerDto>(cacheKey, null), Times.Once);
    }

    [TestMethod]
    public async Task GetById_WithoutCachedProfile_FetchesFromDbAndCaches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cacheKey = $"user:{userId}:profile:explorer:full";
        
        _mockCache.Setup(c => c.GetAsync<ExplorerDto>(cacheKey, null))
            .ReturnsAsync((ExplorerDto)null);
        
        _mockCache.Setup(c => c.SetAsync(cacheKey, It.IsAny<ExplorerDto>(), 
            TimeSpan.FromMinutes(15), null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetById(userId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockCache.Verify(c => c.SetAsync(cacheKey, It.IsAny<ExplorerDto>(), 
            TimeSpan.FromMinutes(15), null), Times.Once);
    }
}
```

---

## Monitoring Query Performance

### Before/After Comparison

```csharp
public class QueryMetricsService
{
    private readonly ILogger<QueryMetricsService> _logger;
    
    public async Task<(int DbQueries, long DurationMs)> MeasureAsync(Func<Task> operation)
    {
        var startTime = DateTime.UtcNow;
        var queryCount = GetCurrentQueryCount();
        
        await operation();
        
        var endTime = DateTime.UtcNow;
        var finalQueryCount = GetCurrentQueryCount();
        
        _logger.LogInformation("Operation completed: {QueryCount} queries in {Duration}ms",
            finalQueryCount - queryCount, (endTime - startTime).TotalMilliseconds);
        
        return (finalQueryCount - queryCount, (long)(endTime - startTime).TotalMilliseconds);
    }
}
```

---

## Deployment Checklist

```
Phase 1: Week 1 (All 4 items)
- [ ] Cache user roles (AuthService)
- [ ] Cache user profiles (All user services)
- [ ] Fix N+1 GetAllUsers
- [ ] Cache vendor categories

Performance targets after Phase 1:
- [ ] Role queries reduced by 95%
- [ ] Profile queries reduced by 70%
- [ ] List endpoint queries reduced by 80%
- [ ] Database load reduced by 60%

Phase 2: Week 1-2 (High priority)
- [ ] Move OTPs to Redis cache
- [ ] Implement cache invalidation events

Phase 3: Week 2-3 (Medium priority)
- [ ] Cache user preferences
- [ ] Cache gamification data

Monitoring (Ongoing)
- [ ] Set up Redis memory monitoring
- [ ] Track database query reduction
- [ ] Monitor cache hit rates
- [ ] Alert on cache evictions
```

---

## Key Metrics to Monitor

### Expected Results After Full Implementation

| Metric | Before | After | % Improvement |
|--------|--------|-------|---|
| Login response time | 150ms | 90ms | 40% |
| Profile fetch time | 200ms | 50ms | 75% |
| GetAllUsers query time | 800ms | 100ms | 87% |
| DB queries/minute | 5000 | 1500 | 70% |
| Redis memory usage | N/A | ~5MB | N/A |

---

## Support & Troubleshooting

### Cache Not Being Used?
1. Check Redis connection: `await _redis.PingAsync()`
2. Verify cache key format matches expectations
3. Check TTL - may have expired
4. Enable debug logging: `ILogger<CacheService>` logs

### High Memory Usage?
1. Check for keys not expiring: `redis-cli KEYS *`
2. Reduce TTL values
3. Implement more aggressive invalidation
4. Set `maxmemory-policy allkeys-lru`

### Stale Cache?
1. Verify invalidation events firing
2. Check cache clear operations
3. Reduce TTL for frequently changing data
4. Implement cache versioning

---

**Ready to implement? Start with Phase 1 for maximum impact with minimal effort!**
