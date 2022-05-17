using Application.Common.Enums;

namespace Application.Common.RequestParameters;

public class BookRequestParameter
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string Query { get; set; } = "";

    public BookSortOptions SortBy { get; set; } = BookSortOptions.Nothing;
    
    public string Author { get; set; }
    
    public DateTime Added { get; set; }

    public string Format { get; set; }

    public bool Read { get; set; } = false;
    
    public bool Unread { get; set; } = false;

    public bool OnlyBooks { get; set; } = false;

    public bool OnlyFiles { get; set; } = false;
    
    public string Tag { get; set; }
}