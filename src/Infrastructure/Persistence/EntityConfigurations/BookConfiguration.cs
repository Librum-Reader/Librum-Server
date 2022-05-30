using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.EntityConfigurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            toDb => toDb,
            fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc)
        );

        var bookFormatConverter = new ValueConverter<BookFormat, string>(
            toDb => toDb.ToString(),
            fromDb => (BookFormat)Enum.Parse(typeof(BookFormat), fromDb)
        );

        builder.Property(x => x.CreationDate)
            .HasConversion(utcConverter);
        
        builder.Property(x => x.LastOpened)
            .HasConversion(utcConverter);
        
        builder.Property(x => x.Format)
            .HasConversion(bookFormatConverter);
    }
}