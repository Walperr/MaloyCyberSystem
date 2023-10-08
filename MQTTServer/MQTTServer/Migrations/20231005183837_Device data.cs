using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MQTTServer.Migrations
{
    /// <inheritdoc />
    public partial class Devicedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "ConnectedDevicesID",
                table: "Users",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionCode",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime[]>(
                name: "Times",
                table: "Devices",
                type: "timestamp with time zone[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "Values",
                table: "Devices",
                type: "double precision[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedDevicesID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConnectionCode",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Times",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Values",
                table: "Devices");
        }
    }
}
