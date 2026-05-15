using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Web.Infrastructure;

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

builder.Services.AddSingleton<IAccountRepoProxy, AccountRepoProxy>();
builder.Services.AddSingleton<IAuthRepoProxy, AuthRepoProxy>();
builder.Services.AddSingleton<ICardRepoProxy, CardRepoProxy>();
builder.Services.AddSingleton<IChatRepoProxy, ChatRepoProxy>();
builder.Services.AddSingleton<IDashboardRepoProxy, DashboardRepoProxy>();
builder.Services.AddSingleton<IInvestmentsRepoProxy, InvestmentsRepoProxy>();
builder.Services.AddSingleton<ILoanApplicationPresentationRepoProxy, LoanApplicationPresentationRepoProxy>();
builder.Services.AddSingleton<ILoanDialogStateRepoProxy, LoanDialogStateRepoProxy>();
builder.Services.AddSingleton<ILoansRepoProxy, LoansRepoProxy>();
builder.Services.AddSingleton<IProfileRepoProxy, ProfileRepoProxy>();
builder.Services.AddSingleton<ISavingsPresentationRepoProxy, SavingsPresentationRepoProxy>();
builder.Services.AddSingleton<ISavingsRepoProxy, SavingsRepoProxy>();
builder.Services.AddSingleton<ISavingsUiRulesRepoProxy, SavingsUiRulesRepoProxy>();
builder.Services.AddSingleton<ISavingsWorkflowRepoProxy, SavingsWorkflowRepoProxy>();
builder.Services.AddSingleton<IStatisticsRepoProxy,  StatisticsRepoProxy>();
builder.Services.AddSingleton<ITransactionRepoProxy, TransactionRepoProxy>();


// ADD CLIENT SERVICES AS SINGLETONS

builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<ICardService, CardService>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IInvestmentsService, InvestmentsService>();
builder.Services.AddSingleton<ILoansService, LoansService>();
builder.Services.AddSingleton<INotificationClientService, NotificationClientService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();
builder.Services.AddSingleton<ISavingsService, SavingsService>();
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();
builder.Services.AddSingleton<ITransactionHistoryService, TransactionHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

