namespace King.Nexa.Platform.Shared.Domain.Model.Entities;

public interface ITenantScoped
{
    int TenantId { get; }
}

