namespace Application.Common.DTOs.Users;

public class UserOutDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string Email { get; set; }

    public DateTime AccountCreation { get; set; }
}