namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
    public string Title { get; set; }
    
    public DateTime ReleaseDate { get; set; }
    
    public int CurrentPage { get; set; }
    
    public DateTime LastOpened { get; set; }
    
    public bool DataIsValid => Title.Length >= 4 && Title.Length <= 120 &&
                               CurrentPage > 0 && CurrentPage < int.MaxValue;
}