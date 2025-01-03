using Api.DTO.Requests;
using Api.DTO.Responses;
using AutoMapper;
using Domain.Models;
using Logic;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly AuthorizeOrchestrationService _orchestrationService;

    public AuthController(IMapper mapper, AuthorizeOrchestrationService orchestrationService)
    {
        _mapper = mapper;
        _orchestrationService = orchestrationService;
    }

    [HttpGet("sign-in")]
    [ProducesResponseType<UserAuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Auth([FromQuery] UserAuthRequest userAuth)
    {
        var mappedCredentials = _mapper.Map<UserCredentials>(userAuth);
        var mappedAuthResponse = _mapper.Map<UserAuthResponse>(await _orchestrationService.Auth(mappedCredentials));
        return Ok(mappedAuthResponse);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest user)
    {
        var mappedUser = _mapper.Map<User>(user);
        try
        {
            return Ok(await _orchestrationService.RegisterUser(mappedUser));
        }
        catch (Exception ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }

    [HttpPost("send-verification-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotificationCode([FromBody] string email)
    {
        await _orchestrationService.SendRegistrationVerificationEmail(email);
        return Ok();
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (await _orchestrationService.VerifyEmail(request.Email, request.Code))
            return Ok();

        return BadRequest();
    }

    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        var result = await _orchestrationService.ValidateToken(token);
        if (result)
            return Ok();

        return BadRequest("Invalid token");
    }
}