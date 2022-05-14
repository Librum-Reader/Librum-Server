using Application.Common.DTOs;
using Application.Common.DTOs.User;

namespace Application.Common.Interfaces.Services;

public interface IAuthenticationService
{
    public Task<string> LoginUserAsync(LoginDto loginDto);
    public Task RegisterUserAsync(RegisterDto registerDto);
}