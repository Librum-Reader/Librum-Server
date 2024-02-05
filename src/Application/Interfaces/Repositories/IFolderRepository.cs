using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IFolderRepository
{
    public Task SaveChangesAsync();
    public Task<Folder> GetFolderAsync(Guid folderId);
    public Task CreateFolderAsync(Folder folder);
    public void RemoveFolder(Folder folder);
}