using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IBlogBlobStorageManager
{
    Task<Stream> DownloadBlogContent(string guid);
    Task DeleteBlogContent(string guid);
    Task AddBlogContent(string guid, MultipartReader reader);
    
    Task<Stream> DownloadBlogCover(string guid);
    Task ChangeBlogCover(string guid, MultipartReader reader);
    Task DeleteBlogCover(string guid);
}
