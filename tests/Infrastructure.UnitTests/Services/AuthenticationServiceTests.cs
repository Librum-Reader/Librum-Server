using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces;
using Application.Services.v1;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock = 
        TestHelpers.MockUserManager<User>();
    private readonly Mock<IAuthenticationManager> _authenticationManagerMock = new();
    private readonly AuthenticationService _authenticationService;
    
    
    public AuthenticationServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserAutoMapperProfile>();
        });
        var mapper = new Mapper(mapperConfig);

        _authenticationService = new AuthenticationService(mapper,
                                                           _authenticationManagerMock.Object,
                                                           _userManagerMock.Object);
    }


    [Fact]
    public async Task AnAuthenticationService_SucceedsAuthenticatingUser()
    {
        // Arrange
        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(true);
        
        var loginDto = new LoginDto
        {
            Email = "JohnDoe@gmail.com",
            Password = "SomePassword123"
        };
        
        const string token = "KJ32ksMyGeneratedTokenBj2/3C";
        
        _authenticationManagerMock.Setup(x => x.CreateTokenAsync(loginDto))
            .ReturnsAsync(token);

        // Act
        var result = await _authenticationService.LoginUserAsync(loginDto);

        // Assert
        Assert.Equal(token, result);
    }
    
    [Fact]
    public async Task AnAuthenticationService_FailsAuthenticatingIfUserDoesNotExist()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "JohnDoe@gmail.com",
            Password = "SomePassword123"
        };
        
        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(false);
        
        
        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _authenticationService.LoginUserAsync(loginDto));
    }
    
    [Fact]
    public async Task AnAuthenticationService_SucceedsRegisteringAUser()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "JohnDoe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SomePassword123",
            
        };
        
        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        

        // Act
        await _authenticationService.RegisterUserAsync(registerDto);
        
        // Assert
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(),
                                                   It.IsAny<string>()),
                                Times.Once);
    }

    [Fact]
    public async Task AnAuthenticationService_FailsRegisteringAUserIfUserAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "JohnDoe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SomePassword123"
        };

        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(true);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _authenticationService.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task AnAuthenticationService_FailsRegistrationIfDataIsInvalid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "JohnDoe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SomePassword123"
        };
        
        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(),
                                                  It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _authenticationService.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task AnAuthenticationService_SucceedsAddingRoles()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "JohnDoe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SomePassword123",
            Roles = new List<string>() {"Manager", "Client"}
            
        };
        
        _authenticationManagerMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
    
        // Act
        await _authenticationService.RegisterUserAsync(registerDto);
        
        // Assert
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<User>(),
                                                       It.IsAny<IEnumerable<string>>()),
                                Times.Once);
    }
}