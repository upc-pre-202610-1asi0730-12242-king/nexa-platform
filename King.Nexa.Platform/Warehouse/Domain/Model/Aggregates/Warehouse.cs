using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;

public class Warehouse : AuditableEntity, ITenantScoped
{
    protected Warehouse()
    {
        Name = null!;
        Location = null!;
        TemperatureRange = null!;
    }

    public Warehouse(CreateWarehouseCommand command)
    {
        Name = command.Name;
        Location = command.Location;
        TemperatureRange = command.TemperatureRange;
        IsActive = true;
    }

    public WarehouseName Name { get; private set; }

    public int TenantId { get; private set; }

    public WarehouseLocation Location { get; private set; }

    public TemperatureRange TemperatureRange { get; private set; }

    public bool IsActive { get; private set; }

    public void AssignTenant(int tenantId)
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id must be positive.", nameof(tenantId));
        TenantId = tenantId;
    }

    public void Update(UpdateWarehouseCommand command)
    {
        Name = command.Name;
        Location = command.Location;
        TemperatureRange = command.TemperatureRange;
    }

    public void Deactivate() => IsActive = false;
}
