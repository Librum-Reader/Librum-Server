using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Users;

public class RegisterDto
{
    // Legacy - to be removed
    [MinLength(2, ErrorMessage = "13 The firstname is too short")]
    [MaxLength(40, ErrorMessage = "14 The firstname is too long")]
    public string FirstName { get; set; }
    
    // Legacy - to be removed
    [MinLength(2, ErrorMessage = "15 The lastname is too short")]
    [MaxLength(50, ErrorMessage = "16 The lastname is too long")]
    public string LastName { get; set; }
    
    [MinLength(2, ErrorMessage = "23 The name is too short")]
    [MaxLength(150, ErrorMessage = "24 The name is too long")]
    public string Name { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "8 Invalid email address format.")]
    [MaxLength(60, ErrorMessage = "10 The email is too long")]
    public string Email { get; set; }

    [Required]
    [MinLength(4, ErrorMessage = "11 The password is too short")]
    [MaxLength(60, ErrorMessage = "12 The password is too long")]
    public string Password { get; set; }

    public IEnumerable<string> Roles { get; set; }
}