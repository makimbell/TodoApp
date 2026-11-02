using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoItemIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_TodoListId",
                schema: "todo",
                table: "TodoItems");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_TodoListId_IsCompleted_CreatedAt",
                schema: "todo",
                table: "TodoItems",
                columns: new[] { "TodoListId", "IsCompleted", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_TodoListId_IsCompleted_CreatedAt",
                schema: "todo",
                table: "TodoItems");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_TodoListId",
                schema: "todo",
                table: "TodoItems",
                column: "TodoListId");
        }
    }
}
