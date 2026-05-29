using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.CatalogManagement.Infrastructure.Persistence.EFC.Repositories;

public class ProductRepository(AppDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    public async Task<Product?> FindByProductCodeAsync(ProductCode productCode, CancellationToken cancellationToken = default) =>
        await Context.Products.FirstOrDefaultAsync(product => product.ProductCode == productCode, cancellationToken);
}
