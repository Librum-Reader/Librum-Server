using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class UserRepository : IUserRepository
{
    private readonly DataContext _dbContext;

    
    public UserRepository(DataContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public Task<User> GetAsync(string email, bool trackChanges)
    {
        return trackChanges
            ? _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email)
            : _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Email == email);
    }
}