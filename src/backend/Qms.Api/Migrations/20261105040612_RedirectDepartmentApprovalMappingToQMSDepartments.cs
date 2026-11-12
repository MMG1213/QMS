
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    public partial class RedirectDepartmentApprovalMappingToQMSDepartments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentApprovalMappings_Departments_DepartmentId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentApprovalMappings_QMSDepartments_DepartmentId",
                table: "DepartmentApprovalMappings",
                column: "DepartmentId",
                principalTable: "QMSDepartments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentApprovalMappings_QMSDepartments_DepartmentId",
                table: "DepartmentApprovalMappings");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentApprovalMappings_Departments_DepartmentId",
                table: "DepartmentApprovalMappings",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
