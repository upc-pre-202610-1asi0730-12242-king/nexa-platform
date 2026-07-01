namespace King.Nexa.Platform.Shared.Application.Security;

public sealed class WorkspaceAccessDeniedException(string message) : Exception(message);

