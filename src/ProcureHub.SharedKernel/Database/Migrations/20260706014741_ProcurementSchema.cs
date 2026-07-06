using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class ProcurementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_requisitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    company_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    pr_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    department = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    delivery_location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    required_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_estimated_value = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("pk_purchase_requisitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_requisitions_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_requisitions_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_requisitions_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rfqs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    company_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchase_requisition_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    bid_deadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    terms = table.Column<string>(type: "TEXT", nullable: true)
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
                    table.PrimaryKey("pk_rfqs", x => x.id);
                    table.ForeignKey(
                        name: "fk_rfqs_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rfqs_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rfqs_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pr_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    purchase_requisition_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    material_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    item_description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    unit_of_measure_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    unit_label = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estimated_unit_price = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pr_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_pr_items_purchase_requisition_purchase_requisition_id",
                        column: x => x.purchase_requisition_id,
                        principalTable: "purchase_requisitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pr_items_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pr_items_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rfq_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    pr_item_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    material_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    item_description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    unit_of_measure_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    unit_label = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rfq_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_rfq_items_rfqs_rfq_id",
                        column: x => x.rfq_id,
                        principalTable: "rfqs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rfq_items_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rfq_items_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rfq_vendors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    invited_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    declined_reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rfq_vendors", x => x.id);
                    table.ForeignKey(
                        name: "fk_rfq_vendors_rfqs_rfq_id",
                        column: x => x.rfq_id,
                        principalTable: "rfqs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rfq_vendors_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rfq_vendors_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_pr_items_created_by_id",
                table: "pr_items",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_pr_items_purchase_requisition_id",
                table: "pr_items",
                column: "purchase_requisition_id");

            migrationBuilder.CreateIndex(
                name: "ix_pr_items_updated_by_id",
                table: "pr_items",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requisitions_company_id_pr_number",
                table: "purchase_requisitions",
                columns: new[] { "company_id", "pr_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requisitions_company_id_status",
                table: "purchase_requisitions",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requisitions_created_by_id",
                table: "purchase_requisitions",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requisitions_deleted_by_id",
                table: "purchase_requisitions",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requisitions_updated_by_id",
                table: "purchase_requisitions",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_items_created_by_id",
                table: "rfq_items",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_items_rfq_id",
                table: "rfq_items",
                column: "rfq_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_items_updated_by_id",
                table: "rfq_items",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_vendors_created_by_id",
                table: "rfq_vendors",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_vendors_rfq_id_vendor_id",
                table: "rfq_vendors",
                columns: new[] { "rfq_id", "vendor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rfq_vendors_updated_by_id",
                table: "rfq_vendors",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfq_vendors_vendor_id",
                table: "rfq_vendors",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_company_id_rfq_number",
                table: "rfqs",
                columns: new[] { "company_id", "rfq_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_company_id_status",
                table: "rfqs",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_created_by_id",
                table: "rfqs",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_deleted_by_id",
                table: "rfqs",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_updated_by_id",
                table: "rfqs",
                column: "updated_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pr_items");

            migrationBuilder.DropTable(
                name: "rfq_items");

            migrationBuilder.DropTable(
                name: "rfq_vendors");

            migrationBuilder.DropTable(
                name: "purchase_requisitions");

            migrationBuilder.DropTable(
                name: "rfqs");
        }
    }
}
