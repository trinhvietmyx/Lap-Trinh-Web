using System;
using GIUA_KY_WEB_3AE.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIUA_KY_WEB_3AE.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260623132000_AddNotifications")]
    public partial class AddNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.CreateTable(
                    name: "Notifications",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        OrderId = table.Column<int>(type: "int", nullable: true),
                        Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                        Message = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                        OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        IsRead = table.Column<bool>(type: "bit", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                        ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Notifications", x => x.Id);
                    });
            }
            else
            {
                migrationBuilder.CreateTable(
                    name: "Notifications",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        UserId = table.Column<string>(type: "TEXT", nullable: false),
                        OrderId = table.Column<int>(type: "INTEGER", nullable: true),
                        Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                        Message = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                        OrderStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                        IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                        ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Notifications", x => x.Id);
                    });
            }

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrderId",
                table: "Notifications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
