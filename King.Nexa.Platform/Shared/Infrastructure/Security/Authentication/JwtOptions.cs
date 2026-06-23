namespace King.Nexa.Platform.Shared.Infrastructure.Security.Authentication;

public class JwtOptions
{
    public string Issuer { get; set; } = "nexa-platform";
    public string Audience { get; set; } = "nexa-webapp";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 720;
}
