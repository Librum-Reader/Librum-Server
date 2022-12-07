using Application.Common.DTOs.Tags;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;


namespace Application.Services.v1;

public class TagService : ITagService
{
    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;


    public TagService(IMapper mapper,
                      ITagRepository tagRepository,
                      IUserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
    }


    public async Task CreateTagAsync(string email, TagInDto tagIn)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var tag = _mapper.Map<Tag>(tagIn);
        tag!.UserId = user.Id;

        _tagRepository.Add(tag);
        await _tagRepository.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(string email, string guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var tag = user.Tags.SingleOrDefault(tag => tag.TagId == new Guid(guid));
        tag!.UserId = user.Id;

        _tagRepository.Delete(tag);
        await _tagRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<TagOutDto>> GetTagsAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        return user.Tags.Select(tag => _mapper.Map<TagOutDto>(tag));
    }
}