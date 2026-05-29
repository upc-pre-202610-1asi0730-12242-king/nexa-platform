namespace King.Nexa.Platform.CatalogManagement.Interfaces.REST.Resources;

public record CreateProductResource(string ProductCode, string Name, string CategoryName, string ColdChainRequirement);
