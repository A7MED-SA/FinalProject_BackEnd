using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryImageFileRelation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "categories");

            migrationBuilder.AddColumn<Guid>(
                name: "category_image_file_id",
                table: "categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_category_image_file_id",
                table: "categories",
                column: "category_image_file_id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_files_category_image_file_id",
                table: "categories",
                column: "category_image_file_id",
                principalTable: "files",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_files_category_image_file_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_category_image_file_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "category_image_file_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "categories");

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
