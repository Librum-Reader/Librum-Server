using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class BookLocalStorageManager : IBookBlobStorageManager
{
 	private readonly string _booksDir;
 	private readonly string _coversDir;
   
    public BookLocalStorageManager()
    {
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		string dataDir = baseDir + "/librum_storage";
		_booksDir = dataDir + "/books";
		_coversDir = _booksDir + "/covers";
		
		// create folders
		if (!Directory.Exists(dataDir))
			Directory.CreateDirectory(dataDir);
		if (!Directory.Exists(_booksDir))
			Directory.CreateDirectory(_booksDir);
		if (!Directory.Exists(_coversDir))
			Directory.CreateDirectory(_coversDir);
		
		Console.WriteLine ("Books are stored in: " + dataDir);
    }

    public Task<Stream> DownloadBookBlob(Guid guid)
    {
		var filename= _booksDir + "/" + guid;
		if (!File.Exists(filename))
			throw new CommonErrorException(400, "File does not exist: " + filename, 0);
		
		return Task.FromResult<Stream>(File.OpenRead(filename)); 
    }

    public async Task UploadBookBlob(Guid guid, MultipartReader reader)
    {
		var filename= _booksDir + "/" + guid;
		if (File.Exists(filename))
			throw new CommonErrorException(400, "File already exists: " + filename, 0);
		
		Stream dest;
		try
		{
			 dest = File.Create(filename);
		}
		catch (Exception _) 
		{ 
			throw new CommonErrorException(400, "Can't create file", 0);
		}
		
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        
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
        
		dest.Close();
        File.SetUnixFileMode(filename,UnixFileMode.UserRead | UnixFileMode.UserWrite);
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
		var path = _booksDir + "/" + guid;
		await Task.Run(() => File.Delete(path));
    }

    public async Task<long> ChangeBookCover(Guid guid, MultipartReader reader)
    {
		var filename=_coversDir + "/" + guid;
		Stream dest;
		try
		{
			 dest = File.Create (filename);
		}
		catch (Exception e)
		{
			if (e is UnauthorizedAccessException)
			{
				FileAttributes attr = (new FileInfo(filename)).Attributes;
				if ((attr & FileAttributes.ReadOnly) > 0)
            		Console.Write("The file is read-only");
				throw new CommonErrorException(400, "Can't overwrite the book cover file", 0);
			}
			
			Console.WriteLine(e.Message);
  		 	throw new CommonErrorException(400, "Can't create file for book cover", 0);
		}

        long coverSize = 0;
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition,  out var contentDisposition);
        
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
        
		dest.Close();
        File.SetUnixFileMode(filename,UnixFileMode.UserRead | UnixFileMode.UserWrite);
        return coverSize;
    }

    public Task<Stream> DownloadBookCover(Guid guid)
    {
		var filename = _coversDir + "/" + guid;
		if (!File.Exists(filename))
			throw new CommonErrorException(400, "file not exists "+filename, 0);
		
		return Task.FromResult<Stream>(File.OpenRead(filename)); 
    }

    public async Task DeleteBookCover(Guid guid)
    {
		var path = _coversDir + "/" + guid;
		await Task.Run(() => File.Delete(path));
    }
}