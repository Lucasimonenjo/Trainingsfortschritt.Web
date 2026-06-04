using Trainingsfortschritt.Core.Abstractions;

namespace Trainingsfortschritt.Web.Services;

public class WebPlatformServices : IPlatformServices
{
    public string GetDatabasePath(string filename)
    {
        // Web: kein echtes File-System
        // Dummy-Pfad nur für Kompatibilität

        return filename;
    }

    public Task ShowAlertAsync(string title, string message)
    {
        Console.WriteLine($"ALERT: {title} - {message}");
        return Task.CompletedTask;
    }
}