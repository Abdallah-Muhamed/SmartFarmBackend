using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBelongTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BELONG_TO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BELONG_TO",
                columns: table => new
                {
                    Cid = table.Column<int>(type: "int", nullable: false),
                    Pid = table.Column<int>(type: "int", nullable: false),
                    Harvest_Time = table.Column<DateOnly>(type: "date", nullable: true),
                    Plant_count = table.Column<int>(type: "int", nullable: true),
                    Sow_Time = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BELONG_T__5DA8DDF25F4CF032", x => new { x.Cid, x.Pid });
                    table.ForeignKey(
                        name: "FK__BELONG_TO__Cid__4D94879B",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                    table.ForeignKey(
                        name: "FK__BELONG_TO__Pid__4E88ABD4",
                        column: x => x.Pid,
                        principalTable: "PLANT",
                        principalColumn: "Pid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BELONG_TO_Pid",
                table: "BELONG_TO",
                column: "Pid");
        }
    }
}
