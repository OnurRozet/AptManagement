using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class transfferDebt_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "Apartments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DebtType",
                table: "ApartmentDebts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 1,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 2,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 3,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 4,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 5,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 6,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 7,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 8,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 9,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 10,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 11,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 12,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 13,
                column: "OpeningBalance",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Apartments",
                keyColumn: "Id",
                keyValue: 14,
                column: "OpeningBalance",
                value: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "DebtType",
                table: "ApartmentDebts");
        }
    }
}
