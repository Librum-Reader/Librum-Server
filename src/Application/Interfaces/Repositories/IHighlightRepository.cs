using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IHighlightRepository
{
    public Task<int> SaveChangesAsync();
    void Add(Highlight highlight);
    void Delete(Highlight highlight);
    Task<Highlight> GetAsync(Guid bookId, Guid highlightGuid);
}