using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<User> GetAsync(string email, bool trackChanges);
    public Task DeleteAsync(User user);
    public Task<int> SaveChangesAsync();
    public Task LoadRelationShipsAsync(User user);
}