using System.Reflection;
using System.Text.Json;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

public sealed class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Capture(DbContext? context)
    {
        if (context is null) return;
        var containers = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        foreach (var container in containers)
        {
            foreach (var domainEvent in container.DomainEvents)
            {
                var eventType = domainEvent.GetType();
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Id = domainEvent.EventId,
                    OccurredOnUtc = domainEvent.OccurredAt,
                    Type = eventType.FullName ?? eventType.Name,
                    Payload = JsonSerializer.Serialize(domainEvent, eventType),
                    TenantId = IntProperty(domainEvent, "TenantId"),
                    WorkspaceId = IntProperty(domainEvent, "WorkspaceId")
                });
            }
            container.ClearDomainEvents();
        }
    }

    private static int? IntProperty(IDomainEvent domainEvent, string name) =>
        domainEvent.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(domainEvent) as int?;
}

