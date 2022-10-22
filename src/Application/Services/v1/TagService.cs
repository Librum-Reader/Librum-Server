using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;


namespace Application.Services.v1;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;


    public TagService(IMapper mapper,
                      ITagRepository tagRepository,
                      IUserRepository userRepository)
    {
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }


    public async Task CreateTagAsync(string email, TagInDto tagIn)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        var tag = _mapper.Map<Tag>(tagIn);
        user.Tags.Add(tag);
        
        await _tagRepository.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(string email, string tagName)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var tag = user.Tags.Single(tag => tag.Name == tagName);

        _tagRepository.Delete(tag);
        
        await _tagRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<TagOutDto>> GetTagsAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        return user.Tags.Select(tag => _mapper.Map<TagOutDto>(tag));
    }
}