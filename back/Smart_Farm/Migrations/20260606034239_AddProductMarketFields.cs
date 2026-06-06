using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMarketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cid",
                table: "PRODUCT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FarmId",
                table: "PRODUCT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "PRODUCT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_Cid",
                table: "PRODUCT",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_FarmId",
                table: "PRODUCT",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCT_CROP_Cid",
                table: "PRODUCT",
                column: "Cid",
                principalTable: "CROP",
                principalColumn: "Cid");

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCT_FARM_FarmId",
                table: "PRODUCT",
                column: "FarmId",
                principalTable: "FARM",
                principalColumn: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCT_CROP_Cid",
                table: "PRODUCT");

            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCT_FARM_FarmId",
                table: "PRODUCT");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCT_Cid",
                table: "PRODUCT");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCT_FarmId",
                table: "PRODUCT");

            migrationBuilder.DropColumn(
                name: "Cid",
                table: "PRODUCT");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "PRODUCT");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "PRODUCT");
        }
    }
}
