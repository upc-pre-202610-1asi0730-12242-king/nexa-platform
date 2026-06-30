using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;

namespace King.Nexa.Platform.Tests;

public class DomainHardeningTests
{
    [Fact]
    public void Dispatch_start_route_fails_before_assignment_or_schedule()
    {
        var dispatch = new DispatchOrder { Status = "ready_for_operations" };

        Assert.Throws<InvalidOperationException>(() => dispatch.StartRoute());
    }

    [Fact]
    public void Dispatch_complete_fails_when_not_in_route()
    {
        var dispatch = new DispatchOrder { Status = "scheduled" };

        Assert.Throws<InvalidOperationException>(() => dispatch.Complete());
    }

    [Fact]
    public void Inventory_reservation_rejects_negative_quantity()
    {
        Assert.Throws<ArgumentException>(() => new InventoryReservation("RES-001", -1));
    }

    [Fact]
    public void Inventory_reservation_rejects_insufficient_stock()
    {
        var item = NewInventoryItem(2);

        Assert.Throws<InvalidOperationException>(() => item.Reserve(new InventoryReservation("RES-001", 3)));
    }

    [Fact]
    public void Purchase_request_convert_fails_without_commercial_validation()
    {
        var request = new PurchaseRequest { Status = "submitted", Code = "REQ-2026-0001" };

        Assert.Throws<InvalidOperationException>(() => request.MarkAcceptedIntoOrder("ORD-2026-0001"));
    }

    [Fact]
    public void Business_document_rejects_unknown_status()
    {
        var document = new BusinessDocument { Status = "pending", Required = true };

        Assert.Throws<InvalidOperationException>(() => document.ChangeStatus("sent-to-random-flow"));
    }

    [Fact]
    public void Payment_process_rejects_invalid_backward_transition()
    {
        var payment = new PaymentProcessRecord { Status = "confirmed" };

        Assert.Throws<InvalidOperationException>(() => payment.ChangeStatus("pending"));
    }

    [Fact]
    public void Payment_amount_must_be_positive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BillingAmount(0, "PEN"));
    }

    [Fact]
    public void Payment_cannot_confirm_twice()
    {
        var payment = NewPayment();
        payment.Confirm();

        Assert.Throws<InvalidOperationException>(() => payment.Confirm());
    }

    [Fact]
    public void Payment_cannot_reject_confirmed_payment()
    {
        var payment = NewPayment();
        payment.Confirm();

        Assert.Throws<InvalidOperationException>(() => payment.Reject());
    }

    [Fact]
    public void Payment_cannot_cancel_confirmed_payment()
    {
        var payment = NewPayment();
        payment.Confirm();

        Assert.Throws<InvalidOperationException>(() => payment.Cancel());
    }

    [Fact]
    public void Invoice_cannot_be_created_with_invalid_total()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Invoice(new GenerateInvoiceCommand(new InvoiceNumber("F001-000001"), 1, new BillingAmount(-1, "PEN"))));
    }

    private static InventoryItem NewInventoryItem(int availableQuantity) =>
        new(new CreateInventoryItemCommand(
            new ProductId("PROD-001"),
            new CatalogItemId("CAT-001"),
            new StockQuantity(availableQuantity),
            new WarehouseLocation("Main cold room"),
            new TemperatureRange(-20, -10)));

    private static Payment NewPayment()
    {
        var payment = new Payment(new RegisterPaymentCommand(
            InvoiceId: null,
            OrderId: 1,
            ClientAccountId: 1,
            PaymentOptionId: 1,
            PaymentMethodRecordId: null,
            new BillingAmount(120, "PEN"),
            "PAY-TEST-001"));
        payment.AssignTenant(1);
        return payment;
    }
}
