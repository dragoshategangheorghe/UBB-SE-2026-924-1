using BankApp.Client.RepoProxies;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add controllers to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
