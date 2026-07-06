using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureHub.SharedKernel.Database.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "in_app_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    link = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_in_app_notifications", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_in_app_notifications_user_id_created_at",
                table: "in_app_notifications",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "in_app_notifications");
        }
    }
}
