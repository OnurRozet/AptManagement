using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class change_relaiton_in_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Apartments_ApartmentId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "IncomeIncomeCategory");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ApartmentId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "Expenses");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentDebtId",
                table: "Incomes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "ApartmentDebts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_ApartmentDebtId",
                table: "Incomes",
                column: "ApartmentDebtId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_IncomeCategoryId",
                table: "Incomes",
                column: "IncomeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentDebts_ApartmentId",
                table: "ApartmentDebts",
                column: "ApartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApartmentDebts_Apartments_ApartmentId",
                table: "ApartmentDebts",
                column: "ApartmentId",
                principalTable: "Apartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_ApartmentDebts_ApartmentDebtId",
                table: "Incomes",
                column: "ApartmentDebtId",
                principalTable: "ApartmentDebts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_IncomeCategories_IncomeCategoryId",
                table: "Incomes",
                column: "IncomeCategoryId",
                principalTable: "IncomeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApartmentDebts_Apartments_ApartmentId",
                table: "ApartmentDebts");

            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_ApartmentDebts_ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_IncomeCategories_IncomeCategoryId",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_IncomeCategoryId",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_ApartmentDebts_ApartmentId",
                table: "ApartmentDebts");

            migrationBuilder.DropColumn(
                name: "ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "ApartmentDebts");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentId",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncomeIncomeCategory",
                columns: table => new
                {
                    IncomeCategoriesId = table.Column<int>(type: "int", nullable: false),
                    IncomesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeIncomeCategory", x => new { x.IncomeCategoriesId, x.IncomesId });
                    table.ForeignKey(
                        name: "FK_IncomeIncomeCategory_IncomeCategories_IncomeCategoriesId",
                        column: x => x.IncomeCategoriesId,
                        principalTable: "IncomeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomeIncomeCategory_Incomes_IncomesId",
                        column: x => x.IncomesId,
                        principalTable: "Incomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApartmentId",
                table: "Expenses",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeIncomeCategory_IncomesId",
                table: "IncomeIncomeCategory",
                column: "IncomesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Apartments_ApartmentId",
                table: "Expenses",
                column: "ApartmentId",
                principalTable: "Apartments",
                principalColumn: "Id");
        }
    }
}
