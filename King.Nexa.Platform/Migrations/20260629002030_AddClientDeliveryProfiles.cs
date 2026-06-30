using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddClientDeliveryProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "client_accounts",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_reference",
                table: "client_accounts",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "district",
                table: "client_accounts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "document_profile",
                table: "client_accounts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "ruc_factura_xml_pdf_guia");

            migrationBuilder.AddColumn<string>(
                name: "province",
                table: "client_accounts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "client_accounts");

            migrationBuilder.DropColumn(
                name: "delivery_reference",
                table: "client_accounts");

            migrationBuilder.DropColumn(
                name: "district",
                table: "client_accounts");

            migrationBuilder.DropColumn(
                name: "document_profile",
                table: "client_accounts");

            migrationBuilder.DropColumn(
                name: "province",
                table: "client_accounts");
        }
    }
}
