using Application.Common.DTOs.Users;
using Domain.Entities;

namespace Application.Interfaces;

public interface IAuthenticationManager
{
    public Task<bool> CreateUserAsync(User user, string password);
    public Task AddRolesToUserAsync(User user, IEnumerable<string> roles);
    public Task<bool> UserExistsAsync(string email, string password);
    public Task<bool> EmailAlreadyExistsAsync(string email);
    public Task<string> CreateTokenAsync(LoginDto loginDto);
}