using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Users;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [MinLength(6, ErrorMessage = "The email is too short")]
    [MaxLength(50, ErrorMessage = "The email is too long")]
    public string Email { get; set; }
        
    [Required]
    [MinLength(6, ErrorMessage = "The password is too short")]
    [MaxLength(60, ErrorMessage = "The password is too long")]
    public string Password { get; set; }
}