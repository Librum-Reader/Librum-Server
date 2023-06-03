using Application.Common.DTOs.Users;
using Domain.Entities;

namespace Application.Interfaces.Managers;

public interface IAuthenticationManager
{
    public Task<bool> CreateUserAsync(User user, string password);
    public Task<bool> UserExistsAsync(string email, string password);
    public Task<bool> EmailAlreadyExistsAsync(string email);
    public Task<string> CreateTokenAsync(LoginDto loginDto);
    public Task<string> GetEmailConfirmationLinkAsync(User user);
    public Task<bool> ConfirmEmailAsync(string email, string token);
}