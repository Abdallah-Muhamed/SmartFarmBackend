using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Disease",
                columns: table => new
                {
                    Did = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Nause = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Symptoms = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Treatment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disease__C03122188275DDE5", x => x.Did);
                });

            migrationBuilder.CreateTable(
                name: "FERTILIZER",
                columns: table => new
                {
                    FrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Unit = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Method = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Fertilizer_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FERTILIZ__32B46715A38ED2B3", x => x.FrId);
                });

            migrationBuilder.CreateTable(
                name: "PLANT",
                columns: table => new
                {
                    Pid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Seed_type = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Fertilizer_need = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Days_to_harvest = table.Column<int>(type: "int", nullable: true),
                    Season = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Humidity_range = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Water_need = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Soil_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Temperature_range = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PLANT__C5705938049F0DBC", x => x.Pid);
                });

            migrationBuilder.CreateTable(
                name: "SEASON",
                columns: table => new
                {
                    Sid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SEASON__CA1E5D786783D710", x => x.Sid);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Last_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Address_line = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    City_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    Role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PasswordHashed = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USERS__C5B69A4A9CA97C86", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "COMPATIBILITY",
                columns: table => new
                {
                    FrId = table.Column<int>(type: "int", nullable: false),
                    Pid = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__COMPATIB__AEE362867FBB5969", x => new { x.FrId, x.Pid });
                    table.ForeignKey(
                        name: "FK__COMPATIBILI__Pid__5BE2A6F2",
                        column: x => x.Pid,
                        principalTable: "PLANT",
                        principalColumn: "Pid");
                    table.ForeignKey(
                        name: "FK__COMPATIBIL__FrId__5AEE82B9",
                        column: x => x.FrId,
                        principalTable: "FERTILIZER",
                        principalColumn: "FrId");
                });

            migrationBuilder.CreateTable(
                name: "GROWS_IN",
                columns: table => new
                {
                    Pid = table.Column<int>(type: "int", nullable: false),
                    Sid = table.Column<int>(type: "int", nullable: false),
                    Plant_in_season_description = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GROWS_IN__59D1BCEFD9462B22", x => new { x.Pid, x.Sid });
                    table.ForeignKey(
                        name: "FK__GROWS_IN__Pid__5165187F",
                        column: x => x.Pid,
                        principalTable: "PLANT",
                        principalColumn: "Pid");
                    table.ForeignKey(
                        name: "FK__GROWS_IN__Sid__52593CB8",
                        column: x => x.Sid,
                        principalTable: "SEASON",
                        principalColumn: "Sid");
                });

            migrationBuilder.CreateTable(
                name: "CROP",
                columns: table => new
                {
                    Cid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Area_size = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    Soil_type = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Current_Stage = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Uid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CROP__C1FFD8616FF697CD", x => x.Cid);
                    table.ForeignKey(
                        name: "FK__CROP__Uid__4316F928",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "Task",
                columns: table => new
                {
                    Task_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Label = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Uid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Task__716846B5F30205F9", x => x.Task_id);
                    table.ForeignKey(
                        name: "FK__Task__Uid__3C69FB99",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "USER_PHONE",
                columns: table => new
                {
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Uid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USER_PHO__E0255C3B7CD4D930", x => new { x.Phone, x.Uid });
                    table.ForeignKey(
                        name: "FK__USER_PHONE__Uid__398D8EEE",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "AI_Diagnosis",
                columns: table => new
                {
                    ADid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosisDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Result = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Did = table.Column<int>(type: "int", nullable: true),
                    Cid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AI_Diagn__7931D1B8DEB8ED6F", x => x.ADid);
                    table.ForeignKey(
                        name: "FK__AI_Diagnosi__Cid__6754599E",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                    table.ForeignKey(
                        name: "FK__AI_Diagnosi__Did__68487DD7",
                        column: x => x.Did,
                        principalTable: "Disease",
                        principalColumn: "Did");
                });

            migrationBuilder.CreateTable(
                name: "BELONG_TO",
                columns: table => new
                {
                    Cid = table.Column<int>(type: "int", nullable: false),
                    Pid = table.Column<int>(type: "int", nullable: false),
                    Plant_count = table.Column<int>(type: "int", nullable: true),
                    Sow_Time = table.Column<DateOnly>(type: "date", nullable: true),
                    Harvest_Time = table.Column<DateOnly>(type: "date", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "CROP_FERTILIZER",
                columns: table => new
                {
                    FrId = table.Column<int>(type: "int", nullable: false),
                    Cid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CROP_FER__3EAB9A937EDA9D18", x => new { x.FrId, x.Cid });
                    table.ForeignKey(
                        name: "FK__CROP_FERTIL__Cid__5812160E",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                    table.ForeignKey(
                        name: "FK__CROP_FERTI__FrId__571DF1D5",
                        column: x => x.FrId,
                        principalTable: "FERTILIZER",
                        principalColumn: "FrId");
                });

            migrationBuilder.CreateTable(
                name: "IRRIGATION_STAGE",
                columns: table => new
                {
                    Sid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_stage = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Stage_order = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Cid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__IRRIGATI__CA1E5D78AA21B4A3", x => x.Sid);
                    table.ForeignKey(
                        name: "FK__IRRIGATION___Cid__5EBF139D",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT",
                columns: table => new
                {
                    Pid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Added_date = table.Column<DateOnly>(type: "date", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Uid = table.Column<int>(type: "int", nullable: true),
                    Cid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PRODUCT__C5705938F6FC9017", x => x.Pid);
                    table.ForeignKey(
                        name: "FK__PRODUCT__Cid__46E78A0C",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                    table.ForeignKey(
                        name: "FK__PRODUCT__Uid__45F365D3",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "IRRIGATION",
                columns: table => new
                {
                    Iid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Frequency_unit = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Frequency_value = table.Column<int>(type: "int", nullable: true),
                    Irrigation_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Water_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Sis = table.Column<int>(type: "int", nullable: true),
                    Cid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__IRRIGATI__C4962F8429AE39D0", x => x.Iid);
                    table.ForeignKey(
                        name: "FK__IRRIGATION__Cid__628FA481",
                        column: x => x.Cid,
                        principalTable: "CROP",
                        principalColumn: "Cid");
                    table.ForeignKey(
                        name: "FK__IRRIGATION__Sis__619B8048",
                        column: x => x.Sis,
                        principalTable: "IRRIGATION_STAGE",
                        principalColumn: "Sid");
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_date = table.Column<DateOnly>(type: "date", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Total_price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Pid = table.Column<int>(type: "int", nullable: true),
                    Uid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ORDERS__CB3E4F31B009E3AD", x => x.Oid);
                    table.ForeignKey(
                        name: "FK__ORDERS__Pid__49C3F6B7",
                        column: x => x.Pid,
                        principalTable: "PRODUCT",
                        principalColumn: "Pid");
                    table.ForeignKey(
                        name: "FK__ORDERS__Uid__4AB81AF0",
                        column: x => x.Uid,
                        principalTable: "USERS",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AI_Diagnosis_Cid",
                table: "AI_Diagnosis",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_AI_Diagnosis_Did",
                table: "AI_Diagnosis",
                column: "Did");

            migrationBuilder.CreateIndex(
                name: "IX_BELONG_TO_Pid",
                table: "BELONG_TO",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_COMPATIBILITY_Pid",
                table: "COMPATIBILITY",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_CROP_Uid",
                table: "CROP",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_CROP_FERTILIZER_Cid",
                table: "CROP_FERTILIZER",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_GROWS_IN_Sid",
                table: "GROWS_IN",
                column: "Sid");

            migrationBuilder.CreateIndex(
                name: "IX_IRRIGATION_Cid",
                table: "IRRIGATION",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_IRRIGATION_Sis",
                table: "IRRIGATION",
                column: "Sis");

            migrationBuilder.CreateIndex(
                name: "IX_IRRIGATION_STAGE_Cid",
                table: "IRRIGATION_STAGE",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_Pid",
                table: "ORDERS",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_Uid",
                table: "ORDERS",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_Cid",
                table: "PRODUCT",
                column: "Cid");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_Uid",
                table: "PRODUCT",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Uid",
                table: "Task",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_USER_PHONE_Uid",
                table: "USER_PHONE",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AI_Diagnosis");

            migrationBuilder.DropTable(
                name: "BELONG_TO");

            migrationBuilder.DropTable(
                name: "COMPATIBILITY");

            migrationBuilder.DropTable(
                name: "CROP_FERTILIZER");

            migrationBuilder.DropTable(
                name: "GROWS_IN");

            migrationBuilder.DropTable(
                name: "IRRIGATION");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "Task");

            migrationBuilder.DropTable(
                name: "USER_PHONE");

            migrationBuilder.DropTable(
                name: "Disease");

            migrationBuilder.DropTable(
                name: "FERTILIZER");

            migrationBuilder.DropTable(
                name: "PLANT");

            migrationBuilder.DropTable(
                name: "SEASON");

            migrationBuilder.DropTable(
                name: "IRRIGATION_STAGE");

            migrationBuilder.DropTable(
                name: "PRODUCT");

            migrationBuilder.DropTable(
                name: "CROP");

            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}
