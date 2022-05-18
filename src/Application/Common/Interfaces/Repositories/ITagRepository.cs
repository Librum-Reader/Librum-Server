using Application.Common.DTOs.Tags;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface ITagRepository
{
    public Task<int> SaveChangesAsync();
    public bool AlreadyExists(User user, TagInDto tagIn);
}