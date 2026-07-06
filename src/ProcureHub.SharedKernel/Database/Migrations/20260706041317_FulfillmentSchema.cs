using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class FulfillmentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    company_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    po_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rfq_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    payment_term_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    delivery_location_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    expected_delivery = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    actual_delivery = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    file_url = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    terms_conditions = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    issued_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    cancelled_reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_orders_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    contract_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    po_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_url = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    signed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    value = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    currency_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchase_order_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contracts", x => x.id);
                    table.ForeignKey(
                        name: "fk_contracts_purchase_order_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_contracts_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contracts_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contracts_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goods_receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    grn_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    po_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    received_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    received_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    delivery_note_no = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchase_order_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goods_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_goods_receipts_purchase_order_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_goods_receipts_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_goods_receipts_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_goods_receipts_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    invoice_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    po_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    file_url = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    due_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    payment_reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rejection_reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    purchase_order_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoices_purchase_order_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_invoices_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoices_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoices_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "po_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    po_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    material_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    description = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    uom_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    unit_price = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    total_price = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    received_qty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_po_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_po_items_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_po_items_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_po_items_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "grn_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    grn_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    po_item_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    received_qty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    rejected_qty = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    quality_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reject_reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grn_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_grn_items_goods_receipts_grn_id",
                        column: x => x.grn_id,
                        principalTable: "goods_receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grn_items_po_item_po_item_id",
                        column: x => x.po_item_id,
                        principalTable: "po_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grn_items_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_grn_items_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_contract_number",
                table: "contracts",
                column: "contract_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contracts_created_by_id",
                table: "contracts",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_deleted_by_id",
                table: "contracts",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_purchase_order_id",
                table: "contracts",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_updated_by_id",
                table: "contracts",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_vendor_id",
                table: "contracts",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_created_by_id",
                table: "goods_receipts",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_deleted_by_id",
                table: "goods_receipts",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_grn_number",
                table: "goods_receipts",
                column: "grn_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_po_id",
                table: "goods_receipts",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_purchase_order_id",
                table: "goods_receipts",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipts_updated_by_id",
                table: "goods_receipts",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_grn_items_created_by_id",
                table: "grn_items",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_grn_items_grn_id",
                table: "grn_items",
                column: "grn_id");

            migrationBuilder.CreateIndex(
                name: "ix_grn_items_po_item_id",
                table: "grn_items",
                column: "po_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_grn_items_updated_by_id",
                table: "grn_items",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_created_by_id",
                table: "invoices",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_deleted_by_id",
                table: "invoices",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_po_id",
                table: "invoices",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_purchase_order_id",
                table: "invoices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_status",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_updated_by_id",
                table: "invoices",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_vendor_id",
                table: "invoices",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_po_items_created_by_id",
                table: "po_items",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_po_items_po_id",
                table: "po_items",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_po_items_updated_by_id",
                table: "po_items",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_company_id_po_number",
                table: "purchase_orders",
                columns: new[] { "company_id", "po_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_company_id_status",
                table: "purchase_orders",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_created_by_id",
                table: "purchase_orders",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_deleted_by_id",
                table: "purchase_orders",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_updated_by_id",
                table: "purchase_orders",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_vendor_id",
                table: "purchase_orders",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "grn_items");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "goods_receipts");

            migrationBuilder.DropTable(
                name: "po_items");

            migrationBuilder.DropTable(
                name: "purchase_orders");
        }
    }
}
