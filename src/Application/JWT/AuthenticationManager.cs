using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.JWT;

public class AuthenticationManager : IAuthenticationManager
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;


    public AuthenticationManager(IConfiguration configuration,
                                 UserManager<User> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }


    public async Task<bool> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded;
    }

    public async Task AddRolesToUserAsync(User user, IEnumerable<string> roles)
    {
        await _userManager.AddToRolesAsync(user, roles);
    }

    public async Task<bool> UserExistsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;
        
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        return user != null;
    }

    public async Task<string> CreateTokenAsync(LoginDto loginDto)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaimsAsync(loginDto);
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
    
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]);
        var secret = new SymmetricSecurityKey(key);

        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
    
    private async Task<List<Claim>> GetClaimsAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto?.Email);
        if (user == null)
        {
            const string message = "Getting claims failed: User does not exist";
            throw new ArgumentException(message);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName)
        };

        IEnumerable<string> roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
    
    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials,
                                                  IEnumerable<Claim> claims)
    {
        var tokenOptions = new JwtSecurityToken
        (
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddYears(1),
            signingCredentials: signingCredentials
            );

        return tokenOptions;
    }
}