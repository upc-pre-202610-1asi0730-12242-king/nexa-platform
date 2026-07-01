namespace King.Nexa.Platform.Shared.Domain.Model.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

