using King.Nexa.Platform.Iam.Application.CommandServices;
using King.Nexa.Platform.Iam.Application.Model;
using King.Nexa.Platform.Iam.Application.OutboundServices;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Domain.Model.Commands;
using King.Nexa.Platform.Iam.Domain.Repositories;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Iam.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHashingService passwordHashingService,
    ITokenService tokenService,
    IWorkspaceSessionLookupService workspaceSessionLookup) : IUserCommandService
{
    public async Task<AuthenticatedUser> SignUpAsync(SignUpCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.FindByUsernameAsync(command.Username, cancellationToken);
        if (existingUser is not null)
            throw new InvalidOperationException("Username is already registered.");

        var user = new User(command.Username, command.Email, passwordHashingService.HashPassword(command.Password), command.Role);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new AuthenticatedUser(user, tokenService.GenerateToken(user));
    }

    public async Task<AuthenticatedUser?> SignInAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username, cancellationToken);
        if (user is null || !passwordHashingService.VerifyPassword(command.Password, user.PasswordHash))
            return null;

        if (string.IsNullOrWhiteSpace(command.WorkspaceSlug))
            return new AuthenticatedUser(user, tokenService.GenerateToken(user));

        var session = await workspaceSessionLookup.FindActiveSessionAsync(user.Id, command.WorkspaceSlug, cancellationToken);
        return session is null
            ? null
            : new AuthenticatedUser(
                user,
                tokenService.GenerateToken(user, session.Tenant, session.Workspace, session.Membership),
                session.Tenant,
                session.Workspace,
                session.Membership);
    }

    public async Task<User> CreateAsync(string username, string email, string password, string role, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = string.IsNullOrWhiteSpace(username) ? email.Split('@')[0] : username.Trim();
        if (await userRepository.FindByUsernameAsync(normalizedUsername, cancellationToken) is not null)
            throw new InvalidOperationException("Username is already registered.");
        var user = new User(normalizedUsername, email, passwordHashingService.HashPassword(password), role);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateProfileAsync(int id, string fullName, string phone, string preferredLanguage, bool criticalNotificationsEnabled, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(id, cancellationToken);
        if (user is null) return null;
        user.UpdateProfile(fullName, phone, preferredLanguage, criticalNotificationsEnabled);
        userRepository.Update(user);
        await unitOfWork.CompleteAsync(cancellationToken);
        return user;
    }
}
