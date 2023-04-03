using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.DTOs.Users;
using Application.Managers;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Application.UnitTests;

public class AuthenticationManagerTests
{
    private readonly AuthenticationManager _authenticationManager;
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock =
        TestHelpers.MockUserManager<User>();


    public AuthenticationManagerTests()
    {
        _authenticationManager = new AuthenticationManager(_configurationMock.Object,
                                                           _userManagerMock.Object);
    }


    [Fact]
    public async Task AnAuthenticationManager_SucceedsCheckingIfAUserExists()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(),
                                                         It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com",
                                                                  "MyPassword123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AnAuthenticationManager_FailsCheckingIfAUserExistsIfEmailIsWrong()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        // Act
        var result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com",
                                                                  "MyPassword123");

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task 
        AnAuthenticationManager_FailsCheckingIfAUserExistsIfPasswordIsWrong()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(),
                                                         It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com",
                                                                  "MyPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AnAuthenticationManager_SucceedsCheckingIfTheEmailExists()
    {
        // Arrange
        string email = "SomeEmail";

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        // Act
        var result = await _authenticationManager.EmailAlreadyExistsAsync(email);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task
        AnAuthenticationManager_FailsCheckingIfTheEmailExistsIfEmailDoesNotExist()
    {
        // Arrange
        string email = "SomeEmail";

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        // Act
        var result = await _authenticationManager.EmailAlreadyExistsAsync(email);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task AnAuthenticationManager_SucceedsCreatingToken()
    {
        // Arrange
        var user = new User
        {
            Email = "johnDoe@gmail.com",
            UserName = "johnDoe@gmail.com",
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        
        var loginDto = new LoginDto
        {
            Email = "JohnDoe@gmail.com",
            Password = "MyPassword123"
        };
        
        _configurationMock.Setup(x => x[It.IsAny<string>()])
            .Returns("SomeLoooooooooooooooooooooooooooooooooongString12345");

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsNotNull<string>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>() {"Manager", "User"});
        
        
        // Act
        var result = await _authenticationManager.CreateTokenAsync(loginDto);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task AnAuthenticationManager_FailsCreatingTokenIfUserDoesNotExist()
    {
        // Arrange
        _configurationMock.Setup(p => p[It.IsAny<string>()])
            .Returns("SomeString12345");
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _authenticationManager.CreateTokenAsync(null));
    }
}