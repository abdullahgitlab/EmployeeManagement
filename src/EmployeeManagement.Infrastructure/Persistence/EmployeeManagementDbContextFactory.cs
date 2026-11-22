using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeManagement.Infrastructure.Persistence;

public sealed class EmployeeManagementDbContextFactory : IDesignTimeDbContextFactory<EmployeeManagementDbContext>
{
    public EmployeeManagementDbContext CreateDbContext(string[] args)
    {
        var settingsPath = FindApiSettingsPath();
        using var settings = JsonDocument.Parse(File.ReadAllText(settingsPath));
        var connectionString = settings.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("Default")
            .GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string 'Default' is missing or empty in '{settingsPath}'.");

        var options = new DbContextOptionsBuilder<EmployeeManagementDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new EmployeeManagementDbContext(options);
    }

    private static string FindApiSettingsPath()
    {
        var roots = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var root in roots)
        {
            for (var directory = new DirectoryInfo(root); directory is not null; directory = directory.Parent)
            {
                var apiSettings = Path.Combine(directory.FullName, "src", "EmployeeManagement.API", "appsettings.json");
                if (File.Exists(apiSettings))
                    return apiSettings;

                if (directory.Name.Equals("EmployeeManagement.API", StringComparison.OrdinalIgnoreCase))
                {
                    var localSettings = Path.Combine(directory.FullName, "appsettings.json");
                    if (File.Exists(localSettings))
                        return localSettings;
                }
            }
        }

        throw new FileNotFoundException(
            "Could not locate src\\EmployeeManagement.API\\appsettings.json. Run EF commands from the solution folder or API project folder.");
    }
}
