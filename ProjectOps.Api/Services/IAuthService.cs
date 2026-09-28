using ProjectOps.Api.Models;

namespace ProjectOps.Api.Services;

public interface IAuthService
{
    Task<AppUser?> AuthenticateAsync(string username, string password);
}