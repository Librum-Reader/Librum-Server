using System.Reflection;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

public class DataContext : IdentityDbContext<User>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Highlight> Highlights { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Folder> Folders { get; set; }

    public DataContext(DbContextOptions<DataContext> options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder
            .Entity<Product>()
            .HasMany(p => p.Features)
            .WithOne(pf => pf.Product)
            .HasForeignKey(pf => pf.ProductId)
            .IsRequired();

        builder
            .Entity<Folder>()
            .Property(f => f.Description)
            .HasColumnType(this.Database.ProviderName == "Pomelo.EntityFrameworkCore.MySql"
                ? "longtext"
                : "nvarchar(max)");
        
        base.OnModelCreating(builder);
    }
}
