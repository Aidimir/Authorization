using System.Security.Authentication;
using Domain.Models;

namespace Logic;

public class AuthorizeOrchestrationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailService _emailService;

    public AuthorizeOrchestrationService(IAuthorizationService authorizationService, IEmailService emailService)
    {
        _authorizationService = authorizationService;
        _emailService = emailService;
    }

    public async Task<UserAuth> Auth(UserCredentials userCredentials)
    {
        return await _authorizationService.Auth(userCredentials);
    }

    public async Task<bool> VerifyEmail(string email, string code)
    {
        var isVerified = await _emailService.VerifyEmailAsync(email, code);
        if (!isVerified)
            throw new ArgumentException("Invalid verification code");

        return isVerified;
    }

    public async Task SendRegistrationVerificationEmail(string email)
    {
        var verificationCode = await _emailService.CreateAndAddToDbVerificationCodeAsync(email);
        await _emailService.SendEmailAsync(email, verificationCode, "Verification Code");
    }

    public async Task<UserAuth> RegisterUser(User user)
    {
        var isEmailNotTaken = await _authorizationService.CheckIsEmailNotTaken(user.Email);
        if (!isEmailNotTaken)
            throw new ArgumentException("Email is already taken");

        if (!await _emailService.IsEmailVerifiedAsync(user.Email))
            throw new AuthenticationException("Email is not verified");

        return await _authorizationService.RegisterUser(user);
    }

    public async Task<bool> ValidateToken(string token)
    {
        return await _authorizationService.ValidateToken(token);
    }
}