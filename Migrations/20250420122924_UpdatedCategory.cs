using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWSERVER.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryIcon",
                table: "Category",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryIcon",
                table: "Category");
        }
    }
}
