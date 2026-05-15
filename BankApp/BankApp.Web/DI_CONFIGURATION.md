# Dependency Injection Configuration Summary

## Program.cs - Dependency Injection Setup

### Current Configuration (Already in Your Program.cs)

The following DI registrations are **already present** and correctly configured in your `Program.cs`:

---

## 1. Repository Proxies Registration

```csharp
// ADD Client Proxy Repositories (line ~60)
builder.Services.AddScoped<IAccountRepoProxy, AccountRepoProxy>();
builder.Services.AddScoped<IStatisticsRepoProxy, StatisticsRepoProxy>();
// ... other repositories
```

---

## 2. Services Registration

```csharp
// ADD CLIENT SERVICES (line ~85)
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
// ... other services
```

---

## 3. Authentication & Authorization Setup

```csharp
// Authentication Configuration (line ~34)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Auth/";  // Redirects unauthorized access here
    });

// Session Configuration (line ~15)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

---

## 4. Middleware Configuration

```csharp
// In app configuration (line ~115)
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
```

---

## Dependency Injection Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│           ASP.NET Core DI Container                      │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  IAccountService ──→ AccountService                       │
│       ↑                      ↓                            │
│       │              Uses: IAccountRepoProxy             │
│       │                     ↓                            │
│       │              AccountRepoProxy                     │
│       │                     ↓                            │
│       │              Uses: ApiService                    │
│       │                                                  │
│       └─ Constructor Injection (No Manual new)           │
│                                                           │
│  IStatisticsService ──→ StatisticsService               │
│       ↑                      ↓                           │
│       │              Uses: IStatisticsRepoProxy          │
│       │                     ↓                           │
│       │              StatisticsRepoProxy                 │
│       │                     ↓                           │
│       │              Uses: ApiService                   │
│       │                                                 │
│       └─ Constructor Injection (No Manual new)          │
│                                                         │
└────────────────────────────────────────────────────────┘
```

---

## Controller Injection Examples

### AccountsController

```csharp
[Authorize]
public class AccountsController : Controller
{
    private readonly IAccountService _accountService;

    // ✅ Service injected via constructor
    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;  // No new MyService() - DI container provides it
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await _accountService.GetAccountsAsync();
        return View(accounts);
    }
}
```

### StatisticsController

```csharp
[Authorize]
public class StatisticsController : Controller
{
    private readonly IStatisticsService _statisticsService;

    // ✅ Service injected via constructor
    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;  // No new MyService() - DI container provides it
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var spendingByCategory = await _statisticsService.GetSpendingByCategoryAsync();
        var incomeVsExpenses = await _statisticsService.GetIncomeVsExpensesAsync();
        var balanceTrends = await _statisticsService.GetBalanceTrendsAsync();
        var topRecipients = await _statisticsService.GetTopRecipientsAsync();

        var viewModel = new
        {
            SpendingByCategory = spendingByCategory,
            IncomeVsExpenses = incomeVsExpenses,
            BalanceTrends = balanceTrends,
            TopRecipients = topRecipients
        };

        return View(viewModel);
    }
}
```

---

## Security - [Authorize] Attribute

### How It Works

1. **Controller-level Authorization**:
   ```csharp
   [Authorize]  // ← ALL actions in this controller require authentication
   public class AccountsController : Controller
   {
       // ...
   }
   ```

2. **Automatic Redirect**:
   - Unauthenticated requests → ASP.NET Core Middleware
   - Middleware checks [Authorize] attribute
   - If unauthorized → Redirect to `options.AccessDeniedPath` (`/Auth/`)

3. **Session Validation**:
   - Additional check in service: `EnsureAuthenticatedSession()`
   - Throws `UnauthorizedAccessException` if session invalid
   - Controller catches and redirects to Auth page

---

## Service Initialization Chain

### Request Flow for Accounts

```
1. User navigates to /Accounts
                    ↓
2. ASP.NET Core calls AccountsController.Index()
                    ↓
3. DI Container resolves IAccountService → Creates AccountService instance
                    ↓
4. AccountService constructor called:
   - Receives IAccountRepoProxy from DI container
   - Receives IAuthService from DI container
                    ↓
5. AccountService.GetAccountsAsync() called:
   - Calls EnsureAuthenticatedSession()
   - Calls IAccountRepoProxy.GetAuthenticatedAccountsAsync()
                    ↓
6. ApiService makes HTTP request to backend
                    ↓
7. Data returned to view
                    ↓
8. View renders with Bootstrap + jQuery
```

---

## Key Configuration Points

### ✅ Must-Have Registrations

| Type | Interface | Implementation | Scope |
|------|-----------|----------------|-------|
| Service | IAccountService | AccountService | Scoped |
| Service | IStatisticsService | StatisticsService | Scoped |
| Proxy | IAccountRepoProxy | AccountRepoProxy | Scoped |
| Proxy | IStatisticsRepoProxy | StatisticsRepoProxy | Scoped |
| Auth | IAuthService | AuthService | Scoped |

### ✅ Critical Middleware Order

```csharp
app.UseSession();           // ← Must come before Authentication
app.UseAuthentication();    // ← Must come before Authorization
app.UseAuthorization();     // ← Must come after Authentication
```

---

## Authorization Attribute Usage

### Protecting Individual Actions

```csharp
[HttpGet]
[Authorize]  // ← Only this action requires auth
public IActionResult AdminOnly()
{
    // ...
}
```

### Allowing Anonymous Access

```csharp
[AllowAnonymous]  // ← Override controller-level [Authorize]
public IActionResult PublicPage()
{
    // ...
}
```

---

## Session Management

### Session Timeouts

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // ← Session expires after 30 min inactivity
    options.Cookie.HttpOnly = true;                  // ← Prevent JavaScript access
    options.Cookie.IsEssential = true;               // ← Require consent
});
```

### Cookie Authentication

```csharp
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);  // ← Cookie expires after 20 min
    options.SlidingExpiration = true;                   // ← Reset on each request
    options.AccessDeniedPath = "/Auth/";                // ← Redirect unauthorized access
});
```

---

## Error Handling Pattern

### Service-Level Error Handling

```csharp
public async Task<IEnumerable<Account>> GetAccountsAsync()
{
    EnsureAuthenticatedSession();  // ← Throws if not authenticated
    return await this.repo.GetAuthenticatedAccountsAsync();
}

private void EnsureAuthenticatedSession()
{
    if (!this.authService.IsAuthenticated())
    {
        throw new UnauthorizedAccessException("An authenticated session is required.");
    }
}
```

### Controller-Level Error Handling

```csharp
public async Task<IActionResult> Index()
{
    try
    {
        var accounts = await _accountService.GetAccountsAsync();
        return View(accounts);
    }
    catch (UnauthorizedAccessException)
    {
        return RedirectToAction("Index", "Auth");  // ← Redirect to login
    }
}
```

---

## Testing the Configuration

### 1. Test Account Service Injection

```csharp
// In AccountsController
public AccountsController(IAccountService accountService)
{
    _accountService = accountService;
    // If DI not configured: "Unable to resolve service for type IAccountService"
    // If configured correctly: Service instance created successfully
}
```

### 2. Test Authorization

```
1. Navigate to http://localhost:5000/Accounts (without authentication)
   ↓
2. Should redirect to http://localhost:5000/Auth/ (AccessDeniedPath)
   ↓
3. After login, navigate to http://localhost:5000/Accounts
   ↓
4. Should display accounts page successfully
```

### 3. Test Session Timeout

```
1. Login to application
2. Wait for session to expire (30 minutes idle)
3. Try to access /Accounts
4. Should redirect to /Auth/
```

---

## Quick Reference - What's Already Configured

| Component | Status | Location |
|-----------|--------|----------|
| IAccountService DI | ✅ Registered | Program.cs line ~88 |
| IStatisticsService DI | ✅ Registered | Program.cs line ~95 |
| IAccountRepoProxy DI | ✅ Registered | Program.cs line ~59 |
| IStatisticsRepoProxy DI | ✅ Registered | Program.cs line ~75 |
| Authentication Middleware | ✅ Configured | Program.cs line ~34 |
| Session Middleware | ✅ Configured | Program.cs line ~15 |
| Authorization Middleware | ✅ Configured | Program.cs line ~119 |
| AccountsController | ✅ Implemented | Controllers/AccountsController.cs |
| StatisticsController | ✅ Implemented | Controllers/StatisticsController.cs |
| Accounts View | ✅ Created | Views/Accounts/Index.cshtml |
| Statistics View | ✅ Created | Views/Statistics/Index.cshtml |

---

## Summary

✅ **All DI services are pre-configured**
✅ **All authentication/authorization middleware is set up**
✅ **Controllers properly inject services via constructor**
✅ **[Authorize] attribute protects endpoints**
✅ **Unauthorized access redirects to login page**
✅ **Session management enforces 30-minute timeout**
✅ **Error handling gracefully manages authentication failures**

**No additional configuration needed - implementation is complete!**
