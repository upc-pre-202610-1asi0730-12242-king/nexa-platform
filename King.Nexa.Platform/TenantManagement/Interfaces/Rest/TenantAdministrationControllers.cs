using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest;

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/tenant-members")]
public class TenantMembersController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<TenantMemberResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListMembersAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<TenantMemberResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetMemberAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantMemberResource>> Create(UpsertTenantMemberResource r, CancellationToken ct) { try { var x = await commands.CreateMemberAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantMemberResource>> Update(int id, UpsertTenantMemberResource r, CancellationToken ct) { try { var x = await commands.UpdateMemberAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteMemberAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/tenant-rules")]
public class TenantRulesController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<TenantRuleResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListRulesAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<TenantRuleResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetRuleAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantRuleResource>> Create(UpsertTenantRuleResource r, CancellationToken ct) { try { var x = await commands.CreateRuleAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantRuleResource>> Update(int id, UpsertTenantRuleResource r, CancellationToken ct) { try { var x = await commands.UpdateRuleAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteRuleAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/tenant-custom-fields")]
public class TenantCustomFieldsController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<TenantCustomFieldResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListCustomFieldsAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<TenantCustomFieldResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetCustomFieldAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantCustomFieldResource>> Create(UpsertTenantCustomFieldResource r, CancellationToken ct) { try { var x = await commands.CreateCustomFieldAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantCustomFieldResource>> Update(int id, UpsertTenantCustomFieldResource r, CancellationToken ct) { try { var x = await commands.UpdateCustomFieldAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteCustomFieldAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/tenant-subscriptions"), Route("api/v1/subscriptions")]
public class TenantSubscriptionsController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<TenantSubscriptionResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListSubscriptionsAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<TenantSubscriptionResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetSubscriptionAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantSubscriptionResource>> Create(UpsertTenantSubscriptionResource r, CancellationToken ct) { try { var x = await commands.CreateSubscriptionAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<TenantSubscriptionResource>> Update(int id, UpsertTenantSubscriptionResource r, CancellationToken ct) { try { var x = await commands.UpdateSubscriptionAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteSubscriptionAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/workspace-features")]
public class WorkspaceFeaturesController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<WorkspaceFeatureResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListFeaturesAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<WorkspaceFeatureResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetFeatureAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<WorkspaceFeatureResource>> Create(UpsertWorkspaceFeatureResource r, CancellationToken ct) { try { var x = await commands.CreateFeatureAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<WorkspaceFeatureResource>> Update(int id, UpsertWorkspaceFeatureResource r, CancellationToken ct) { try { var x = await commands.UpdateFeatureAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteFeatureAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/workspaces")]
public class WorkspacesController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? slug, CancellationToken ct) { if (!string.IsNullOrWhiteSpace(slug)) { var workspace = await queries.FindWorkspaceBySlugAsync(slug, ct); return workspace is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(workspace)); } return Ok((await queries.ListWorkspacesAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x))); }
    [HttpGet("{id:int}")] public async Task<ActionResult<WorkspaceResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetWorkspaceAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpGet("by-slug/{slug}"), Obsolete("Use GET /api/v1/workspaces?slug={slug}.")] public async Task<ActionResult<WorkspaceResource>> GetBySlug(string slug, CancellationToken ct) { var x = await queries.FindWorkspaceBySlugAsync(slug, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<WorkspaceResource>> Create(UpsertWorkspaceResource r, CancellationToken ct) { try { var x = await commands.CreateWorkspaceAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return WorkspaceFailure(e); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<WorkspaceResource>> Update(int id, UpsertWorkspaceResource r, CancellationToken ct) { try { var x = await commands.UpdateWorkspaceAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return WorkspaceFailure(e); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteWorkspaceAsync(id, ct) ? NoContent() : NotFound();
    private ActionResult<WorkspaceResource> WorkspaceFailure(InvalidOperationException e) =>
        e.Message.Contains("slug", StringComparison.OrdinalIgnoreCase)
            ? Conflict(new { message = e.Message })
            : BadRequest(new { message = e.Message });
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/user-workspace-memberships")]
public class UserWorkspaceMembershipsController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<UserWorkspaceMembershipResource>>> GetAll(CancellationToken ct) => Ok((await queries.ListMembershipsAsync(ct)).Select(x => TenantAdministrationResourceAssembler.ToResource(x)));
    [HttpGet("{id:int}")] public async Task<ActionResult<UserWorkspaceMembershipResource>> GetById(int id, CancellationToken ct) { var x = await queries.GetMembershipAsync(id, ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); }
    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<UserWorkspaceMembershipResource>> Create(UpsertUserWorkspaceMembershipResource r, CancellationToken ct) { try { var x = await commands.CreateMembershipAsync(TenantAdministrationResourceAssembler.ToEntity(r), ct); return CreatedAtAction(nameof(GetById), new { id = x.Id }, TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<ActionResult<UserWorkspaceMembershipResource>> Update(int id, UpsertUserWorkspaceMembershipResource r, CancellationToken ct) { try { var x = await commands.UpdateMembershipAsync(id, TenantAdministrationResourceAssembler.ToEntity(r), ct); return x is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(x)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await commands.DeleteMembershipAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController, Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember), Route("api/v1/workspace-preferences")]
public class WorkspacePreferencesController(ITenantAdministrationQueryService queries, ITenantAdministrationCommandService commands) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspacePreferenceResource>>> GetAll(CancellationToken ct) =>
        Ok((await queries.ListPreferencesAsync(ct)).Select(preference => TenantAdministrationResourceAssembler.ToResource(preference)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkspacePreferenceResource>> GetById(int id, CancellationToken ct)
    {
        var preference = await queries.GetPreferenceAsync(id, ct);
        return preference is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(preference));
    }

    [HttpPost, Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<WorkspacePreferenceResource>> Create(UpsertWorkspacePreferenceResource resource, CancellationToken ct)
    {
        try
        {
            var preference = await commands.CreatePreferenceAsync(TenantAdministrationResourceAssembler.ToEntity(resource), ct);
            return CreatedAtAction(nameof(GetById), new { id = preference.Id }, TenantAdministrationResourceAssembler.ToResource(preference));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}"), HttpPatch("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<WorkspacePreferenceResource>> Update(int id, UpsertWorkspacePreferenceResource resource, CancellationToken ct)
    {
        try
        {
            var preference = await commands.UpdatePreferenceAsync(id, TenantAdministrationResourceAssembler.ToEntity(resource), ct);
            return preference is null ? NotFound() : Ok(TenantAdministrationResourceAssembler.ToResource(preference));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:int}"), Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await commands.DeletePreferenceAsync(id, ct) ? NoContent() : NotFound();
}
