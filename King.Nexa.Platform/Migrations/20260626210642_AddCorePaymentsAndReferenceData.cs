using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddCorePaymentsAndReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payments_reference_code",
                table: "payments");

            migrationBuilder.AlterColumn<int>(
                name: "invoice_id",
                table: "payments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "client_account_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirmed_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "order_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_method_record_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_option_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "rejected_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE payments AS payment
                SET tenant_id = invoice.tenant_id
                FROM invoices AS invoice
                WHERE payment.invoice_id = invoice.id
                  AND payment.tenant_id IS NULL;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM payments WHERE tenant_id IS NULL) THEN
                        RAISE EXCEPTION 'Cannot migrate payments without invoice tenant scope.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "tenant_id",
                table: "payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_id",
                table: "payment_process_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "tenant_id",
                table: "invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "document_type_id",
                table: "business_documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unit_of_measures",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_of_measures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                    table.ForeignKey(
                        name: "fk_departments_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provinces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provinces", x => x.id);
                    table.ForeignKey(
                        name: "fk_provinces_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    province_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_districts", x => x.id);
                    table.ForeignKey(
                        name: "fk_districts_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_client_account_id",
                table: "payments",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_payment_method_record_id",
                table: "payments",
                column: "payment_method_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_payment_option_id",
                table: "payments",
                column: "payment_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_created_at",
                table: "payments",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_reference_code",
                table: "payments",
                columns: new[] { "tenant_id", "reference_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_status",
                table: "payments",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_payment_id",
                table: "payment_process_records",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_documents_document_type_id",
                table: "business_documents",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_countries_code",
                table: "countries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_code",
                table: "departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_country_id",
                table: "departments",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_districts_code",
                table: "districts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_districts_province_id",
                table: "districts",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_types_key",
                table: "document_types",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_options_key",
                table: "payment_options",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provinces_code",
                table: "provinces",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provinces_department_id",
                table: "provinces",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_of_measures_key",
                table: "unit_of_measures",
                column: "key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_business_documents_document_types_document_type_id",
                table: "business_documents",
                column: "document_type_id",
                principalTable: "document_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_payments_payment_id",
                table: "payment_process_records",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_client_accounts_client_account_id",
                table: "payments",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_invoices_invoice_id",
                table: "payments",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_orders_order_id",
                table: "payments",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_method_records_payment_method_record_id",
                table: "payments",
                column: "payment_method_record_id",
                principalTable: "payment_method_records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_options_payment_option_id",
                table: "payments",
                column: "payment_option_id",
                principalTable: "payment_options",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_tenants_tenant_id",
                table: "payments",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_business_documents_document_types_document_type_id",
                table: "business_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_payments_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_client_accounts_client_account_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_invoices_invoice_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_orders_order_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_method_records_payment_method_record_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_options_payment_option_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_tenants_tenant_id",
                table: "payments");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "payment_options");

            migrationBuilder.DropTable(
                name: "unit_of_measures");

            migrationBuilder.DropTable(
                name: "provinces");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropIndex(
                name: "ix_payments_client_account_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_invoice_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_order_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_payment_method_record_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_payment_option_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_created_at",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_reference_code",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_status",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_business_documents_document_type_id",
                table: "business_documents");

            migrationBuilder.DropColumn(
                name: "client_account_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "order_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_method_record_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_option_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "payment_process_records");

            migrationBuilder.DropColumn(
                name: "document_type_id",
                table: "business_documents");

            migrationBuilder.AlterColumn<int>(
                name: "invoice_id",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "tenant_id",
                table: "invoices",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "ix_payments_reference_code",
                table: "payments",
                column: "reference_code",
                unique: true);
        }
    }
}
