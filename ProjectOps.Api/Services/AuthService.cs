using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectOps.Api.Data;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public AuthService(AppDbContext dbContext, IPasswordHasher<AppUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AppUser?> AuthenticateAsync(string username, string password)
    {
        var user = await _dbContext.AppUsers
            .SingleOrDefaultAsync(appUser => appUser.Username == username);

        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}