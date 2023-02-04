using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Users;

public class UserForUpdateDto
{
    [MinLength(2, ErrorMessage = "The firstname is too short")]
    [MaxLength(40, ErrorMessage = "The firstname is too long")]
    public string FirstName { get; set; }
    
    [MinLength(2, ErrorMessage = "The lastname is too short")]
    [MaxLength(50, ErrorMessage = "The lastname is too long")]
    public string LastName { get; set; }
    
    public bool DataIsValid => LastName.Length is >= 2 and <= 50 && 
                               FirstName.Length is >= 2 and <= 40;
}