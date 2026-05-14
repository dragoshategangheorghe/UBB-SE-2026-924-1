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

builder.Services.AddScoped<ILoansRepoProxy, LoansRepoProxy>();
builder.Services.AddScoped<ILoanDialogStateRepoProxy, LoanDialogStateRepoProxy>();
builder.Services.AddScoped<ILoanApplicationPresentationRepoProxy, LoanApplicationPresentationRepoProxy>();
builder.Services.AddScoped<ISavingsRepoProxy, SavingsRepoProxy>();
builder.Services.AddScoped<ISavingsUiRulesRepoProxy, SavingsUiRulesRepoProxy>();
builder.Services.AddScoped<ISavingsPresentationRepoProxy, SavingsPresentationRepoProxy>();
builder.Services.AddScoped<ISavingsWorkflowRepoProxy, SavingsWorkflowRepoProxy>();

builder.Services.AddScoped<ILoansService, LoansService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();

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

