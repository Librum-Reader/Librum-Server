using Application.Common.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Application.Common.RequestParameters;

public class BookRequestParameter
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    private string _searchString = "";
    public string SearchString
    {
        get => _searchString.ToLower();
        set => _searchString = value;
    }

    public BookSortOptions SortBy { get; set; } = BookSortOptions.Nothing;

    private string _author = "";
    public string Author
    {
        get => _author.ToLower();
        set => _author = value;
    }

    public TimeSpan Added { get; set; }

    public string Format { get; set; }

    public bool Read { get; set; }
    
    public bool Unread { get; set; }

    public bool OnlyBooks { get; set; }

    public bool OnlyFiles { get; set; }
    
    public string Tag { get; set; }
}