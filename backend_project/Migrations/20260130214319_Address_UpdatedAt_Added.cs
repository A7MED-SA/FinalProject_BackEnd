using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class Address_UpdatedAt_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "addresses",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "addresses");
        }
    }
}
