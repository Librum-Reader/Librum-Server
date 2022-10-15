namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
    public string Title { get; set; }
    
    public int CurrentPage { get; set; }
    
    public string Language { get; set; }
    
    public string Creator { get; set; }
    
    public string ReleaseDate { get; set; }
    
    public string LastOpened { get; set; }
    
    public bool DataIsValid => Title.Length is >= 4 and <= 120 &&
                               CurrentPage is >= 0 and < int.MaxValue;
}