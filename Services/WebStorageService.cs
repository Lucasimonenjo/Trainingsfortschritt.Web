using Microsoft.JSInterop;
using Trainingsfortschritt.Core.Abstractions;

namespace Trainingsfortschritt.Web.Services;

public class WebStorageService : IStorageService
{
    private readonly IJSRuntime _js;

    public WebStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetBoolAsync(string key, bool value)
        => await _js.InvokeVoidAsync("idbStorage.set", key, value.ToString());

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var result = await _js.InvokeAsync<string>("idbStorage.get", key);

        return bool.TryParse(result, out var b) ? b : defaultValue;
    }

    public async Task SetDoubleAsync(string key, double value)
        => await _js.InvokeVoidAsync("idbStorage.set", key, value.ToString());

    public async Task<double> GetDoubleAsync(string key, double defaultValue = 0)
    {
        var result = await _js.InvokeAsync<string>("idbStorage.get", key);

        return double.TryParse(result, out var d) ? d : defaultValue;
    }

    public async Task RemoveAsync(string key)
        => await _js.InvokeVoidAsync("idbStorage.remove", key);
}