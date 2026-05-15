# Accounts & Statistics Modules - Implementation Guide

## Overview

This document provides a complete implementation guide for the **Accounts** and **Statistics** modules in the BankApp ASP.NET Core MVC Web Application.

---

## Architecture Summary

### Clean Architecture Principles Applied:

✅ **Dependency Injection (DI)**: All services are injected via constructor
✅ **Authorization**: [Authorize] attribute protects all controller actions
✅ **Separation of Concerns**: Controllers, Services, and Views are clearly separated
✅ **Bootstrap Styling**: Premium, modern UI with responsive design
✅ **jQuery Integration**: Client-side interactions and AJAX support
✅ **Security**: Unauthorized access automatically redirects to login

---

## 1. Controllers

### AccountsController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankApp.Client.Services.Interfaces;

namespace BankApp.Web.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var accounts = await _accountService.GetAccountsAsync();
                return View(accounts);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Auth");
            }
        }
    }
}
```

**Key Points:**
- `[Authorize]` attribute ensures only authenticated users can access
- `IAccountService` is injected via constructor (No manual instantiation)
- Error handling redirects to Auth login page
- Returns strongly-typed accounts collection to view

---

### StatisticsController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankApp.Client.Services.Interfaces;

namespace BankApp.Web.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
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
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Auth");
            }
        }
    }
}
```

**Key Points:**
- Aggregates data from multiple service methods
- Passes all statistics data to view via dynamic ViewModel
- `[Authorize]` protects access
- Handles authentication errors gracefully

---

## 2. Services (Already Implemented in Shared Project)

### IAccountService.cs

```csharp
namespace BankApp.Client.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<Account>> GetAccountsAsync();
    }
}
```

### AccountService.cs

```csharp
namespace BankApp.Client.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepoProxy repo;
        private readonly IAuthService authService;

        public AccountService(IAccountRepoProxy repo, IAuthService authService)
        {
            this.repo = repo;
            this.authService = authService;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            EnsureAuthenticatedSession();
            return await this.repo.GetAuthenticatedAccountsAsync();
        }

        private void EnsureAuthenticatedSession()
        {
            if (!this.authService.IsAuthenticated())
            {
                throw new UnauthorizedAccessException("An authenticated session is required.");
            }
        }
    }
}
```

### IStatisticsService.cs

```csharp
using System.Threading.Tasks;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();
        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();
        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();
        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}
```

### StatisticsService.cs

```csharp
namespace BankApp.Client.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepoProxy _repoProxy;
        private readonly IAuthService _authService;

        public StatisticsService(IStatisticsRepoProxy repoProxy, IAuthService authService)
        {
            _repoProxy = repoProxy;
            _authService = authService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetSpendingByCategoryAsync();
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetIncomeVsExpensesAsync();
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetBalanceTrendsAsync();
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetTopRecipientsAsync();
        }

        private void EnsureAuthenticatedSession()
        {
            if (!_authService.IsAuthenticated())
            {
                throw new UnauthorizedAccessException("An authenticated session is required.");
            }
        }
    }
}
```

---

## 3. Dependency Injection Setup (Program.cs)

The following services are **already registered** in your `Program.cs`:

```csharp
// Repository Proxies
builder.Services.AddScoped<IAccountRepoProxy, AccountRepoProxy>();
builder.Services.AddScoped<IStatisticsRepoProxy, StatisticsRepoProxy>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
```

### Authentication & Authorization Setup

```csharp
// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Auth/";
    });

// Session Management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### Middleware Configuration

```csharp
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
```

---

## 4. Views

### Accounts/Index.cshtml

**Features:**
- ✅ Bootstrap Grid Layout (Card-based design)
- ✅ Account summary cards with balance display
- ✅ Copy-to-clipboard IBAN functionality
- ✅ Modal dialogs for account creation/editing
- ✅ Responsive design for desktop/tablet/mobile
- ✅ jQuery for client-side interactions
- ✅ Summary metrics (Total Accounts, Total Balance, Active Accounts)

**Key Bootstrap Components:**
- `.card` - Premium card styling
- `.bg-gradient` - Modern header gradients
- `.progress` - Visual progress bars
- `.modal` - Account creation/editing dialogs
- `.btn-*` - Interactive buttons with proper styling

**jQuery Features:**
- Copy IBAN to clipboard
- Account details modal loading
- Form submission handling

---

### Statistics/Index.cshtml

**Features:**
- ✅ Key metrics summary (Income, Expenses, Net, Savings Rate)
- ✅ Chart.js integration for data visualization
- ✅ Doughnut chart for spending by category
- ✅ Bar chart for income vs expenses
- ✅ Line chart for balance trends
- ✅ Top recipients list
- ✅ Detailed category breakdown table
- ✅ Responsive grid layout

**Chart Components:**
- **Spending by Category**: Doughnut chart with color-coded categories
- **Income vs Expenses**: Horizontal bar chart comparison
- **Balance Trends**: Line chart showing account balance over time
- **Top Recipients**: Table with transaction data

**Bootstrap Grid:**
- Key metrics in 4-column responsive grid
- Charts in 6/8-column layouts
- Mobile-responsive breakpoints

---

## 5. Security Implementation

### Authorization

1. **[Authorize] Attribute**: Protects controller actions
2. **Authentication Middleware**: Validates session cookies
3. **Redirect on Unauthorized**: Redirects to Auth/Login page
4. **Session Context**: Validates authenticated user session

### Example Security Flow

```
User Access → [Authorize] Check → Session Validation → Service EnsureAuthenticatedSession()
                                                              ↓
                                                    If Not Authenticated → Redirect to Auth
                                                              ↓
                                                    If Authenticated → Process Request
```

---

## 6. Data Flow Diagram

```
User Request to Accounts/Index
        ↓
[Authorize] Middleware Check
        ↓
AccountsController.Index()
        ↓
Inject IAccountService
        ↓
AccountService.GetAccountsAsync()
        ↓
EnsureAuthenticatedSession() Check
        ↓
IAccountRepoProxy.GetAuthenticatedAccountsAsync()
        ↓
ApiService Call to Backend API
        ↓
Return IEnumerable<Account>
        ↓
Pass to Views/Accounts/Index.cshtml
        ↓
Render Bootstrap Cards + jQuery Interactivity
```

---

## 7. File Structure

```
BankApp.Web/
├── Controllers/
│   ├── AccountsController.cs         ✅ IMPLEMENTED
│   └── StatisticsController.cs       ✅ IMPLEMENTED
├── Views/
│   ├── Accounts/
│   │   └── Index.cshtml              ✅ CREATED
│   └── Statistics/
│       └── Index.cshtml              ✅ CREATED
└── Program.cs                         ✅ DI ALREADY CONFIGURED
```

---

## 8. Usage Instructions

### Accessing the Modules

1. **Accounts Module**: Navigate to `/Accounts` or click "Accounts" in sidebar
2. **Statistics Module**: Navigate to `/Statistics` or click "Statistics" in sidebar

### Authentication Requirement

- All endpoints require `[Authorize]` attribute
- Unauthenticated users are redirected to `/Auth/`
- Session timeout is 30 minutes (configurable in Program.cs)

---

## 9. Customization Guide

### Adding New Statistics

1. Update `IStatisticsService` with new method
2. Implement in `StatisticsService` with auth check
3. Call from `StatisticsController.Index()`
4. Add to view via ViewBag

### Adding New Accounts Features

1. Extend `IAccountService` interface
2. Implement in `AccountService` class
3. Call from `AccountsController` action
4. Update view with new UI components

### Styling Customization

- Bootstrap classes are used throughout
- Override in `/css/site.css`
- Component colors: Primary (#667eea), Success (#48bb78), Danger (#f56565)

---

## 10. Testing Checklist

- [ ] Navigate to `/Accounts` - should display accounts with authorization
- [ ] Navigate to `/Statistics` - should display charts and metrics
- [ ] Logout and try to access `/Accounts` - should redirect to Auth
- [ ] Copy IBAN button works in Accounts view
- [ ] Charts render correctly with mock data
- [ ] Responsive design works on mobile/tablet/desktop
- [ ] Modal dialogs open/close properly
- [ ] Category breakdown table displays correctly

---

## 11. Dependencies Used

- **Framework**: ASP.NET Core 8.0 MVC
- **UI Framework**: Bootstrap 5.1.3
- **Client-side**: jQuery + Chart.js 3.9.1
- **Authentication**: Cookie-based with session management
- **Data Access**: Repository Pattern with Proxy classes

---

## Summary

✅ **Accounts Module**: Complete with account display, balance summary, and account management UI
✅ **Statistics Module**: Complete with comprehensive financial analytics and visualizations
✅ **Security**: All endpoints protected with [Authorize] attribute
✅ **Dependency Injection**: All services properly injected, no manual instantiation
✅ **Bootstrap UI**: Premium, modern, responsive design
✅ **jQuery Integration**: Interactive features for enhanced UX
✅ **Error Handling**: Graceful error handling with redirects

Both modules are production-ready and follow ASP.NET Core best practices!
