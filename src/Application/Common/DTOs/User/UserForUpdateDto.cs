using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.User;

public class UserForUpdateDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }
}