using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunicationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reports_reporter_id",
                table: "reports");

            migrationBuilder.AddColumn<string>(
                name: "admin_note",
                table: "reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "course_id",
                table: "announcements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id_entity_type_entity_id",
                table: "reports",
                columns: new[] { "reporter_id", "entity_type", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcements_course_id",
                table: "announcements",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_announcements_courses_course_id",
                table: "announcements",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_announcements_courses_course_id",
                table: "announcements");

            migrationBuilder.DropIndex(
                name: "IX_reports_reporter_id_entity_type_entity_id",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_announcements_course_id",
                table: "announcements");

            migrationBuilder.DropColumn(
                name: "admin_note",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "announcements");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id",
                table: "reports",
                column: "reporter_id");
        }
    }
}
