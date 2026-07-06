using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class ProductionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── vendors ──────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_vendors_status",
                table:   "vendors",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_vendors_is_deleted",
                table:   "vendors",
                column:  "is_deleted");

            migrationBuilder.CreateIndex(
                name:    "ix_vendors_created_at",
                table:   "vendors",
                column:  "created_at");

            // ── vendor_documents ─────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_vendor_documents_is_deleted",
                table:   "vendor_documents",
                column:  "is_deleted");

            // ── purchase_requisitions ────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_purchase_requisitions_status",
                table:   "purchase_requisitions",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_purchase_requisitions_is_deleted",
                table:   "purchase_requisitions",
                column:  "is_deleted");

            migrationBuilder.CreateIndex(
                name:    "ix_purchase_requisitions_created_at",
                table:   "purchase_requisitions",
                column:  "created_at");

            // ── rfqs ─────────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_rfqs_status",
                table:   "rfqs",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_rfqs_is_deleted",
                table:   "rfqs",
                column:  "is_deleted");

            migrationBuilder.CreateIndex(
                name:    "ix_rfqs_created_at",
                table:   "rfqs",
                column:  "created_at");

            // ── purchase_orders ──────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_purchase_orders_status",
                table:   "purchase_orders",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_purchase_orders_is_deleted",
                table:   "purchase_orders",
                column:  "is_deleted");

            migrationBuilder.CreateIndex(
                name:    "ix_purchase_orders_created_at",
                table:   "purchase_orders",
                column:  "created_at");

            // ── invoices ─────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_invoices_status",
                table:   "invoices",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_invoices_is_deleted",
                table:   "invoices",
                column:  "is_deleted");

            migrationBuilder.CreateIndex(
                name:    "ix_invoices_created_at",
                table:   "invoices",
                column:  "created_at");

            // ── approval_workflows ───────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_approval_workflows_status",
                table:   "approval_workflows",
                column:  "status");

            migrationBuilder.CreateIndex(
                name:    "ix_approval_workflows_created_at",
                table:   "approval_workflows",
                column:  "created_at");

            // ── notifications ────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name:    "ix_notifications_is_read",
                table:   "notifications",
                column:  "is_read");

            migrationBuilder.CreateIndex(
                name:    "ix_notifications_created_at",
                table:   "notifications",
                column:  "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_vendors_status",              table: "vendors");
            migrationBuilder.DropIndex(name: "ix_vendors_is_deleted",          table: "vendors");
            migrationBuilder.DropIndex(name: "ix_vendors_created_at",          table: "vendors");

            migrationBuilder.DropIndex(name: "ix_vendor_documents_is_deleted", table: "vendor_documents");

            migrationBuilder.DropIndex(name: "ix_purchase_requisitions_status",     table: "purchase_requisitions");
            migrationBuilder.DropIndex(name: "ix_purchase_requisitions_is_deleted", table: "purchase_requisitions");
            migrationBuilder.DropIndex(name: "ix_purchase_requisitions_created_at", table: "purchase_requisitions");

            migrationBuilder.DropIndex(name: "ix_rfqs_status",     table: "rfqs");
            migrationBuilder.DropIndex(name: "ix_rfqs_is_deleted", table: "rfqs");
            migrationBuilder.DropIndex(name: "ix_rfqs_created_at", table: "rfqs");

            migrationBuilder.DropIndex(name: "ix_purchase_orders_status",     table: "purchase_orders");
            migrationBuilder.DropIndex(name: "ix_purchase_orders_is_deleted", table: "purchase_orders");
            migrationBuilder.DropIndex(name: "ix_purchase_orders_created_at", table: "purchase_orders");

            migrationBuilder.DropIndex(name: "ix_invoices_status",     table: "invoices");
            migrationBuilder.DropIndex(name: "ix_invoices_is_deleted", table: "invoices");
            migrationBuilder.DropIndex(name: "ix_invoices_created_at", table: "invoices");

            migrationBuilder.DropIndex(name: "ix_approval_workflows_status",     table: "approval_workflows");
            migrationBuilder.DropIndex(name: "ix_approval_workflows_created_at", table: "approval_workflows");

            migrationBuilder.DropIndex(name: "ix_notifications_is_read",   table: "notifications");
            migrationBuilder.DropIndex(name: "ix_notifications_created_at", table: "notifications");
        }
    }
}
