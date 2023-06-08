using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class UserBlobStorageManager : IUserBlobStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _profilePicturePrefix = "profilePicture_";
    
    public UserBlobStorageManager(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    
    
    public Task<Stream> DownloadProfilePicture(string guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_profilePicturePrefix + guid);
        if (!blobClient.Exists())
            throw new CommonErrorException(400, "No profile picture exists", 0);
        
        return blobClient.OpenReadAsync();
    }

    public async Task ChangeProfilePicture(string guid, MultipartReader reader)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_profilePicturePrefix);

        await using var dest = await blobClient.OpenWriteAsync(true);

        long coverSize = 0;
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out var contentDisposition);
        
            if (!hasContentDispositionHeader)
                continue;
        
            if (!HasFileContentDisposition(contentDisposition))
            {
                var message = "Missing content disposition header";
                throw new CommonErrorException(400, message, 0);
            }
            
            await section.Body.CopyToAsync(dest);
            coverSize += section.Body.Length;
            
            section = await reader.ReadNextSectionAsync();
        }
    }

    private static bool HasFileContentDisposition(
        ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }

    public async Task DeleteProfilePicture(string guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_profilePicturePrefix + guid);
        if(!blobClient.Exists())
            throw new CommonErrorException(400, "No profile picture exists", 0);

        await blobClient.DeleteAsync();
    }
}