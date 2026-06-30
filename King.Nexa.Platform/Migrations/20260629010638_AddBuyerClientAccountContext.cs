using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerClientAccountContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "client_account_id",
                table: "user_workspace_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_workspace_memberships_tenant_id_client_account_id",
                table: "user_workspace_memberships",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.Sql("""
                UPDATE user_workspace_memberships AS membership
                SET client_account_id = client.id
                FROM client_accounts AS client
                WHERE membership.tenant_id = client.tenant_id
                  AND client.code = 'CLI-001'
                  AND membership.role IN ('Buyer', 'B2B Buyer');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_workspace_memberships_tenant_id_client_account_id",
                table: "user_workspace_memberships");

            migrationBuilder.DropColumn(
                name: "client_account_id",
                table: "user_workspace_memberships");
        }
    }
}
