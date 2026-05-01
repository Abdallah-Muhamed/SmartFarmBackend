using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class AddWaterBalanceLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Kc",
                table: "PLANT_IRRIGATION_TEMPLATE",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Zr_m",
                table: "PLANT_IRRIGATION_TEMPLATE",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "p_fraction",
                table: "PLANT_IRRIGATION_TEMPLATE",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Depletion_mm",
                table: "CROP",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastBalanceDate",
                table: "CROP",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CROP_WATER_BALANCE_LOG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cid = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ET0_mm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Kc = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    ETc_mm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    EffRain_mm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Irrig_mm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    DeplStart_mm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    DeplEnd_mm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    TAW_mm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    RAW_mm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CROP_WATER_BALANCE_LOG", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CROP_WATER_BALANCE_LOG_CROP_Cid",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CROP_WATER_BALANCE_LOG_Cid",
                table: "CROP_WATER_BALANCE_LOG",
                column: "Cid");

            // ═══════════════════════════════════════════════════════════════════
            // Seed FAO-56 Kc / p / Zr for every existing PLANT_IRRIGATION_TEMPLATE
            // row, looked up by the parent PLANT.Name and PLANT_STAGE.Stage_order.
            // Stage_order mapping (all crops in the knowledge base use the same 6-
            // step lifecycle except lettuce/wheat which use 5):
            //   1 = germination / seedling bed
            //   2 = seedling / transplant
            //   3 = vegetative / tillering
            //   4 = flowering / booting
            //   5 = fruit / grain development
            //   6 = ripening / harvest
            // Values are FAO-56 Table 12 midpoints adapted for Egyptian conditions.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
;WITH StageMap (PlantAliases, StageOrder, Kc, p_fraction, Zr_m) AS (
    SELECT * FROM (VALUES
        -- Tomato
        (N'Tomato|طماطم', 1, 0.60, 0.40, 0.15),
        (N'Tomato|طماطم', 2, 0.70, 0.40, 0.25),
        (N'Tomato|طماطم', 3, 1.00, 0.40, 0.50),
        (N'Tomato|طماطم', 4, 1.15, 0.40, 0.80),
        (N'Tomato|طماطم', 5, 1.15, 0.40, 1.00),
        (N'Tomato|طماطم', 6, 0.80, 0.50, 1.00),

        -- Wheat
        (N'Wheat|قمح', 1, 0.40, 0.55, 0.15),
        (N'Wheat|قمح', 2, 0.70, 0.55, 0.40),
        (N'Wheat|قمح', 3, 1.15, 0.55, 0.80),
        (N'Wheat|قمح', 4, 1.15, 0.55, 1.20),
        (N'Wheat|قمح', 5, 1.00, 0.55, 1.50),
        (N'Wheat|قمح', 6, 0.40, 0.60, 1.50),

        -- Corn / Maize
        (N'Corn|Maize|ذرة', 1, 0.40, 0.55, 0.15),
        (N'Corn|Maize|ذرة', 2, 0.70, 0.55, 0.40),
        (N'Corn|Maize|ذرة', 3, 1.15, 0.55, 0.80),
        (N'Corn|Maize|ذرة', 4, 1.20, 0.55, 1.00),
        (N'Corn|Maize|ذرة', 5, 1.20, 0.55, 1.20),
        (N'Corn|Maize|ذرة', 6, 0.60, 0.60, 1.20),

        -- Potato
        (N'Potato|بطاطس', 1, 0.50, 0.35, 0.15),
        (N'Potato|بطاطس', 2, 0.70, 0.35, 0.30),
        (N'Potato|بطاطس', 3, 1.00, 0.35, 0.45),
        (N'Potato|بطاطس', 4, 1.15, 0.35, 0.50),
        (N'Potato|بطاطس', 5, 1.15, 0.35, 0.60),
        (N'Potato|بطاطس', 6, 0.75, 0.45, 0.60),

        -- Cucumber
        (N'Cucumber|خيار', 1, 0.60, 0.50, 0.15),
        (N'Cucumber|خيار', 2, 0.70, 0.50, 0.25),
        (N'Cucumber|خيار', 3, 1.00, 0.50, 0.50),
        (N'Cucumber|خيار', 4, 1.00, 0.50, 0.70),
        (N'Cucumber|خيار', 5, 1.00, 0.50, 0.80),
        (N'Cucumber|خيار', 6, 0.75, 0.55, 0.80),

        -- Watermelon
        (N'Watermelon|بطيخ', 1, 0.40, 0.40, 0.15),
        (N'Watermelon|بطيخ', 2, 0.70, 0.40, 0.25),
        (N'Watermelon|بطيخ', 3, 1.00, 0.40, 0.50),
        (N'Watermelon|بطيخ', 4, 1.00, 0.40, 0.80),
        (N'Watermelon|بطيخ', 5, 1.00, 0.40, 1.00),
        (N'Watermelon|بطيخ', 6, 0.75, 0.50, 1.00),

        -- Onion
        (N'Onion|بصل', 1, 0.70, 0.30, 0.15),
        (N'Onion|بصل', 2, 0.70, 0.30, 0.25),
        (N'Onion|بصل', 3, 1.05, 0.30, 0.30),
        (N'Onion|بصل', 4, 1.05, 0.30, 0.35),
        (N'Onion|بصل', 5, 1.00, 0.30, 0.40),
        (N'Onion|بصل', 6, 0.80, 0.40, 0.40),

        -- Pepper
        (N'Pepper|فلفل', 1, 0.60, 0.30, 0.15),
        (N'Pepper|فلفل', 2, 0.70, 0.30, 0.25),
        (N'Pepper|فلفل', 3, 1.00, 0.30, 0.50),
        (N'Pepper|فلفل', 4, 1.05, 0.30, 0.70),
        (N'Pepper|فلفل', 5, 1.05, 0.30, 0.80),
        (N'Pepper|فلفل', 6, 0.90, 0.40, 0.80),

        -- Eggplant
        (N'Eggplant|Aubergine|باذنجان', 1, 0.60, 0.40, 0.15),
        (N'Eggplant|Aubergine|باذنجان', 2, 0.70, 0.40, 0.25),
        (N'Eggplant|Aubergine|باذنجان', 3, 1.00, 0.40, 0.50),
        (N'Eggplant|Aubergine|باذنجان', 4, 1.10, 0.40, 0.80),
        (N'Eggplant|Aubergine|باذنجان', 5, 1.10, 0.40, 1.00),
        (N'Eggplant|Aubergine|باذنجان', 6, 0.90, 0.50, 1.00),

        -- Lettuce (5 stages)
        (N'Lettuce|خس', 1, 0.70, 0.30, 0.10),
        (N'Lettuce|خس', 2, 0.70, 0.30, 0.20),
        (N'Lettuce|خس', 3, 1.00, 0.30, 0.30),
        (N'Lettuce|خس', 4, 1.00, 0.30, 0.30),
        (N'Lettuce|خس', 5, 0.95, 0.30, 0.30),

        -- Rice (flooded, kept for completeness)
        (N'Rice|أرز', 1, 1.05, 0.20, 0.20),
        (N'Rice|أرز', 2, 1.10, 0.20, 0.40),
        (N'Rice|أرز', 3, 1.20, 0.20, 0.60),
        (N'Rice|أرز', 4, 1.20, 0.20, 0.60),
        (N'Rice|أرز', 5, 1.20, 0.20, 0.60),
        (N'Rice|أرز', 6, 0.90, 0.20, 0.60)
    ) AS v(PlantAliases, StageOrder, Kc, p_fraction, Zr_m)
)
UPDATE pt
SET    pt.Kc         = sm.Kc,
       pt.p_fraction = sm.p_fraction,
       pt.Zr_m       = sm.Zr_m
FROM   PLANT_IRRIGATION_TEMPLATE pt
JOIN   PLANT_STAGE ps ON ps.PSid = pt.PSid
JOIN   PLANT        p ON p.Pid   = ps.Pid
JOIN   StageMap    sm ON sm.StageOrder = ps.Stage_order
                      AND CHARINDEX(N'|' + p.Name + N'|', N'|' + sm.PlantAliases + N'|') > 0
WHERE  pt.Kc IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CROP_WATER_BALANCE_LOG");

            migrationBuilder.DropColumn(
                name: "Kc",
                table: "PLANT_IRRIGATION_TEMPLATE");

            migrationBuilder.DropColumn(
                name: "Zr_m",
                table: "PLANT_IRRIGATION_TEMPLATE");

            migrationBuilder.DropColumn(
                name: "p_fraction",
                table: "PLANT_IRRIGATION_TEMPLATE");

            migrationBuilder.DropColumn(
                name: "Depletion_mm",
                table: "CROP");

            migrationBuilder.DropColumn(
                name: "LastBalanceDate",
                table: "CROP");
        }
    }
}
