using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface IAuthenticationManager
{
    public Task<bool> UserExistsAsync(string email, string password);
    public Task<string> CreateTokenAsync(LoginDto loginDto);
}