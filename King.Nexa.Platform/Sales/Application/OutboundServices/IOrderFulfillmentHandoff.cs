using King.Nexa.Platform.Sales.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Sales.Application.OutboundServices;

public interface IOrderFulfillmentHandoff
{
    Task<OrderFulfillmentHandoffResult> HandoffAsync(Order order, CancellationToken cancellationToken = default);
}

public record OrderFulfillmentHandoffResult(int? DispatchOrderId);
