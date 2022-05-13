using Application.Common.DTOs;

namespace Application.Common.Interfaces.Services;

public interface IAuthenticationService
{
    public Task<string> LoginUserAsync(LoginDto loginDto);
    public Task RegisterUserAsync(RegisterDto registerDto);
}