namespace King.Nexa.Platform.Shared.Domain.Model.Events;

public abstract class DomainEventContainer : IHasDomainEvents
{
    private readonly List<IDomainEvent> domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    public void ClearDomainEvents() => domainEvents.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent) => domainEvents.Add(domainEvent);
}

