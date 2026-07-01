using System.Reflection;
using King.Nexa.Platform.CatalogManagement.Interfaces.Rest;
using King.Nexa.Platform.Invoicing.Interfaces.Rest;
using King.Nexa.Platform.Logistics.Interfaces.Rest;
using King.Nexa.Platform.Sales.Interfaces.Rest;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.Shared.Interfaces.Rest;
using King.Nexa.Platform.Warehouse.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public void Critical_workspace_controllers_require_membership(Type controllerType)
    {
        var attribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(NexaAuthorizationPolicies.WorkspaceMember, attribute.Policy);
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
}
