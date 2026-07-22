using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_VendorUserNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "vendor_id",
                table: "purchase_orders",
                type: "CHAR(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "vendor_user_id",
                table: "in_app_notifications",
                type: "CHAR(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_vendors_vendor_id",
                table: "purchase_orders",
                column: "vendor_id",
                principalTable: "vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_vendors_vendor_id",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "vendor_user_id",
                table: "in_app_notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "vendor_id",
                table: "purchase_orders",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "CHAR(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
