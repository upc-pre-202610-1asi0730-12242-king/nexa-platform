using King.Nexa.Platform.Shared.Domain.Model.Events;

namespace King.Nexa.Platform.Shared.Domain.Model.Entities;

public abstract class Entity : DomainEventContainer
{
    public int Id { get; protected set; }
}
