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
            ? await _context.Users.Include(user => user.Books)
                .Include(user => user.Tags)
                .SingleOrDefaultAsync(user => user.Email == email)
            : await _context.Users
                .Include(user => user.Tags)
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Email == email);
    }

    public Task<User> GetByCustomerIdAsync(string customerId, bool trackChanges)
    {
        return trackChanges
            ? _context.Users.Include(user => user.Books)
                .SingleOrDefaultAsync(user => user.CustomerId == customerId)
            : _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.CustomerId == customerId);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }
    
    public async Task DeleteUnconfirmedUsers()
    {
        // Users with unconfirmed emails created more than 30 minutes ago.
        var usersToRemove = _context.Users.Where(u => !u.EmailConfirmed &&
                                                      u.AccountCreation < DateTime.Now.AddMinutes(-30));
        _context.Users.RemoveRange(usersToRemove);
        await _context.SaveChangesAsync();
    }

    public async Task ResetAiExplanationCount()
    {
        await _context.Users.ForEachAsync(u => u.AiExplanationRequestsMadeToday = 0);
        await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}