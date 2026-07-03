using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyTenantManagementConfiguration(this ModelBuilder builder)
    {
        var tenant = builder.Entity<Tenant>();
        tenant.ToTable("tenants");
        tenant.HasKey(entity => entity.Id);
        tenant.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        tenant.Property(entity => entity.LegalName).HasColumnName("legal_name").HasMaxLength(180).IsRequired();
        tenant.Property(entity => entity.Slug).HasColumnName("slug").HasMaxLength(64).IsRequired();
        tenant.Property(entity => entity.Ruc).HasColumnName("ruc").HasMaxLength(16);
        tenant.Property(entity => entity.WorkspaceUrl).HasColumnName("workspace_url").HasMaxLength(160).IsRequired();
        tenant.Property(entity => entity.EmailDomain).HasColumnName("email_domain").HasMaxLength(160).IsRequired();
        tenant.Property(entity => entity.Plan).HasColumnName("plan").HasMaxLength(64).IsRequired();
        tenant.Property(entity => entity.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        tenant.Property(entity => entity.Country).HasColumnName("country").HasMaxLength(8).IsRequired();
        tenant.HasIndex(entity => entity.Slug).IsUnique();
        tenant.HasIndex(entity => entity.Ruc);

        ConfigureTenantMember(builder);
        ConfigureTenantRule(builder);
        ConfigureTenantCustomField(builder);
        ConfigureTenantSubscription(builder);
        ConfigureWorkspaceFeature(builder);
        ConfigureWorkspace(builder);
        ConfigureUserWorkspaceMembership(builder);
        ConfigureWorkspacePreference(builder);
    }

    private static void ConfigureTenantMember(ModelBuilder builder)
    {
        var entity = builder.Entity<TenantMember>();
        entity.ToTable("tenant_members");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.FullName).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Email).HasMaxLength(160).IsRequired();
        entity.Property(row => row.Role).HasMaxLength(80).IsRequired();
        entity.Property(row => row.Department).HasMaxLength(80);
        entity.Property(row => row.Status).HasMaxLength(32).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Email }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTenantRule(ModelBuilder builder)
    {
        var entity = builder.Entity<TenantRule>();
        entity.ToTable("tenant_rules");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Code).HasMaxLength(80).IsRequired();
        entity.Property(row => row.Name).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Description).HasMaxLength(360);
        entity.Property(row => row.Category).HasMaxLength(80).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTenantCustomField(ModelBuilder builder)
    {
        var entity = builder.Entity<TenantCustomField>();
        entity.ToTable("tenant_custom_fields");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Code).HasMaxLength(80).IsRequired();
        entity.Property(row => row.Label).HasMaxLength(140).IsRequired();
        entity.Property(row => row.TargetResource).HasMaxLength(80).IsRequired();
        entity.Property(row => row.FieldType).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTenantSubscription(ModelBuilder builder)
    {
        var entity = builder.Entity<TenantSubscription>();
        entity.ToTable("tenant_subscriptions");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Plan).HasMaxLength(64).IsRequired();
        entity.Property(row => row.PaymentStatus).HasMaxLength(64).IsRequired();
        entity.Property(row => row.BillingContact).HasMaxLength(160);
        entity.HasIndex(row => row.TenantId).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWorkspaceFeature(ModelBuilder builder)
    {
        var entity = builder.Entity<WorkspaceFeature>();
        entity.ToTable("workspace_features");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Code).HasMaxLength(80).IsRequired();
        entity.Property(row => row.Name).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Segment).HasMaxLength(80).IsRequired();
        entity.Property(row => row.PlanRequired).HasMaxLength(64).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWorkspace(ModelBuilder builder)
    {
        var entity = builder.Entity<Workspace>();
        entity.ToTable("workspaces");
        entity.HasKey(row => row.Id);
        entity.HasAlternateKey(row => new { row.TenantId, row.Id });
        entity.Property(row => row.Name).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Slug).HasMaxLength(64).IsRequired();
        entity.Property(row => row.Url).HasMaxLength(160).IsRequired();
        entity.Property(row => row.EmailDomain).HasMaxLength(160).IsRequired();
        entity.Property(row => row.Status).HasMaxLength(32).IsRequired();
        entity.HasIndex(row => row.Slug).IsUnique();
        entity.HasIndex(row => row.TenantId).IsUnique().HasFilter("is_primary = true");
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUserWorkspaceMembership(ModelBuilder builder)
    {
        var entity = builder.Entity<UserWorkspaceMembership>();
        entity.ToTable("user_workspace_memberships");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Email).HasMaxLength(160).IsRequired();
        entity.Property(row => row.FullName).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Role).HasMaxLength(80).IsRequired();
        entity.Property(row => row.Department).HasMaxLength(80);
        entity.Property(row => row.Status).HasMaxLength(32).IsRequired();
        entity.HasIndex(row => new { row.WorkspaceId, row.UserId }).IsUnique();
        entity.HasIndex(row => new { row.TenantId, row.Email });
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.WorkspaceId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<User>().WithMany().HasForeignKey(row => row.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWorkspacePreference(ModelBuilder builder)
    {
        var entity = builder.Entity<WorkspacePreference>();
        entity.ToTable("workspace_preferences");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Key).HasColumnName("preference_key").HasMaxLength(100).IsRequired();
        entity.Property(row => row.Value).HasColumnName("preference_value").HasMaxLength(500).IsRequired();
        entity.Property(row => row.ValueType).HasColumnName("value_type").HasMaxLength(24).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.WorkspaceId, row.Key }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.WorkspaceId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
