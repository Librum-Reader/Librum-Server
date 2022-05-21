using Application.Common.DTOs.Tags;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface ITagRepository
{
    public Task<int> SaveChangesAsync();
    void Delete(Tag tag);
}