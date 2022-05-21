using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Users;

public class UserForUpdateDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }

    public bool DataIsValid => Email.Length >= 6 && LastName.Length > 2 && FirstName.Length > 2;
}