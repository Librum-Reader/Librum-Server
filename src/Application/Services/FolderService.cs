using Application.Common.DTOs.Folders;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class FolderService(IUserRepository userRepository, IFolderRepository folderRepository) : IFolderService
{
    public IUserRepository UserRepository { get; } = userRepository;
    public IFolderRepository FolderRepository { get; } = folderRepository;

    public async Task UpdateFoldersAsync(string email, FolderInDto folderInDto)
    {
        var user = await UserRepository.GetAsync(email, true);
        if (user == null)
            throw new CommonErrorException(400, "No user with this email exists", 17);
        
        var folder = FolderInDtoToFolder(folderInDto);
        // If the user has no root folder, create it and set it as the user's root folder
        if(user.RootFolderId == Guid.Empty)
        {
            await FolderRepository.CreateFolderAsync(folder);
            user.RootFolderId = folder.FolderId;
            
            await UserRepository.SaveChangesAsync();
            return;
        }
        
        if(folder.FolderId != user.RootFolderId)
            throw new CommonErrorException(400, "Folder id does not match user root folder id", 0);
        
        var existingFolder = await FolderRepository.GetFolderAsync(user.RootFolderId);
        if(existingFolder == null)
            throw new CommonErrorException(400, "User has no root folder", 0);

        FolderRepository.RemoveFolder(existingFolder);
        await FolderRepository.CreateFolderAsync(folder);
        await FolderRepository.SaveChangesAsync();
    }

    private Folder FolderInDtoToFolder(FolderInDto folderInDto)
    {
        var folder = new Folder
        {
            FolderId = new Guid(folderInDto.Guid),
            Name = folderInDto.Name,
            Color = folderInDto.Color,
            Icon = folderInDto.Icon,
            Description = folderInDto.Description,
            LastModified = folderInDto.LastModified,
            Children = new List<Folder>()
        };
        
        foreach (var child in folderInDto.Children)
        {
            var childFolder = FolderInDtoToFolder(child);
            folder.Children.Add(childFolder);
        }

        return folder;
    }
    
    public async Task<FolderOutDto> GetFoldersAsync(string email)
    {
        var user = await UserRepository.GetAsync(email, false);
        if (user == null)
        {
            throw new CommonErrorException(400, "No user with this email exists", 17);
        }
        
        if(user.RootFolderId == Guid.Empty)
        {
            throw new CommonErrorException(400, "User has no root folder", 22);
        }
        
        var folder = await FolderRepository.GetFolderAsync(user.RootFolderId);
        return await FolderToFolderOutDto(folder);
    }
    
    private async Task<FolderOutDto> FolderToFolderOutDto(Folder folder)
    {
        var folderOutDto = new FolderOutDto
        {
            Guid = folder.FolderId.ToString(),
            Name = folder.Name,
            Color = folder.Color,
            Icon = folder.Icon,
            LastModified = folder.LastModified,
            Description = folder.Description
        };
        
        foreach (var child in folder.Children)
        {
            var childFolderOutDto = await FolderToFolderOutDto(child);
            folderOutDto.Children.Add(childFolderOutDto);
        }

        return folderOutDto;
    }
}