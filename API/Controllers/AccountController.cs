using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using API.Data;
using API.Dtos;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

[Authorize]
public class AccountController:APIBaseController
{
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext dbContext, ITokenService tokenService)
    {
        _dataContext = dbContext;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (registerDto == null)
        {
            return BadRequest("Null Entry");
        }    

        if (! await IsValidUsername(registerDto.Username))
        {
            return BadRequest("Username already exists");
        }

        using var hmac = new HMACSHA512();

        var appUser = new AppUser
        {
            UserName = registerDto.Username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key,
        };
 
        _dataContext.Add(appUser);
        await _dataContext.SaveChangesAsync();
       
       var user = new UserDto {
            Username = registerDto.Username,
            Token = _tokenService.CreateToken(appUser)
       };

       return user;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult<UserDto>> Login([FromBody]LoginDto registerDto)
    {

        AppUser appUser = await _dataContext.AppUsers
        .SingleOrDefaultAsync(x => x.UserName.ToLower().Equals(registerDto.Username));
        if (appUser == null)
        {
            return Unauthorized("Wrong username or password");
        }

        byte[] userSalt = appUser.PasswordSalt;

        using var hmac = new HMACSHA512(userSalt);

        byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes( registerDto.Password));

        int passHashLength = passwordHash.Length;

        for (int i = 0; i <  passHashLength ; i ++)
        {
            if (passwordHash[i] != appUser.PasswordHash[i])
            {
                return Unauthorized("Wrong username or password");
            }
        }

        var user = new UserDto {
            Username = registerDto.Username,
            Token = _tokenService.CreateToken(appUser)
        };

        return Ok(user);
    }

    private async Task<bool> IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        bool usernameExists = 
        await _dataContext.AppUsers
        .AnyAsync(item => item.UserName.ToLower().Equals( username.ToLower()));

        return !usernameExists;
    }
}
