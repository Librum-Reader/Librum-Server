using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<User> GetAsync(string email);
}