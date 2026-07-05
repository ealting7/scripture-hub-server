using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace scripture_hub_server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add the new IsActive column to the existing AspNetUsers table
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Identity",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Identity",
                table: "AspNetUsers");
        }
    }
}
