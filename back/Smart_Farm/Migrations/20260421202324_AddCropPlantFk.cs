using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class AddCropPlantFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Pid",
                table: "CROP",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CROP_Pid",
                table: "CROP",
                column: "Pid");

            migrationBuilder.AddForeignKey(
                name: "FK_CROP_PLANT_Pid",
                table: "CROP",
                column: "Pid",
                principalTable: "PLANT",
                principalColumn: "Pid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CROP_PLANT_Pid",
                table: "CROP");

            migrationBuilder.DropIndex(
                name: "IX_CROP_Pid",
                table: "CROP");

            migrationBuilder.DropColumn(
                name: "Pid",
                table: "CROP");
        }
    }
}
