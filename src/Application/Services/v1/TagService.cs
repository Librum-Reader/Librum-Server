using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;


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

    public async Task DeleteTagAsync(string email, Guid guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var tag = user.Tags.SingleOrDefault(tag => tag.TagId == guid);
        if (tag == default)
            return;

        tag.UserId = user.Id;
        _tagRepository.Delete(tag);
        
        await _tagRepository.SaveChangesAsync();
    }

    public async Task UpdateTagAsync(string email, TagForUpdateDto tagDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var tag = user.Tags.SingleOrDefault(tag => tag.TagId == tagDto.Guid);
        if (tag == default)
        {
            const string message = "No tag with this name exists";
            throw new CommonErrorException(404, message);
        }
        
        tag.Name = tagDto.Name;
        await _tagRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<TagOutDto>> GetTagsAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        return user.Tags.Select(tag => _mapper.Map<TagOutDto>(tag));
    }
}