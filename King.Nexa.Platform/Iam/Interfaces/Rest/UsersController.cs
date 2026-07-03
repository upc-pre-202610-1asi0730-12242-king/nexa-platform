using King.Nexa.Platform.Iam.Application.CommandServices;
using King.Nexa.Platform.Iam.Application.QueryServices;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Iam.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/users")]
public class UsersController(
    IUserQueryService queries,
    IUserCommandService commands,
    ICurrentUserContext currentUser,
    ICurrentWorkspaceContext currentWorkspace) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<IEnumerable<UserResource>>> GetAll(CancellationToken cancellationToken)
    {
        var tenantId = currentWorkspace.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        return Ok((await queries.ListByTenantAsync(tenantId, cancellationToken)).Select(ToResource));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<UserResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var tenantId = currentWorkspace.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        var user = (await queries.ListByTenantAsync(tenantId, cancellationToken)).FirstOrDefault(row => row.Id == id);
        return user is null ? NotFound() : Ok(ToResource(user));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResource>> GetCurrent(CancellationToken cancellationToken)
    {
        var user = await queries.FindByIdAsync(CurrentUserId(), cancellationToken);
        return user is null ? NotFound() : Ok(ToResource(user));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<UserResource>> Create(CreateUserResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var user = await commands.CreateAsync(resource.Username, resource.Email, resource.Password, resource.Role, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResource(user));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserResource>> UpdateCurrent(UpdateUserProfileResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var user = await commands.UpdateProfileAsync(
                CurrentUserId(), resource.FullName, resource.Email, resource.Phone,
                resource.PreferredLanguage, resource.CriticalNotificationsEnabled,
                cancellationToken);
            return user is null ? NotFound() : Ok(ToResource(user));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private int CurrentUserId() => currentUser.UserId ?? throw new InvalidOperationException("Current user is required.");

    private static UserResource ToResource(User user) => new(
        user.Id, user.Username, user.Email, user.Role, user.FullName, user.Phone,
        user.PreferredLanguage, user.CriticalNotificationsEnabled);
}

public record CreateUserResource(string Username, string Email, string Password, string Role);
public record UpdateUserProfileResource(string FullName, string? Email, string Phone, string PreferredLanguage, bool CriticalNotificationsEnabled);
public record UserResource(int Id, string Username, string Email, string Role, string FullName, string Phone, string PreferredLanguage, bool CriticalNotificationsEnabled);
