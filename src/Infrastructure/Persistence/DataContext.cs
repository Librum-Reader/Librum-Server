using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DataContext : IdentityDbContext<User>
{
    public DbSet<Book> Books { get; set; }


    public DataContext(DbContextOptions<DataContext> options) :
        base(options)
    {
    }
    
    public DataContext()
    {
    }
}