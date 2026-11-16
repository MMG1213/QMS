using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignDepartmentApprovalMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropColumn(
                name: "RedirectedApproverEmployeeId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.RenameColumn(
                name: "PrimaryApproverEmployeeId",
                table: "DepartmentApprovalMappings",
                newName: "CreatedByEmployeeId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DepartmentApprovalMappings",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "ConfigVersion",
                table: "DepartmentApprovalMappings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "DepartmentApprovalMappings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "DepartmentApprovalMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AppUsers_EmployeeId",
                table: "AppUsers",
                column: "EmployeeId");

            migrationBuilder.CreateTable(
                name: "DepartmentApprovalLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentApprovalMappingId = table.Column<int>(type: "int", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    ApprovalType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApproverEmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RedirectedApproverEmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentApprovalLevels", x => x.Id);
                    table.CheckConstraint("CK_DepartmentApprovalLevels_ApprovalType", "[ApprovalType] IN ('Department', 'QMSFinal')");
                    table.CheckConstraint("CK_DepartmentApprovalLevels_LevelNumber", "[LevelNumber] >= 1");
                    table.ForeignKey(
                        name: "FK_DepartmentApprovalLevels_AppUsers_ApproverEmployeeId",
                        column: x => x.ApproverEmployeeId,
                        principalTable: "AppUsers",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentApprovalLevels_AppUsers_RedirectedApproverEmployeeId",
                        column: x => x.RedirectedApproverEmployeeId,
                        principalTable: "AppUsers",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentApprovalLevels_DepartmentApprovalMappings_DepartmentApprovalMappingId",
                        column: x => x.DepartmentApprovalMappingId,
                        principalTable: "DepartmentApprovalMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentDCRCreators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentApprovalMappingId = table.Column<int>(type: "int", nullable: false),
                    CreatorEmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentDCRCreators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentDCRCreators_AppUsers_CreatorEmployeeId",
                        column: x => x.CreatorEmployeeId,
                        principalTable: "AppUsers",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentDCRCreators_DepartmentApprovalMappings_DepartmentApprovalMappingId",
                        column: x => x.DepartmentApprovalMappingId,
                        principalTable: "DepartmentApprovalMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalMappings_CreatedByEmployeeId",
                table: "DepartmentApprovalMappings",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId",
                table: "DepartmentApprovalMappings",
                column: "DepartmentId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId_ConfigVersion",
                table: "DepartmentApprovalMappings",
                columns: new[] { "DepartmentId", "ConfigVersion" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_DepartmentApprovalMappings_ConfigVersion",
                table: "DepartmentApprovalMappings",
                sql: "[ConfigVersion] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalLevels_ApproverEmployeeId",
                table: "DepartmentApprovalLevels",
                column: "ApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalLevels_DepartmentApprovalMappingId_LevelNumber",
                table: "DepartmentApprovalLevels",
                columns: new[] { "DepartmentApprovalMappingId", "LevelNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalLevels_RedirectedApproverEmployeeId",
                table: "DepartmentApprovalLevels",
                column: "RedirectedApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentDCRCreators_CreatorEmployeeId",
                table: "DepartmentDCRCreators",
                column: "CreatorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentDCRCreators_DepartmentApprovalMappingId_CreatorEmployeeId",
                table: "DepartmentDCRCreators",
                columns: new[] { "DepartmentApprovalMappingId", "CreatorEmployeeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentApprovalMappings_AppUsers_CreatedByEmployeeId",
                table: "DepartmentApprovalMappings",
                column: "CreatedByEmployeeId",
                principalTable: "AppUsers",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentApprovalMappings_AppUsers_CreatedByEmployeeId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropTable(
                name: "DepartmentApprovalLevels");

            migrationBuilder.DropTable(
                name: "DepartmentDCRCreators");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentApprovalMappings_CreatedByEmployeeId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId_ConfigVersion",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DepartmentApprovalMappings_ConfigVersion",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AppUsers_EmployeeId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ConfigVersion",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "DepartmentApprovalMappings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "DepartmentApprovalMappings");

            migrationBuilder.RenameColumn(
                name: "CreatedByEmployeeId",
                table: "DepartmentApprovalMappings",
                newName: "PrimaryApproverEmployeeId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DepartmentApprovalMappings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirectedApproverEmployeeId",
                table: "DepartmentApprovalMappings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId",
                table: "DepartmentApprovalMappings",
                column: "DepartmentId",
                unique: true);
        }
    }
}
