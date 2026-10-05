using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoList.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_idempotency_key = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.CheckConstraint("ck_tasks_completion", "(status = 'completed' AND completed_at IS NOT NULL) OR (status <> 'completed' AND completed_at IS NULL)");
                    table.CheckConstraint("ck_tasks_description_length", "description IS NULL OR char_length(description) <= 10000");
                    table.CheckConstraint("ck_tasks_priority", "priority IN ('none', 'low', 'medium', 'high', 'urgent')");
                    table.CheckConstraint("ck_tasks_status", "status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                    table.CheckConstraint("ck_tasks_title_length", "char_length(title) BETWEEN 1 AND 200");
                    table.CheckConstraint("ck_tasks_updated_at", "updated_at >= created_at");
                });

            migrationBuilder.CreateTable(
                name: "task_status_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_status_events", x => x.id);
                    table.CheckConstraint("ck_task_status_events_changed", "from_status <> to_status");
                    table.CheckConstraint("ck_task_status_events_from_status", "from_status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                    table.CheckConstraint("ck_task_status_events_to_status", "to_status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                    table.ForeignKey(
                        name: "fk_task_status_events_tasks",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_task_status_events_task_occurred_at",
                table: "task_status_events",
                columns: new[] { "task_id", "occurred_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_completed_at",
                table: "tasks",
                column: "completed_at",
                filter: "status = 'completed'");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_status_created_at",
                table: "tasks",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_tasks_creation_idempotency_key",
                table: "tasks",
                column: "creation_idempotency_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_status_events");

            migrationBuilder.DropTable(
                name: "tasks");
        }
    }
}
