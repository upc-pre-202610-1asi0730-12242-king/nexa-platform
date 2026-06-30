using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceResourceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspace_resource_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    resource_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    external_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    search_text = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_resource_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspace_resource_records_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workspace_resource_records_tenant_id_resource_key_external_~",
                table: "workspace_resource_records",
                columns: new[] { "tenant_id", "resource_key", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_resource_records_tenant_id_resource_key_status",
                table: "workspace_resource_records",
                columns: new[] { "tenant_id", "resource_key", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_resource_records");
        }
    }
}
