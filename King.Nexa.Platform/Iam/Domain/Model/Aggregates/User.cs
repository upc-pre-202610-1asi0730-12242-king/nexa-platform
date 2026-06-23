using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Iam.Domain.Model.Aggregates;

public class User : AuditableEntity
{
    protected User()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
        FullName = string.Empty;
        Phone = string.Empty;
        PreferredLanguage = "en";
    }

    public User(string username, string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Role is required.", nameof(role));

        Username = username.Trim().ToLowerInvariant();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role.Trim();
        FullName = username.Trim();
        Phone = string.Empty;
        PreferredLanguage = "en";
    }

    public string Username { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string Role { get; private set; }

    public string FullName { get; private set; }

    public string Phone { get; private set; }

    public string PreferredLanguage { get; private set; }

    public bool CriticalNotificationsEnabled { get; private set; } = true;

    public void UpdateProfile(string fullName, string email, string phone, string preferredLanguage, bool criticalNotificationsEnabled)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new InvalidOperationException("Full name is required.");
        if (!System.Net.Mail.MailAddress.TryCreate(email?.Trim(), out var parsedEmail))
            throw new InvalidOperationException("A valid email is required.");
        var language = preferredLanguage?.Trim().ToLowerInvariant() ?? string.Empty;
        if (language is not ("en" or "es")) throw new InvalidOperationException("Preferred language must be en or es.");
        FullName = fullName.Trim();
        Email = parsedEmail.Address.ToLowerInvariant();
        Phone = phone?.Trim() ?? string.Empty;
        PreferredLanguage = language;
        CriticalNotificationsEnabled = criticalNotificationsEnabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new InvalidOperationException("Password hash is required.");
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}

