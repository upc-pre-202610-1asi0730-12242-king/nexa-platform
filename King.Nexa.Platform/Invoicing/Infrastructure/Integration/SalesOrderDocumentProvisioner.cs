using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Application.OutboundServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Integration;

public class SalesOrderDocumentProvisioner(
    AppDbContext context,
    IBusinessDocumentCommandService documents,
    IPaymentProcessRecordCommandService paymentProcesses,
    INotificationRecordCommandService notifications) : IOrderDocumentProvisioner
{
    private static readonly (string Type, string Label, string Extension)[] RequiredDocuments =
    [
        ("factura_xml", "Factura XML", "xml"),
        ("factura_pdf", "Factura PDF", "pdf"),
        ("guia_pdf", "Guia de remision PDF", "pdf")
    ];

    public async Task ProvisionRequiredDocumentsAsync(Order order, CancellationToken cancellationToken = default)
    {
        var clientId = await context.ClientAccounts.AsNoTracking()
            .Where(row => row.TenantId == order.TenantId && row.Code == order.CustomerId.Value)
            .Select(row => (int?)row.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Client account is required to provision order documents.");

        var existingTypes = await context.BusinessDocuments.AsNoTracking()
            .Where(row => row.TenantId == order.TenantId && row.OrderId == order.Id)
            .Select(row => row.Type)
            .ToListAsync(cancellationToken);

        foreach (var required in RequiredDocuments.Where(required => !existingTypes.Contains(required.Type)))
        {
            await documents.CreateAsync(new BusinessDocument
            {
                TenantId = order.TenantId,
                OrderId = order.Id,
                ClientAccountId = clientId,
                Type = required.Type,
                Label = required.Label,
                FileName = string.Empty,
                Required = true,
                VisibleToBuyer = false
            }, cancellationToken);
        }
    }

    public async Task ProvisionAcceptedOrderAsync(Order order, int clientAccountId, decimal shippingEstimate, CancellationToken cancellationToken = default)
    {
        await ProvisionRequiredDocumentsAsync(order, cancellationToken);
        var shipping = Math.Max(0, shippingEstimate);
        var igv = Math.Round(order.Total.Amount * 0.18m, 2);
        await paymentProcesses.CreateAsync(new PaymentProcessRecord
        {
            TenantId = order.TenantId,
            OrderId = order.Id,
            ClientAccountId = clientAccountId,
            Subtotal = order.Total.Amount,
            Discount = 0,
            Shipping = shipping,
            Igv = igv,
            Total = Math.Round(order.Total.Amount + shipping + igv, 2),
            Status = "pending"
        }, cancellationToken);
        await notifications.CreateAsync(new NotificationRecord
        {
            TenantId = order.TenantId,
            ClientAccountId = clientAccountId,
            RecipientRole = "buyer",
            Type = "order_created",
            Title = $"Order {order.OrderNumber.Value} created",
            Body = "Your request is now in logistics preparation.",
            Read = false
        }, cancellationToken);
    }
}
