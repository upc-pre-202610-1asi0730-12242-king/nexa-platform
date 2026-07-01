using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Queries;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Invoicing.Application.QueryServices;

public interface IPaymentQueryService
{
    Task<IEnumerable<Payment>> Handle(GetAllPaymentsQuery query, CancellationToken cancellationToken = default);

    Task<Payment?> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken = default);

    Task<IEnumerable<Payment>> Handle(GetPaymentsByInvoiceIdQuery query, CancellationToken cancellationToken = default);

    Task<IEnumerable<Payment>> Handle(GetPaymentsByStatusQuery query, CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> SearchAsync(PaymentCollectionQuery query, CancellationToken cancellationToken = default);
}
