using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace Application.Common.DTOs.Users;

public class UserForUpdateDto
{
    // Legacy - to be removed
    [MinLength(2, ErrorMessage = "The firstname is too short")]
    [MaxLength(40, ErrorMessage = "The firstname is too long")]
    public string FirstName { get; set; }
    
    // Legacy - to be removed
    [MinLength(2, ErrorMessage = "The lastname is too short")]
    [MaxLength(50, ErrorMessage = "The lastname is too long")]
    public string LastName { get; set; }
    
    [MinLength(2, ErrorMessage = "23 The name is too short")]
    [MaxLength(150, ErrorMessage = "24 The name is too long")]
    public string Name { get; set; }
    
    public DateTime ProfilePictureLastUpdated { get; set; }
    
    public bool HasProfilePicture { get; set; }

    public bool DataIsValid()
    {
        if (Name.IsNullOrEmpty())
        {
            return LastName.Length is >= 2 and <= 50 &&
                FirstName.Length is >= 2 and <= 40;
        }
        return Name.Length is >= 2 and <= 150;
    }
}