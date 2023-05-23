using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IBookBlobStorageManager
{
    public Task<Stream> DownloadBookBlob(Guid guid);
    public Task UploadBookBlob(Guid guid, MultipartReader reader);
    public Task DeleteBookBlob(Guid guid);
    public Task<long> ChangeBookCover(Guid guid, MultipartReader reader);
    public Task<Stream> DownloadBookCover(Guid guid);
    public Task DeleteBookCover(Guid guid);
}