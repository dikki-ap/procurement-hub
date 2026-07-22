using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_ContractCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_url",
                table: "contracts",
                newName: "file_key");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "contracts",
                type: "CHAR(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_company_id",
                table: "contracts",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_contracts_companies_company_id",
                table: "contracts",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contracts_companies_company_id",
                table: "contracts");

            migrationBuilder.DropIndex(
                name: "ix_contracts_company_id",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "contracts");

            migrationBuilder.RenameColumn(
                name: "file_key",
                table: "contracts",
                newName: "file_url");
        }
    }
}
