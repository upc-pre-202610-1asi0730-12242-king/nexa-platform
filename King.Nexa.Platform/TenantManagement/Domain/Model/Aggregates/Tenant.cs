using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;

public class Tenant : AuditableEntity
{
    protected Tenant()
    {
        Name = string.Empty;
        LegalName = string.Empty;
        Slug = string.Empty;
        Ruc = string.Empty;
        WorkspaceUrl = string.Empty;
        EmailDomain = string.Empty;
        Plan = "Standard";
        Status = "active";
        Country = "PE";
    }

    public Tenant(string name, string legalName, string slug, string ruc, string workspaceUrl, string emailDomain, string plan, string status, string country)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tenant name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Tenant slug is required.", nameof(slug));

        Name = name.Trim();
        LegalName = string.IsNullOrWhiteSpace(legalName) ? Name : legalName.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Ruc = ruc.Trim();
        WorkspaceUrl = workspaceUrl.Trim();
        EmailDomain = NormalizeEmailDomain(emailDomain, workspaceUrl);
        Plan = string.IsNullOrWhiteSpace(plan) ? "Standard" : plan.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? "active" : status.Trim().ToLowerInvariant();
        Country = string.IsNullOrWhiteSpace(country) ? "PE" : country.Trim().ToUpperInvariant();
    }

    public string Name { get; private set; }
    public string LegalName { get; private set; }
    public string Slug { get; private set; }
    public string Ruc { get; private set; }
    public string WorkspaceUrl { get; private set; }
    public string EmailDomain { get; private set; }
    public string Plan { get; private set; }
    public string Status { get; private set; }
    public string Country { get; private set; }

    public void Update(string name, string legalName, string workspaceUrl, string emailDomain, string plan, string status, string country)
    {
        Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        LegalName = string.IsNullOrWhiteSpace(legalName) ? LegalName : legalName.Trim();
        WorkspaceUrl = string.IsNullOrWhiteSpace(workspaceUrl) ? WorkspaceUrl : workspaceUrl.Trim();
        EmailDomain = NormalizeEmailDomain(emailDomain, WorkspaceUrl);
        Plan = string.IsNullOrWhiteSpace(plan) ? Plan : plan.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? Status : status.Trim().ToLowerInvariant();
        Country = string.IsNullOrWhiteSpace(country) ? Country : country.Trim().ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeEmailDomain(string emailDomain, string workspaceUrl)
    {
        var value = string.IsNullOrWhiteSpace(emailDomain) ? workspaceUrl : emailDomain;
        value = value.Trim().ToLowerInvariant()
            .Replace("https://", string.Empty)
            .Replace("http://", string.Empty)
            .Trim('/');
        if (value.StartsWith("www.")) value = value[4..];
        return value;
    }
}
