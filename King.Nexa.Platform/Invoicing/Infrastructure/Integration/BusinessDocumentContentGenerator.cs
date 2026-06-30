using System.Text;
using System.Xml.Linq;
using King.Nexa.Platform.Invoicing.Application.OutboundServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Integration;

public class BusinessDocumentContentGenerator(AppDbContext context) : IBusinessDocumentContentGenerator
{
    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "factura_xml",
        "factura_pdf",
        "guia_pdf"
    };

    public async Task<GeneratedBusinessDocumentContent> GenerateAsync(
        int tenantId,
        int orderId,
        string type,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = type.Trim().ToLowerInvariant();
        if (!SupportedTypes.Contains(normalizedType))
            throw new InvalidOperationException("Document type cannot be generated.");

        var order = await context.Orders.AsNoTracking()
            .Include(row => row.Items)
            .SingleOrDefaultAsync(row => row.TenantId == tenantId && row.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order does not belong to the current tenant.");

        var client = await ResolveClientAsync(tenantId, order, cancellationToken);
        var tenant = await context.Tenants.AsNoTracking()
            .SingleAsync(row => row.Id == tenantId, cancellationToken);
        var extension = normalizedType == "factura_xml" ? "xml" : "pdf";
        var fileName = $"{SafeFilePart(order.OrderNumber.Value)}-{normalizedType}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.{extension}";
        var label = normalizedType switch
        {
            "factura_xml" => "Factura XML",
            "factura_pdf" => "Factura PDF",
            _ => "Guia de remision PDF"
        };

        var content = normalizedType == "factura_xml"
            ? BuildInvoiceXml(tenant.LegalName, tenant.Ruc, client, order)
            : BuildPdf(normalizedType == "guia_pdf" ? "GUIA DE REMISION" : "FACTURA", tenant.LegalName, tenant.Ruc, client, order);

        return new GeneratedBusinessDocumentContent(
            content,
            fileName,
            extension == "xml" ? "application/xml" : "application/pdf",
            label,
            client.Id);
    }

    private async Task<ClientAccount> ResolveClientAsync(int tenantId, Order order, CancellationToken cancellationToken)
    {
        var query = context.ClientAccounts.AsNoTracking().Where(row => row.TenantId == tenantId);
        var client = order.ClientAccountId.HasValue
            ? await query.SingleOrDefaultAsync(row => row.Id == order.ClientAccountId.Value, cancellationToken)
            : await query.SingleOrDefaultAsync(row => row.Code == order.CustomerId.Value, cancellationToken);
        return client ?? throw new InvalidOperationException("Order client account is not available in the current tenant.");
    }

    private static byte[] BuildInvoiceXml(string supplierName, string supplierRuc, ClientAccount client, Order order)
    {
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Invoice",
                new XAttribute("version", "1.0"),
                new XElement("DocumentNumber", order.OrderNumber.Value),
                new XElement("IssueDate", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement("Currency", order.Total.Currency),
                new XElement("Supplier",
                    new XElement("LegalName", supplierName),
                    new XElement("Ruc", supplierRuc)),
                new XElement("Customer",
                    new XElement("BusinessName", client.BusinessName),
                    new XElement("Ruc", client.Ruc),
                    new XElement("Email", client.ContactEmail)),
                new XElement("Delivery",
                    new XElement("Address", order.Delivery.FullAddress),
                    new XElement("Reference", order.Delivery.Reference)),
                new XElement("Lines", order.Items.Select((item, index) =>
                    new XElement("Line",
                        new XAttribute("number", index + 1),
                        new XElement("Description", item.ItemName.Value),
                        new XElement("Quantity", item.Quantity.Value),
                        new XElement("UnitPrice", item.UnitPrice.Amount),
                        new XElement("Subtotal", item.Subtotal.Amount)))),
                new XElement("Total", order.Total.Amount)));
        return Encoding.UTF8.GetBytes(document.ToString());
    }

    private static byte[] BuildPdf(string title, string supplierName, string supplierRuc, ClientAccount client, Order order)
    {
        var lines = new List<string>
        {
            title,
            supplierName,
            $"RUC: {supplierRuc}",
            $"Pedido: {order.OrderNumber.Value}",
            $"Fecha: {DateTime.UtcNow:yyyy-MM-dd}",
            $"Cliente: {client.BusinessName}",
            $"RUC cliente: {client.Ruc}",
            $"Entrega: {order.Delivery.FullAddress}",
            string.Empty,
            "DETALLE"
        };
        lines.AddRange(order.Items.Select(item =>
            $"{item.Quantity.Value} x {item.ItemName.Value} | {item.UnitPrice.Currency} {item.UnitPrice.Amount:0.00} | {item.Subtotal.Amount:0.00}"));
        lines.Add(string.Empty);
        lines.Add($"TOTAL: {order.Total.Currency} {order.Total.Amount:0.00}");
        return SimplePdf(lines);
    }

    private static byte[] SimplePdf(IEnumerable<string> lines)
    {
        var streamText = new StringBuilder("BT\n/F1 11 Tf\n50 750 Td\n14 TL\n");
        foreach (var line in lines.Take(44))
            streamText.Append('(').Append(EscapePdfText(line)).Append(") Tj\nT*\n");
        streamText.Append("ET\n");
        var streamBytes = Encoding.ASCII.GetBytes(streamText.ToString());
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {streamBytes.Length} >>\nstream\n{streamText}endstream"
        };

        using var output = new MemoryStream();
        WriteAscii(output, "%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(output.Position);
            WriteAscii(output, $"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }
        var xrefOffset = output.Position;
        WriteAscii(output, $"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) WriteAscii(output, $"{offset:0000000000} 00000 n \n");
        WriteAscii(output, $"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        return output.ToArray();
    }

    private static string EscapePdfText(string value)
    {
        var ascii = new string(value.Normalize(NormalizationForm.FormD)
            .Where(character => character <= 127 && System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());
        return ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static string SafeFilePart(string value) =>
        string.Concat(value.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-')).ToLowerInvariant();

    private static void WriteAscii(Stream stream, string value) => stream.Write(Encoding.ASCII.GetBytes(value));
}
