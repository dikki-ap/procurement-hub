using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_ReturnOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "return_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    return_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    grn_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    po_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acknowledged_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    shipped_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    received_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    goods_receipt_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("pk_return_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_return_orders_goods_receipts_goods_receipt_id",
                        column: x => x.goods_receipt_id,
                        principalTable: "goods_receipts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_return_orders_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_return_orders_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_return_orders_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "return_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    return_order_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    po_item_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    item_description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    uom = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    return_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_return_order_items_return_orders_return_order_id",
                        column: x => x.return_order_id,
                        principalTable: "return_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_return_order_items_return_order_id",
                table: "return_order_items",
                column: "return_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_created_by_id",
                table: "return_orders",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_deleted_by_id",
                table: "return_orders",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_goods_receipt_id",
                table: "return_orders",
                column: "goods_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_grn_id",
                table: "return_orders",
                column: "grn_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_return_number",
                table: "return_orders",
                column: "return_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_updated_by_id",
                table: "return_orders",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_vendor_id",
                table: "return_orders",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "return_order_items");

            migrationBuilder.DropTable(
                name: "return_orders");
        }
    }
}
