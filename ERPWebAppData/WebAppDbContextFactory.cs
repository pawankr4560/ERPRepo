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
            ?? ReadConnectionString(FindAppSettingsPath());

        var options = new DbContextOptionsBuilder<WebAppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new WebAppDbContext(options);
    }

    private static string ReadConnectionString(string appSettingsPath)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(appSettingsPath),
            new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

        if (document.RootElement
            .GetProperty("ConnectionStrings")
            .TryGetProperty("Default", out var connectionString)
            && !string.IsNullOrWhiteSpace(connectionString.GetString()))
        {
            return connectionString.GetString()!;
        }

        throw new InvalidOperationException(
            $"Connection string 'Default' was not found in '{appSettingsPath}'.");
    }

    private static string FindAppSettingsPath()
    {
        foreach (var startingPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startingPath);

            while (directory is not null)
            {
                var directPath = Path.Combine(directory.FullName, "appsettings.json");
                if (File.Exists(directPath))
                {
                    return directPath;
                }

                var serverPath = Path.Combine(
                    directory.FullName,
                    "ERPWebApp.Server",
                    "appsettings.json");

                if (File.Exists(serverPath))
                {
                    return serverPath;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            "Could not locate ERPWebApp.Server/appsettings.json for EF Core design-time operations.");
    }
}
