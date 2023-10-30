using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Application.Managers;

public class UserLocalStorageManager : IUserBlobStorageManager
{
    private readonly string _profilePicturePrefix = "profilePicture_";
    private string profilesDir;
	
    public UserLocalStorageManager()
    {
        string baseDir=System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // $HOME folder 
	    string dataDir= baseDir+"/librum_storage";
		profilesDir=dataDir+"/profiles";
		// create folders
		if(! System.IO.Directory.Exists(dataDir)) System.IO.Directory.CreateDirectory(dataDir);
		if(! System.IO.Directory.Exists(profilesDir)) System.IO.Directory.CreateDirectory(profilesDir);
		Console.WriteLine ("Profile photos directory is "+ profilesDir);
    }
    
    
    public Task<Stream> DownloadProfilePicture(string guid)
    {
      	var filename=profilesDir+"/"+guid;
		if (!System.IO.File.Exists(filename)){
			throw new CommonErrorException(400,"file not exists "+filename ,0);
		}
		return Task.FromResult<Stream>(File.OpenRead(filename));
    }

    public async Task ChangeProfilePicture(string guid, MultipartReader reader)
    {
        //if already exists
		var filename=profilesDir+"/"+guid;
		if ( System.IO.File.Exists(filename)){
			throw new CommonErrorException(400,"file already exists "+filename ,0);
		}
		System.IO.Stream dest;
		try {
			 dest = System.IO.File.Create (filename);
		}
		catch (Exception e)
		{
  		 	throw new CommonErrorException(400, "Can't create file "+filename, 0);
		 	Console.WriteLine(e.Message);
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

    public async Task DeleteProfilePicture(string guid)
    {
		var path=profilesDir+"/"+guid;
		await Task.Run(() => System.IO.File.Delete(path));
    }
}