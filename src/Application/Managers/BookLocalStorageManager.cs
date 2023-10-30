using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class BookLocalStorageManager : IBookBlobStorageManager
{
    private readonly string _bookCoverPrefix = "cover_";
 	private string booksDir;
 	private string coversDir;
   
    public BookLocalStorageManager()
    {
        string baseDir=System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
		string dataDir= baseDir+"/librum_storage";
		booksDir= dataDir + "/books";
		coversDir=booksDir + "/covers";
		// create folders
		if(! System.IO.Directory.Exists(dataDir)) System.IO.Directory.CreateDirectory(dataDir);
		if(! System.IO.Directory.Exists(booksDir)) System.IO.Directory.CreateDirectory(booksDir);
		if(! System.IO.Directory.Exists(coversDir)) System.IO.Directory.CreateDirectory(coversDir);
		Console.WriteLine ("Current directory is "+ dataDir);
    }

    public Task<Stream> DownloadBookBlob(Guid guid)
    {
		var filename=booksDir+"/"+guid;
		if (!System.IO.File.Exists(filename)){
			throw new CommonErrorException(400,"file not exists "+filename ,0);
		}
		return Task.FromResult<Stream>(File.OpenRead(filename)); 
    }

    public async Task UploadBookBlob(Guid guid, MultipartReader reader)
    {
        //if already exists
		var filename=booksDir+"/"+guid;
		if ( System.IO.File.Exists(filename)){
			throw new CommonErrorException(400,"file already exists "+filename ,0);
		}
		System.IO.Stream dest;
		try {
			 dest = System.IO.File.Create (filename);
		}
		catch (Exception e)
		{
  		 throw new CommonErrorException(400, "Can't create file", 0);
		}

        
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
            
            section = await reader.ReadNextSectionAsync();
        }
		dest.Dispose();
        System.IO.File.SetUnixFileMode(filename,UnixFileMode.UserRead | UnixFileMode.UserWrite);
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
		var path=booksDir+"/"+guid;
		 await Task.Run(() => System.IO.File.Delete(path));
    }

    public async Task<long> ChangeBookCover(Guid guid, MultipartReader reader)
    {
		var filename=coversDir+"/"+guid;
		System.IO.Stream dest;
		try {
			 dest = System.IO.File.Create (filename);
		}
		catch (Exception e)
		{
			if (e is System.UnauthorizedAccessException)
			{
				throw new CommonErrorException(400, "Can't overwrite file for book cover", 0);
				FileAttributes attr = (new FileInfo(filePath)).Attributes;
				if ((attr & FileAttributes.ReadOnly) > 0)
            		Console.Write("The file is read-only.");
			}
			else
  		 		throw new CommonErrorException(400, "Can't create file for book cover", 0);
		}

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
		dest.Dispose();
        System.IO.File.SetUnixFileMode(filename,UnixFileMode.UserRead | UnixFileMode.UserWrite);

        return coverSize;
    }

    public Task<Stream> DownloadBookCover(Guid guid)
    {
		var filename=coversDir+"/"+guid;
		if (!System.IO.File.Exists(filename)){
			throw new CommonErrorException(400,"file not exists "+filename ,0);
		}
		return Task.FromResult<Stream>(File.OpenRead(filename)); 
    }

    public async Task DeleteBookCover(Guid guid)
    {
		var path=coversDir+"/"+guid;
		await Task.Run(() => System.IO.File.Delete(path));
    }
}