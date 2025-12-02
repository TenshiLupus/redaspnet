using System;
using booksBackend.Models;
using booksBackend.Users;

namespace booksBackend.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserDto request);

    Task<LoginResponse?> LoginAsync(UserDto request);
}
