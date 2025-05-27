using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWSERVER.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDBAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreRep",
                table: "Store",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreRep",
                table: "Store");
        }
    }
}
