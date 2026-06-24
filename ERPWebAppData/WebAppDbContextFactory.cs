using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApp.Data;

public sealed class WebAppDbContextFactory : IDesignTimeDbContextFactory<WebAppDbContext>
{
    public WebAppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? ReadConnectionString(FindAppSettingsDirectory());

        var options = new DbContextOptionsBuilder<WebAppDbContext>()
            .UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            })
            .Options;

        return new WebAppDbContext(options);
    }

    private static string ReadConnectionString(string appSettingsDirectory)
    {
        var environmentName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var appSettingsPath = Path.Combine(appSettingsDirectory, "appsettings.json");
        var environmentAppSettingsPath = string.IsNullOrWhiteSpace(environmentName)
            ? null
            : Path.Combine(appSettingsDirectory, $"appsettings.{environmentName}.json");

        if (!string.IsNullOrWhiteSpace(environmentAppSettingsPath)
            && File.Exists(environmentAppSettingsPath)
            && TryReadConnectionString(environmentAppSettingsPath, out var environmentConnectionString))
        {
            return environmentConnectionString;
        }

        if (TryReadConnectionString(appSettingsPath, out var connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"Connection string 'Default' was not found in '{appSettingsPath}'.");
    }

    private static bool TryReadConnectionString(string appSettingsPath, out string connectionString)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(appSettingsPath),
            new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.TryGetProperty("Default", out var defaultConnectionString)
            && !string.IsNullOrWhiteSpace(defaultConnectionString.GetString()))
        {
            connectionString = defaultConnectionString.GetString()!;
            return true;
        }

        connectionString = string.Empty;
        return false;
    }

    private static string FindAppSettingsDirectory()
    {
        foreach (var startingPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startingPath);

            while (directory is not null)
            {
                var directPath = Path.Combine(directory.FullName, "appsettings.json");
                if (File.Exists(directPath))
                {
                    return directory.FullName;
                }

                var serverPath = Path.Combine(
                    directory.FullName,
                    "ERPWebApp.Server",
                    "appsettings.json");

                if (File.Exists(serverPath))
                {
                    return Path.GetDirectoryName(serverPath)!;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            "Could not locate ERPWebApp.Server/appsettings.json for EF Core design-time operations.");
    }
}
