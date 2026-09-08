using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_tasks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "name", "password_hash" },
                values: new object[] { new Guid("29ed1f5b-d6f4-4f06-8d8b-54089a0f2d13"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@taskmanager.com", "Demo User", "100000.VGFza01hbmFnZXJEZW1vUw==.q6A5uUNkKGibGn/j9tDTF2XcnhUTYr2dwKtB761KA88=" });

            migrationBuilder.InsertData(
                table: "tasks",
                columns: new[] { "id", "created_at", "description", "due_date", "status", "title", "updated_at", "user_id" },
                values: new object[,]
                {
                    { new Guid("312b1257-39f7-4ca6-a40b-4db4f934ce3f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Check dependency boundaries before the interview.", new DateOnly(2026, 12, 31), "Pending", "Review Clean Architecture notes", null, new Guid("29ed1f5b-d6f4-4f06-8d8b-54089a0f2d13") },
                    { new Guid("9657b12d-8aac-4f60-9c9d-b4aa546582d8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Package the solution and README.", new DateOnly(2026, 12, 31), "Done", "Submit technical exercise", null, new Guid("29ed1f5b-d6f4-4f06-8d8b-54089a0f2d13") },
                    { new Guid("f34f5b63-954d-40b4-aa90-af2174af5551"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create a concise walkthrough of the architecture and tests.", new DateOnly(2026, 12, 31), "InProgress", "Prepare project presentation", null, new Guid("29ed1f5b-d6f4-4f06-8d8b-54089a0f2d13") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_user_id",
                table: "tasks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
