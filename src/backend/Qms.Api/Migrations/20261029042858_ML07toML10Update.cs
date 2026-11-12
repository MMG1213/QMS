using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class ML07toML10Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityPerformanceIndices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessOrDept = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeasurementTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QualityPerformanceIndexFormula = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Targets = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Responsibilities = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentReview = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QPIReview = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityPerformanceIndices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityPerformanceIndices_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Standards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pages = table.Column<int>(type: "int", nullable: true),
                    Revision = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Standards_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestingOrMeasuringInstruments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstrumentCodeNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstrumentSlNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstrumentRange = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CalibrationFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestingOrMeasuringInstruments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestingOrMeasuringInstruments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityPerformanceIndices_DocumentTypeId",
                table: "QualityPerformanceIndices",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_DocumentTypeId",
                table: "Standards",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingOrMeasuringInstruments_DocumentTypeId",
                table: "TestingOrMeasuringInstruments",
                column: "DocumentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityPerformanceIndices");

            migrationBuilder.DropTable(
                name: "Standards");

            migrationBuilder.DropTable(
                name: "TestingOrMeasuringInstruments");
        }
    }
}
