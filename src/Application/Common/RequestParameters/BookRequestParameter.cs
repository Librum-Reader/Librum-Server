using Application.Common.DTOs.Tags;
using Application.Common.Enums;


namespace Application.Common.RequestParameters;


public class BookRequestParameter
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    private string _searchString = "";
    public string SearchString
    {
        get => _searchString;
        set => _searchString = value.ToLower();
    }

    public BookSortOptions SortBy { get; set; } = BookSortOptions.Nothing;

    private string _author = "";
    public string Author
    {
        get => _author;
        set => _author = value.ToLower();
    }

    public string TimePassedAsString
    {
        set => TimePassed = TimeSpan.Parse(value);
    }

    public TimeSpan TimePassed { get; private set; }

    public string Format { get; set; }

    public bool Read { get; set; }
    
    public bool Unread { get; set; }

    public TagInDto Tag { get; set; } = null;
}