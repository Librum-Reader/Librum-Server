using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Services.v1;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly UserManager<User> _userManager;


    public AuthenticationService(IMapper mapper, IAuthenticationManager authenticationManager, UserManager<User> userManager)
    {
        _mapper = mapper;
        _authenticationManager = authenticationManager;
        _userManager = userManager;
    }
    
    
    public async Task<string> LoginUserAsync(LoginDto loginDto)
    {
        if (!await _authenticationManager.UserExistsAsync(loginDto.Email, loginDto.Password))
        {
            throw new InvalidParameterException("The provided login credentials are wrong");
        }

        return await _authenticationManager.CreateTokenAsync(loginDto);
    }
    
    public async Task RegisterUserAsync(RegisterDto registerDto)
    {
        if (await _authenticationManager.UserExistsAsync(registerDto.Email, registerDto.Password))
        {
            throw new InvalidParameterException("A user with this email already exists");
        }

        var user = _mapper.Map<User>(registerDto);

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidParameterException("The provided data was invalid");
        }

        if(registerDto.Roles != null)
            await _userManager.AddToRolesAsync(user, registerDto.Roles);
    }
}