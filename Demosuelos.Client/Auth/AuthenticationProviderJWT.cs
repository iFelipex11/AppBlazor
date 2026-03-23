using Demosuelos.Client.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Demosuelos.Client.Auth;

public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    private const string TokenKey = "TOKEN_KEY";

    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _anonymous;

    public AuthenticationProviderJWT(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.GetSessionStorage(TokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return _anonymous;
        }

        if (TokenExpired(token))
        {
            await _jsRuntime.RemoveSessionStorage(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return _anonymous;
        }

        return BuildAuthenticationState(token);
    }

    public async Task LoginAsync(string token)
    {
        await _jsRuntime.SetSessionStorage(TokenKey, token);
        var authState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.RemoveSessionStorage(TokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private AuthenticationState BuildAuthenticationState(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }

    private static bool TokenExpired(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo <= DateTime.UtcNow;
    }
}
