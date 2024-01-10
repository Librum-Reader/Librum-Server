using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<User> GetAsync(string email, bool trackChanges);
    public Task<User> GetByCustomerIdAsync(string customerId, bool trackChanges);
    public void Delete(User user);
    public Task DeleteUnconfirmedUsers();
    public Task ResetAiExplanationCount();
    public Task<int> SaveChangesAsync();
}