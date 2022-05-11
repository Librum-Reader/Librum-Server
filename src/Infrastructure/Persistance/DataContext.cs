using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Book> Books { get; set; }


    public DataContext(DbContextOptions<DataContext> options) :
        base(options)
    {
    }
}