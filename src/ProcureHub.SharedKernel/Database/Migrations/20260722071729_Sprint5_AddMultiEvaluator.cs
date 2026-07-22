using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint5_AddMultiEvaluator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evaluator_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    bid_evaluation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    assigned_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    assigned_user_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    has_submitted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evaluator_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_evaluator_assignments_bid_evaluations_bid_evaluation_id",
                        column: x => x.bid_evaluation_id,
                        principalTable: "bid_evaluations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_evaluator_assignments_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evaluator_assignments_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "evaluator_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    evaluator_assignment_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    quotation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    quality_score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    delivery_score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evaluator_scores", x => x.id);
                    table.ForeignKey(
                        name: "fk_evaluator_scores_evaluator_assignments_evaluator_assignment_",
                        column: x => x.evaluator_assignment_id,
                        principalTable: "evaluator_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_assignments_bid_evaluation_id",
                table: "evaluator_assignments",
                column: "bid_evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_assignments_bid_evaluation_id_assigned_user_id",
                table: "evaluator_assignments",
                columns: new[] { "bid_evaluation_id", "assigned_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_assignments_created_by_id",
                table: "evaluator_assignments",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_assignments_updated_by_id",
                table: "evaluator_assignments",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_scores_evaluator_assignment_id",
                table: "evaluator_scores",
                column: "evaluator_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluator_scores_evaluator_assignment_id_quotation_id",
                table: "evaluator_scores",
                columns: new[] { "evaluator_assignment_id", "quotation_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evaluator_scores");

            migrationBuilder.DropTable(
                name: "evaluator_assignments");
        }
    }
}
