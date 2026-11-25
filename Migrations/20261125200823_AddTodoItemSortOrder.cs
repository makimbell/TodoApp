using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoItemSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "todo",
                table: "TodoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                WITH Ranked AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY TodoListId ORDER BY IsCompleted, CreatedAt DESC) - 1 AS NewOrder
                    FROM [todo].[TodoItems]
                )
                UPDATE t
                SET t.SortOrder = r.NewOrder
                FROM [todo].[TodoItems] t
                INNER JOIN Ranked r ON t.Id = r.Id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "todo",
                table: "TodoItems");
        }
    }
}
