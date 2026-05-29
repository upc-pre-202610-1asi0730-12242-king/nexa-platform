using King.Nexa.Platform.CatalogManagement.Domain.Model.Commands;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model;

namespace King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;

public class Product : AuditableEntity
{
    protected Product()
    {
        ProductCode = null!;
        Name = string.Empty;
        CategoryName = null!;
    }

    public Product(CreateProductCommand command)
    {
        ProductCode = command.ProductCode;
        Name = command.Name.Trim();
        CategoryName = command.CategoryName;
        ColdChainRequirement = command.ColdChainRequirement;
    }

    public ProductCode ProductCode { get; private set; }

    public string Name { get; private set; }

    public CategoryName CategoryName { get; private set; }

    public ColdChainRequirement ColdChainRequirement { get; private set; }

    public void Update(UpdateProductCommand command)
    {
        Name = command.Name.Trim();
        CategoryName = command.CategoryName;
        ColdChainRequirement = command.ColdChainRequirement;
    }
}
