using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Utility;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IProductRepository _productRepository;
    private readonly IUserBlobStorageManager _userBlobStorageManager;
    private readonly IBookBlobStorageManager _bookBlobStorageManager;


    public UserService(IUserRepository userRepository,
                       IBookRepository bookRepository,
                       IUserBlobStorageManager userBlobStorageManager,
                       IBookBlobStorageManager bookBlobStorageManager, IMapper mapper,
                       IEmailSender emailSender,
                       IConfiguration configuration,
                       UserManager<User> userManager,
                       IProductRepository productRepository)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _bookBlobStorageManager = bookBlobStorageManager;
        _userBlobStorageManager = userBlobStorageManager;
        _mapper = mapper;
        _emailSender = emailSender;
        _configuration = configuration;
        _userManager = userManager;
        _productRepository = productRepository;
    }


    public async Task<UserOutDto> GetUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        var userOut = _mapper.Map<UserOutDto>(user);
        userOut.UsedBookStorage = await _bookRepository.GetUsedBookStorage(user.Id);
        userOut.Role = user.ProductId.IsNullOrEmpty()
            ? "Unknown"
            : (await _productRepository.GetAll().FirstOrDefaultAsync(p => p.ProductId == user.ProductId)).Name;

        return userOut;
    }

    public async Task DeleteUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            throw new CommonErrorException(400, "No user with this email exists", 17);
        }
        
        // Delete all books
        foreach(var book in _bookRepository.GetAllAsync(user.Id))
        {
            // Continue deleting the user even if some parts of it contains invalid data.
            try
            {
                _bookRepository.DeleteBook(book);
                await _bookBlobStorageManager.DeleteBookBlob(book.BookId);
            }
            catch (Exception e)
            {
                // ignored
            }
            
            try
            {
                if(book.HasCover)
                    await _bookBlobStorageManager.DeleteBookCover(book.BookId);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
        
        try
        {
            if(user.HasProfilePicture)
                await DeleteProfilePicture(email);
            user.HasProfilePicture = false;
        }
        catch (Exception e)
        {
            // ignored
        }

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task PatchUserAsync(string email,
                                     JsonPatchDocument<UserForUpdateDto> patchDoc,
                                     ControllerBase controllerBase)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        var userToPatch = _mapper.Map<UserForUpdateDto>(user);

        patchDoc.ApplyTo(userToPatch, controllerBase.ModelState);
        controllerBase.TryValidateModel(controllerBase.ModelState);

        if (!controllerBase.ModelState.IsValid || !userToPatch.DataIsValid)
        {
            const string message = "Updating the user failed";
            throw new CommonErrorException(400, message, 0);
        }

        _mapper.Map(userToPatch, user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangeProfilePicture(string email, MultipartReader reader)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        await _userBlobStorageManager.ChangeProfilePicture(user.Id, reader);
    }

    public async Task<Stream> GetProfilePicture(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        return await _userBlobStorageManager.DownloadProfilePicture(user.Id);
    }

    public async Task DeleteProfilePicture(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        await _userBlobStorageManager.DeleteProfilePicture(user.Id);
    }

    public async Task ChangePasswordAsync(string email, string newPassword)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.SelectMany(
                                         _ =>result.Errors.Select(error => error.Code)));
            var message = "Changing the password failed: " + errors;
            throw new CommonErrorException(400, message, 0);
        }
    }

    public async Task ChangePasswordWithTokenAsync(string email, string token, 
                                                   string newPassword)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            return;
        }
        
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.SelectMany(
                                         _ =>result.Errors.Select(error => error.Code)));
            var message = "Changing the password failed: " + errors;
            throw new CommonErrorException(400, message, 0);
        }
    }

    public async Task ForgotPassword(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            // Don't throw, we don't want the caller to know if a user with this
            // email exists.
            return;
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        if (_configuration["LIBRUM_SELFHOSTED"] != "true")
        {
            await _emailSender.SendPasswordResetEmail(user, token);
        }
        else
        {
            var message = "Reset the password via queries to your DB directly.";
            throw new CommonErrorException(400, message, 0);
        }
    }

    public async Task AddCustomerIdToUser(string email, string customerId)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            throw new CommonErrorException(400, "No user with this email exists", 17);
        }
        
        user.CustomerId = customerId;
        await _userRepository.SaveChangesAsync();
    }
    
    public async Task AddTierToUser(string customerId, string productId)
    {
        var user = await _userRepository.GetByCustomerIdAsync(customerId, trackChanges: true);
        if (user == null)
        {
            throw new CommonErrorException(400, "No user with this customer Id exists", 0);
        }
        
        user.ProductId = productId;
        await _userRepository.SaveChangesAsync();
    }

    public async Task ResetUserToFreeTier(string customerId)
    {
        var user = await _userRepository.GetByCustomerIdAsync(customerId, trackChanges: true);
        if (user == null)
        {
            throw new CommonErrorException(400, "No user with this customerId exists", 0);
        }

        var freeTier = await _productRepository.GetAll().FirstOrDefaultAsync(p => p.Price == 0.0);
        if (freeTier == null)
        {
            throw new CommonErrorException(400, "No free tier exists", 17);
        }

        user.ProductId = freeTier.ProductId;
        await _userRepository.SaveChangesAsync();
    }
}