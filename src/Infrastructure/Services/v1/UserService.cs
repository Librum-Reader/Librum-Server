using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Infrastructure.Persistance;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.v1;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;


    public UserService(IUserRepository userRepository, ILogger<UserService> logger, IMapper mapper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }
    
    
    public async Task<UserOutDto> GetUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Getting user failed: No user with the given email exists");
            throw new InvalidParameterException("No user with the given email exists");
        }

        return _mapper.Map<UserOutDto>(user);
    }
}