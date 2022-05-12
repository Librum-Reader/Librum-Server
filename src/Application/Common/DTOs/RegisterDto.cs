using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs;

public class RegisterDto
{
    [Required]
    [MinLength(2, ErrorMessage = "The provided firstname is too short")]
    [MaxLength(40, ErrorMessage = "The provided firstname is too long")]
    public string FirstName { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "The provided firstname is too short")]
    [MaxLength(50, ErrorMessage = "The provided firstname is too long")]
    public string LastName { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "The provided email is too short")]
    [MaxLength(50, ErrorMessage = "The provided email is too long")]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "The provided password is too short")]
    [MaxLength(60, ErrorMessage = "The provided password is too long")]
    public string Password { get; set; }

    public IEnumerable<string> Roles { get; set; }
}