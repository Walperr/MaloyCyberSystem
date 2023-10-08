using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MQTTServer.Migrations
{
    /// <inheritdoc />
    public partial class SaltToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "Devices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Devices");
        }
    }
}
