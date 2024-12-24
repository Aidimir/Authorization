using System.IdentityModel.Tokens.Jwt;
using Api.DTO.Requests;
using AutoMapper;
using Domain.Models;
using Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api;

[ApiController]
public class UserController : ControllerBase
{
    private readonly IUsersService _usersService;

    private readonly IMapper _mapper;

    public UserController(IUsersService usersService, IMapper mapper)
    {
        _usersService = usersService;
        _mapper = mapper;
    }

    private IActionResult? CheckIfCanEdit(string username)
    {
        var currentUsersLogin = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

        if (currentUsersLogin.IsNullOrEmpty())
            return Unauthorized("You are not logged in");
        
        bool isSelfUpdate = username.Equals(currentUsersLogin, StringComparison.OrdinalIgnoreCase);

        bool isAdmin = User.IsInRole("Admin");

        if (!isSelfUpdate && !isAdmin)
            return Forbid("Вы не можете обновлять данные других пользователей.");

        if (!isSelfUpdate && !isAdmin)
            return Forbid("You do not have permission to update this user");
        
        return null;
    }
    
    [Authorize]
    [HttpPut("{userName}/update")]
    [ProducesResponseType<User>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(string userName,
        [FromBody] UpdateUserPersonalDataRequest userPersonalData)
    {
        CheckIfCanEdit(userName);
        var mappedPersonalData = _mapper.Map<PersonalData>(userPersonalData);

        await _usersService.UpdateUserPersonalDataAsync(userName, mappedPersonalData);

        return Accepted();
    }

    [Authorize]
    [HttpDelete("{userName}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser(string userName)
    {
        CheckIfCanEdit(userName);
        await _usersService.RemoveUserAsync(userName);
        return NoContent();
    }
}