namespace Trainingsfortschritt.Core.Abstractions;

public interface IStorageService
{
    Task<double> GetDoubleAsync(string key, double defaultValue = 0);
    Task SetDoubleAsync(string key, double value);

    Task<bool> GetBoolAsync(string key, bool defaultValue = false);
    Task SetBoolAsync(string key, bool value);

    Task RemoveAsync(string key);
}