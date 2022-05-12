using Application.Common.DTOs;

namespace Application.Common.Interfaces.Services;

public interface IAuthenticationService
{
    public Task<string> LoginUser(LoginDto loginDto);
    public Task RegisterUser(RegisterDto registerDto);
}