using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationRegistrationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organization_registration_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    company_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    workspace_name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    workspace_slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    admin_email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_registration_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organization_registration_requests_admin_email",
                table: "organization_registration_requests",
                column: "admin_email");

            migrationBuilder.CreateIndex(
                name: "ix_organization_registration_requests_external_id",
                table: "organization_registration_requests",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_registration_requests_workspace_slug_status",
                table: "organization_registration_requests",
                columns: new[] { "workspace_slug", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_registration_requests");
        }
    }
}
