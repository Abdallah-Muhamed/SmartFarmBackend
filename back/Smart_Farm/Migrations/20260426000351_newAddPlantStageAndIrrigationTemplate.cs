using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class newAddPlantStageAndIrrigationTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration_days",
                table: "IRRIGATION_STAGE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PSid",
                table: "IRRIGATION_STAGE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PTid",
                table: "IRRIGATION",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PLANT_STAGE",
                columns: table => new
                {
                    PSid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pid = table.Column<int>(type: "int", nullable: true),
                    Name_stage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stage_order = table.Column<int>(type: "int", nullable: true),
                    Duration_days = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLANT_STAGE", x => x.PSid);
                    table.ForeignKey(
                        name: "FK_PLANT_STAGE_PLANT_Pid",
                        column: x => x.Pid,
                        principalTable: "PLANT",
                        principalColumn: "Pid");
                });

            migrationBuilder.CreateTable(
                name: "PLANT_IRRIGATION_TEMPLATE",
                columns: table => new
                {
                    PTid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PSid = table.Column<int>(type: "int", nullable: true),
                    Pid = table.Column<int>(type: "int", nullable: true),
                    Irrigation_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Water_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Frequency_value = table.Column<int>(type: "int", nullable: true),
                    Frequency_unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLANT_IRRIGATION_TEMPLATE", x => x.PTid);
                    table.ForeignKey(
                        name: "FK_PLANT_IRRIGATION_TEMPLATE_PLANT_Pid",
                        column: x => x.Pid,
                        principalTable: "PLANT",
                        principalColumn: "Pid");
                    table.ForeignKey(
                        name: "FK_PLANT_IRRIGATION_TEMPLATE_PLANT_STAGE_PSid",
                        column: x => x.PSid,
                        principalTable: "PLANT_STAGE",
                        principalColumn: "PSid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IRRIGATION_STAGE_PSid",
                table: "IRRIGATION_STAGE",
                column: "PSid");

            migrationBuilder.CreateIndex(
                name: "IX_IRRIGATION_PTid",
                table: "IRRIGATION",
                column: "PTid");

            migrationBuilder.CreateIndex(
                name: "IX_PLANT_IRRIGATION_TEMPLATE_Pid",
                table: "PLANT_IRRIGATION_TEMPLATE",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_PLANT_IRRIGATION_TEMPLATE_PSid",
                table: "PLANT_IRRIGATION_TEMPLATE",
                column: "PSid");

            migrationBuilder.CreateIndex(
                name: "IX_PLANT_STAGE_Pid",
                table: "PLANT_STAGE",
                column: "Pid");

            migrationBuilder.AddForeignKey(
                name: "FK_IRRIGATION_PLANT_IRRIGATION_TEMPLATE_PTid",
                table: "IRRIGATION",
                column: "PTid",
                principalTable: "PLANT_IRRIGATION_TEMPLATE",
                principalColumn: "PTid");

            migrationBuilder.AddForeignKey(
                name: "FK_IRRIGATION_STAGE_PLANT_STAGE_PSid",
                table: "IRRIGATION_STAGE",
                column: "PSid",
                principalTable: "PLANT_STAGE",
                principalColumn: "PSid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IRRIGATION_PLANT_IRRIGATION_TEMPLATE_PTid",
                table: "IRRIGATION");

            migrationBuilder.DropForeignKey(
                name: "FK_IRRIGATION_STAGE_PLANT_STAGE_PSid",
                table: "IRRIGATION_STAGE");

            migrationBuilder.DropTable(
                name: "PLANT_IRRIGATION_TEMPLATE");

            migrationBuilder.DropTable(
                name: "PLANT_STAGE");

            migrationBuilder.DropIndex(
                name: "IX_IRRIGATION_STAGE_PSid",
                table: "IRRIGATION_STAGE");

            migrationBuilder.DropIndex(
                name: "IX_IRRIGATION_PTid",
                table: "IRRIGATION");

            migrationBuilder.DropColumn(
                name: "Duration_days",
                table: "IRRIGATION_STAGE");

            migrationBuilder.DropColumn(
                name: "PSid",
                table: "IRRIGATION_STAGE");

            migrationBuilder.DropColumn(
                name: "PTid",
                table: "IRRIGATION");
        }
    }
}
