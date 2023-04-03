using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IBookBlobStorageManager
{
    public Task GetBookBlob();
    public Task UploadBookBlob(Guid guid, MultipartReader reader);
    public Task DeleteBookBlob();
}