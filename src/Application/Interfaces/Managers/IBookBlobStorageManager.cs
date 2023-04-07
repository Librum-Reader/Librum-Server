using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IBookBlobStorageManager
{
    public Task<Stream> DownloadBookBlob(Guid guid);
    public Task UploadBookBlob(Guid guid, MultipartReader reader);
    public Task DeleteBookBlob(Guid guid);
    public Task ChangeBookCover(Guid guid, MultipartReader reader);
}