using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class BookBlobStorageManager : IBookBlobStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _bookCoverPrefix = "cover_";

    public BookBlobStorageManager(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public Task<Stream> DownloadBookBlob(Guid guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(guid.ToString());

        return blobClient.OpenReadAsync();
    }

    public async Task UploadBookBlob(Guid guid, MultipartReader reader)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(guid.ToString());

        await using var dest = await blobClient.OpenWriteAsync(true);

        
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
                throw new InvalidParameterException(message);
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
    

    public async Task DeleteBookBlob(Guid guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(guid.ToString());

        await blobClient.DeleteAsync();
    }

    public async Task<long> ChangeBookCover(Guid guid, MultipartReader reader)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_bookCoverPrefix + guid.ToString());

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
                throw new InvalidParameterException(message);
            }
            
            await section.Body.CopyToAsync(dest);
            coverSize += section.Body.Length;
            
            section = await reader.ReadNextSectionAsync();
        }

        return coverSize;
    }

    public Task<Stream> DownloadBookCover(Guid guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_bookCoverPrefix + guid.ToString());

        return blobClient.OpenReadAsync();
    }

    public async Task DeleteBookCover(Guid guid)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(_bookCoverPrefix + guid.ToString());

        await blobClient.DeleteAsync();
    }
}