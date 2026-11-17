using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class AddListOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                schema: "todo",
                table: "TodoLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_OwnerId_CreatedAt",
                schema: "todo",
                table: "TodoLists",
                columns: new[] { "OwnerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoLists_OwnerId_CreatedAt",
                schema: "todo",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "todo",
                table: "TodoLists");
        }
    }
}
