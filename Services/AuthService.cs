using System;
using System.Security.Claims;
using booksBackend.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using booksBackend.Models;
namespace booksBackend.Services;

public class AuthService(AppDbContext _dbContext, IConfiguration _configuration) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(UserDto request)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(e => e.Username == request.Username);
        if (user == null)
        {
            return null;
        }

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.Password!, request.Password!) == PasswordVerificationResult.Failed)
        {
            return null;
        }

        string token = CreateToken(user);
        var lr = new LoginResponse
        {
            Token = token,
            UserId = user.Id
        };
        return lr;
    }

    public async Task<User?> RegisterAsync(UserDto request)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
        {
            return null;
        }
        var user = new User { Username = request.Username, Password = string.Empty };

        var hasher = new PasswordHasher<User>();

        user.Password = hasher.HashPassword(user, request.Password);


        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds

        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }


}
