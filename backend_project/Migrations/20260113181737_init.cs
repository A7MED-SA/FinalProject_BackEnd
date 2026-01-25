using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_project.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    position = table.Column<int>(type: "int", nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_type = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    file_size_kb = table.Column<int>(type: "int", nullable: false),
                    download_count = table.Column<int>(type: "int", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    configuration = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    passing_score_percent = table.Column<int>(type: "int", nullable: false),
                    max_attempts = table.Column<int>(type: "int", nullable: true),
                    shuffle_questions = table.Column<bool>(type: "bit", nullable: false),
                    shuffle_options = table.Column<bool>(type: "bit", nullable: false),
                    show_results_immediately = table.Column<bool>(type: "bit", nullable: false),
                    allow_review = table.Column<bool>(type: "bit", nullable: false),
                    available_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    available_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizzes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    profile_picture_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<int>(type: "int", maxLength: 50, nullable: true),
                    nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    provider = table.Column<int>(type: "int", maxLength: 30, nullable: false),
                    provider_video_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    quality = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    duration_seconds = table.Column<int>(type: "int", nullable: false),
                    transcript = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    has_subtitles = table.Column<bool>(type: "bit", nullable: false),
                    skip_intro_seconds = table.Column<int>(type: "int", nullable: true),
                    skip_outro_seconds = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    view_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    points = table.Column<int>(type: "int", nullable: false),
                    explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    granted_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    street_line_1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    street_line_2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    state_province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    contact_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    target = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcements", x => x.id);
                    table.ForeignKey(
                        name: "FK_announcements_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.id);
                    table.ForeignKey(
                        name: "FK_carts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    max_discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    minimum_purchase_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    applicable_to = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    usage_limit = table.Column<int>(type: "int", nullable: true),
                    user_limit_per_user = table.Column<int>(type: "int", nullable: true),
                    times_used = table.Column<int>(type: "int", nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false),
                    valid_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    valid_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.id);
                    table.ForeignKey(
                        name: "FK_coupons_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    course_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    level = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    language = table.Column<int>(type: "int", maxLength: 5, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    is_published = table.Column<bool>(type: "bit", nullable: false),
                    approved_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    archived_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_duration_minutes = table.Column<int>(type: "int", nullable: false),
                    enrollment_count = table.Column<int>(type: "int", nullable: false),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_courses_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_courses_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_courses_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    original_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    size_kb = table.Column<int>(type: "int", nullable: false),
                    entity_type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_files_users_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sender_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_users_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    link_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reporter_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reason = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reports_users_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    refresh_token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    refresh_expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    setting_group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_public = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_system_settings_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    admin_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_requests_users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_logins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_phones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    is_default = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_phones", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_phones_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verification_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    token_type = table.Column<int>(type: "int", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_verification_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "video_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    video_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_edited = table.Column<bool>(type: "bit", nullable: false),
                    likes_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_video_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_video_comments_video_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "video_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_video_comments_videos_video_id",
                        column: x => x.video_id,
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_options_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    billing_address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    subtotal_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    coupon_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    final_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_addresses_billing_address_id",
                        column: x => x.billing_address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cart_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    price_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    certificate_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    verification_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificates", x => x.id);
                    table.ForeignKey(
                        name: "FK_certificates_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_certificates_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coupon_courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coupon_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_coupon_courses_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_coupon_courses_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_learning_outcomes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_learning_outcomes", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_learning_outcomes_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_logs_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "course_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_requirements_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "live_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scheduled_start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    scheduled_end = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actual_start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    actual_end = table.Column<DateTime>(type: "datetime2", nullable: true),
                    meeting_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    meeting_password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    max_attendees = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_recorded = table.Column<bool>(type: "bit", nullable: false),
                    recording_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_live_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_live_sessions_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    helpful_count = table.Column<int>(type: "int", nullable: false),
                    not_helpful_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    moderated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    moderated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_users_moderated_by",
                        column: x => x.moderated_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    position = table.Column<int>(type: "int", nullable: false),
                    is_locked = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_sections_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlists", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlists_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wishlists_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher_request_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    document_type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_request_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_request_documents_teacher_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "teacher_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment_likes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_comment_likes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comment_likes_video_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "video_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coupon_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coupon_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_coupon_usages_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_coupon_usages_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coupon_usages_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    price_at_purchase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<int>(type: "int", maxLength: 3, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    transaction_ref = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    gateway_response = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    course_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enrolled_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_accessed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    progress_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    certificate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    source = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    access_expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_refunded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollments_certificates_certificate_id",
                        column: x => x.certificate_id,
                        principalTable: "certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "live_attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    left_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_live_attendances", x => x.id);
                    table.ForeignKey(
                        name: "FK_live_attendances_live_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "live_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_live_attendances_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review_helpfuls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    review_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_helpful = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_helpfuls", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_helpfuls_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_helpfuls_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "section_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    section_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    is_preview_allowed = table.Column<bool>(type: "bit", nullable: false),
                    is_mandatory = table.Column<bool>(type: "bit", nullable: false),
                    available_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    available_until = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_section_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_section_items_documents_item_id",
                        column: x => x.item_id,
                        principalTable: "documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_section_items_live_sessions_item_id",
                        column: x => x.item_id,
                        principalTable: "live_sessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_section_items_quizzes_item_id",
                        column: x => x.item_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_section_items_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_section_items_videos_item_id",
                        column: x => x.item_id,
                        principalTable: "videos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    requested_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.id);
                    table.ForeignKey(
                        name: "FK_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_refunds_users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_logs_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    content_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_completed = table.Column<bool>(type: "bit", nullable: false),
                    watch_time_seconds = table.Column<int>(type: "int", nullable: false),
                    attempts_count = table.Column<int>(type: "int", nullable: false),
                    completion_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_accessed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_progresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_progresses_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    max_score = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    attempt_number = table.Column<int>(type: "int", nullable: false),
                    time_taken_seconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    selected_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    points_earned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_answers_options_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_answers_quiz_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_user_id",
                table: "activity_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_addresses_user_id",
                table: "addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_announcements_created_by",
                table: "announcements",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_cart_id",
                table: "cart_items",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_course_id",
                table: "cart_items",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_carts_user_id",
                table: "carts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_certificates_course_id",
                table: "certificates",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_user_id",
                table: "certificates",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_verification_code",
                table: "certificates",
                column: "verification_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comment_likes_comment_id",
                table: "comment_likes",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_likes_user_id",
                table: "comment_likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_progresses_enrollment_id",
                table: "content_progresses",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_courses_coupon_id",
                table: "coupon_courses",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_courses_course_id",
                table: "coupon_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_usages_coupon_id",
                table: "coupon_usages",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_usages_order_id",
                table: "coupon_usages",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_usages_user_id",
                table: "coupon_usages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupons_code",
                table: "coupons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coupons_created_by",
                table: "coupons",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_course_learning_outcomes_course_id",
                table: "course_learning_outcomes",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_logs_course_id",
                table: "course_logs",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_logs_user_id",
                table: "course_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_requirements_course_id",
                table: "course_requirements",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_approved_by",
                table: "courses",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_courses_category_id",
                table: "courses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_created_by",
                table: "courses",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_courses_slug",
                table: "courses",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_certificate_id",
                table: "enrollments",
                column: "certificate_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_course_id",
                table: "enrollments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_user_id",
                table: "enrollments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_uploaded_by",
                table: "files",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_live_attendances_session_id",
                table: "live_attendances",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_live_attendances_user_id",
                table: "live_attendances",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_live_sessions_course_id",
                table: "live_sessions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_receiver_id",
                table: "messages",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_options_question_id",
                table: "options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_course_id",
                table: "order_items",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_billing_address_id",
                table: "orders",
                column: "billing_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_coupon_id",
                table: "orders",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payment_method_id",
                table: "payments",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction_ref",
                table: "payments",
                column: "transaction_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_user_id",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_questions_quiz_id",
                table: "questions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_enrollment_id",
                table: "quiz_attempts",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_processed_by",
                table: "refunds",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id",
                table: "reports",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_resolved_by",
                table: "reports",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "IX_review_helpfuls_review_id",
                table: "review_helpfuls",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_helpfuls_user_id",
                table: "review_helpfuls",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_course_id",
                table: "reviews",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_moderated_by",
                table: "reviews",
                column: "moderated_by");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_RoleId",
                table: "role_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_section_items_item_id",
                table: "section_items",
                column: "item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_section_items_section_id",
                table: "section_items",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_sections_course_id",
                table: "sections",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_refresh_token_hash",
                table: "sessions",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_token_hash",
                table: "sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_updated_by",
                table: "system_settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_request_documents_request_id",
                table: "teacher_request_documents",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_requests_processed_by",
                table: "teacher_requests",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_requests_user_id",
                table: "teacher_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_logs_payment_id",
                table: "transaction_logs",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_attempt_id",
                table: "user_answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_question_id",
                table: "user_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_selected_option_id",
                table: "user_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_UserId",
                table: "user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_UserId",
                table: "user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_phones_user_id",
                table: "user_phones",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_verification_tokens_token_hash",
                table: "verification_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationTokens_UserId_TokenType",
                table: "verification_tokens",
                columns: new[] { "user_id", "token_type" });

            migrationBuilder.CreateIndex(
                name: "IX_video_comments_parent_comment_id",
                table: "video_comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_video_comments_user_id",
                table: "video_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_video_comments_video_id",
                table: "video_comments",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_course_id",
                table: "wishlists",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_user_id",
                table: "wishlists",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "comment_likes");

            migrationBuilder.DropTable(
                name: "content_progresses");

            migrationBuilder.DropTable(
                name: "coupon_courses");

            migrationBuilder.DropTable(
                name: "coupon_usages");

            migrationBuilder.DropTable(
                name: "course_learning_outcomes");

            migrationBuilder.DropTable(
                name: "course_logs");

            migrationBuilder.DropTable(
                name: "course_requirements");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "live_attendances");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "review_helpfuls");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "section_items");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "teacher_request_documents");

            migrationBuilder.DropTable(
                name: "transaction_logs");

            migrationBuilder.DropTable(
                name: "user_answers");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_phones");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "verification_tokens");

            migrationBuilder.DropTable(
                name: "wishlists");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "video_comments");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "live_sessions");

            migrationBuilder.DropTable(
                name: "sections");

            migrationBuilder.DropTable(
                name: "teacher_requests");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "options");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
