using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint6_AddVendorTaxInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_pkp",
                table: "vendors",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "pph_rate",
                table: "vendors",
                type: "DECIMAL(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_payable",
                table: "invoices",
                type: "DECIMAL(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "withholding_tax",
                table: "invoices",
                type: "DECIMAL(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_pkp",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "pph_rate",
                table: "vendors");

            migrationBuilder.DropColumn(
                name: "net_payable",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "withholding_tax",
                table: "invoices");
        }
    }
}
