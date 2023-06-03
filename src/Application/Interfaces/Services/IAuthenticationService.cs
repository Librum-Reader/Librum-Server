using Application.Common.DTOs.Users;

namespace Application.Interfaces.Services;

public interface IAuthenticationService
{
    public Task<string> LoginUserAsync(LoginDto loginDto);
    public Task RegisterUserAsync(RegisterDto registerDto);
    public Task ConfirmEmail(string email, string token);
}