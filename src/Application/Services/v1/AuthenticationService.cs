using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services.v1;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationManager _authenticationManager;


    public AuthenticationService(IMapper mapper,
                                 IAuthenticationManager authenticationManager)
    {
        _mapper = mapper;
        _authenticationManager = authenticationManager;
    }
    
    
    public async Task<string> LoginUserAsync(LoginDto loginDto)
    {
        var email = loginDto.Email;
        var password = loginDto.Password;
        if (await _authenticationManager.UserExistsAsync(email, password))
            return await _authenticationManager.CreateTokenAsync(loginDto);
        
        const string message = "Invalid email or password";
        throw new CommonErrorException(401, message);
    }
    
    public async Task RegisterUserAsync(RegisterDto registerDto)
    {
        if (await _authenticationManager.EmailAlreadyExistsAsync(registerDto.Email))
        {
            const string message = "A user with this email already exists";
            throw new CommonErrorException(400, message);
        }

        var user = _mapper.Map<User>(registerDto);

        var success =
            await _authenticationManager.CreateUserAsync(user, registerDto.Password);
        if (!success)
        {
            const string message = "The provided data was invalid";
            throw new CommonErrorException(400, message);
        }
    }
}