using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddUniqueApplicationUserDni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_ApplicationUsers_Dni",
                table: "ApplicationUsers",
                column: "Dni",
                unique: true,
                filter: "[Dni] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ApplicationUsers_Dni",
                table: "ApplicationUsers");
        }
    }
}
