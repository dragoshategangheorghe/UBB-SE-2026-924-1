using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<RequireSessionLoginFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<RequireSessionLoginFilter>();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.LoginPath = "/Auth";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddScoped<IWebSessionContext, WebSessionContext>();
builder.Services.AddScoped<ApiService>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var sessionContext = serviceProvider.GetRequiredService<IWebSessionContext>();
    var apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5024";
    var apiService = new ApiService(apiBaseUrl);

    if (!string.IsNullOrWhiteSpace(sessionContext.AccessToken))
    {
        apiService.SetToken(sessionContext.AccessToken);
    }

    if (sessionContext.CurrentUserId.HasValue)
    {
        apiService.SetCurrentUserId(sessionContext.CurrentUserId.Value);
    }

    return apiService;
});

// ADD Client Proxy Repositories as singletons (because they don't actually store any data)
// there are no dependencies below them, besides giving them the ApiService, which is truly independent

builder.Services.AddScoped<IAccountRepoProxy, AccountRepoProxy>();
builder.Services.AddScoped<IAuthRepoProxy, AuthRepoProxy>();
builder.Services.AddScoped<ICardRepoProxy, CardRepoProxy>();
builder.Services.AddScoped<IChatRepoProxy, ChatRepoProxy>();
builder.Services.AddScoped<IDashboardRepoProxy, DashboardRepoProxy>();
builder.Services.AddScoped<IInvestmentsRepoProxy, InvestmentsRepoProxy>();
builder.Services.AddScoped<ILoanApplicationPresentationRepoProxy, LoanApplicationPresentationRepoProxy>();
builder.Services.AddScoped<ILoanDialogStateRepoProxy, LoanDialogStateRepoProxy>();
builder.Services.AddScoped<ILoansRepoProxy, LoansRepoProxy>();
builder.Services.AddScoped<IProfileRepoProxy, ProfileRepoProxy>();
builder.Services.AddScoped<ISavingsPresentationRepoProxy, SavingsPresentationRepoProxy>();
builder.Services.AddScoped<ISavingsRepoProxy, SavingsRepoProxy>();
builder.Services.AddScoped<ISavingsUiRulesRepoProxy, SavingsUiRulesRepoProxy>();
builder.Services.AddScoped<ISavingsWorkflowRepoProxy, SavingsWorkflowRepoProxy>();
builder.Services.AddScoped<IStatisticsRepoProxy, StatisticsRepoProxy>();
builder.Services.AddScoped<ITransactionRepoProxy, TransactionRepoProxy>();


// ADD CLIENT SERVICES AS SINGLETONS

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IInvestmentsService, InvestmentsService>();
builder.Services.AddScoped<ILoansService, LoansService>();
builder.Services.AddScoped<INotificationClientService, NotificationClientService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

