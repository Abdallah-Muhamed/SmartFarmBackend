using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrderTableUseSellerUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerAddress",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "BuyerCity",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "SellerAddress",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "SellerCity",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "SellerName",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "SellerPhone",
                table: "ORDERS");

            migrationBuilder.AddColumn<int>(
                name: "SellerUid",
                table: "ORDERS",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_SellerUid",
                table: "ORDERS",
                column: "SellerUid");

            migrationBuilder.AddForeignKey(
                name: "FK_ORDERS_USERS_SellerUid",
                table: "ORDERS",
                column: "SellerUid",
                principalTable: "USERS",
                principalColumn: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ORDERS_USERS_SellerUid",
                table: "ORDERS");

            migrationBuilder.DropIndex(
                name: "IX_ORDERS_SellerUid",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "SellerUid",
                table: "ORDERS");

            migrationBuilder.AddColumn<string>(
                name: "BuyerAddress",
                table: "ORDERS",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerCity",
                table: "ORDERS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "ORDERS",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerAddress",
                table: "ORDERS",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerCity",
                table: "ORDERS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                table: "ORDERS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPhone",
                table: "ORDERS",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
