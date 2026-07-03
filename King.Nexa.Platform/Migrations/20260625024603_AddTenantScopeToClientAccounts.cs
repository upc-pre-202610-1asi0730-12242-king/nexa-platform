using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToClientAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_client_accounts_code",
                table: "client_accounts");

            migrationBuilder.DropIndex(
                name: "ix_client_accounts_ruc",
                table: "client_accounts");

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "client_accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE client_accounts
                SET tenant_id = (SELECT id FROM tenants ORDER BY id LIMIT 1)
                WHERE tenant_id IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "tenant_id",
                table: "client_accounts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_tenant_id_code",
                table: "client_accounts",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_tenant_id_ruc",
                table: "client_accounts",
                columns: new[] { "tenant_id", "ruc" });

            migrationBuilder.AddForeignKey(
                name: "fk_client_accounts_tenants_tenant_id",
                table: "client_accounts",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_client_accounts_tenants_tenant_id",
                table: "client_accounts");

            migrationBuilder.DropIndex(
                name: "ix_client_accounts_tenant_id_code",
                table: "client_accounts");

            migrationBuilder.DropIndex(
                name: "ix_client_accounts_tenant_id_ruc",
                table: "client_accounts");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "client_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_code",
                table: "client_accounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_ruc",
                table: "client_accounts",
                column: "ruc");
        }
    }
}
