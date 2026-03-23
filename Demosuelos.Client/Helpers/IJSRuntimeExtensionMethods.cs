using Microsoft.JSInterop;

namespace Demosuelos.Client.Helpers;

public static class IJSRuntimeExtensionMethods
{
    public static ValueTask<object?> SetSessionStorage(this IJSRuntime js, string key, string content)
    {
        return js.InvokeAsync<object?>("sessionStorage.setItem", key, content);
    }

    public static async ValueTask<string?> GetSessionStorage(this IJSRuntime js, string key)
    {
        return await js.InvokeAsync<string?>("sessionStorage.getItem", key);
    }

    public static ValueTask<object?> RemoveSessionStorage(this IJSRuntime js, string key)
    {
        return js.InvokeAsync<object?>("sessionStorage.removeItem", key);
    }
}
