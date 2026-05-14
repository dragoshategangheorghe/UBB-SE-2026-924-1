using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddScoped<ApiService>(_ => new ApiService("http://localhost:5024"));
builder.Services.AddScoped<ICardRepoProxy, CardRepoProxy>();
builder.Services.AddScoped<ICardService, CardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

