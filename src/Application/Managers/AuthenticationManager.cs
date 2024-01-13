using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Managers;

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

    public async Task<string> GetEmailConfirmationLinkAsync(User user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByNameAsync(email);
        if (user == null)
            throw new CommonErrorException(400, "No user with this email address was found", 17);
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> IsEmailConfirmed(string email)
    {
        var user = await _userManager.FindByNameAsync(email);
        if (user == null)
            throw new CommonErrorException(400, "No user with this email address was found", 17);

        return user.EmailConfirmed;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JWTKey"]!);
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

        return claims;
    }
    
    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials,
                                                  IEnumerable<Claim> claims)
    {
        var tokenOptions = new JwtSecurityToken
        (
            issuer: _configuration["JWTValidIssuer"],
            audience: "librumapi",
            claims: claims,
            expires: DateTime.Now.AddYears(50),
            signingCredentials: signingCredentials
            );

        return tokenOptions;
    }
}