using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<User> GetAsync(string email, bool trackChanges);
    public void Delete(User user);
    public Task<int> SaveChangesAsync();
}