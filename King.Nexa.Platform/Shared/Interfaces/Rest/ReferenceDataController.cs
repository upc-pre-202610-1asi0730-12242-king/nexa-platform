using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/reference")]
public class ReferenceDataController(AppDbContext context) : ControllerBase
{
    [HttpGet("departments")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var countries = await context.Countries.AsNoTracking().ToDictionaryAsync(row => row.Id, row => row.Code, cancellationToken);
        var rows = await context.Departments.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Code, row.Label, countries.GetValueOrDefault(row.CountryId), row.IsActive)));
    }

    [HttpGet("provinces")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken)
    {
        var departments = await context.Departments.AsNoTracking().ToDictionaryAsync(row => row.Id, row => row.Code, cancellationToken);
        var rows = await context.Provinces.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Code, row.Label, departments.GetValueOrDefault(row.DepartmentId), row.IsActive)));
    }

    [HttpGet("districts")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDistricts(CancellationToken cancellationToken)
    {
        var provinces = await context.Provinces.AsNoTracking().ToDictionaryAsync(row => row.Id, row => row.Code, cancellationToken);
        var rows = await context.Districts.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Code, row.Label, provinces.GetValueOrDefault(row.ProvinceId), row.IsActive)));
    }

    [HttpGet("countries")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var rows = await context.Countries.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Code, row.Label, null, row.IsActive)));
    }

    [HttpGet("payment-options")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentOptions(CancellationToken cancellationToken)
    {
        var rows = await context.PaymentOptions.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Key, row.Label, null, row.IsActive)));
    }

    [HttpGet("delivery-methods")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetDeliveryMethods() => Ok(new[]
    {
        new ReferenceOptionResource(0, "scheduled_route", "Scheduled cold route"),
        new ReferenceOptionResource(0, "buyer_pickup", "Buyer pickup"),
        new ReferenceOptionResource(0, "third_party_cold_carrier", "Third-party cold carrier")
    });

    [HttpGet("document-types")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken cancellationToken)
    {
        var rows = await context.DocumentTypes.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Key, row.Label, null, row.IsActive)));
    }

    [HttpGet("statuses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetStatuses() => Ok(new
    {
        purchaseRequests = new[] { "submitted", "buyer_adjustment_requested", "commercially_validated", "rejected", "cancelled", "converted_to_order" },
        dispatchOrders = new[] { "ready_for_operations", "assigned", "scheduled", "in_route", "delivered", "incident", "reprogrammed" },
        orders = new[] { "pending", "confirmed", "rejected", "cancelled", "paid" },
        invoices = new[] { "pending", "paid", "cancelled" },
        payments = new[] { "pending", "confirmed", "failed", "rejected", "cancelled" },
        documents = new[] { "pending", "uploaded", "ready", "missing", "accepted" }
    });

    [HttpGet("units-of-measure")]
    [ProducesResponseType(typeof(IEnumerable<ReferenceOptionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUnitsOfMeasure(CancellationToken cancellationToken)
    {
        var rows = await context.UnitsOfMeasure.AsNoTracking()
            .Where(row => row.IsActive)
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(row => new ReferenceOptionResource(row.Id, row.Key, row.Label, null, row.IsActive)));
    }
}

public record ReferenceOptionResource(int Id, string Code, string Label, string? ParentCode = null, bool IsActive = true);

