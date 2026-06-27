using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Aggregates;

public class ClientAccount : AuditableEntity, ITenantScoped
{
    protected ClientAccount()
    {
        Code = string.Empty;
        BusinessName = string.Empty;
        CommercialName = string.Empty;
        Ruc = string.Empty;
        Segment = string.Empty;
        Contact = string.Empty;
        ContactEmail = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
        District = string.Empty;
        Province = string.Empty;
        DeliveryReference = string.Empty;
        DocumentProfile = "ruc_factura_xml_pdf_guia";
        PaymentCondition = "credit_15";
        MonthlyCreditStatus = "ok";
        DeliveryPreference = string.Empty;
        SellerWorkspaceEmail = string.Empty;
        Status = "active";
    }

    public ClientAccount(
        int tenantId,
        string code,
        string businessName,
        string commercialName,
        string ruc,
        string segment,
        string contact,
        string contactEmail,
        string phone,
        string paymentCondition,
        decimal monthlyCreditLimit,
        decimal monthlyCreditUsed,
        string monthlyCreditStatus,
        string deliveryPreference,
        bool portalAccess,
        string sellerWorkspaceEmail,
        string status,
        string address = "",
        string district = "",
        string province = "",
        string deliveryReference = "",
        string documentProfile = "ruc_factura_xml_pdf_guia")
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Client code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(businessName)) throw new ArgumentException("Business name is required.", nameof(businessName));

        TenantId = tenantId;
        Code = code.Trim();
        BusinessName = businessName.Trim();
        CommercialName = string.IsNullOrWhiteSpace(commercialName) ? BusinessName : commercialName.Trim();
        Ruc = ruc.Trim();
        Segment = segment.Trim();
        Contact = contact.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Address = address.Trim();
        District = district.Trim();
        Province = province.Trim();
        DeliveryReference = deliveryReference.Trim();
        DocumentProfile = string.IsNullOrWhiteSpace(documentProfile)
            ? "ruc_factura_xml_pdf_guia"
            : documentProfile.Trim().ToLowerInvariant();
        PaymentCondition = string.IsNullOrWhiteSpace(paymentCondition) ? "credit_15" : paymentCondition.Trim();
        MonthlyCreditLimit = monthlyCreditLimit;
        MonthlyCreditUsed = monthlyCreditUsed;
        MonthlyCreditStatus = string.IsNullOrWhiteSpace(monthlyCreditStatus) ? "ok" : monthlyCreditStatus.Trim();
        DeliveryPreference = deliveryPreference.Trim();
        PortalAccess = portalAccess;
        SellerWorkspaceEmail = sellerWorkspaceEmail.Trim().ToLowerInvariant();
        Status = string.IsNullOrWhiteSpace(status) ? "active" : status.Trim().ToLowerInvariant();
    }

    public int TenantId { get; private set; }
    public string Code { get; private set; }
    public string BusinessName { get; private set; }
    public string CommercialName { get; private set; }
    public string Ruc { get; private set; }
    public string Segment { get; private set; }
    public string Contact { get; private set; }
    public string ContactEmail { get; private set; }
    public string Phone { get; private set; }
    public string Address { get; private set; }
    public string District { get; private set; }
    public string Province { get; private set; }
    public string DeliveryReference { get; private set; }
    public string DocumentProfile { get; private set; }
    public string PaymentCondition { get; private set; }
    public decimal MonthlyCreditLimit { get; private set; }
    public decimal MonthlyCreditUsed { get; private set; }
    public string MonthlyCreditStatus { get; private set; }
    public string DeliveryPreference { get; private set; }
    public bool PortalAccess { get; private set; }
    public string SellerWorkspaceEmail { get; private set; }
    public string Status { get; private set; }

    public decimal MonthlyCreditAvailable => Math.Max(0, MonthlyCreditLimit - MonthlyCreditUsed);

    public void AssignTenant(int tenantId)
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        TenantId = tenantId;
    }

    public void UpdateFrom(ClientAccount source)
    {
        BusinessName = source.BusinessName;
        CommercialName = source.CommercialName;
        Ruc = source.Ruc;
        Segment = source.Segment;
        Contact = source.Contact;
        ContactEmail = source.ContactEmail;
        Phone = source.Phone;
        Address = source.Address;
        District = source.District;
        Province = source.Province;
        DeliveryReference = source.DeliveryReference;
        DocumentProfile = source.DocumentProfile;
        PaymentCondition = source.PaymentCondition;
        MonthlyCreditLimit = source.MonthlyCreditLimit;
        MonthlyCreditUsed = source.MonthlyCreditUsed;
        MonthlyCreditStatus = source.MonthlyCreditStatus;
        DeliveryPreference = source.DeliveryPreference;
        PortalAccess = source.PortalAccess;
        SellerWorkspaceEmail = source.SellerWorkspaceEmail;
        Status = source.Status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureDeliveryProfile(string address, string district, string province, string deliveryReference)
    {
        var changed = false;
        if (string.IsNullOrWhiteSpace(Address) && !string.IsNullOrWhiteSpace(address))
        {
            Address = address.Trim();
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(District) && !string.IsNullOrWhiteSpace(district))
        {
            District = district.Trim();
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(Province) && !string.IsNullOrWhiteSpace(province))
        {
            Province = province.Trim();
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(DeliveryReference) && !string.IsNullOrWhiteSpace(deliveryReference))
        {
            DeliveryReference = deliveryReference.Trim();
            changed = true;
        }
        if (changed) UpdatedAt = DateTime.UtcNow;
    }

    public void NormalizeSellerWorkspaceEmail(string emailDomain)
    {
        if (string.IsNullOrWhiteSpace(SellerWorkspaceEmail) || string.IsNullOrWhiteSpace(emailDomain)) return;
        var localPart = SellerWorkspaceEmail.Split('@', StringSplitOptions.TrimEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(localPart)) return;
        SellerWorkspaceEmail = $"{localPart.ToLowerInvariant()}@{emailDomain.Trim().ToLowerInvariant()}";
        UpdatedAt = DateTime.UtcNow;
    }
}

