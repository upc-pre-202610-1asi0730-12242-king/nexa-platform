using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredOrderDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "delivery_address",
                table: "orders",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_address_type",
                table: "orders",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_city",
                table: "orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_district",
                table: "orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_province",
                table: "orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_reference",
                table: "orders",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dispatch_note",
                table: "orders",
                type: "character varying(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "requested_delivery_date",
                table: "orders",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivery_address",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_address_type",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_city",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_district",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_province",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_reference",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "dispatch_note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "requested_delivery_date",
                table: "orders");
        }
    }
}
