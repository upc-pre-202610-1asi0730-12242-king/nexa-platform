using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using King.Nexa.Platform.Iam.Application.OutboundServices;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authentication;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace King.Nexa.Platform.Tests;

public class SecurityIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Security_scenarios_enforce_auth_membership_and_tenant_scope()
    {
        await using var factory = new NexaApiFactory();
        await factory.EnsureDatabaseCreatedAsync();
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/catalog-items")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/dispatch-orders/by-tenant/1")).StatusCode);

        var seed = await SeedMinimumWorkspaceAsync(factory);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/v1/tenants/{seed.TenantBId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/workspaces/by-slug/icisa")).StatusCode);
        var publicTenantPreview = await client.GetAsync("/api/v1/tenants/by-slug/icisa");
        Assert.Equal(HttpStatusCode.OK, publicTenantPreview.StatusCode);
        using (var previewJson = JsonDocument.Parse(await publicTenantPreview.Content.ReadAsStringAsync()))
        {
            Assert.True(previewJson.RootElement.TryGetProperty("name", out _));
            Assert.True(previewJson.RootElement.TryGetProperty("slug", out _));
            Assert.False(previewJson.RootElement.TryGetProperty("id", out _));
            Assert.False(previewJson.RootElement.TryGetProperty("legalName", out _));
            Assert.False(previewJson.RootElement.TryGetProperty("ruc", out _));
            Assert.False(previewJson.RootElement.TryGetProperty("emailDomain", out _));
        }
        var session = await SignInAsync(client, "valeria.sanchez@icisa.pe");
        UseSession(client, session, session.TenantId);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/payments")).StatusCode);

        var ownerSession = await SignInAsync(client, "owner@icisa.pe");
        UseSession(client, ownerSession, ownerSession.TenantId);
        var crossTenantUpdate = await client.PutAsJsonAsync($"/api/v1/tenants/{seed.TenantBId}", new
        {
            name = "Tenant B overwritten",
            legalName = "Tenant B overwritten",
            slug = "tenant-b",
            ruc = "20999999999",
            workspaceUrl = "tenant-b.nexa.test",
            emailDomain = "tenant-b.test",
            plan = "Enterprise",
            status = "active",
            country = "PE"
        });
        Assert.Equal(HttpStatusCode.NotFound, crossTenantUpdate.StatusCode);
        var ownTenantUpdate = await client.PutAsJsonAsync($"/api/v1/tenants/{ownerSession.TenantId}", new
        {
            name = "ICISA",
            legalName = "ICISA SAC",
            slug = "icisa",
            ruc = "20123456789",
            workspaceUrl = "icisa.nexa.local",
            emailDomain = "icisa.pe",
            plan = "Standard",
            status = "active",
            country = "PE"
        });
        Assert.Equal(HttpStatusCode.OK, ownTenantUpdate.StatusCode);
        Assert.Equal(
            HttpStatusCode.MethodNotAllowed,
            (await client.PostAsJsonAsync("/api/v1/tenants", new { })).StatusCode);

        UseSession(client, session, 999999);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/dispatch-orders/by-tenant/999999")).StatusCode);

        UseSession(client, session, session.TenantId);
        var ordersJson = await client.GetStringAsync("/api/v1/orders");
        Assert.DoesNotContain("ORD-TENANT-B-ISOLATION", ordersJson, StringComparison.OrdinalIgnoreCase);
        var paymentsJson = await client.GetStringAsync("/api/v1/payments");
        Assert.Contains("PAY-TENANT-A", paymentsJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PAY-TENANT-B", paymentsJson, StringComparison.OrdinalIgnoreCase);
        var clientsJson = await client.GetStringAsync("/api/v1/clients");
        Assert.DoesNotContain("Tenant B client", clientsJson, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/clients/{seed.TenantBClientId}")).StatusCode);

        var createdClient = await client.PostAsJsonAsync("/api/v1/clients", new
        {
            tenantId = seed.TenantBId,
            code = "CLIENT-AUDIT-A",
            businessName = "Tenant A secured client",
            commercialName = "Tenant A secured client",
            ruc = "20111111111",
            segment = "Distributor",
            contact = "Audit contact",
            contactEmail = "audit@icisa.pe",
            phone = "999999999",
            paymentCondition = "credit_15",
            monthlyCreditLimit = 1000m,
            monthlyCreditUsed = 0m,
            monthlyCreditStatus = "ok",
            deliveryPreference = "scheduled_route",
            portalAccess = true,
            sellerWorkspaceEmail = "sales@icisa.pe",
            status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, createdClient.StatusCode);
        using (var createdClientJson = JsonDocument.Parse(await createdClient.Content.ReadAsStringAsync()))
            Assert.Equal(session.TenantId, createdClientJson.RootElement.GetProperty("tenantId").GetInt32());

        var buyerSession = await SignInAsync(client, "buyer@icisa.pe");
        UseSession(client, buyerSession, buyerSession.TenantId);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/sales/order-summaries")).StatusCode);
        var buyerClients = await client.GetStringAsync("/api/v1/clients");
        Assert.Contains("Buyer assigned client", buyerClients, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Buyer unassigned client", buyerClients, StringComparison.OrdinalIgnoreCase);
        var buyerOrdersResponse = await client.GetAsync("/api/v1/orders");
        var buyerOrders = await buyerOrdersResponse.Content.ReadAsStringAsync();
        Assert.True(buyerOrdersResponse.IsSuccessStatusCode, $"{buyerOrdersResponse.StatusCode}: {buyerOrders}");
        Assert.Contains("ORD-BUYER-A", buyerOrders, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ORD-BUYER-OTHER", buyerOrders, StringComparison.OrdinalIgnoreCase);
        var buyerPayments = await client.GetStringAsync("/api/v1/payments");
        Assert.Contains("PAY-TENANT-A", buyerPayments, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PAY-TENANT-A-OTHER", buyerPayments, StringComparison.OrdinalIgnoreCase);
        var buyerDocuments = await client.GetStringAsync("/api/v1/business-documents");
        Assert.Contains("Buyer invoice", buyerDocuments, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Other buyer invoice", buyerDocuments, StringComparison.OrdinalIgnoreCase);
        var buyerCreditRequests = await client.GetStringAsync("/api/v1/credit-requests");
        Assert.Contains("CRQ-BUYER-A", buyerCreditRequests, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CRQ-BUYER-OTHER", buyerCreditRequests, StringComparison.OrdinalIgnoreCase);
        var buyerAuditLogs = await client.GetStringAsync("/api/v1/audit-logs");
        Assert.DoesNotContain("company-wide-audit", buyerAuditLogs, StringComparison.OrdinalIgnoreCase);

        var crossClientCreditRequest = await client.PostAsJsonAsync("/api/v1/credit-requests", new
        {
            clientAccountId = seed.TenantAOtherClientId,
            code = "CRQ-BUYER-CROSS-CLIENT",
            requestedAmount = 500m,
            reason = "Cross-client leak attempt"
        });
        Assert.Equal(HttpStatusCode.Forbidden, crossClientCreditRequest.StatusCode);

        var crossClientRequest = await client.PostAsJsonAsync("/api/v1/purchase-requests", new
        {
            clientAccountId = seed.TenantAOtherClientId,
            code = "REQ-BUYER-CROSS-CLIENT",
            origin = "buyer_portal",
            status = "submitted",
            priority = "normal",
            paymentOption = "bank_transfer"
        });
        Assert.Equal(HttpStatusCode.Forbidden, crossClientRequest.StatusCode);

        var privilegedInitialStatus = await client.PostAsJsonAsync("/api/v1/purchase-requests", new
        {
            clientAccountId = seed.TenantAClientId,
            code = "REQ-BUYER-PRIVILEGED-STATUS",
            origin = "buyer_portal",
            status = "commercially_validated",
            priority = "normal",
            paymentOption = "bank_transfer"
        });
        Assert.Equal(HttpStatusCode.Conflict, privilegedInitialStatus.StatusCode);

        var validBuyerRequest = await client.PostAsJsonAsync("/api/v1/purchase-requests", new
        {
            clientAccountId = seed.TenantAClientId,
            code = "REQ-BUYER-VALID",
            origin = "buyer_portal",
            status = "submitted",
            priority = "normal",
            paymentOption = "bank_transfer"
        });
        Assert.Equal(HttpStatusCode.Created, validBuyerRequest.StatusCode);
        using var validBuyerRequestJson = JsonDocument.Parse(await validBuyerRequest.Content.ReadAsStringAsync());
        var validBuyerRequestId = validBuyerRequestJson.RootElement.GetProperty("id").GetInt32();
        var genericStatusEscalation = await client.PutAsJsonAsync($"/api/v1/purchase-requests/{validBuyerRequestId}", new
        {
            clientAccountId = seed.TenantAOtherClientId,
            code = "REQ-BUYER-VALID",
            origin = "buyer_portal",
            status = "commercially_validated",
            priority = "normal",
            paymentOption = "bank_transfer"
        });
        Assert.Equal(HttpStatusCode.OK, genericStatusEscalation.StatusCode);
        using var genericStatusEscalationJson = JsonDocument.Parse(await genericStatusEscalation.Content.ReadAsStringAsync());
        Assert.Equal("submitted", genericStatusEscalationJson.RootElement.GetProperty("status").GetString());
        Assert.Equal(seed.TenantAClientId, genericStatusEscalationJson.RootElement.GetProperty("clientAccountId").GetInt32());

        var logisticsSession = await SignInAsync(client, "logistics@icisa.pe");
        UseSession(client, logisticsSession, logisticsSession.TenantId);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/organization-registrations")).StatusCode);

        UseSession(client, session, session.TenantId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await CreateMismatchedMembershipTokenAsync(factory, session, seed.OtherMembershipId));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/orders")).StatusCode);

        UseSession(client, session, session.TenantId);
        var created = await client.PostAsJsonAsync("/api/v1/payments", new
        {
            amount = 155.50m,
            currency = "PEN",
            referenceCode = "PAY-AUDIT-001"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        await AssertPaymentAuditExistsAsync(factory, "payment.created");

        await DisableMembershipAsync(factory, session.MembershipStatusEmail);
        UseSession(client, session, session.TenantId);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/orders")).StatusCode);
    }

    private static async Task AssertPaymentAuditExistsAsync(NexaApiFactory factory, string action)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(row => row.Action == action));
    }

    private static async Task<SignInSession> SignInAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/authentication/sign-in", new
        {
            email,
            password = "Password123!",
            workspaceSlug = "icisa"
        });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SignInSession>(json, JsonOptions)
            ?? throw new InvalidOperationException("Sign-in did not return a session.");
    }

    private static void UseSession(HttpClient client, SignInSession session, int tenantHeader)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        client.DefaultRequestHeaders.Remove("X-Nexa-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Nexa-Tenant-Id", tenantHeader.ToString());
    }

    private static async Task DisableMembershipAsync(NexaApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var membership = await db.UserWorkspaceMemberships.FirstAsync(row => row.Email == email);
        membership.Status = "disabled";
        await db.SaveChangesAsync();
    }

    private static async Task<string> CreateMismatchedMembershipTokenAsync(
        NexaApiFactory factory,
        SignInSession session,
        int membershipId)
    {
        using var scope = factory.Services.CreateScope();
        var codec = scope.ServiceProvider.GetRequiredService<JwtTokenCodec>();
        return codec.CreateToken(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
            new Claim("sub", session.Id.ToString()),
            new Claim("email", session.MembershipStatusEmail),
            new Claim(ClaimTypes.Role, "Sales"),
            new Claim("role", "Sales"),
            new Claim("tenant_id", session.TenantId.ToString()),
            new Claim("workspace_id", session.WorkspaceId.ToString()),
            new Claim("workspace_slug", session.WorkspaceSlug),
            new Claim("membership_id", membershipId.ToString())
        });
    }

    private static async Task<SeedResult> SeedMinimumWorkspaceAsync(NexaApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHashing = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

        var tenantA = new Tenant(
            "ICISA",
            "ICISA SAC",
            "icisa",
            "20123456789",
            "icisa.nexa.local",
            "icisa.pe",
            "Standard",
            "active",
            "PE");
        await db.Tenants.AddAsync(tenantA);
        await db.SaveChangesAsync();

        var workspace = new Workspace
        {
            TenantId = tenantA.Id,
            Name = "ICISA Workspace",
            Slug = "icisa",
            Url = "icisa.nexa.local",
            EmailDomain = "icisa.pe",
            Status = "active",
            IsPrimary = true
        };
        await db.Workspaces.AddAsync(workspace);
        await db.SaveChangesAsync();

        var tenantAClient = new ClientAccount(
            tenantA.Id, "CLIENT-BUYER-A", "Buyer assigned client", "Buyer assigned client",
            "20111111112", "Distributor", "Buyer contact", "buyer@icisa.pe", "999999997",
            "credit_15", 1000m, 0m, "ok", "scheduled_route", true,
            "sales@icisa.pe", "active");
        var tenantAOtherClient = new ClientAccount(
            tenantA.Id, "CLIENT-BUYER-OTHER", "Buyer unassigned client", "Buyer unassigned client",
            "20111111113", "Distributor", "Other contact", "other-buyer@icisa.pe", "999999996",
            "credit_15", 1000m, 0m, "ok", "scheduled_route", true,
            "sales@icisa.pe", "active");
        await db.ClientAccounts.AddRangeAsync(tenantAClient, tenantAOtherClient);
        await db.SaveChangesAsync();

        var user = new User(
            "valeria.sanchez@icisa.pe",
            "valeria.sanchez@icisa.pe",
            passwordHashing.HashPassword("Password123!"),
            "Sales");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        await db.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Email = user.Email,
            Role = "Sales",
            Status = "active"
        });

        var owner = new User(
            "owner@icisa.pe",
            "owner@icisa.pe",
            passwordHashing.HashPassword("Password123!"),
            "Company Owner");
        await db.Users.AddAsync(owner);
        await db.SaveChangesAsync();
        await db.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            UserId = owner.Id,
            Email = owner.Email,
            Role = "Company Owner",
            Status = "active"
        });

        var buyer = new User(
            "buyer@icisa.pe",
            "buyer@icisa.pe",
            passwordHashing.HashPassword("Password123!"),
            "Buyer");
        await db.Users.AddAsync(buyer);
        await db.SaveChangesAsync();
        await db.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            UserId = buyer.Id,
            Email = buyer.Email,
            Role = "Buyer",
            Status = "active",
            PortalAccess = true,
            ClientAccountId = tenantAClient.Id
        });

        var logistics = new User(
            "logistics@icisa.pe",
            "logistics@icisa.pe",
            passwordHashing.HashPassword("Password123!"),
            "Logistics Manager");
        await db.Users.AddAsync(logistics);
        await db.SaveChangesAsync();
        await db.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            UserId = logistics.Id,
            Email = logistics.Email,
            Role = "Logistics Manager",
            Status = "active"
        });

        var otherUser = new User(
            "other.user@icisa.pe",
            "other.user@icisa.pe",
            passwordHashing.HashPassword("Password123!"),
            "Sales");
        await db.Users.AddAsync(otherUser);
        await db.SaveChangesAsync();
        var otherMembership = new UserWorkspaceMembership
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            UserId = otherUser.Id,
            Email = otherUser.Email,
            Role = "Sales",
            Status = "active"
        };
        await db.UserWorkspaceMemberships.AddAsync(otherMembership);

        var tenant = new Tenant(
            "Tenant B",
            "Tenant B SAC",
            "tenant-b",
            "20999999991",
            "tenant-b.nexa.local",
            "tenant-b.test",
            "Standard",
            "active",
            "PE");
        await db.Tenants.AddAsync(tenant);
        await db.SaveChangesAsync();

        var tenantBClient = new ClientAccount(
            tenant.Id,
            "CLIENT-TENANT-B",
            "Tenant B client",
            "Tenant B client",
            "20999999992",
            "Distributor",
            "Tenant B contact",
            "contact@tenant-b.test",
            "999999998",
            "credit_15",
            1000m,
            0m,
            "ok",
            "scheduled_route",
            true,
            "sales@tenant-b.test",
            "active");
        await db.ClientAccounts.AddAsync(tenantBClient);
        await db.SaveChangesAsync();

        var order = new Order(new CreateOrderCommand(
            new OrderNumber("ORD-TENANT-B-ISOLATION"),
            new CustomerId(tenantBClient.Code),
            new[]
            {
                new CreateOrderItemCommand(
                    new ProductId("PROD-B"),
                    new CatalogItemId("CAT-B"),
                    new ItemName("Tenant B product"),
                    new Quantity(1),
                    new Money(10, "PEN"))
            }));
        order.AssignTenant(tenant.Id);
        order.AssignClientAccount(tenantBClient.Id);
        await db.Orders.AddAsync(order);
        var buyerOrder = new Order(new CreateOrderCommand(
            new OrderNumber("ORD-BUYER-A"),
            new CustomerId(tenantAClient.Code),
            [new CreateOrderItemCommand(new ProductId("PROD-A"), new CatalogItemId("CAT-A"), new ItemName("Assigned product"), new Quantity(1), new Money(10, "PEN"))]));
        buyerOrder.AssignTenant(tenantA.Id);
        buyerOrder.AssignClientAccount(tenantAClient.Id);
        await db.Orders.AddAsync(buyerOrder);
        var otherBuyerOrder = new Order(new CreateOrderCommand(
            new OrderNumber("ORD-BUYER-OTHER"),
            new CustomerId(tenantAOtherClient.Code),
            [new CreateOrderItemCommand(new ProductId("PROD-A2"), new CatalogItemId("CAT-A2"), new ItemName("Unassigned product"), new Quantity(1), new Money(20, "PEN"))]));
        otherBuyerOrder.AssignTenant(tenantA.Id);
        otherBuyerOrder.AssignClientAccount(tenantAOtherClient.Id);
        await db.Orders.AddAsync(otherBuyerOrder);
        var paymentA = new Payment(new RegisterPaymentCommand(
            InvoiceId: null,
            OrderId: null,
            ClientAccountId: tenantAClient.Id,
            PaymentOptionId: null,
            PaymentMethodRecordId: null,
            new BillingAmount(120, "PEN"),
            "PAY-TENANT-A"));
        paymentA.AssignTenant(tenantA.Id);
        await db.Payments.AddAsync(paymentA);

        var paymentAOther = new Payment(new RegisterPaymentCommand(
            InvoiceId: null,
            OrderId: null,
            ClientAccountId: tenantAOtherClient.Id,
            PaymentOptionId: null,
            PaymentMethodRecordId: null,
            new BillingAmount(180, "PEN"),
            "PAY-TENANT-A-OTHER"));
        paymentAOther.AssignTenant(tenantA.Id);
        await db.Payments.AddAsync(paymentAOther);

        await db.BusinessDocuments.AddRangeAsync(
            new BusinessDocument
            {
                TenantId = tenantA.Id,
                ClientAccountId = tenantAClient.Id,
                Type = "invoice_pdf",
                Label = "Buyer invoice",
                Status = "ready",
                VisibleToBuyer = true,
                Required = true
            },
            new BusinessDocument
            {
                TenantId = tenantA.Id,
                ClientAccountId = tenantAOtherClient.Id,
                Type = "invoice_pdf",
                Label = "Other buyer invoice",
                Status = "ready",
                VisibleToBuyer = true,
                Required = true
            });

        await db.CreditRequests.AddRangeAsync(
            new CreditRequest
            {
                TenantId = tenantA.Id,
                ClientAccountId = tenantAClient.Id,
                Code = "CRQ-BUYER-A",
                RequestedAmount = 1000m,
                Reason = "Buyer assigned credit request",
                Status = "submitted"
            },
            new CreditRequest
            {
                TenantId = tenantA.Id,
                ClientAccountId = tenantAOtherClient.Id,
                Code = "CRQ-BUYER-OTHER",
                RequestedAmount = 1500m,
                Reason = "Buyer unassigned credit request",
                Status = "submitted"
            });
        await db.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantA.Id,
            WorkspaceId = workspace.Id,
            ActorUserId = user.Id,
            Action = "company-wide-audit",
            ResourceType = "client_account",
            ResourceId = tenantAOtherClient.Id.ToString(),
            MetadataJson = """{"scope":"other-client"}"""
        });

        var paymentB = new Payment(new RegisterPaymentCommand(
            InvoiceId: null,
            OrderId: null,
            ClientAccountId: null,
            PaymentOptionId: null,
            PaymentMethodRecordId: null,
            new BillingAmount(220, "PEN"),
            "PAY-TENANT-B"));
        paymentB.AssignTenant(tenant.Id);
        await db.Payments.AddAsync(paymentB);
        await db.SaveChangesAsync();
        return new SeedResult(
            tenant.Id,
            tenantBClient.Id,
            otherMembership.Id,
            tenantAClient.Id,
            tenantAOtherClient.Id);
    }

    private sealed record SignInSession(
        string AccessToken,
        int Id,
        int TenantId,
        int WorkspaceId,
        string WorkspaceSlug,
        string? MembershipStatus)
    {
        public string MembershipStatusEmail => "valeria.sanchez@icisa.pe";
    }

    private sealed record SeedResult(
        int TenantBId,
        int TenantBClientId,
        int OtherMembershipId,
        int TenantAClientId,
        int TenantAOtherClientId);

    private sealed class NexaApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"nexa_platform_test_{Guid.NewGuid():N}";

        public async Task EnsureDatabaseCreatedAsync()
        {
            await using var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=postgres;Username=nexa;Password=nexa_dev_password;SSL Mode=Disable");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"""CREATE DATABASE "{_databaseName}";""";
            await command.ExecuteNonQueryAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        $"Host=localhost;Port=5432;Database={_databaseName};Username=nexa;Password=nexa_dev_password;SSL Mode=Disable",
                    ["SeedData:Enabled"] = "true",
                    ["AllowedOrigins:0"] = "http://localhost:5173",
                    ["Jwt:SigningKey"] = "nexa-integration-test-signing-key-not-for-production"
                });
            });
        }
    }
}
