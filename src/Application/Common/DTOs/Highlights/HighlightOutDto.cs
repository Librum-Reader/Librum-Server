namespace Application.Common.DTOs.Highlights;

public class HighlightOutDto
{
    public string Guid { get; set; }
    
    public string Color { get; set; }
    
    public int PageNumber { get; set; }
    
    public ICollection<RectFOutDto> Rects { get; set; } = new List<RectFOutDto>();
}