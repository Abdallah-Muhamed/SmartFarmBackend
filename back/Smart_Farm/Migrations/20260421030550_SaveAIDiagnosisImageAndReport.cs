using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class SaveAIDiagnosisImageAndReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeminiArabicReport",
                table: "AI_Diagnosis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "AI_Diagnosis",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "AI_Diagnosis",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "AI_Diagnosis",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeminiArabicReport",
                table: "AI_Diagnosis");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "AI_Diagnosis");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "AI_Diagnosis");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "AI_Diagnosis");
        }
    }
}
