using King.Nexa.Platform.Sales.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Sales.Application.OutboundServices;

public interface IOrderDocumentProvisioner
{
    Task ProvisionRequiredDocumentsAsync(Order order, CancellationToken cancellationToken = default);

    Task ProvisionAcceptedOrderAsync(Order order, int clientAccountId, decimal shippingEstimate, CancellationToken cancellationToken = default);
}
