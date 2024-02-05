using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder
            .HasOne(f => f.ParentFolder)
            .WithMany(f => f.Children)
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}