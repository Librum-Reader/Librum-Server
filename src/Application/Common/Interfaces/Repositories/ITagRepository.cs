using Application.Common.DTOs.Tags;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface ITagRepository
{
    public Task<int> SaveChangesAsync();
    public bool Exists(User user, TagInDto tagIn);
    public Tag Get(User user, string name);
    void DeleteTag(Tag tag);
}