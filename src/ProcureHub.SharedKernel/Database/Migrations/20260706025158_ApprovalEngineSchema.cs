using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalEngineSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approval_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    company_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reference_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_value = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    max_value = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    required_levels = table.Column<int>(type: "int", nullable: false),
                    is_strategic_override = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_single_source_override = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_policies", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_policies_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_policies_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "approval_workflows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    company_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reference_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reference_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference_title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_value = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    is_strategic_item = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_single_source = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    requested_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    current_level = table.Column<int>(type: "int", nullable: false),
                    max_level = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    iteration = table.Column<int>(type: "int", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("pk_approval_workflows", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_workflows_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_workflows_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_workflows_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "approval_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    workflow_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    level = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    actor_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acted_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_histories_approval_workflow_workflow_id",
                        column: x => x.workflow_id,
                        principalTable: "approval_workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_approval_histories_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_histories_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "approver_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    workflow_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    level = table.Column<int>(type: "int", nullable: false),
                    assigned_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    assigned_user_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_delegate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    delegated_from_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approver_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_approver_assignments_approval_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalTable: "approval_workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_approver_assignments_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approver_assignments_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_approval_histories_created_by_id",
                table: "approval_histories",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_histories_updated_by_id",
                table: "approval_histories",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_histories_workflow_id",
                table: "approval_histories",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_policies_company_id",
                table: "approval_policies",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_policies_company_id_reference_type_min_value",
                table: "approval_policies",
                columns: new[] { "company_id", "reference_type", "min_value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_policies_created_by_id",
                table: "approval_policies",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_policies_updated_by_id",
                table: "approval_policies",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_company_id",
                table: "approval_workflows",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_created_by_id",
                table: "approval_workflows",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_deleted_by_id",
                table: "approval_workflows",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_reference_type_reference_id",
                table: "approval_workflows",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_status",
                table: "approval_workflows",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_approval_workflows_updated_by_id",
                table: "approval_workflows",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approver_assignments_created_by_id",
                table: "approver_assignments",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approver_assignments_updated_by_id",
                table: "approver_assignments",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_approver_assignments_workflow_id",
                table: "approver_assignments",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "ix_approver_assignments_workflow_id_level",
                table: "approver_assignments",
                columns: new[] { "workflow_id", "level" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_histories");

            migrationBuilder.DropTable(
                name: "approval_policies");

            migrationBuilder.DropTable(
                name: "approver_assignments");

            migrationBuilder.DropTable(
                name: "approval_workflows");
        }
    }
}
