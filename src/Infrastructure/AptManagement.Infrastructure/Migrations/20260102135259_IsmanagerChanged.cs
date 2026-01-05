using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsmanagerChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "IncomeCategories");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "DuesSettings");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "ApartmentDebts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "Incomes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "IncomeCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "Expenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "ExpenseCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "DuesSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "ApartmentDebts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
