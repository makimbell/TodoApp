using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class FixTodoListsName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoList_TodoListId",
                schema: "todo",
                table: "TodoItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoList",
                schema: "todo",
                table: "TodoList");

            migrationBuilder.RenameTable(
                name: "TodoList",
                schema: "todo",
                newName: "TodoLists",
                newSchema: "todo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoLists",
                schema: "todo",
                table: "TodoLists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_Slug",
                schema: "todo",
                table: "TodoLists",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoLists_TodoListId",
                schema: "todo",
                table: "TodoItems",
                column: "TodoListId",
                principalSchema: "todo",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoLists_TodoListId",
                schema: "todo",
                table: "TodoItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoLists",
                schema: "todo",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_Slug",
                schema: "todo",
                table: "TodoLists");

            migrationBuilder.RenameTable(
                name: "TodoLists",
                schema: "todo",
                newName: "TodoList",
                newSchema: "todo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoList",
                schema: "todo",
                table: "TodoList",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoList_TodoListId",
                schema: "todo",
                table: "TodoItems",
                column: "TodoListId",
                principalSchema: "todo",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
