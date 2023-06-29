using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IBlogBlobStorageManager
{
    public Task<Stream> DownloadBlogCover(string guid);
    public Task ChangeBlogCover(string guid, MultipartReader reader);
    public Task DeleteBlogCover(string guid);
}