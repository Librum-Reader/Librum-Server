using System.Reflection;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasMany(x => x.Books)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(builder);
    }
}