using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class FolderRepository(DataContext context) : IFolderRepository
{
    public DataContext Context { get; } = context;

    public async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
    
    public async Task<Folder> GetFolderAsync(Guid folderId)
    {
        return await Context.Folders
            .Where(f => f.FolderId == folderId)
            .Include(f => f.Children)
            .ThenInclude(f => f.Children)
            .ThenInclude(f => f.Children)
            .ThenInclude(f => f.Children)
            .ThenInclude(f => f.Children)
            .FirstOrDefaultAsync();
    }

    public async Task CreateFolderAsync(Folder folder)
    {
        await Context.Folders.AddAsync(folder);
    }

    public void RemoveFolder(Folder folder)
    {
        var allFolders = new List<Folder>();
        GetAllChildren(folder, allFolders);
        allFolders.Add(folder);
        
        Context.Folders.RemoveRange(allFolders);
    }
    
    private void GetAllChildren(Folder folder, List<Folder> allChildren)
    {
        allChildren.Add(folder);
        foreach (var child in folder.Children)
        {
            GetAllChildren(child, allChildren);
        }
    }
}