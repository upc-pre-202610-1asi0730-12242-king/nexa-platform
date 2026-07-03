using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Application.OutboundServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Logistics.Infrastructure.Integration;

public class SalesOrderFulfillmentHandoff(
    IDispatchOrderCommandService dispatchOrders,
    ILogisticsOperationalRecordCommandService logisticsRecords) : IOrderFulfillmentHandoff
{
    public async Task<OrderFulfillmentHandoffResult> HandoffAsync(Order order, CancellationToken cancellationToken = default)
    {
        var routeName = order.Delivery.FullAddress;
        if (routeName.Length > 160) routeName = routeName[..160];

        var dispatch = await dispatchOrders.CreateForOrderAsync(
            order.Id,
            null,
            null,
            routeName,
            cancellationToken);
        if (dispatch is null) throw new InvalidOperationException("Logistics could not receive the created order.");
        await logisticsRecords.CreateProofOfDeliveryAsync(new ProofOfDeliveryRecord
        {
            DispatchOrderId = dispatch.Id,
            ReceivedBy = string.Empty,
            CompletedAt = null,
            PhotoReference = false,
            SignatureReference = false,
            Status = "pending",
            Notes = "Pending delivery confirmation."
        }, cancellationToken);

        return new OrderFulfillmentHandoffResult(dispatch.Id);
    }
}
