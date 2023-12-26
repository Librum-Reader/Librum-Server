using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class UserLocalStorageManager : IUserBlobStorageManager
{
    private string _profilesDir;
	
    public UserLocalStorageManager()
    {
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	    string dataDir = baseDir + "/librum_storage";
		_profilesDir = dataDir + "/profiles";
		
		if(!Directory.Exists(dataDir))
			Directory.CreateDirectory(dataDir);
		if(!Directory.Exists(_profilesDir))
			Directory.CreateDirectory(_profilesDir);
		
		Console.WriteLine ("Profile pictures are stored in: " + _profilesDir);
    }
    
    
    public Task<Stream> DownloadProfilePicture(string guid)
    {
      	var filename=_profilesDir + "/" + guid;
		if (!File.Exists(filename))
			throw new CommonErrorException(400, "file not exists " + filename, 0);
		
		return Task.FromResult<Stream>(File.OpenRead(filename));
    }

    public async Task ChangeProfilePicture(string guid, MultipartReader reader)
    {
        var filename = _profilesDir + "/" + guid;
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
            		Console.Write("The file is read-only.Can't overwrite.");
				throw new CommonErrorException(400, "Can't overwrite file for picture profile", 0);
			}
			
			Console.WriteLine(e.Message);
  		 	throw new CommonErrorException(400, "Failed creating file at: " + filename, 0);
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

    public async Task DeleteProfilePicture(string guid)
    {
		var path = _profilesDir + "/" + guid;
		await Task.Run(() => File.Delete(path));
    }
}