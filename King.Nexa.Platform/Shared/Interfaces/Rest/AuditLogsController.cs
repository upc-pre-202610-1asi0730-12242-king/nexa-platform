using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Shared.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/audit-logs")]
public class AuditLogsController(IAuditLogQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogResource>>> GetAll([FromQuery] int limit = 100, CancellationToken cancellationToken = default) =>
        Ok((await queryService.ListAsync(limit, cancellationToken)).Select(ToResource));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuditLogResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await queryService.FindByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(ToResource(row));
    }

    private static AuditLogResource ToResource(AuditLog row) => new(
        row.Id, row.TenantId, row.WorkspaceId, row.ActorUserId, row.ActorMembershipId,
        row.Action, row.ResourceType, row.ResourceId, row.MetadataJson, row.CreatedAt);
}

public record AuditLogResource(
    int Id,
    int TenantId,
    int? WorkspaceId,
    int ActorUserId,
    int? ActorMembershipId,
    string Action,
    string ResourceType,
    string ResourceId,
    string? MetadataJson,
    DateTime CreatedAt);

