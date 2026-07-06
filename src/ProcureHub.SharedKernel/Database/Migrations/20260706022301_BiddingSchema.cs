using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class BiddingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bid_evaluations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    price_weight = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    quality_weight = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    delivery_weight = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    awarded_vendor_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    awarded_quotation_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("pk_bid_evaluations", x => x.id);
                    table.ForeignKey(
                        name: "fk_bid_evaluations_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bid_evaluations_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bid_evaluations_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vendor_quotations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_price = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
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
                    table.PrimaryKey("pk_vendor_quotations", x => x.id);
                    table.ForeignKey(
                        name: "fk_vendor_quotations_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vendor_quotations_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_vendor_quotations_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "evaluation_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    bid_evaluation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    quotation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    vendor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    price_score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    quality_score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    delivery_score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    weighted_total = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evaluation_scores", x => x.id);
                    table.ForeignKey(
                        name: "fk_evaluation_scores_bid_evaluations_bid_evaluation_id",
                        column: x => x.bid_evaluation_id,
                        principalTable: "bid_evaluations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_evaluation_scores_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evaluation_scores_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quotation_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    quotation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    rfq_item_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    unit_price = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotation_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_quotation_items_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotation_items_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotation_items_vendor_quotation_quotation_id",
                        column: x => x.quotation_id,
                        principalTable: "vendor_quotations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_bid_evaluations_created_by_id",
                table: "bid_evaluations",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_bid_evaluations_deleted_by_id",
                table: "bid_evaluations",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_bid_evaluations_rfq_id",
                table: "bid_evaluations",
                column: "rfq_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bid_evaluations_updated_by_id",
                table: "bid_evaluations",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_scores_bid_evaluation_id",
                table: "evaluation_scores",
                column: "bid_evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_scores_created_by_id",
                table: "evaluation_scores",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_scores_quotation_id",
                table: "evaluation_scores",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_scores_updated_by_id",
                table: "evaluation_scores",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_items_created_by_id",
                table: "quotation_items",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_items_quotation_id",
                table: "quotation_items",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_items_rfq_item_id",
                table: "quotation_items",
                column: "rfq_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_items_updated_by_id",
                table: "quotation_items",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_quotations_created_by_id",
                table: "vendor_quotations",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_quotations_deleted_by_id",
                table: "vendor_quotations",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_quotations_rfq_id_vendor_id",
                table: "vendor_quotations",
                columns: new[] { "rfq_id", "vendor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendor_quotations_updated_by_id",
                table: "vendor_quotations",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_quotations_vendor_id",
                table: "vendor_quotations",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evaluation_scores");

            migrationBuilder.DropTable(
                name: "quotation_items");

            migrationBuilder.DropTable(
                name: "bid_evaluations");

            migrationBuilder.DropTable(
                name: "vendor_quotations");
        }
    }
}
