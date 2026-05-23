using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledDeletionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                table: "courses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_read_only_for_students",
                table: "courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_deletion_at",
                table: "courses",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deletion_reason",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "is_read_only_for_students",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "scheduled_deletion_at",
                table: "courses");
        }
    }
}
