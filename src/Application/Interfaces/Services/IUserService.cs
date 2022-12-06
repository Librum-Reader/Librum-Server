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
    public Task AddTagAsync(string email, string tagName);
    public Task DeleteTagAsync(string email, string tagName);
}