using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BankApp.Server.DataAccess
{
    /// <summary>
    /// Used by EF Core tools (Package Manager Console, dotnet ef) so migrations do not depend
    /// on the Visual Studio startup project or WinUI client output paths.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(AppDbContextFactory).Assembly.Location)
                ?? AppContext.BaseDirectory;
            string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", ".."));

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found. Check appsettings.json in BankApp.Server.");
            }

            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
