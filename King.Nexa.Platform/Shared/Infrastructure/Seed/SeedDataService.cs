using System.Text.Json;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Commands;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Iam.Application.OutboundServices;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Domain.Repositories;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CatalogBrandName = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.BrandName;
using CatalogCatalogItemId = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.CatalogItemId;
using CatalogCategoryName = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.CategoryName;
using CatalogColdChainRequirement = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.ColdChainRequirement;
using CatalogItemName = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.ItemName;
using CatalogMoney = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.Money;
using CatalogProductId = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.ProductId;
using CatalogStockQuantity = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.StockQuantity;
using WarehouseCatalogItemId = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.CatalogItemId;
using WarehouseProductId = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.ProductId;
using WarehouseStockQuantity = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.StockQuantity;
using WarehouseTemperatureRange = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.TemperatureRange;
using WarehouseLocation = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.WarehouseLocation;
using WarehouseName = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.WarehouseName;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Shared.Infrastructure.Seed;

public class SeedDataService(
    IWebHostEnvironment environment,
    IOptions<SeedDataOptions> options,
    ICatalogItemRepository catalogItemRepository,
    IInventoryItemRepository inventoryItemRepository,
    IUserRepository userRepository,
    IClientAccountRepository clientAccountRepository,
    ITenantRepository tenantRepository,
    AppDbContext context,
    IPasswordHashingService passwordHashingService,
    IUnitOfWork unitOfWork) : ISeedDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled) return;

        await SeedTenantsAsync(cancellationToken);
        await SeedReferenceDataAsync(cancellationToken);
        await SeedClientAccountsAsync(cancellationToken);
        await SeedCatalogMetadataAsync(cancellationToken);
        await SeedCatalogItemsAsync(cancellationToken);
        await SeedInventoryItemsAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await SeedWorkspacesAndMembershipsAsync(cancellationToken);
        await SeedOperationalFoundationAsync(cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task SeedTenantsAsync(CancellationToken cancellationToken)
    {
        var records = await ReadSeedFileAsync<TenantSeedRecord>("tenants.seed.json", cancellationToken);
        foreach (var record in records)
        {
            var existingTenant = await tenantRepository.FindBySlugAsync(record.Slug, cancellationToken);
            if (existingTenant is not null)
            {
                existingTenant.Update(record.Name, record.LegalName, record.WorkspaceUrl, record.EmailDomain, record.Plan, record.Status, record.Country);
                tenantRepository.Update(existingTenant);
                continue;
            }

            await tenantRepository.AddAsync(new Tenant(
                record.Name,
                record.LegalName,
                record.Slug,
                record.Ruc,
                record.WorkspaceUrl,
                record.EmailDomain,
                record.Plan,
                record.Status,
                record.Country), cancellationToken);
        }
    }

    private async Task SeedReferenceDataAsync(CancellationToken cancellationToken)
    {
        await UpsertPaymentOptionsAsync([
            ("credit_line", "B2B credit line"),
            ("bank_transfer", "Bank transfer"),
            ("cash", "Cash before dispatch"),
            ("cash_on_delivery", "Cash on delivery")
        ], cancellationToken);

        await UpsertDocumentTypesAsync([
            ("factura_xml", "Factura XML"),
            ("factura_pdf", "Factura PDF"),
            ("guia_pdf", "Guia PDF"),
            ("business_document", "Business document")
        ], cancellationToken);

        await UpsertUnitsOfMeasureAsync([
            ("box", "Box"),
            ("kg", "Kilogram"),
            ("unit", "Unit"),
            ("pack", "Pack")
        ], cancellationToken);

        var country = await UpsertCountryAsync("PE", "Peru", cancellationToken);
        var lima = await UpsertDepartmentAsync(country.Id, "lima", "Lima", cancellationToken);
        var limaProvince = await UpsertProvinceAsync(lima.Id, "lima", "Lima Metropolitana", cancellationToken);
        var callaoProvince = await UpsertProvinceAsync(lima.Id, "callao", "Callao", cancellationToken);
        await NormalizeCallaoDistrictAsync(callaoProvince.Id, cancellationToken);

        foreach (var (code, label) in new[]
        {
            ("miraflores", "Miraflores"),
            ("san-isidro", "San Isidro"),
            ("san-borja", "San Borja"),
            ("surco", "Surco"),
            ("santiago-de-surco", "Santiago de Surco"),
            ("la-molina", "La Molina"),
            ("barranco", "Barranco"),
            ("surquillo", "Surquillo"),
            ("lince", "Lince"),
            ("jesus-maria", "Jesús María"),
            ("magdalena-del-mar", "Magdalena del Mar"),
            ("pueblo-libre", "Pueblo Libre"),
            ("san-miguel", "San Miguel"),
            ("cercado-de-lima", "Cercado de Lima"),
            ("la-victoria", "La Victoria"),
            ("san-luis", "San Luis"),
            ("ate", "Ate"),
            ("santa-anita", "Santa Anita"),
            ("san-juan-de-lurigancho", "San Juan de Lurigancho"),
            ("san-juan-de-miraflores", "San Juan de Miraflores"),
            ("villa-el-salvador", "Villa El Salvador"),
            ("villa-maria-del-triunfo", "Villa María del Triunfo"),
            ("los-olivos", "Los Olivos"),
            ("independencia", "Independencia"),
            ("comas", "Comas"),
            ("carabayllo", "Carabayllo"),
            ("puente-piedra", "Puente Piedra"),
            ("chorrillos", "Chorrillos")
        })
        {
            await UpsertDistrictAsync(limaProvince.Id, code, label, cancellationToken);
        }

        foreach (var (code, label) in new[]
        {
            ("callao-district", "Callao"),
            ("bellavista", "Bellavista"),
            ("carmen-de-la-legua-reynoso", "Carmen de la Legua Reynoso"),
            ("la-perla", "La Perla"),
            ("la-punta", "La Punta"),
            ("ventanilla", "Ventanilla"),
            ("mi-peru", "Mi Perú")
        })
        {
            await UpsertDistrictAsync(callaoProvince.Id, code, label, cancellationToken);
        }
    }

    private async Task UpsertPaymentOptionsAsync(IEnumerable<(string Key, string Label)> rows, CancellationToken cancellationToken)
    {
        foreach (var row in rows)
        {
            var key = row.Key.Trim().ToLowerInvariant();
            var option = await context.PaymentOptions.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
            if (option is null)
            {
                await context.PaymentOptions.AddAsync(new PaymentOption { Key = key, Label = row.Label, IsActive = true }, cancellationToken);
                continue;
            }

            option.Label = row.Label;
            option.IsActive = true;
            option.UpdatedAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertDocumentTypesAsync(IEnumerable<(string Key, string Label)> rows, CancellationToken cancellationToken)
    {
        foreach (var row in rows)
        {
            var key = row.Key.Trim().ToLowerInvariant();
            var type = await context.DocumentTypes.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
            if (type is null)
            {
                await context.DocumentTypes.AddAsync(new DocumentType { Key = key, Label = row.Label, IsActive = true }, cancellationToken);
                continue;
            }

            type.Label = row.Label;
            type.IsActive = true;
            type.UpdatedAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertUnitsOfMeasureAsync(IEnumerable<(string Key, string Label)> rows, CancellationToken cancellationToken)
    {
        foreach (var row in rows)
        {
            var key = row.Key.Trim().ToLowerInvariant();
            var unit = await context.UnitsOfMeasure.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
            if (unit is null)
            {
                await context.UnitsOfMeasure.AddAsync(new UnitOfMeasure { Key = key, Label = row.Label, IsActive = true }, cancellationToken);
                continue;
            }

            unit.Label = row.Label;
            unit.IsActive = true;
            unit.UpdatedAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Country> UpsertCountryAsync(string code, string label, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var row = await context.Countries.FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);
        if (row is null)
        {
            row = new Country { Code = normalized, Label = label, IsActive = true };
            await context.Countries.AddAsync(row, cancellationToken);
        }
        else
        {
            row.Label = label;
            row.IsActive = true;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return row;
    }

    private async Task<Department> UpsertDepartmentAsync(int countryId, string code, string label, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLowerInvariant();
        var row = await context.Departments.FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);
        if (row is null)
        {
            row = new Department { CountryId = countryId, Code = normalized, Label = label, IsActive = true };
            await context.Departments.AddAsync(row, cancellationToken);
        }
        else
        {
            row.CountryId = countryId;
            row.Label = label;
            row.IsActive = true;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return row;
    }

    private async Task<Province> UpsertProvinceAsync(int departmentId, string code, string label, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLowerInvariant();
        var row = await context.Provinces.FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);
        if (row is null)
        {
            row = new Province { DepartmentId = departmentId, Code = normalized, Label = label, IsActive = true };
            await context.Provinces.AddAsync(row, cancellationToken);
        }
        else
        {
            row.DepartmentId = departmentId;
            row.Label = label;
            row.IsActive = true;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return row;
    }

    private async Task UpsertDistrictAsync(int provinceId, string code, string label, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLowerInvariant();
        var row = await context.Districts.FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);
        if (row is null)
        {
            await context.Districts.AddAsync(new District { ProvinceId = provinceId, Code = normalized, Label = label, IsActive = true }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        row.ProvinceId = provinceId;
        row.Label = label;
        row.IsActive = true;
        row.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task NormalizeCallaoDistrictAsync(int callaoProvinceId, CancellationToken cancellationToken)
    {
        var oldCallaoDistrict = await context.Districts.FirstOrDefaultAsync(item => item.Code == "callao", cancellationToken);
        if (oldCallaoDistrict is null) return;

        var replacementExists = await context.Districts.AnyAsync(item => item.Code == "callao-district", cancellationToken);
        if (!replacementExists)
        {
            oldCallaoDistrict.Code = "callao-district";
            oldCallaoDistrict.Label = "Callao";
            oldCallaoDistrict.ProvinceId = callaoProvinceId;
            oldCallaoDistrict.IsActive = true;
        }
        else
        {
            oldCallaoDistrict.IsActive = false;
        }

        oldCallaoDistrict.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedClientAccountsAsync(CancellationToken cancellationToken)
    {
        var records = await ReadSeedFileAsync<ClientAccountSeedRecord>("client-accounts.seed.json", cancellationToken);
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "icisa", cancellationToken);
        if (tenant is null) return;

        foreach (var record in records)
        {
            var clientAccount = new ClientAccount(
                tenant.Id,
                record.Code,
                record.BusinessName,
                record.CommercialName,
                record.Ruc,
                record.Segment,
                record.Contact,
                record.ContactEmail,
                record.Phone,
                record.PaymentCondition,
                record.MonthlyCreditLimit,
                record.MonthlyCreditUsed,
                record.MonthlyCreditStatus,
                record.DeliveryPreference,
                record.PortalAccess,
                record.SellerWorkspaceEmail,
                record.Status,
                record.Address,
                record.District,
                record.Province,
                record.DeliveryReference);
            var existingClient = await context.ClientAccounts.FirstOrDefaultAsync(client =>
                client.TenantId == tenant.Id && client.Code == record.Code,
                cancellationToken);
            if (existingClient is not null)
            {
                existingClient.EnsureDeliveryProfile(
                    record.Address,
                    record.District,
                    record.Province,
                    record.DeliveryReference);
                continue;
            }

            await clientAccountRepository.AddAsync(clientAccount, cancellationToken);
        }
    }

    private async Task SeedCatalogItemsAsync(CancellationToken cancellationToken)
    {
        var records = await ReadSeedFileAsync<CatalogItemSeedRecord>("catalog-items.seed.json", cancellationToken);
        var tenant = await context.Tenants.FirstOrDefaultAsync(row => row.Slug == "icisa", cancellationToken);
        if (tenant is null) return;
        foreach (var record in records)
        {
            var catalogItemId = new CatalogCatalogItemId(record.CatalogItemId);
            var existing = await context.CatalogItems.FirstOrDefaultAsync(item =>
                    item.TenantId == tenant.Id && item.CatalogItemId == catalogItemId,
                    cancellationToken);
            if (existing is not null)
            {
                existing.Update(new UpdateCatalogItemCommand(
                    existing.Id,
                    new CatalogItemName(record.ItemName),
                    new CatalogBrandName(record.BrandName),
                    new CatalogCategoryName(record.CategoryName),
                    record.Description,
                    record.ImageUrl,
                    new CatalogMoney(record.UnitPriceAmount, record.UnitPriceCurrency),
                    new CatalogStockQuantity(record.AvailableStock),
                    Enum.Parse<CatalogColdChainRequirement>(record.ColdChainRequirement, true)));
                continue;
            }

            var command = new CreateCatalogItemCommand(
                catalogItemId,
                new CatalogProductId(record.ProductId),
                new CatalogItemName(record.ItemName),
                new CatalogBrandName(record.BrandName),
                new CatalogCategoryName(record.CategoryName),
                record.Description,
                record.ImageUrl,
                new CatalogMoney(record.UnitPriceAmount, record.UnitPriceCurrency),
                new CatalogStockQuantity(record.AvailableStock),
                Enum.Parse<CatalogColdChainRequirement>(record.ColdChainRequirement, true));

            var catalogItem = new CatalogItem(command);
            catalogItem.AssignTenant(tenant.Id);
            await catalogItemRepository.AddAsync(catalogItem, cancellationToken);
        }
    }

    private async Task SeedCatalogMetadataAsync(CancellationToken cancellationToken)
    {
        var existingCategories = (await context.Categories.AsNoTracking().ToListAsync(cancellationToken))
            .Select(category => category.Name.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingBrands = (await context.Brands.AsNoTracking().ToListAsync(cancellationToken))
            .Select(brand => brand.Name.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingWarehouseLocations = (await context.Warehouses.AsNoTracking().ToListAsync(cancellationToken))
            .Select(warehouse => warehouse.Location.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var catalogRecords = await ReadSeedFileAsync<CatalogItemSeedRecord>("catalog-items.seed.json", cancellationToken);
        foreach (var categoryName in catalogRecords.Select(record => record.CategoryName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!existingCategories.Add(categoryName)) continue;
            await context.Categories.AddAsync(new Category(new CreateCategoryCommand(
                new CatalogCategoryName(categoryName),
                $"{categoryName} products imported from the operating catalog.")), cancellationToken);
        }

        foreach (var brandName in catalogRecords.Select(record => record.BrandName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!existingBrands.Add(brandName)) continue;
            await context.Brands.AddAsync(new Brand(new CreateBrandCommand(
                new CatalogBrandName(brandName),
                $"{brandName} brand imported from the operating catalog.")), cancellationToken);
        }
    }

    private async Task SeedInventoryItemsAsync(CancellationToken cancellationToken)
    {
        var records = await ReadSeedFileAsync<InventoryItemSeedRecord>("inventory-items.seed.json", cancellationToken);
        var tenant = await context.Tenants.FirstOrDefaultAsync(row => row.Slug == "icisa", cancellationToken);
        if (tenant is null) return;
        foreach (var record in records)
        {
            var catalogItemId = new WarehouseCatalogItemId(record.CatalogItemId);
            var existing = await context.InventoryItems.FirstOrDefaultAsync(item =>
                item.TenantId == tenant.Id && item.CatalogItemId == catalogItemId,
                cancellationToken);
            if (existing is not null)
            {
                SynchronizeSeedInventory(existing, record);
                continue;
            }

            var command = new CreateInventoryItemCommand(
                new WarehouseProductId(record.ProductId),
                catalogItemId,
                new WarehouseStockQuantity(record.AvailableQuantity + record.ReservedQuantity),
                new WarehouseLocation(record.WarehouseLocation),
                new WarehouseTemperatureRange(record.MinimumTemperature, record.MaximumTemperature));

            var inventoryItem = new InventoryItem(command);
            inventoryItem.AssignTenant(tenant.Id);
            if (record.ReservedQuantity > 0)
            {
                inventoryItem.Reserve(new King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.InventoryReservation(
                    $"SEED-{record.ProductId}", record.ReservedQuantity));
            }
            await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
        }
    }

    private static void SynchronizeSeedInventory(InventoryItem item, InventoryItemSeedRecord record)
    {
        var currentReserved = item.ReservedQuantity.Value;
        if (currentReserved > record.ReservedQuantity)
        {
            item.Release(new King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.InventoryReservation(
                $"SEED-RELEASE-{record.ProductId}", currentReserved - record.ReservedQuantity));
        }

        var reserveDelta = Math.Max(0, record.ReservedQuantity - item.ReservedQuantity.Value);
        item.Update(new UpdateInventoryItemCommand(
            item.Id,
            new WarehouseProductId(record.ProductId),
            new WarehouseCatalogItemId(record.CatalogItemId),
            new WarehouseStockQuantity(record.AvailableQuantity + reserveDelta),
            new WarehouseLocation(record.WarehouseLocation),
            new WarehouseTemperatureRange(record.MinimumTemperature, record.MaximumTemperature)));

        if (reserveDelta > 0)
        {
            item.Reserve(new King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.InventoryReservation(
                $"SEED-RESERVE-{record.ProductId}", reserveDelta));
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var records = await ReadSeedFileAsync<UserSeedRecord>("users.seed.json", cancellationToken);
        
        var allUsers = await userRepository.ListAsync(cancellationToken);
        foreach (var user in allUsers)
        {
            if (!records.Any(r => r.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                userRepository.Remove(user);
            }
        }

        foreach (var record in records)
        {
            var existingUser = await userRepository.FindByUsernameAsync(record.Username, cancellationToken);
            if (existingUser is not null)
            {
                if (existingUser.Role != record.Role)
                {
                    userRepository.Remove(existingUser);
                    var passwordHash = passwordHashingService.HashPassword(record.Password);
                    var replacement = new User(record.Username, record.Email, passwordHash, record.Role);
                    replacement.UpdateProfile(record.FullName, record.Email, string.Empty, "en", true);
                    await userRepository.AddAsync(replacement, cancellationToken);
                }
                else if (string.IsNullOrWhiteSpace(existingUser.FullName) ||
                         existingUser.FullName.Equals(existingUser.Username, StringComparison.OrdinalIgnoreCase) ||
                         existingUser.FullName.Equals(existingUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var preferredLanguage = existingUser.PreferredLanguage is "en" or "es"
                        ? existingUser.PreferredLanguage
                        : "en";
                    existingUser.UpdateProfile(record.FullName, existingUser.Email, existingUser.Phone, preferredLanguage, existingUser.CriticalNotificationsEnabled);
                    userRepository.Update(existingUser);
                }
            }
            else
            {
                var passwordHash = passwordHashingService.HashPassword(record.Password);
                var user = new User(record.Username, record.Email, passwordHash, record.Role);
                user.UpdateProfile(record.FullName, record.Email, string.Empty, "en", true);
                await userRepository.AddAsync(user, cancellationToken);
            }
        }
    }

    private async Task SeedWorkspacesAndMembershipsAsync(CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(row => row.Slug == "icisa", cancellationToken);
        if (tenant is null) return;

        var workspace = await context.Workspaces.FirstOrDefaultAsync(row => row.Slug == tenant.Slug, cancellationToken);
        if (workspace is null)
        {
            workspace = new Workspace
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                Url = tenant.WorkspaceUrl,
                EmailDomain = tenant.EmailDomain,
                Status = tenant.Status,
                IsPrimary = true
            };
            await context.Workspaces.AddAsync(workspace, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            workspace.TenantId = tenant.Id;
            workspace.Name = tenant.Name;
            workspace.Url = tenant.WorkspaceUrl;
            workspace.EmailDomain = tenant.EmailDomain;
            workspace.Status = tenant.Status;
            workspace.IsPrimary = true;
            workspace.UpdatedAt = DateTime.UtcNow;
        }

        var users = await context.Users.ToListAsync(cancellationToken);
        var buyerClientId = await context.ClientAccounts
            .Where(client => client.TenantId == tenant.Id && client.Code == "CLI-001")
            .Select(client => (int?)client.Id)
            .FirstOrDefaultAsync(cancellationToken);
        foreach (var user in users.Where(user => user.Email.EndsWith($"@{tenant.EmailDomain}", StringComparison.OrdinalIgnoreCase)))
        {
            var existing = await context.UserWorkspaceMemberships.FirstOrDefaultAsync(row =>
                row.WorkspaceId == workspace.Id && row.UserId == user.Id, cancellationToken);

            if (existing is null)
            {
                await context.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
                {
                    TenantId = tenant.Id,
                    WorkspaceId = workspace.Id,
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Department = DepartmentForRole(user.Role),
                    Status = "active",
                    PortalAccess = user.Role is "Buyer" or "B2B Buyer",
                    ClientAccountId = user.Role is "Buyer" or "B2B Buyer" ? buyerClientId : null
                }, cancellationToken);
                continue;
            }

            existing.TenantId = tenant.Id;
            existing.Email = user.Email;
            if (string.IsNullOrWhiteSpace(existing.FullName)) existing.FullName = user.FullName;
            existing.Role = user.Role;
            if (string.IsNullOrWhiteSpace(existing.Department)) existing.Department = DepartmentForRole(user.Role);
            existing.Status = "active";
            existing.PortalAccess = user.Role is "Buyer" or "B2B Buyer";
            existing.ClientAccountId = user.Role is "Buyer" or "B2B Buyer" ? buyerClientId : null;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task SeedOperationalFoundationAsync(CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "icisa", cancellationToken);
        if (tenant is null) return;

        var clients = await context.ClientAccounts.OrderBy(client => client.Id).ToListAsync(cancellationToken);
        foreach (var client in clients.Where(client => client.TenantId == tenant.Id))
        {
            client.NormalizeSellerWorkspaceEmail(tenant.EmailDomain);
        }

        var firstClient = clients.FirstOrDefault(client => client.Code == "CLI-001") ?? clients.FirstOrDefault();
        foreach (var member in await context.TenantMembers.Where(member => member.TenantId == tenant.Id).ToListAsync(cancellationToken))
        {
            var normalizedEmail = NormalizeTenantEmail(member.Email, tenant.EmailDomain);
            if (member.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)) continue;
            member.Email = normalizedEmail;
            member.UpdatedAt = DateTime.UtcNow;
        }

        if (!await context.TenantMembers.AnyAsync(member => member.TenantId == tenant.Id, cancellationToken))
        {
            await context.TenantMembers.AddRangeAsync([
                new TenantMember { TenantId = tenant.Id, FullName = "Carlos Ríos", Email = "carlos.rios@icisa.pe", Role = "Company Owner", Department = "Executive Office", Status = "active" },
                new TenantMember { TenantId = tenant.Id, FullName = "Valeria Sánchez", Email = "valeria.sanchez@icisa.pe", Role = "Sales", Department = "Sales", Status = "active" },
                new TenantMember { TenantId = tenant.Id, FullName = "Roberto García", Email = "roberto.garcia@icisa.pe", Role = "Logistics Manager", Department = "Operations", Status = "active" },
                new TenantMember { TenantId = tenant.Id, FullName = "Elena Litano", Email = "elena.litano@icisa.pe", Role = "B2B Buyer", Department = "Buyer Portal", Status = "invited", PortalAccess = true }
            ], cancellationToken);
        }

        if (!await context.TenantRules.AnyAsync(rule => rule.TenantId == tenant.Id, cancellationToken))
        {
            await context.TenantRules.AddRangeAsync([
                new TenantRule { TenantId = tenant.Id, Code = "fefo_required", Name = "FEFO required", Description = "Dispatches must prioritize lots closest to expiration.", Category = "Warehouse", Enabled = true },
                new TenantRule { TenantId = tenant.Id, Code = "temperature_log_required", Name = "Temperature log required", Description = "Temperature evidence is required before dispatch.", Category = "Logistics", Enabled = true },
                new TenantRule { TenantId = tenant.Id, Code = "commercial_approval_before_dispatch", Name = "Commercial approval before dispatch", Description = "Orders with credit or document risk require commercial approval.", Category = "Commercial", Enabled = true }
            ], cancellationToken);
        }

        if (!await context.TenantCustomFields.AnyAsync(field => field.TenantId == tenant.Id, cancellationToken))
        {
            await context.TenantCustomFields.AddRangeAsync([
                new TenantCustomField { TenantId = tenant.Id, Code = "lot_number", Label = "Lot number", TargetResource = "Product", FieldType = "text", Required = true, Enabled = true },
                new TenantCustomField { TenantId = tenant.Id, Code = "expiration_date", Label = "Expiration date", TargetResource = "Product", FieldType = "date", Required = true, Enabled = true },
                new TenantCustomField { TenantId = tenant.Id, Code = "temperature_range", Label = "Temperature range", TargetResource = "Warehouse", FieldType = "select", Required = true, Enabled = true }
            ], cancellationToken);
        }

        if (!await context.TenantSubscriptions.AnyAsync(subscription => subscription.TenantId == tenant.Id, cancellationToken))
        {
            await context.TenantSubscriptions.AddAsync(new TenantSubscription
            {
                TenantId = tenant.Id,
                Plan = "Standard",
                Seats = 8,
                Warehouses = 2,
                PaymentStatus = "review_active",
                NextBillingDate = new DateOnly(2026, 7, 1),
                BillingContact = "administracion@icisa.pe"
            }, cancellationToken);
        }

        foreach (var (code, name, segment, plan) in new[]
        {
            ("catalog-management", "Catalog management", "Commercial", "Starter"),
            ("sales-requests", "Sales requests", "Commercial", "Starter"),
            ("warehouse-management", "Warehouse management", "Warehouse", "Standard"),
            ("inventory-lots", "Inventory lots", "Logistics", "Standard"),
            ("logistics-dispatch", "Logistics dispatch", "Logistics", "Standard"),
            ("temperature-logs", "Temperature logs", "Logistics", "Standard"),
            ("invoicing-payments", "Invoicing and payments", "Invoicing", "Standard"),
            ("buyer-portal", "Buyer portal", "B2B", "Standard"),
            ("workspace-operations-setup", "Workspace operations setup", "Operations", "Standard"),
            ("business-documents", "Business documents", "Invoicing", "Standard"),
            ("promotions", "Promotions", "Company", "Professional")
        })
        {
            var feature = await context.WorkspaceFeatures.FirstOrDefaultAsync(
                row => row.TenantId == tenant.Id && row.Code == code,
                cancellationToken);
            if (feature is null)
            {
                await context.WorkspaceFeatures.AddAsync(new WorkspaceFeature
                {
                    TenantId = tenant.Id,
                    Code = code,
                    Name = name,
                    Segment = segment,
                    Enabled = true,
                    PlanRequired = plan
                }, cancellationToken);
                continue;
            }

            feature.Name = name;
            feature.Segment = segment;
            feature.Enabled = true;
            feature.PlanRequired = plan;
            feature.UpdatedAt = DateTime.UtcNow;
        }

        if (!await context.Promotions.AnyAsync(promotion => promotion.TenantId == tenant.Id, cancellationToken))
        {
            await context.Promotions.AddAsync(new Promotion
            {
                TenantId = tenant.Id,
                Code = "PROM-FATHER-2026",
                Name = "Father's Day gourmet cold pack",
                Campaign = "Father's Day",
                DiscountLabel = "8% on selected frozen products",
                StartsOn = new DateOnly(2026, 6, 10),
                EndsOn = new DateOnly(2026, 6, 30),
                Status = "active"
            }, cancellationToken);
        }

        PaymentMethodRecord? defaultPaymentMethod = null;
        if (firstClient is not null)
        {
            defaultPaymentMethod = await context.PaymentMethodRecords
                .FirstOrDefaultAsync(method => method.TenantId == tenant.Id && method.ClientAccountId == firstClient.Id, cancellationToken);

            if (defaultPaymentMethod is null)
            {
                defaultPaymentMethod = new PaymentMethodRecord { TenantId = tenant.Id, ClientAccountId = firstClient.Id, Type = "credit_line", Label = "ICISA credit line", Status = "active", IsDefault = true };
                await context.PaymentMethodRecords.AddAsync(defaultPaymentMethod, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        await SeedMainOperationalFlowsAsync(tenant, firstClient, defaultPaymentMethod, cancellationToken);
    }

    private async Task SeedMainOperationalFlowsAsync(
        Tenant tenant,
        ClientAccount? firstClient,
        PaymentMethodRecord? defaultPaymentMethod,
        CancellationToken cancellationToken)
    {
        if (firstClient is null) return;
        await NormalizePresentationSeedRecordsAsync(tenant.Id, cancellationToken);

        var catalogItems = await context.CatalogItems
            .Where(item => item.TenantId == tenant.Id)
            .OrderBy(item => item.Id)
            .Take(2)
            .ToListAsync(cancellationToken);
        if (catalogItems.Count == 0) return;

        var primaryCatalogItem = catalogItems[0];
        var secondaryCatalogItem = catalogItems.Count > 1 ? catalogItems[1] : catalogItems[0];
        var primaryInventoryItem = await context.InventoryItems
            .Where(item => item.TenantId == tenant.Id)
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (primaryInventoryItem is null) return;

        var warehouse = await context.Warehouses
            .Where(row => row.TenantId == tenant.Id)
            .OrderBy(row => row.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (warehouse is null)
        {
            warehouse = new WarehouseAggregate(new CreateWarehouseCommand(
                new WarehouseName("ICISA Lima Cold Hub"),
                new WarehouseLocation("Av. Guillermo Dansey 2211, Cercado de Lima, Lima, Perú"),
                new WarehouseTemperatureRange(-18m, 4m)));
            warehouse.AssignTenant(tenant.Id);
            await context.Warehouses.AddAsync(warehouse, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (warehouse.Name.Value != "ICISA Lima Cold Hub" ||
                 warehouse.Location.Value != "Av. Guillermo Dansey 2211, Cercado de Lima, Lima, Perú")
        {
            warehouse.Update(new UpdateWarehouseCommand(
                warehouse.Id,
                new WarehouseName("ICISA Lima Cold Hub"),
                new WarehouseLocation("Av. Guillermo Dansey 2211, Cercado de Lima, Lima, Perú"),
                new WarehouseTemperatureRange(-18m, 4m)));
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(row => row.TenantId == tenant.Id && row.OrderNumber == new OrderNumber("ORD-2026-0001"), cancellationToken);
        if (order is null)
        {
            order = new Order(new CreateOrderCommand(
                new OrderNumber("ORD-2026-0001"),
                new CustomerId(firstClient.Code),
                [
                    new CreateOrderItemCommand(
                        new ProductId(primaryCatalogItem.ProductId.Value),
                        new Sales.Domain.Model.ValueObjects.CatalogItemId(primaryCatalogItem.CatalogItemId.Value),
                        new ItemName(primaryCatalogItem.ItemName.Value),
                        new Quantity(3),
                        new Money(primaryCatalogItem.UnitPrice.Amount, primaryCatalogItem.UnitPrice.Currency))
                ],
                "high",
                "Priority B2B cold-chain order for ICISA operations.",
                new DeliveryDetails(
                    "warehouse",
                    firstClient.Address,
                    firstClient.District,
                    "Lima",
                    firstClient.Province,
                    firstClient.DeliveryReference,
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                    "Maintain cold chain evidence during dispatch."),
                firstClient.Id));
            order.AssignTenant(tenant.Id);
            await context.Orders.AddAsync(order, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        var purchaseRequest = await context.PurchaseRequests
            .FirstOrDefaultAsync(row => row.TenantId == tenant.Id && row.Code == "PR-2026-0001", cancellationToken);
        if (purchaseRequest is null)
        {
            purchaseRequest = new PurchaseRequest
            {
                TenantId = tenant.Id,
                ClientAccountId = firstClient.Id,
                Code = "PR-2026-0001",
                Origin = "buyer_portal",
                Status = "submitted",
                Priority = "high",
                RequestedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                DeliveryAddress = firstClient.Address,
                DeliveryDistrict = firstClient.District,
                DeliveryCity = "Lima",
                DeliveryProvince = firstClient.Province,
                DeliveryReference = firstClient.DeliveryReference,
                PaymentOption = "credit_line",
                ShippingEstimate = 85m,
                Comments = "Buyer request for Sales review and cold-chain dispatch coordination.",
                CommercialOwner = "valeria.sanchez@icisa.pe"
            };
            purchaseRequest.ValidateStructuredFields();
            await context.PurchaseRequests.AddAsync(purchaseRequest, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.PurchaseRequestLines.AnyAsync(row => row.TenantId == tenant.Id && row.PurchaseRequestId == purchaseRequest.Id, cancellationToken))
        {
            await context.PurchaseRequestLines.AddRangeAsync([
                new PurchaseRequestLine { TenantId = tenant.Id, PurchaseRequestId = purchaseRequest.Id, CatalogItemId = primaryCatalogItem.Id, Quantity = 12, Unit = "box", EstimatedWeightKg = 48, Notes = "Priority frozen SKU for buyer portal request." },
                new PurchaseRequestLine { TenantId = tenant.Id, PurchaseRequestId = purchaseRequest.Id, CatalogItemId = secondaryCatalogItem.Id, Quantity = 6, Unit = "box", EstimatedWeightKg = 24, Notes = "Secondary cold-chain SKU for mixed dispatch." }
            ], cancellationToken);
        }

        if (!await context.ConversationMessages.AnyAsync(row => row.TenantId == tenant.Id && row.PurchaseRequestId == purchaseRequest.Id, cancellationToken))
        {
            await context.ConversationMessages.AddAsync(new ConversationMessage { TenantId = tenant.Id, ClientAccountId = firstClient.Id, PurchaseRequestId = purchaseRequest.Id, SenderRole = "sales", SenderName = "Valeria Sanchez", Body = "Solicitud recibida. Validaremos disponibilidad y ventana de despacho.", VisibleToBuyer = true }, cancellationToken);
        }

        if (!await context.CreditRequests.AnyAsync(row => row.TenantId == tenant.Id && row.Code == "CRQ-2026-0001", cancellationToken))
        {
            await context.CreditRequests.AddAsync(new CreditRequest { TenantId = tenant.Id, ClientAccountId = firstClient.Id, Code = "CRQ-2026-0001", RequestedAmount = 15000m, Reason = "Temporary credit extension for recurring cold-chain purchase cycle.", Status = "submitted" }, cancellationToken);
        }

        var dispatchOrder = await context.DispatchOrders.FirstOrDefaultAsync(row => row.TenantId == tenant.Id && row.Code == "DSP-2026-0001", cancellationToken);
        if (dispatchOrder is null)
        {
            dispatchOrder = new DispatchOrder { TenantId = tenant.Id, OrderId = order.Id, ClientAccountId = firstClient.Id, Code = "DSP-2026-0001", Status = "scheduled", RouteName = "Lima Este Cold Route", Responsible = "roberto.garcia@icisa.pe", Eta = DateTime.UtcNow.AddDays(2), DeliveryWindow = "08:00-12:00" };
            await context.DispatchOrders.AddAsync(dispatchOrder, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.DispatchEvents.AnyAsync(row => row.TenantId == tenant.Id && row.DispatchOrderId == dispatchOrder.Id, cancellationToken))
        {
            await context.DispatchEvents.AddRangeAsync([
                new DispatchEvent { TenantId = tenant.Id, DispatchOrderId = dispatchOrder.Id, Status = "scheduled", Description = "Dispatch scheduled from ICISA Lima Cold Hub.", VisibleToBuyer = true },
                new DispatchEvent { TenantId = tenant.Id, DispatchOrderId = dispatchOrder.Id, Status = "temperature_checked", Description = "Pre-dispatch temperature evidence recorded.", VisibleToBuyer = true }
            ], cancellationToken);
        }

        if (!await context.ProofOfDeliveryRecords.AnyAsync(row => row.TenantId == tenant.Id && row.DispatchOrderId == dispatchOrder.Id, cancellationToken))
        {
            await context.ProofOfDeliveryRecords.AddAsync(new ProofOfDeliveryRecord { TenantId = tenant.Id, DispatchOrderId = dispatchOrder.Id, ReceivedBy = firstClient.Contact, Status = "pending", Notes = "Pending delivery confirmation." }, cancellationToken);
        }

        if (!await context.TemperatureLogs.AnyAsync(row => row.TenantId == tenant.Id && row.DispatchOrderId == dispatchOrder.Id, cancellationToken))
        {
            await context.TemperatureLogs.AddAsync(new TemperatureLog { TenantId = tenant.Id, DispatchOrderId = dispatchOrder.Id, OrderId = order.Id, Celsius = -18.4m, Zone = "Frozen cargo bay", Status = "ok", RecordedAt = DateTime.UtcNow }, cancellationToken);
        }

        var lotSeeds = new[]
        {
            ("LOT-2026-0001", "PROD-0001", 108, 12, -12, 6, "Cheese A1", 4m, 8m),
            ("LOT-2026-0002", "PROD-0004", 8, 2, -8, 4, "Charcuterie C1", 0m, 5m),
            ("LOT-2026-0003", "PROD-0014", 5, 0, -6, 9, "Cheese B2", 2m, 6m),
            ("LOT-2026-0004", "PROD-0033", 52, 7, -10, 22, "Dairy D1", 2m, 6m),
            ("LOT-2026-0005", "PROD-0049", 42, 4, -5, 16, "Dessert D2", 0m, 4m),
            ("LOT-2026-0006", "PROD-0002", 79, 18, -14, 58, "Cheese A2", 4m, 8m),
            ("LOT-2026-0007", "PROD-0006", 35, 7, -12, 31, "Charcuterie C2", 0m, 5m),
            ("LOT-2026-0008", "PROD-0036", 64, 4, -7, 76, "Cheese B1", 2m, 6m)
        };
        InventoryLot? lot = null;
        foreach (var (code, productId, quantity, reserved, entryOffset, expiryOffset, zone, minimum, maximum) in lotSeeds)
        {
            var inventoryItem = await context.InventoryItems.FirstOrDefaultAsync(row =>
                row.TenantId == tenant.Id && row.ProductId == new WarehouseProductId(productId), cancellationToken);
            if (inventoryItem is null) continue;

            var seededLot = await context.InventoryLots.FirstOrDefaultAsync(row =>
                row.TenantId == tenant.Id && row.LotCode == code, cancellationToken);
            if (seededLot is null)
            {
                seededLot = new InventoryLot { TenantId = tenant.Id, LotCode = code };
                await context.InventoryLots.AddAsync(seededLot, cancellationToken);
            }

            seededLot.InventoryItemId = inventoryItem.Id;
            seededLot.WarehouseId = warehouse.Id;
            seededLot.Quantity = quantity;
            seededLot.ReservedQuantity = reserved;
            seededLot.EntryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(entryOffset));
            seededLot.ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(expiryOffset));
            seededLot.Zone = zone;
            seededLot.Status = "active";
            seededLot.MinimumTemperature = minimum;
            seededLot.MaximumTemperature = maximum;
            seededLot.UpdatedAt = DateTime.UtcNow;
            lot ??= seededLot;
        }
        await context.SaveChangesAsync(cancellationToken);

        if (lot is not null && !await context.InventoryMovements.AnyAsync(row => row.TenantId == tenant.Id && row.Code == "MOV-2026-0001", cancellationToken))
        {
            await context.InventoryMovements.AddAsync(new InventoryMovement { TenantId = tenant.Id, InventoryItemId = primaryInventoryItem.Id, InventoryLotId = lot.Id, WarehouseId = warehouse.Id, OrderId = order.Id, Code = "MOV-2026-0001", MovementType = "reservation", Quantity = -12, Reason = "Reservation for B2B cold-chain order.", TemperatureReading = -18.4m, PerformedBy = "roberto.garcia@icisa.pe", OccurredAt = DateTime.UtcNow }, cancellationToken);
        }

        if (lot is not null && !await context.InventoryReservations.AnyAsync(row => row.TenantId == tenant.Id && row.Code == "RSV-2026-0001", cancellationToken))
        {
            await context.InventoryReservations.AddAsync(new InventoryReservationRecord { TenantId = tenant.Id, InventoryItemId = primaryInventoryItem.Id, InventoryLotId = lot.Id, OrderId = order.Id, PurchaseRequestId = purchaseRequest.Id, Code = "RSV-2026-0001", Units = 12, Status = "reserved" }, cancellationToken);
        }

        var invoice = await context.Invoices.FirstOrDefaultAsync(row => row.TenantId == tenant.Id && row.InvoiceNumber == new InvoiceNumber("F001-000001"), cancellationToken);
        if (invoice is null)
        {
            invoice = new Invoice(new GenerateInvoiceCommand(new InvoiceNumber("F001-000001"), order.Id, new BillingAmount(Math.Max(order.Total.Amount, 1m), order.Total.Currency)));
            invoice.AssignTenant(tenant.Id);
            await context.Invoices.AddAsync(invoice, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        var paymentOption = await context.PaymentOptions.FirstOrDefaultAsync(row => row.Key == "credit_line", cancellationToken);
        var payment = await context.Payments.FirstOrDefaultAsync(row => row.TenantId == tenant.Id && row.ReferenceCode == "PAY-2026-0001", cancellationToken);
        if (payment is null)
        {
            payment = new Payment(new RegisterPaymentCommand(invoice.Id, order.Id, firstClient.Id, paymentOption?.Id, defaultPaymentMethod?.Id, new BillingAmount(Math.Max(order.Total.Amount, 1m), order.Total.Currency), "PAY-2026-0001"));
            payment.AssignTenant(tenant.Id);
            await context.Payments.AddAsync(payment, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.PaymentProcessRecords.AnyAsync(row => row.TenantId == tenant.Id && row.OrderId == order.Id, cancellationToken))
        {
            await context.PaymentProcessRecords.AddAsync(new PaymentProcessRecord { TenantId = tenant.Id, OrderId = order.Id, ClientAccountId = firstClient.Id, PaymentId = payment.Id, PaymentMethodRecordId = defaultPaymentMethod?.Id, Subtotal = Math.Max(order.Total.Amount, 1m), Discount = 0m, Shipping = 85m, Igv = Math.Round(Math.Max(order.Total.Amount, 1m) * 0.18m, 2), Total = Math.Round(Math.Max(order.Total.Amount, 1m) * 1.18m + 85m, 2), Status = "pending" }, cancellationToken);
        }

        if (!await context.BusinessDocuments.AnyAsync(row => row.TenantId == tenant.Id && row.OrderId == order.Id && row.Type == "factura_pdf", cancellationToken))
        {
            var facturaTypeId = await context.DocumentTypes.Where(row => row.Key == "factura_pdf").Select(row => (int?)row.Id).FirstOrDefaultAsync(cancellationToken);
            await context.BusinessDocuments.AddAsync(new BusinessDocument { TenantId = tenant.Id, OrderId = order.Id, ClientAccountId = firstClient.Id, DocumentTypeId = facturaTypeId, Type = "factura_pdf", Label = "Factura F001-000001", Status = "ready", FileName = "factura-f001-000001.pdf", VisibleToBuyer = true, Required = true }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task NormalizePresentationSeedRecordsAsync(int tenantId, CancellationToken cancellationToken)
    {
        var legacyScenarioToken = string.Concat("de", "mo");
        var legacyAutomationToken = string.Concat("co", "dex");
        var legacyBootstrapToken = string.Concat("seed", "ed");
        var legacyTextPattern = string.Join('|', legacyScenarioToken, legacyAutomationToken, legacyBootstrapToken);
        var scenarioPattern = $"%{legacyScenarioToken}%";
        var automationPattern = $"%{legacyAutomationToken}%";
        var bootstrapPattern = $"%{legacyBootstrapToken}%";
        var scenarioUpper = legacyScenarioToken.ToUpperInvariant();
        var automationUpper = legacyAutomationToken.ToUpperInvariant();
        var requestLegacyPrefix = $"PR-{scenarioUpper}";
        var requestLegacyPrefixPattern = $"{requestLegacyPrefix}-%";
        var orderLegacyPrefixPattern = $"ORD-{scenarioUpper}-%";
        var creditLegacyPrefix = $"CRQ-{scenarioUpper}";
        var creditLegacyPrefixPattern = $"{creditLegacyPrefix}-%";
        var dispatchLegacyPrefix = $"DSP-{scenarioUpper}";
        var dispatchLegacyPrefixPattern = $"{dispatchLegacyPrefix}-%";
        var lotLegacyPrefix = $"LOT-{scenarioUpper}";
        var lotLegacyPrefixPattern = $"{lotLegacyPrefix}-%";
        var movementLegacyPrefix = $"MOV-{scenarioUpper}";
        var movementLegacyPrefixPattern = $"{movementLegacyPrefix}-%";
        var reservationLegacyPrefix = $"RSV-{scenarioUpper}";
        var reservationLegacyPrefixPattern = $"{reservationLegacyPrefix}-%";
        var paymentLegacyPrefix = $"PAY-{scenarioUpper}";
        var paymentLegacyPrefixPattern = $"{paymentLegacyPrefix}-%";
        var automationPrefix = $"{automationUpper}-";

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            WITH primary_warehouse AS (
                SELECT id
                FROM warehouses
                WHERE tenant_id = {tenantId}
                ORDER BY id
                LIMIT 1
            )
            UPDATE warehouses warehouse
            SET location = warehouse.location || ' - reference ' || warehouse.id,
                updated_at = now()
            FROM primary_warehouse primary_row
            WHERE warehouse.tenant_id = {tenantId}
              AND warehouse.id <> primary_row.id
              AND warehouse.location = 'Av. Guillermo Dansey 2211, Cercado de Lima, Lima, Perú';

            WITH primary_warehouse AS (
                SELECT id
                FROM warehouses
                WHERE tenant_id = {tenantId}
                ORDER BY id
                LIMIT 1
            )
            UPDATE warehouses warehouse
            SET name = 'ICISA Lima Cold Hub',
                location = 'Av. Guillermo Dansey 2211, Cercado de Lima, Lima, Perú',
                updated_at = now()
            FROM primary_warehouse primary_row
            WHERE warehouse.id = primary_row.id;

            UPDATE orders
            SET order_number = CASE
                    WHEN order_number LIKE {orderLegacyPrefixPattern} THEN 'ORD-2026-' || lpad(id::text, 4, '0')
                    WHEN order_number ILIKE {automationPattern} THEN replace(order_number, {automationPrefix}, '')
                    ELSE order_number
                END,
                notes = regexp_replace(replace(replace(notes, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (order_number ILIKE {scenarioPattern} OR order_number ILIKE {automationPattern} OR notes ILIKE {bootstrapPattern} OR notes ILIKE {scenarioPattern} OR notes ILIKE {automationPattern});

            UPDATE purchase_requests
            SET code = CASE
                    WHEN code LIKE {requestLegacyPrefixPattern} THEN 'PR-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(code, {requestLegacyPrefix}, 'PR-2026')
                END,
                comments = regexp_replace(replace(comments, {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (code ILIKE {scenarioPattern} OR comments ILIKE {bootstrapPattern} OR comments ILIKE {scenarioPattern});

            UPDATE credit_requests
            SET code = CASE
                    WHEN code LIKE {creditLegacyPrefixPattern} THEN 'CRQ-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(code, {creditLegacyPrefix}, 'CRQ-2026')
                END
            WHERE tenant_id = {tenantId} AND code ILIKE {scenarioPattern};

            UPDATE dispatch_orders
            SET code = CASE
                    WHEN code LIKE {dispatchLegacyPrefixPattern} THEN 'DSP-2026-' || lpad(id::text, 4, '0')
                    WHEN code ILIKE {automationPattern} THEN replace(code, {automationPrefix}, '')
                    ELSE replace(code, {dispatchLegacyPrefix}, 'DSP-2026')
                END
            WHERE tenant_id = {tenantId} AND (code ILIKE {scenarioPattern} OR code ILIKE {automationPattern});

            UPDATE proof_of_delivery_records
            SET notes = regexp_replace(notes, {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId} AND notes ILIKE {scenarioPattern};

            UPDATE dispatch_events
            SET description = regexp_replace(replace(replace(description, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (description ILIKE {scenarioPattern} OR description ILIKE {automationPattern} OR description ILIKE {bootstrapPattern});

            UPDATE conversation_messages
            SET body = regexp_replace(replace(replace(body, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (body ILIKE {scenarioPattern} OR body ILIKE {automationPattern} OR body ILIKE {bootstrapPattern});

            UPDATE notification_records
            SET title = regexp_replace(replace(replace(title, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi'),
                body = regexp_replace(replace(replace(body, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (title ILIKE {scenarioPattern} OR body ILIKE {scenarioPattern} OR title ILIKE {automationPattern} OR body ILIKE {automationPattern} OR title ILIKE {bootstrapPattern} OR body ILIKE {bootstrapPattern});

            UPDATE audit_logs
            SET action = regexp_replace(replace(replace(action, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi'),
                resource_id = regexp_replace(replace(replace(resource_id, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi'),
                metadata_json = regexp_replace(replace(replace(metadata_json::text, {automationPrefix}, ''), {requestLegacyPrefix}, 'PR-2026'), {legacyTextPattern}, 'operational', 'gi')::jsonb
            WHERE tenant_id = {tenantId}
              AND (action ILIKE {scenarioPattern} OR action ILIKE {automationPattern} OR action ILIKE {bootstrapPattern} OR resource_id ILIKE {scenarioPattern} OR resource_id ILIKE {automationPattern} OR metadata_json::text ILIKE {scenarioPattern} OR metadata_json::text ILIKE {automationPattern} OR metadata_json::text ILIKE {bootstrapPattern});

            UPDATE inventory_lots
            SET lot_code = CASE
                    WHEN lot_code LIKE {lotLegacyPrefixPattern} THEN 'LOT-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(lot_code, {lotLegacyPrefix}, 'LOT-2026')
                END
            WHERE tenant_id = {tenantId} AND lot_code ILIKE {scenarioPattern};

            UPDATE inventory_movements
            SET code = CASE
                    WHEN code LIKE {movementLegacyPrefixPattern} THEN 'MOV-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(code, {movementLegacyPrefix}, 'MOV-2026')
                END,
                reason = regexp_replace(reason, {legacyTextPattern}, 'operational', 'gi')
            WHERE tenant_id = {tenantId}
              AND (code ILIKE {scenarioPattern} OR reason ILIKE {bootstrapPattern});

            UPDATE inventory_reservation_records
            SET code = CASE
                    WHEN code LIKE {reservationLegacyPrefixPattern} THEN 'RSV-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(code, {reservationLegacyPrefix}, 'RSV-2026')
                END
            WHERE tenant_id = {tenantId} AND code ILIKE {scenarioPattern};

            UPDATE payments
            SET reference_code = CASE
                    WHEN reference_code LIKE {paymentLegacyPrefixPattern} THEN 'PAY-2026-' || lpad(id::text, 4, '0')
                    ELSE replace(reference_code, {paymentLegacyPrefix}, 'PAY-2026')
                END
            WHERE tenant_id = {tenantId} AND reference_code ILIKE {scenarioPattern};

            UPDATE business_documents
            SET label = regexp_replace(label, {legacyTextPattern}, '', 'gi'),
                file_name = regexp_replace(file_name, {legacyTextPattern}, '', 'gi')
            WHERE tenant_id = {tenantId}
              AND (label ILIKE {scenarioPattern} OR file_name ILIKE {scenarioPattern} OR label ILIKE {automationPattern} OR file_name ILIKE {automationPattern});
            """, cancellationToken);
    }

    private static string NormalizeTenantEmail(string email, string emailDomain)
    {
        var localPart = email.Split('@', StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(localPart) || string.IsNullOrWhiteSpace(emailDomain)
            ? email.Trim().ToLowerInvariant()
            : $"{localPart.ToLowerInvariant()}@{emailDomain.Trim().ToLowerInvariant()}";
    }

    private async Task<IReadOnlyCollection<T>> ReadSeedFileAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(environment.ContentRootPath, "Resources", "SeedData", fileName);
        if (!File.Exists(path)) return [];

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<T>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    private sealed record CatalogItemSeedRecord(
        string CatalogItemId,
        string ProductId,
        string ItemName,
        string BrandName,
        string CategoryName,
        string Description,
        string ImageUrl,
        decimal UnitPriceAmount,
        string UnitPriceCurrency,
        int AvailableStock,
        string ColdChainRequirement);

    private sealed record InventoryItemSeedRecord(
        string ProductId,
        string CatalogItemId,
        int AvailableQuantity,
        int ReservedQuantity,
        string WarehouseLocation,
        decimal MinimumTemperature,
        decimal MaximumTemperature);

    private static string DepartmentForRole(string role) => role switch
    {
        "Company Owner" => "Executive Office",
        "Sales" => "Sales",
        "Logistics Manager" => "Logistics",
        "Buyer" or "B2B Buyer" => "Purchasing",
        _ => "Operations"
    };

    private sealed record UserSeedRecord(string Username, string FullName, string Email, string Password, string Role);

    private sealed record TenantSeedRecord(
        string Name,
        string LegalName,
        string Slug,
        string Ruc,
        string WorkspaceUrl,
        string EmailDomain,
        string Plan,
        string Status,
        string Country);

    private sealed record ClientAccountSeedRecord(
        string Code,
        string BusinessName,
        string CommercialName,
        string Ruc,
        string Segment,
        string Contact,
        string ContactEmail,
        string Phone,
        string PaymentCondition,
        decimal MonthlyCreditLimit,
        decimal MonthlyCreditUsed,
        string MonthlyCreditStatus,
        string DeliveryPreference,
        bool PortalAccess,
        string SellerWorkspaceEmail,
        string Status,
        string Address = "",
        string District = "",
        string Province = "",
        string DeliveryReference = "");

}
