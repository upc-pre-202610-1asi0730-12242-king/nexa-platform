namespace King.Nexa.Platform.Shared.Domain.Model.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int? TenantId { get; set; }
    public int? WorkspaceId { get; set; }
}
