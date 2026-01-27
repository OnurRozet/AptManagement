using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class incomeDebtAllocation_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_ApartmentDebts_ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "ApartmentDebtId",
                table: "Incomes");

            migrationBuilder.CreateTable(
                name: "IncomeDebtAllocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomeId = table.Column<int>(type: "int", nullable: false),
                    ApartmentDebtId = table.Column<int>(type: "int", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeDebtAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeDebtAllocation_ApartmentDebts_ApartmentDebtId",
                        column: x => x.ApartmentDebtId,
                        principalTable: "ApartmentDebts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_IncomeDebtAllocation_Incomes_IncomeId",
                        column: x => x.IncomeId,
                        principalTable: "Incomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeDebtAllocation_ApartmentDebtId",
                table: "IncomeDebtAllocation",
                column: "ApartmentDebtId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeDebtAllocation_IncomeId",
                table: "IncomeDebtAllocation",
                column: "IncomeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomeDebtAllocation");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentDebtId",
                table: "Incomes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_ApartmentDebtId",
                table: "Incomes",
                column: "ApartmentDebtId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_ApartmentDebts_ApartmentDebtId",
                table: "Incomes",
                column: "ApartmentDebtId",
                principalTable: "ApartmentDebts",
                principalColumn: "Id");
        }
    }
}
