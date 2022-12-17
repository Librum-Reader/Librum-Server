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

    public async Task DeleteTagAsync(string email, string guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var tag = user.Tags.SingleOrDefault(tag => tag.TagId == new Guid(guid));
        tag!.UserId = user.Id;

        _tagRepository.Delete(tag);
        await _tagRepository.SaveChangesAsync();
    }

    public async Task UpdateTagAsync(string email, string guid,
                                     TagForUpdateDto tagUpdate)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var tag = user.Tags.SingleOrDefault(tag => tag.TagId == new Guid(guid));
        if (tag == default)
        {
            const string message = "No tag with this guid exists";
            throw new InvalidParameterException(message);
        }
        
        tag.Name = tagUpdate.Name;
        await _tagRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<TagOutDto>> GetTagsAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        return user.Tags.Select(tag => _mapper.Map<TagOutDto>(tag));
    }
}