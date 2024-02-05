using Application.Common.DTOs.Folders;

namespace Application.Interfaces.Services;

public interface IFolderService
{
    public Task UpdateFoldersAsync(string email, FolderInDto folderInDto);
    public Task<FolderOutDto> GetFoldersAsync(string email);
}