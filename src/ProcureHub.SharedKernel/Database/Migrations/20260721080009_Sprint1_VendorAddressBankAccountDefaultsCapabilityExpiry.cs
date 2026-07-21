using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint1_VendorAddressBankAccountDefaultsCapabilityExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "vendors",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "vendors",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "vendors",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "default_currency_id",
                table: "vendors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "default_payment_term_id",
                table: "vendors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "vendors",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "province",
                table: "vendors",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "effective_date",
                table: "vendor_capabilities",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "expiry_date",
                table: "vendor_capabilities",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_expired",
                table: "vendor_capabilities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "max_order_qty",
                table: "vendor_capabilities",
                type: "DECIMAL(18,4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vendor_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    bank_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    branch_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    currency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "IDR")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_vendor_bank_accounts_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vendor_bank_accounts_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vendor_bank_accounts_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_bank_accounts_created_by_id",
                table: "vendor_bank_accounts",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_bank_accounts_updated_by_id",
                table: "vendor_bank_accounts",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_bank_accounts_vendor_id",
                table: "vendor_bank_accounts",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vendor_bank_accounts");

            migrationBuilder.DropColumn(
                name: "address",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "city",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "country",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "default_currency_id",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "default_payment_term_id",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "province",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "effective_date",
                table: "vendor_capabilities");

            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "vendor_capabilities");

            migrationBuilder.DropColumn(
                name: "is_expired",
                table: "vendor_capabilities");

            migrationBuilder.DropColumn(
                name: "max_order_qty",
                table: "vendor_capabilities");
        }
    }
}
