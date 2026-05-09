using BankApp.Server.Configuration;
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Middleware;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Infrastructure.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- INVESTMENTS & TRADING REGISTRATION ---
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
//builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddSingleton<IMarketDataService, MarketDataService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<TeamCOptions>(builder.Configuration.GetSection(TeamCOptions.SectionName));

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your JWT token here"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(_ => new AppDbContext(connectionString!));
//builder.Services.AddDbContext<BankAppContext>(_options => _options.UseSqlServer(connectionString!));

// --- DATA ACCESS OBJECTS ---
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<ISessionDAO, SessionDAO>();
builder.Services.AddScoped<IOAuthLinkDAO, OAuthLinkDAO>();
builder.Services.AddScoped<IPasswordResetTokenDAO, PasswordResetTokenDAO>();
builder.Services.AddScoped<INotificationPreferenceDAO, NotificationPreferenceDAO>();
builder.Services.AddScoped<IAccountDAO, AccountDAO>();
builder.Services.AddScoped<ICardDAO, CardDAO>();
builder.Services.AddScoped<ITransactionDAO, TransactionDAO>();
builder.Services.AddScoped<INotificationDAO, NotificationDAO>();
builder.Services.AddScoped<IUserCardPreferenceDAO, UserCardPreferenceDAO>();
builder.Services.AddScoped<IChatDAO, ChatDAO>();
builder.Services.AddScoped<IChatMessageDAO, ChatMessageDAO>();

// --- INFRASTRUCTURE SERVICES ---
builder.Services.AddScoped<IHashService, HashService>();
string? jwtSecret = builder.Configuration["Jwt:Secret"];
builder.Services.AddScoped<IJWTService>(_ => new JWTService(jwtSecret!));
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// --- REPOSITORIES ---
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ChatMessageRepository>();

// --- BUSINESS SERVICES ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ITransactionExportService, TransactionExportService>();
builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IApiService, ApiService>();

var app = builder.Build();

app.UseExceptionHandler(applicationBuilder => applicationBuilder.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<SessionValidationMiddleware>();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception exception)
    {
        Console.WriteLine($"UNHANDLED: {exception.Message}");
        Console.WriteLine($"Stack: {exception.StackTrace}");
        throw;
    }
});

app.MapControllers();
app.Run();