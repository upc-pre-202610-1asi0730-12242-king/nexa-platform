namespace King.Nexa.Platform.CatalogManagement.Interfaces.REST.Resources;

public record ProductResource(int Id, string ProductCode, string Name, string CategoryName, string ColdChainRequirement);
