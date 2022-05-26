using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    
    public UserRepository(DataContext context)
    {
        _context = context;
    }
    
    
    public async Task<User> GetAsync(string email, bool trackChanges)
    {
        return trackChanges
            ? await _context.Users.Include(user => user.Books).Include(user => user.Tags)
                .SingleOrDefaultAsync(user => user.Email == email)
            : await _context.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Email == email);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}