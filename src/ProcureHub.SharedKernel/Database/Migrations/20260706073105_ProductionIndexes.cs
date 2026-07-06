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
            // Using CREATE INDEX IF NOT EXISTS for idempotency — some indexes may already
            // exist if a previous partial migration run created them before failing.

            // ── vendors ──────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_vendors_status`     ON `vendors` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_vendors_is_deleted` ON `vendors` (`is_deleted`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_vendors_created_at` ON `vendors` (`created_at`)");

            // ── vendor_documents ─────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_vendor_documents_is_deleted` ON `vendor_documents` (`is_deleted`)");

            // ── purchase_requisitions ────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_requisitions_status`     ON `purchase_requisitions` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_requisitions_is_deleted` ON `purchase_requisitions` (`is_deleted`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_requisitions_created_at` ON `purchase_requisitions` (`created_at`)");

            // ── rfqs ─────────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_rfqs_status`     ON `rfqs` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_rfqs_is_deleted` ON `rfqs` (`is_deleted`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_rfqs_created_at` ON `rfqs` (`created_at`)");

            // ── purchase_orders ──────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_orders_status`     ON `purchase_orders` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_orders_is_deleted` ON `purchase_orders` (`is_deleted`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_purchase_orders_created_at` ON `purchase_orders` (`created_at`)");

            // ── invoices ─────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_invoices_status`     ON `invoices` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_invoices_is_deleted` ON `invoices` (`is_deleted`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_invoices_created_at` ON `invoices` (`created_at`)");

            // ── approval_workflows ───────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_approval_workflows_status`     ON `approval_workflows` (`status`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_approval_workflows_created_at` ON `approval_workflows` (`created_at`)");

            // ── in_app_notifications ─────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_in_app_notifications_is_read`   ON `in_app_notifications` (`is_read`)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `ix_in_app_notifications_created_at` ON `in_app_notifications` (`created_at`)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_vendors_status`     ON `vendors`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_vendors_is_deleted` ON `vendors`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_vendors_created_at` ON `vendors`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_vendor_documents_is_deleted` ON `vendor_documents`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_requisitions_status`     ON `purchase_requisitions`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_requisitions_is_deleted` ON `purchase_requisitions`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_requisitions_created_at` ON `purchase_requisitions`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_rfqs_status`     ON `rfqs`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_rfqs_is_deleted` ON `rfqs`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_rfqs_created_at` ON `rfqs`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_orders_status`     ON `purchase_orders`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_orders_is_deleted` ON `purchase_orders`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_purchase_orders_created_at` ON `purchase_orders`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_invoices_status`     ON `invoices`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_invoices_is_deleted` ON `invoices`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_invoices_created_at` ON `invoices`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_approval_workflows_status`     ON `approval_workflows`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_approval_workflows_created_at` ON `approval_workflows`");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_in_app_notifications_is_read`    ON `in_app_notifications`");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `ix_in_app_notifications_created_at` ON `in_app_notifications`");
        }
    }
}
