using King.Nexa.Platform.Iam.Application.Model;
using King.Nexa.Platform.Iam.Domain.Model.Commands;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Iam.Application.CommandServices;

public interface IUserCommandService
{
    Task<AuthenticatedUser> SignUpAsync(SignUpCommand command, CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> SignInAsync(SignInCommand command, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(string username, string email, string password, string role, CancellationToken cancellationToken = default);
    Task<User?> UpdateProfileAsync(int id, string fullName, string? email, string phone, string preferredLanguage, bool criticalNotificationsEnabled, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}

