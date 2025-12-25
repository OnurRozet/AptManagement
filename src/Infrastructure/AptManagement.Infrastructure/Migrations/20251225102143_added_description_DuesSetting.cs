using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_description_DuesSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DuesSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DuesSettings");
        }
    }
}
