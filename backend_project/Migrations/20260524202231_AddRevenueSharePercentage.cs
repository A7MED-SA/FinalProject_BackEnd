using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class AddRevenueSharePercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reviews_user_id",
                table: "reviews");

            migrationBuilder.AddColumn<decimal>(
                name: "revenue_share_percentage",
                table: "users",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "flagged_at",
                table: "reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "flagged_by",
                table: "reviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_flagged",
                table: "reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "certificates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "enrollment_id",
                table: "certificates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "certificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "revoked_by",
                table: "certificates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "certificates",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_flagged_by",
                table: "reviews",
                column: "flagged_by");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_is_flagged",
                table: "reviews",
                column: "is_flagged");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id_course_id",
                table: "reviews",
                columns: new[] { "user_id", "course_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_certificates_enrollment_id",
                table: "certificates",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_revoked_by",
                table: "certificates",
                column: "revoked_by");

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_enrollments_enrollment_id",
                table: "certificates",
                column: "enrollment_id",
                principalTable: "enrollments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_users_revoked_by",
                table: "certificates",
                column: "revoked_by",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_users_flagged_by",
                table: "reviews",
                column: "flagged_by",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificates_enrollments_enrollment_id",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_certificates_users_revoked_by",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_users_flagged_by",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_flagged_by",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_is_flagged",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_user_id_course_id",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_certificates_enrollment_id",
                table: "certificates");

            migrationBuilder.DropIndex(
                name: "IX_certificates_revoked_by",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "revenue_share_percentage",
                table: "users");

            migrationBuilder.DropColumn(
                name: "flagged_at",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "flagged_by",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "is_flagged",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "enrollment_id",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "revoked_by",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "status",
                table: "certificates");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");
        }
    }
}
