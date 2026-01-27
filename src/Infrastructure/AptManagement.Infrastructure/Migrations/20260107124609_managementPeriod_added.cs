using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class managementPeriod_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncomeDebtAllocation_ApartmentDebts_ApartmentDebtId",
                table: "IncomeDebtAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomeDebtAllocation_Incomes_IncomeId",
                table: "IncomeDebtAllocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IncomeDebtAllocation",
                table: "IncomeDebtAllocation");

            migrationBuilder.RenameTable(
                name: "IncomeDebtAllocation",
                newName: "IncomeDebtAllocations");

            migrationBuilder.RenameIndex(
                name: "IX_IncomeDebtAllocation_IncomeId",
                table: "IncomeDebtAllocations",
                newName: "IX_IncomeDebtAllocations_IncomeId");

            migrationBuilder.RenameIndex(
                name: "IX_IncomeDebtAllocation_ApartmentDebtId",
                table: "IncomeDebtAllocations",
                newName: "IX_IncomeDebtAllocations_ApartmentDebtId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncomeDebtAllocations",
                table: "IncomeDebtAllocations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ManagementPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApartmentId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsExemptFromDues = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ManagementPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementPeriods_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementPeriods_ApartmentId",
                table: "ManagementPeriods",
                column: "ApartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeDebtAllocations_ApartmentDebts_ApartmentDebtId",
                table: "IncomeDebtAllocations",
                column: "ApartmentDebtId",
                principalTable: "ApartmentDebts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeDebtAllocations_Incomes_IncomeId",
                table: "IncomeDebtAllocations",
                column: "IncomeId",
                principalTable: "Incomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncomeDebtAllocations_ApartmentDebts_ApartmentDebtId",
                table: "IncomeDebtAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomeDebtAllocations_Incomes_IncomeId",
                table: "IncomeDebtAllocations");

            migrationBuilder.DropTable(
                name: "ManagementPeriods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IncomeDebtAllocations",
                table: "IncomeDebtAllocations");

            migrationBuilder.RenameTable(
                name: "IncomeDebtAllocations",
                newName: "IncomeDebtAllocation");

            migrationBuilder.RenameIndex(
                name: "IX_IncomeDebtAllocations_IncomeId",
                table: "IncomeDebtAllocation",
                newName: "IX_IncomeDebtAllocation_IncomeId");

            migrationBuilder.RenameIndex(
                name: "IX_IncomeDebtAllocations_ApartmentDebtId",
                table: "IncomeDebtAllocation",
                newName: "IX_IncomeDebtAllocation_ApartmentDebtId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncomeDebtAllocation",
                table: "IncomeDebtAllocation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeDebtAllocation_ApartmentDebts_ApartmentDebtId",
                table: "IncomeDebtAllocation",
                column: "ApartmentDebtId",
                principalTable: "ApartmentDebts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeDebtAllocation_Incomes_IncomeId",
                table: "IncomeDebtAllocation",
                column: "IncomeId",
                principalTable: "Incomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
