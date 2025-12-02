using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using booksBackend.Models;
using booksBackend.Services;
using booksBackend.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace booksBackend.Controllers
{
    public class AuthenticationController(IAuthService authService, ILogger<AuthenticationController> _logger) : BaseController
    {


        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(UserDto request)
        {
            var user = await authService.RegisterAsync(request);

            //What abomination is this
            return Ok(user);

        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> LoginUser(UserDto request)
        {
            _logger.LogWarning("request password type {UserPass}", request.Password);

            var loginResult = await authService.LoginAsync(request);
            if (loginResult is null)
            {
                return BadRequest("Invalid Credentials");
            }

            return Ok(loginResult);
        }


    }
}
