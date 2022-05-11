using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface IUserService
{
    public Task<UserOutDto> GetUser(string email);
}