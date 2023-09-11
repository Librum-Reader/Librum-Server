using Application.Common.DTOs.Highlights;

namespace Application.Interfaces.Services;

public interface IHighlightService
{
    Task CreateHighlightAsync(string email, Guid bookGuid, HighlightInDto highlightIn);
    Task DeleteHighlightAsync(string email, Guid bookGuid, Guid highlightGuid);
}