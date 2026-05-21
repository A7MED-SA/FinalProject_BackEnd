using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class ContentLearningDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_enrollments_user_id",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "IX_content_progresses_enrollment_id",
                table: "content_progresses");

            migrationBuilder.DropIndex(
                name: "IX_comment_likes_comment_id",
                table: "comment_likes");

            migrationBuilder.AddColumn<DateTime>(
                name: "actual_end_at",
                table: "live_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "actual_start_at",
                table: "live_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_attendees",
                table: "live_sessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "meeting_url",
                table: "live_sessions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "live_sessions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_user_id_course_id",
                table: "enrollments",
                columns: new[] { "user_id", "course_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_progresses_enrollment_id_content_type_content_id",
                table: "content_progresses",
                columns: new[] { "enrollment_id", "content_type", "content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comment_likes_comment_id_user_id",
                table: "comment_likes",
                columns: new[] { "comment_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_enrollments_user_id_course_id",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "IX_content_progresses_enrollment_id_content_type_content_id",
                table: "content_progresses");

            migrationBuilder.DropIndex(
                name: "IX_comment_likes_comment_id_user_id",
                table: "comment_likes");

            migrationBuilder.DropColumn(
                name: "actual_end_at",
                table: "live_sessions");

            migrationBuilder.DropColumn(
                name: "actual_start_at",
                table: "live_sessions");

            migrationBuilder.DropColumn(
                name: "max_attendees",
                table: "live_sessions");

            migrationBuilder.DropColumn(
                name: "meeting_url",
                table: "live_sessions");

            migrationBuilder.DropColumn(
                name: "password",
                table: "live_sessions");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_user_id",
                table: "enrollments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_progresses_enrollment_id",
                table: "content_progresses",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_likes_comment_id",
                table: "comment_likes",
                column: "comment_id");
        }
    }
}
