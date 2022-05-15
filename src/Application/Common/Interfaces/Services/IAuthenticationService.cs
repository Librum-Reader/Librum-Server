using Application.Common.DTOs.Users;

namespace Application.Common.Interfaces.Services;

public interface IAuthenticationService
{
    public Task<string> LoginUserAsync(LoginDto loginDto);
    public Task RegisterUserAsync(RegisterDto registerDto);
}