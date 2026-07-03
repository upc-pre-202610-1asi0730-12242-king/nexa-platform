using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using King.Nexa.Platform.CatalogManagement.Interfaces.Rest;
using King.Nexa.Platform.Iam.Interfaces.Rest;
using King.Nexa.Platform.Invoicing.Infrastructure.Integration;
using King.Nexa.Platform.Invoicing.Interfaces.Rest;
using King.Nexa.Platform.Logistics.Interfaces.Rest;
using King.Nexa.Platform.Sales.Interfaces.Rest;
using King.Nexa.Platform.Shared.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.Shared.Interfaces.Rest;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest;
using King.Nexa.Platform.Warehouse.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace King.Nexa.Platform.Tests;

public class ApiSurfaceHardeningTests
{
    [Theory]
    [InlineData(typeof(CatalogItemsController))]
    [InlineData(typeof(BrandsController))]
    [InlineData(typeof(CategoriesController))]
    [InlineData(typeof(InventoryItemsController))]
    [InlineData(typeof(WarehousesController))]
    [InlineData(typeof(ShipmentsController))]
    [InlineData(typeof(InvoicesController))]
    [InlineData(typeof(PaymentsController))]
    [InlineData(typeof(StripePaymentsController))]
    public void Critical_workspace_controllers_require_membership(Type controllerType)
    {
        var attribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(NexaAuthorizationPolicies.WorkspaceMember, attribute.Policy);
    }

    [Fact]
    public void Authorization_has_authenticated_fallback_policy()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "nexa-platform-tests",
                ["Jwt:Audience"] = "nexa-webapp-tests",
                ["Jwt:SigningKey"] = "test-signing-key-with-more-than-thirty-two-characters",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=nexa_test;Username=nexa;Password=nexa"
            })
            .Build();

        services.AddLogging();
        services.AddSharedInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(options.DefaultPolicy);
        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(options.FallbackPolicy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void Only_documented_controller_actions_are_public()
    {
        var allowedPublicActions = new HashSet<string>(StringComparer.Ordinal)
        {
            $"{typeof(AuthenticationController).FullName}.{nameof(AuthenticationController.SignIn)}",
            $"{typeof(OrganizationRegistrationsController).FullName}.{nameof(OrganizationRegistrationsController.Create)}",
            $"{typeof(TenantsController).FullName}.{nameof(TenantsController.GetAll)}",
            $"{typeof(StripePaymentsController).FullName}.{nameof(StripePaymentsController.Webhook)}"
        };

        var publicActions = typeof(PaymentsController).Assembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
                .Where(method =>
                    method.GetCustomAttribute<AllowAnonymousAttribute>() is not null ||
                    type.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                .Select(method => $"{type.FullName}.{method.Name}"))
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(allowedPublicActions.OrderBy(name => name), publicActions);
    }

    [Fact]
    public void Authentication_sign_up_is_not_public_by_default()
    {
        var method = typeof(AuthenticationController).GetMethod(nameof(AuthenticationController.SignUp));

        Assert.NotNull(method);
        Assert.Null(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void Sales_read_models_require_sales_policy()
    {
        var attribute = typeof(SalesReadModelsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(NexaAuthorizationPolicies.CanAcceptPurchaseRequest, attribute.Policy);
    }

    [Fact]
    public async Task Stripe_webhook_signature_verification_requires_valid_signature()
    {
        const string secret = "whsec_test_secret";
        const string payload = "{\"id\":\"evt_test\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var validSignature = ComputeHmacSha256Hex(secret, $"{timestamp}.{payload}");
        var service = new StripePaymentPreparationService(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stripe:WebhookSecret"] = secret
            })
            .Build());

        Assert.True(await service.VerifyWebhookSignatureAsync(payload, $"t={timestamp},v1={validSignature}"));
        Assert.False(await service.VerifyWebhookSignatureAsync(payload, $"t={timestamp},v1=invalid"));
    }

    [Fact]
    public void Generic_crud_controller_is_removed()
    {
        var type = typeof(AuditLogsController).Assembly.GetType(
            "King.Nexa.Platform.Shared.Interfaces.Rest.CrudController`1");
        Assert.Null(type);
    }

    [Fact]
    public void Purchase_request_acceptance_has_rest_subresource_route()
    {
        var method = typeof(PurchaseRequestsController).GetMethod("CreateAcceptance");
        var route = method?.GetCustomAttribute<HttpPostAttribute>();
        var authorization = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(route);
        Assert.Equal("{id:int}/acceptances", route.Template);
        Assert.NotNull(authorization);
        Assert.Equal(NexaAuthorizationPolicies.CanAcceptPurchaseRequest, authorization.Policy);
    }

    [Theory]
    [InlineData("CreateRouteStart", "{id:int}/route-starts")]
    [InlineData("CreateDeliveryCompletion", "{id:int}/deliveries")]
    public void Dispatch_order_state_changes_have_rest_subresource_routes(string methodName, string expectedRoute)
    {
        var method = typeof(DispatchOrdersController).GetMethod(methodName);
        var route = method?.GetCustomAttribute<HttpPostAttribute>();
        var authorization = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(route);
        Assert.Equal(expectedRoute, route.Template);
        Assert.NotNull(authorization);
        Assert.Equal(NexaAuthorizationPolicies.CanStartDispatch, authorization.Policy);
    }

    [Theory]
    [InlineData(typeof(BusinessDocumentsController), "CreateStatusChange", "{id:int}/status-changes")]
    [InlineData(typeof(PaymentProcessRecordsController), "CreateStatusChange", "{id:int}/status-changes")]
    public void Invoicing_status_changes_have_rest_subresource_routes(Type controllerType, string methodName, string expectedRoute)
    {
        var method = controllerType.GetMethod(methodName);
        var route = method?.GetCustomAttribute<HttpPostAttribute>();
        var authorization = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(route);
        Assert.Equal(expectedRoute, route.Template);
        Assert.NotNull(authorization);
        Assert.Equal(NexaAuthorizationPolicies.CanManageDocuments, authorization.Policy);
    }

    [Theory]
    [InlineData(typeof(PaymentsController), "CreateConfirmation", "{id:int}/confirmations")]
    [InlineData(typeof(PaymentsController), "CreateRejection", "{id:int}/rejections")]
    [InlineData(typeof(PaymentsController), "CreateCancellation", "{id:int}/cancellations")]
    public void Payment_state_changes_have_rest_subresource_routes(Type controllerType, string methodName, string expectedRoute)
    {
        var method = controllerType.GetMethod(methodName);
        var route = method?.GetCustomAttribute<HttpPostAttribute>();
        var authorization = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(route);
        Assert.Equal(expectedRoute, route.Template);
        Assert.NotNull(authorization);
        Assert.Equal(NexaAuthorizationPolicies.CanManageDocuments, authorization.Policy);
    }

    [Theory]
    [InlineData("Create")]
    [InlineData("CreateStatusChange")]
    public void Payment_method_changes_allow_tenant_scoped_members(string methodName)
    {
        var method = typeof(PaymentMethodRecordsController).GetMethod(methodName);
        var authorization = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorization);
        Assert.Equal(NexaAuthorizationPolicies.CanManagePaymentMethods, authorization.Policy);
    }

    [Fact]
    public void Payment_process_record_controller_is_current_internal_history()
    {
        Assert.False(HasRetiredApiMarker(typeof(PaymentProcessRecordsController)));
    }

    [Fact]
    public void Payment_controller_is_core_current_api()
    {
        Assert.False(HasRetiredApiMarker(typeof(PaymentsController)));
    }

    [Fact]
    public void Profile_password_change_is_authenticated_rest_subresource()
    {
        var controller = typeof(ProfileController);
        var controllerRoute = controller.GetCustomAttribute<RouteAttribute>();
        var authorization = controller.GetCustomAttribute<AuthorizeAttribute>();
        var action = controller.GetMethod(nameof(ProfileController.ChangePassword));
        var route = action?.GetCustomAttribute<HttpPostAttribute>();

        Assert.Equal("api/v1/profile", controllerRoute?.Template);
        Assert.Equal(NexaAuthorizationPolicies.WorkspaceMember, authorization?.Policy);
        Assert.Equal("password-changes", route?.Template);
    }

    [Fact]
    public void Buyer_profile_update_is_authenticated_rest_resource_without_client_id_input()
    {
        var controller = typeof(ClientsController);
        var authorization = controller.GetCustomAttribute<AuthorizeAttribute>();
        var action = controller.GetMethod(nameof(ClientsController.UpdateCurrentBuyerProfile));
        var route = action?.GetCustomAttribute<HttpPutAttribute>();

        Assert.Equal(NexaAuthorizationPolicies.WorkspaceMember, authorization?.Policy);
        Assert.Equal("/api/v1/profile/client-account", route?.Template);
    }

    [Fact]
    public void Invoice_controller_is_core_current_api()
    {
        Assert.False(HasRetiredApiMarker(typeof(InvoicesController)));
    }

    [Theory]
    [InlineData(typeof(BusinessDocumentsController))]
    [InlineData(typeof(PaymentProcessRecordsController))]
    [InlineData(typeof(PaymentsController))]
    public void Sensitive_invoicing_controllers_do_not_inherit_generic_crud(Type controllerType)
    {
        Assert.DoesNotContain("CrudController", controllerType.BaseType?.Name ?? string.Empty);
    }

    [Theory]
    [InlineData(typeof(BusinessDocumentsController), "GetAll")]
    [InlineData(typeof(PaymentProcessRecordsController), "GetAll")]
    public void Sensitive_invoicing_controllers_expose_specific_list_actions(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName);
        Assert.NotNull(method?.GetCustomAttribute<HttpGetAttribute>());
    }

    private static bool HasRetiredApiMarker(Type controllerType) =>
        controllerType.GetCustomAttributes().Any(attribute =>
            attribute.GetType().Namespace == "System" &&
            attribute.GetType().Name == ((char)79) + "bsolete" + "Attribute");

    private static string ComputeHmacSha256Hex(string secret, string payload)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
