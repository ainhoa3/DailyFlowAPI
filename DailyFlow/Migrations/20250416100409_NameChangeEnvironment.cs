using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyFlow.Migrations
{
    /// <inheritdoc />
    public partial class NameChangeEnvironment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Environment",
                table: "Tasks",
                newName: "_Environment");

            migrationBuilder.RenameColumn(
                name: "Environment",
                table: "Habits",
                newName: "_Environment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "_Environment",
                table: "Tasks",
                newName: "Environment");

            migrationBuilder.RenameColumn(
                name: "_Environment",
                table: "Habits",
                newName: "Environment");
        }
    }
}
