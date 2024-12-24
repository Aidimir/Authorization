namespace Domain.Models;

public record UserCredentials
{
    public UserCredentials(string login, string? password, string? refreshToken)
    {
        if (refreshToken is null && password is null)
            throw new ArgumentNullException("Password or auth token must be provided");

        Login = login;
        Password = password;
        RefreshToken = refreshToken;
    }

    public string Login { get; }
    public string? Password { get; }
    public string? RefreshToken { get; }
}