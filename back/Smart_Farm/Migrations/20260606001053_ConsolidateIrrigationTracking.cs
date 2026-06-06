using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateIrrigationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CROP_WATER_BALANCE_LOG_Cid",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.AddColumn<int>(
                name: "Cid",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedAt",
                table: "CROP_WATER_BALANCE_LOG",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Applied_mm",
                table: "CROP_WATER_BALANCE_LOG",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeplAfterEt_mm",
                table: "CROP_WATER_BALANCE_LOG",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsIrrigationDay",
                table: "CROP_WATER_BALANCE_LOG",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Recommended_Liters",
                table: "CROP_WATER_BALANCE_LOG",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageName",
                table: "CROP_WATER_BALANCE_LOG",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasApplied",
                table: "CROP_WATER_BALANCE_LOG",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_Cid",
                table: "Task",
                column: "Cid");

            migrationBuilder.Sql("""
                ;WITH dups AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY Cid, [Date] ORDER BY Id DESC) AS rn
                    FROM CROP_WATER_BALANCE_LOG
                )
                DELETE FROM CROP_WATER_BALANCE_LOG
                WHERE Id IN (SELECT Id FROM dups WHERE rn > 1);

                UPDATE CROP_WATER_BALANCE_LOG
                SET IsIrrigationDay = CASE WHEN ISNULL(Irrig_mm, 0) > 0 THEN 1 ELSE 0 END
                WHERE IsIrrigationDay = 0;

                UPDATE CROP_WATER_BALANCE_LOG
                SET DeplAfterEt_mm = DeplStart_mm
                WHERE DeplAfterEt_mm IS NULL AND DeplStart_mm IS NOT NULL;

                UPDATE t
                SET
                    t.Kc = s6.Kc,
                    t.p_fraction = s6.p_fraction,
                    t.Zr_m = s6.Zr_m
                FROM PLANT_IRRIGATION_TEMPLATE t
                INNER JOIN PLANT_STAGE ps ON t.PSid = ps.PSid
                INNER JOIN PLANT_STAGE ps6 ON ps6.Pid = ps.Pid AND ps6.Stage_order = 6
                INNER JOIN PLANT_IRRIGATION_TEMPLATE s6 ON s6.PSid = ps6.PSid
                WHERE ps.Stage_order = 7
                  AND (t.Kc IS NULL OR t.p_fraction IS NULL OR t.Zr_m IS NULL);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CROP_WATER_BALANCE_LOG_Cid_Date",
                table: "CROP_WATER_BALANCE_LOG",
                columns: new[] { "Cid", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_CROP_Cid",
                table: "Task",
                column: "Cid",
                principalTable: "CROP",
                principalColumn: "Cid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_CROP_Cid",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_Cid",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_CROP_WATER_BALANCE_LOG_Cid_Date",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "Cid",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "AppliedAt",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "Applied_mm",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "DeplAfterEt_mm",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "IsIrrigationDay",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "Recommended_Liters",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "StageName",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "WasApplied",
                table: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.CreateIndex(
                name: "IX_CROP_WATER_BALANCE_LOG_Cid",
                table: "CROP_WATER_BALANCE_LOG",
                column: "Cid");
        }
    }
}
