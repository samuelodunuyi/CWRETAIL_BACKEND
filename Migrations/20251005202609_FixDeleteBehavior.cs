using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW_RETAIL.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Stores_StoreId",
                table: "Recipes");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Stores_StoreId",
                table: "Recipes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "StoreId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Stores_StoreId",
                table: "Recipes");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Stores_StoreId",
                table: "Recipes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "StoreId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
