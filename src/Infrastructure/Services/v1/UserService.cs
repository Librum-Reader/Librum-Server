using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using AutoMapper;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.v1;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;


    public UserService(DataContext context, ILogger<UserService> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }
    
    
    public async Task<UserOutDto> GetUser(string email)
    {
        var user = await _context.Users.SingleOrDefaultAsync(user => user.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Getting user failed: No user with the given email exists");
            throw new InvalidParameterException("No user with the given email exists");
        }

        return _mapper.Map<UserOutDto>(user);
    }
}