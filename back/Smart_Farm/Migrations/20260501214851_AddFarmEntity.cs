using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FarmId",
                table: "CROP",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FARM",
                columns: table => new
                {
                    FarmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_line = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Area_size = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Default_Soil_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FARM", x => x.FarmId);
                    table.ForeignKey(
                        name: "FK_FARM_USERS_Uid",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CROP_FarmId",
                table: "CROP",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FARM_Uid",
                table: "FARM",
                column: "Uid");

            migrationBuilder.AddForeignKey(
                name: "FK_CROP_FARM_FarmId",
                table: "CROP",
                column: "FarmId",
                principalTable: "FARM",
                principalColumn: "FarmId");

            // ═══════════════════════════════════════════════════════════════════
            // Backfill: every user who owns at least one CROP gets a default FARM
            // (لو ماعندوش مزرعة بالاسم ده). Then we link all his orphan crops to it.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
INSERT INTO FARM (Name, Latitude, Longitude, City, Address_line, CreatedAt, Uid)
SELECT  N'مزرعتي الافتراضية',
        u.Latitude,
        u.Longitude,
        u.City_name,
        u.Address_line,
        SYSUTCDATETIME(),
        u.Uid
FROM    USERS u
WHERE   EXISTS (SELECT 1 FROM CROP c WHERE c.Uid = u.Uid AND c.FarmId IS NULL)
  AND   NOT EXISTS (SELECT 1 FROM FARM f WHERE f.Uid = u.Uid AND f.Name = N'مزرعتي الافتراضية');

UPDATE  c
SET     c.FarmId = f.FarmId
FROM    CROP c
JOIN    FARM f ON f.Uid = c.Uid AND f.Name = N'مزرعتي الافتراضية'
WHERE   c.FarmId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CROP_FARM_FarmId",
                table: "CROP");

            migrationBuilder.DropTable(
                name: "FARM");

            migrationBuilder.DropIndex(
                name: "IX_CROP_FarmId",
                table: "CROP");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "CROP");
        }
    }
}
