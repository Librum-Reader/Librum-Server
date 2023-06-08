using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Managers;

public interface IUserBlobStorageManager
{
    public Task<Stream> DownloadProfilePicture(string guid);
    public Task ChangeProfilePicture(string guid, MultipartReader reader);
    public Task DeleteProfilePicture(string guid);
}