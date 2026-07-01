using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderClientAccountScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "client_account_id",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE orders AS order_row
                SET client_account_id = client.id
                FROM client_accounts AS client
                WHERE order_row.tenant_id = client.tenant_id
                  AND order_row.customer_id = client.code
                  AND order_row.client_account_id IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_orders_client_account_id",
                table: "orders",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id_client_account_id",
                table: "orders",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_orders_client_accounts_client_account_id",
                table: "orders",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_client_accounts_client_account_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_client_account_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_tenant_id_client_account_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "client_account_id",
                table: "orders");
        }
    }
}
