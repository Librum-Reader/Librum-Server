using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITagRepository
{
    public Task<int> SaveChangesAsync();
    void Delete(Tag tag);
    void Add(Tag tag);
}