using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Users;

public class RegisterDto
{
    [Required]
    [MinLength(2, ErrorMessage = "The firstname is too short")]
    [MaxLength(40, ErrorMessage = "The firstname is too long")]
    public string FirstName { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The lastname is too short")]
    [MaxLength(50, ErrorMessage = "The lastname is too long")]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MinLength(6, ErrorMessage = "The email is too short")]
    [MaxLength(50, ErrorMessage = "The email is too long")]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "The password is too short")]
    [MaxLength(60, ErrorMessage = "The password is too long")]
    public string Password { get; set; }

    public IEnumerable<string> Roles { get; set; }
}