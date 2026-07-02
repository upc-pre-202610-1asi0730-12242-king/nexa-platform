using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Queries;

public sealed record PaymentCollectionQuery(
    PaginationRequest Pagination,
    PaymentStatus? Status,
    int? ClientAccountId,
    int? OrderId,
    int? InvoiceId,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo);
