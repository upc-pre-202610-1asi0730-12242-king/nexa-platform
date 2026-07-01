using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCrossContextReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "workspace_resource_records",
                newName: "legacy_workspace_resource_records");

            migrationBuilder.AddColumn<int>(
                name: "purchase_request_id",
                table: "inventory_reservation_records",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_purchase_request_id",
                table: "inventory_reservation_records",
                column: "purchase_request_id");

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_purchase_re~",
                table: "inventory_reservation_records",
                column: "purchase_request_id",
                principalTable: "purchase_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_purchase_re~",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_purchase_request_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropColumn(
                name: "purchase_request_id",
                table: "inventory_reservation_records");

            migrationBuilder.RenameTable(
                name: "legacy_workspace_resource_records",
                newName: "workspace_resource_records");
        }
    }
}
