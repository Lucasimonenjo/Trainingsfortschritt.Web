using Microsoft.JSInterop;

namespace Trainingsfortschritt.Web.Services;

public static class BrowserStorage
{
    private static IJSRuntime? _js;

    public static void Init(IJSRuntime js)
        => _js = js;

    public static ValueTask SetAsync(string key, string value)
        => _js!.InvokeVoidAsync("localStorage.setItem", key, value);

    public static ValueTask<string?> GetAsync(string key)
        => _js!.InvokeAsync<string?>("localStorage.getItem", key);
}