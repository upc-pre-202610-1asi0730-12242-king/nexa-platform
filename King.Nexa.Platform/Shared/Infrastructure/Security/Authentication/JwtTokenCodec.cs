using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace King.Nexa.Platform.Shared.Infrastructure.Security.Authentication;

public class JwtTokenCodec(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string CreateToken(IEnumerable<Claim> claims)
    {
        var now = timeProvider.GetUtcNow();
        var payload = claims.ToDictionary(claim => claim.Type, claim => (object)claim.Value);
        payload["iss"] = _options.Issuer;
        payload["aud"] = _options.Audience;
        payload["iat"] = now.ToUnixTimeSeconds();
        payload["nbf"] = now.ToUnixTimeSeconds();
        payload["exp"] = now.AddMinutes(_options.ExpirationMinutes).ToUnixTimeSeconds();

        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        }, JsonOptions));
        var body = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
        var unsigned = $"{header}.{body}";
        return $"{unsigned}.{Sign(unsigned)}";
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return null;

        var unsigned = $"{parts[0]}.{parts[1]}";
        var expected = Sign(unsigned);
        if (expected.Length != parts[2].Length ||
            !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(parts[2])))
            return null;

        using var payloadDocument = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = payloadDocument.RootElement;
        if (!ClaimValue(root, "iss").Equals(_options.Issuer, StringComparison.Ordinal)) return null;
        if (!ClaimValue(root, "aud").Equals(_options.Audience, StringComparison.Ordinal)) return null;
        if (!TryUnix(root, "exp", out var exp)) return null;
        if (timeProvider.GetUtcNow().ToUnixTimeSeconds() >= exp) return null;
        if (TryUnix(root, "nbf", out var nbf) && timeProvider.GetUtcNow().ToUnixTimeSeconds() < nbf) return null;

        var claims = new List<Claim>();
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name is "iss" or "aud" or "iat" or "nbf" or "exp") continue;
            claims.Add(new Claim(property.Name, property.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, NexaAuthenticationConstants.Scheme, "email", ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    private string Sign(string unsigned)
    {
        var key = Encoding.UTF8.GetBytes(_options.SigningKey);
        using var hmac = new HMACSHA256(key);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(unsigned)));
    }

    private static string ClaimValue(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? value.ToString() : string.Empty;

    private static bool TryUnix(JsonElement root, string name, out long value)
    {
        value = 0;
        if (!root.TryGetProperty(name, out var jsonValue)) return false;
        if (jsonValue.TryGetInt64(out value)) return true;
        return long.TryParse(jsonValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => string.Empty };
        return Convert.FromBase64String(padded);
    }
}
