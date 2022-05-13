using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.v1;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;


    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    
    public async Task<UserOutDto> GetUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, false);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        return _mapper.Map<UserOutDto>(user);
    }

    public async Task DeleteUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, true);
        if (user == null)
            throw new InvalidParameterException("No user with the given email exists");
        
        _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
    }
}