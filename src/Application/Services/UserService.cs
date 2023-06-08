using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly IUserBlobStorageManager _userBlobStorageManager;


    public UserService(IUserRepository userRepository,
                       IBookRepository bookRepository,
                       IUserBlobStorageManager userBlobStorageManager,
                       IMapper mapper)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _mapper = mapper;
        _userBlobStorageManager = userBlobStorageManager;
    }


    public async Task<UserOutDto> GetUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        var userOut = _mapper.Map<UserOutDto>(user);
        userOut.UsedBookStorage = await _bookRepository.GetUsedBookStorage(user.Id);

        return userOut;
    }

    public async Task DeleteUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

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

        return await _userBlobStorageManager.DownloadProfilePicture(email);
    }

    public async Task DeleteProfilePicture(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        await _userBlobStorageManager.DeleteProfilePicture(user.Id);
    }
}