using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class ClientAccountResourceAssembler
{
    public static ClientAccountResource ToResource(ClientAccount client) =>
        new(
            client.Id,
            client.TenantId,
            client.Code,
            client.BusinessName,
            client.CommercialName,
            client.Ruc,
            client.Segment,
            client.Contact,
            client.ContactEmail,
            client.Phone,
            client.Address,
            client.District,
            client.Province,
            client.DeliveryReference,
            client.DocumentProfile,
            client.PaymentCondition,
            client.MonthlyCreditLimit,
            client.MonthlyCreditUsed,
            client.MonthlyCreditAvailable,
            client.MonthlyCreditStatus,
            client.DeliveryPreference,
            client.PortalAccess,
            client.SellerWorkspaceEmail,
            client.Status);

    public static ClientAccount ToEntity(CreateClientAccountResource resource, int tenantId) =>
        new(
            tenantId,
            resource.Code,
            resource.BusinessName,
            resource.CommercialName,
            resource.Ruc,
            resource.Segment,
            resource.Contact,
            resource.ContactEmail,
            resource.Phone,
            resource.PaymentCondition,
            resource.MonthlyCreditLimit,
            resource.MonthlyCreditUsed,
            resource.MonthlyCreditStatus,
            resource.DeliveryPreference,
            resource.PortalAccess,
            resource.SellerWorkspaceEmail,
            resource.Status,
            resource.Address ?? string.Empty,
            resource.District ?? string.Empty,
            resource.Province ?? string.Empty,
            resource.DeliveryReference ?? string.Empty,
            resource.DocumentProfile ?? "ruc_factura_xml_pdf_guia");
}
