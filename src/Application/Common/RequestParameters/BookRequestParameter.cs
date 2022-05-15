namespace Application.Common.RequestParameters;

public class BookRequestParameter
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public string Query { get; set; }

    public string SortBy { get; set; }
    
    public string Author { get; set; }
    
    public DateTime Added { get; set; }

    public string Format { get; set; }

    public bool Read { get; set; } = false;
    
    public bool Unread { get; set; } = false;

    public bool OnlyBooks { get; set; } = false;

    public bool OnlyFiles { get; set; } = false;
    
    public string Tag { get; set; }
}