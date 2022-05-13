using Application.Common.DTOs;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<UserOutDto> GetUserAsync(string email);
    public Task DeleteUserAsync(string email);
}