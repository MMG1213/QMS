using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameQualityPerformanceIndexTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityPerformanceIndices_DocumentTypes_DocumentTypeId",
                table: "QualityPerformanceIndices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QualityPerformanceIndices",
                table: "QualityPerformanceIndices");

            migrationBuilder.RenameTable(
                name: "QualityPerformanceIndices",
                newName: "QualityPerformanceIndex");

            migrationBuilder.RenameIndex(
                name: "IX_QualityPerformanceIndices_DocumentTypeId",
                table: "QualityPerformanceIndex",
                newName: "IX_QualityPerformanceIndex_DocumentTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Targets",
                table: "QualityPerformanceIndex",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Responsibilities",
                table: "QualityPerformanceIndex",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "QualityPerformanceIndexFormula",
                table: "QualityPerformanceIndex",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "QPIReview",
                table: "QualityPerformanceIndex",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessOrDept",
                table: "QualityPerformanceIndex",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MeasurementTitle",
                table: "QualityPerformanceIndex",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentReview",
                table: "QualityPerformanceIndex",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_QualityPerformanceIndex",
                table: "QualityPerformanceIndex",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityPerformanceIndex_DocumentTypes_DocumentTypeId",
                table: "QualityPerformanceIndex",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityPerformanceIndex_DocumentTypes_DocumentTypeId",
                table: "QualityPerformanceIndex");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QualityPerformanceIndex",
                table: "QualityPerformanceIndex");

            migrationBuilder.RenameTable(
                name: "QualityPerformanceIndex",
                newName: "QualityPerformanceIndices");

            migrationBuilder.RenameIndex(
                name: "IX_QualityPerformanceIndex_DocumentTypeId",
                table: "QualityPerformanceIndices",
                newName: "IX_QualityPerformanceIndices_DocumentTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Targets",
                table: "QualityPerformanceIndices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Responsibilities",
                table: "QualityPerformanceIndices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "QualityPerformanceIndexFormula",
                table: "QualityPerformanceIndices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "QPIReview",
                table: "QualityPerformanceIndices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessOrDept",
                table: "QualityPerformanceIndices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MeasurementTitle",
                table: "QualityPerformanceIndices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentReview",
                table: "QualityPerformanceIndices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey(
                name: "PK_QualityPerformanceIndices",
                table: "QualityPerformanceIndices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityPerformanceIndices_DocumentTypes_DocumentTypeId",
                table: "QualityPerformanceIndices",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
