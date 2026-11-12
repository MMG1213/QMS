using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentApprovalMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentApprovalMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PrimaryApproverEmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RedirectedApproverEmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentApprovalMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentApprovalMappings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentApprovalMappings_DepartmentId",
                table: "DepartmentApprovalMappings",
                column: "DepartmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentApprovalMappings");
        }
    }
}
