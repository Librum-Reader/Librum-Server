using Application.Common.DTOs.Users;

namespace Application.Interfaces;

public interface IAuthenticationManager
{
    public Task<bool> UserExistsAsync(string email, string password);
    public Task<string> CreateTokenAsync(LoginDto loginDto);
}