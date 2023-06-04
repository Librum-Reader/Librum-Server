using System.Threading.Tasks;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces.Managers;
using Application.Interfaces.Utility;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IAuthenticationManager> _authenticationManagerMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly AuthenticationService _authenticationService;
    
    
    public AuthenticationServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserAutoMapperProfile>();
        });
        var mapper = new Mapper(mapperConfig);

        _authenticationService = new AuthenticationService(
            mapper,
            _authenticationManagerMock.Object,
            _emailSenderMock.Object);
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
        _authenticationManagerMock.Setup(x => x.IsEmailConfirmed(loginDto.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.LoginUserAsync(loginDto);

        // Assert
        Assert.Equal(token, result);
    }
    
    [Fact]
    public async Task AnAuthenticationService_FailsAuthenticatingIfCredentialsWrong()
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
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _authenticationService.LoginUserAsync(loginDto));
    }

    [Fact]
    public async Task AnAuthenticationService_FailsAuthenticatingUserIfAccountIsNotConfirmed()
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
        _authenticationManagerMock.Setup(x => x.IsEmailConfirmed(loginDto.Email))
            .ReturnsAsync(false);

        // Assert
        await Assert.ThrowsAnyAsync<CommonErrorException>(
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
        
        _authenticationManagerMock.Setup(x => x.EmailAlreadyExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _authenticationManagerMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), 
                                                                It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailSenderMock.Setup(
            x => x.SendAccountConfirmationEmail(It.IsAny<User>()));
        

        // Act
        await _authenticationService.RegisterUserAsync(registerDto);
    }

    [Fact]
    public async Task 
        AnAuthenticationService_FailsRegisteringAUserIfUserAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "JohnDoe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SomePassword123"
        };

        _authenticationManagerMock.Setup(
                x => x.EmailAlreadyExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        

        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
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
        
        _authenticationManagerMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(),
                                                                It.IsAny<string>()))
            .ReturnsAsync(false);
        

        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _authenticationService.RegisterUserAsync(registerDto));
    }
}