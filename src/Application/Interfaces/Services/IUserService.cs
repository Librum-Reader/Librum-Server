using Application.Common.DTOs.Tags;
using Application.Common.DTOs.Users;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces.Services;

public interface IUserService
{
    public Task<UserOutDto> GetUserAsync(string email);
    public Task DeleteUserAsync(string email);
    public Task PatchUserAsync(string email,
                               JsonPatchDocument<UserForUpdateDto> patchDoc,
                               ControllerBase controllerBase);
}