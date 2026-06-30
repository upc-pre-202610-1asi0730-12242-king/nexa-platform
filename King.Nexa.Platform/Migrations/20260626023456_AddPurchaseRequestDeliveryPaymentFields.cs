using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequestDeliveryPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "delivery_address",
                table: "purchase_requests",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_city",
                table: "purchase_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_district",
                table: "purchase_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_province",
                table: "purchase_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_reference",
                table: "purchase_requests",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_option",
                table: "purchase_requests",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_estimate",
                table: "purchase_requests",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivery_address",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "delivery_city",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "delivery_district",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "delivery_province",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "delivery_reference",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "payment_option",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "shipping_estimate",
                table: "purchase_requests");
        }
    }
}
