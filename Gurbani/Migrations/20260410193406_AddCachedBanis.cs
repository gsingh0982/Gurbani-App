using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gurbani.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedBanis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedBanis",
                columns: table => new
                {
                    BaniId = table.Column<int>(type: "int", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CachedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedBanis", x => x.BaniId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedBanis");
        }
    }
}
