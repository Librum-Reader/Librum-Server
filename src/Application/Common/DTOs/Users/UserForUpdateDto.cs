namespace Application.Common.DTOs.Users;

public class UserForUpdateDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public bool DataIsValid => LastName.Length is >= 2 and <= 50 && 
                               FirstName.Length is >= 2 and <= 40;
}