using Microsoft.JSInterop;

namespace ProjectOps.Web.Services;

public class AuthService
{
    private const string TokenKey = "projectops_jwt";
    private const string RoleKey = "projectops_role";
    private readonly IJSRuntime _jsRuntime;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task StoreTokenAsync(string token, string role)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenKey, token);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", RoleKey, role);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<bool> IsAdminAsync()
    {
        var role = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", RoleKey);
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", RoleKey);
    }
}
