using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Persistence;

public class DesignTimeFarContextFactory : IDesignTimeDbContextFactory<farContext>
{
    public farContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("farContext")
                               ?? configuration["ConnectionStrings:farContext"];

        // Allow EF tooling (e.g. migrations list) even when secrets are not present.
        // For database update, provide a real connection string via appsettings.Local.json or env vars.
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Smart_Farm_DesignTime;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<farContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new farContext(optionsBuilder.Options);
    }
}

