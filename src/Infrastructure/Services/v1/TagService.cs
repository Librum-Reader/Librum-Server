using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services.v1;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;


    public TagService(IMapper mapper, ITagRepository tagRepository, IUserRepository userRepository)
    {
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }


    public async Task CreateTagAsync(string userEmail, TagInDto tagIn)
    {
        var user = await _userRepository.GetAsync(userEmail, trackChanges: true);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        await _userRepository.LoadRelationShipsAsync(user);
        
        if (user.Tags.Any(tag => tag.Name == tagIn.Name))
        {
            throw new InvalidParameterException("A tag with the given name already exists");
        }

        
        var tag = _mapper.Map<Tag>(tagIn);
        user.Tags.Add(tag);
        
        await _tagRepository.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(string userEmail, string tagName)
    {
        var user = await _userRepository.GetAsync(userEmail, trackChanges: true);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }
        
        await _userRepository.LoadRelationShipsAsync(user);

        var tag = user.Tags.SingleOrDefault(tag => tag.Name == tagName);
        if (tag == null)
        {
            throw new InvalidParameterException("No tag with the given name exists");
        }


        _tagRepository.DeleteTag(tag);
        
        await _tagRepository.SaveChangesAsync();
    }
}