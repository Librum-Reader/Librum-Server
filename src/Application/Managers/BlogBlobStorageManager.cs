using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class BlogBlobStorageManager : IBlogBlobStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private string _coverPrefix = "cover_";

    public BlogBlobStorageManager(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    
    public Task<Stream> DownloadBlogCover(string guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev-website-blogs");
        var blobClient = containerClient.GetBlobClient(_coverPrefix + guid);
        
        return blobClient.OpenReadAsync();
    }

    public async Task ChangeBlogCover(string guid, MultipartReader reader)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev-website-blogs");
        var blobClient = containerClient.GetBlobClient(_coverPrefix + guid);

        await using var dest = await blobClient.OpenWriteAsync(true);

        
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                                                       out var contentDisposition);
        
            if (!hasContentDispositionHeader)
                continue;
        
            if (!HasFileContentDisposition(contentDisposition))
            {
                var message = "Missing content disposition header";
                throw new CommonErrorException(400, message, 0);
            }
            
            await section.Body.CopyToAsync(dest);
            
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

    public async Task DeleteBlogCover(string guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev-website-blogs");
        var blobClient = containerClient.GetBlobClient(guid);

        await blobClient.DeleteAsync();
    }
}