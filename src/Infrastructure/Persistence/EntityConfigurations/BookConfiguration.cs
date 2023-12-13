using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Persistence.EntityConfigurations;


public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(b => b.ProjectGutenbergId)
            .HasDefaultValue(0);
        builder.Property(b => b.ColorTheme)
            .HasDefaultValue("Normal");
    }
}