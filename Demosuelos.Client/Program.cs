using Demosuelos.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Demosuelos.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7082")
        });
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationProviderJWT>();
        builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderJWT>(
            sp => sp.GetRequiredService<AuthenticationProviderJWT>());
        builder.Services.AddScoped<ILoginService, AuthenticationProviderJWT>(
            sp => sp.GetRequiredService<AuthenticationProviderJWT>());

        await builder.Build().RunAsync();
    }
}
