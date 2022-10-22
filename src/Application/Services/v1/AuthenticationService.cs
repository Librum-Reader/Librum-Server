using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.v1;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly UserManager<User> _userManager;


    public AuthenticationService(IMapper mapper,
                                 IAuthenticationManager authenticationManager,
                                 UserManager<User> userManager)
    {
        _mapper = mapper;
        _authenticationManager = authenticationManager;
        _userManager = userManager;
    }
    
    
    public async Task<string> LoginUserAsync(LoginDto loginDto)
    {
        var email = loginDto.Email;
        var password = loginDto.Password;
        if (await _authenticationManager.UserExistsAsync(email, password))
            return await _authenticationManager.CreateTokenAsync(loginDto);
        
        const string message = "The login credentials are wrong";
        throw new InvalidParameterException(message);
    }
    
    public async Task RegisterUserAsync(RegisterDto registerDto)
    {
        if (await _authenticationManager
                .UserExistsAsync(registerDto.Email, registerDto.Password))
        {
            const string message = "A user with this email already exists";
            throw new InvalidParameterException(message);
        }

        var user = _mapper.Map<User>(registerDto);

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            const string message = "The provided data was invalid";
            throw new InvalidParameterException(message);
        }

        if(registerDto.Roles != null)
            await _userManager.AddToRolesAsync(user, registerDto.Roles);
    }
}