using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseCleanupAndImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHashed",
                table: "USERS");

            // Rename Nause → Cause to preserve any existing data.
            migrationBuilder.RenameColumn(
                name: "Nause",
                table: "Disease",
                newName: "Cause");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Task",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "PRODUCT",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PRODUCT",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ORDERS",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Treatment",
                table: "Disease",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "Disease",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Disease",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // Widen the renamed Cause column to nvarchar(max).
            migrationBuilder.AlterColumn<string>(
                name: "Cause",
                table: "Disease",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CROP",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "AI_Diagnosis",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DiagnosisDate",
                table: "AI_Diagnosis",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_Category",
                table: "PRODUCT",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PRODUCT_Category",
                table: "PRODUCT");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PRODUCT");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ORDERS");

            // (Cause column handled by RenameColumn below.)

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CROP");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHashed",
                table: "USERS",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "PRODUCT",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Treatment",
                table: "Disease",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "Disease",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Disease",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            // Shrink Cause back then rename to Nause.
            migrationBuilder.AlterColumn<string>(
                name: "Cause",
                table: "Disease",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "Cause",
                table: "Disease",
                newName: "Nause");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "AI_Diagnosis",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DiagnosisDate",
                table: "AI_Diagnosis",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
