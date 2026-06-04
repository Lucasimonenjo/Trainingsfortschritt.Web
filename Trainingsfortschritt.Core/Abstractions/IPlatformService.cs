namespace Trainingsfortschritt.Core.Abstractions;

public interface IPlatformServices
{
    string GetDatabasePath(string fileName);

    Task ShowAlertAsync(string title, string message);
}