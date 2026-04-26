using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseEditApprovalSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_content_update_at",
                table: "courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "course_edit_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requested_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    request_type = table.Column<int>(type: "int", nullable: false),
                    target_section_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    target_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    operation = table.Column<int>(type: "int", nullable: false),
                    json_payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    admin_notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_emergency = table.Column<bool>(type: "bit", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    requested_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_edit_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_edit_requests_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_edit_requests_users_requested_by",
                        column: x => x.requested_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_course_edit_requests_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_edit_requests_requested_by",
                table: "course_edit_requests",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_course_edit_requests_reviewed_by",
                table: "course_edit_requests",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEditRequests_CourseId_Status",
                table: "course_edit_requests",
                columns: new[] { "course_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEditRequests_IsEmergency",
                table: "course_edit_requests",
                column: "is_emergency",
                filter: "is_emergency = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEditRequests_RequestedAt",
                table: "course_edit_requests",
                column: "requested_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_edit_requests");

            migrationBuilder.DropColumn(
                name: "last_content_update_at",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "version",
                table: "courses");
        }
    }
}
