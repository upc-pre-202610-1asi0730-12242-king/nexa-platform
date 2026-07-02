using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Queries;

public sealed record InvoiceCollectionQuery(
    PaginationRequest Pagination,
    PaymentStatus? PaymentStatus,
    int? ClientAccountId,
    int? OrderId,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo);
