namespace King.Nexa.Platform.Shared.Domain.Model.Events;

public abstract record DomainEvent(Guid EventId, DateTime OccurredAt) : IDomainEvent
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }
}

