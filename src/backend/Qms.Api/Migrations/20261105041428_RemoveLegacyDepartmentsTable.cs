
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    public partial class RemoveLegacyDepartmentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Remove old foreign keys.
            migrationBuilder.DropForeignKey(
                name: "FK_AccountRequests_Departments_DepartmentId",
                table: "AccountRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_Departments_DepartmentId",
                table: "AppUsers");

            // Step 2: Clear old department assignments.
            migrationBuilder.Sql("""
                UPDATE AppUsers
                SET DepartmentId = NULL;

                DELETE FROM AccountRequests;
                """);

            // Step 3: Drop the legacy Departments table.
            migrationBuilder.DropTable(
                name: "Departments");

            // Step 4: Add AppUsers relationship to QMSDepartments.
            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_QMSDepartments_DepartmentId",
                table: "AppUsers",
                column: "DepartmentId",
                principalTable: "QMSDepartments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            // Step 5: Add AccountRequests relationship to QMSDepartments.
            migrationBuilder.AddForeignKey(
                name: "FK_AccountRequests_QMSDepartments_DepartmentId",
                table: "AccountRequests",
                column: "DepartmentId",
                principalTable: "QMSDepartments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Recreate the legacy Departments table.
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    DeptId = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false),

                    DeptName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Departments",
                        x => x.Id);
                });

            // Step 2: Restore indexes.
            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptId",
                table: "Departments",
                column: "DeptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptName",
                table: "Departments",
                column: "DeptName",
                unique: true);

            // Step 3: Remove new QMSDepartments foreign keys.
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_QMSDepartments_DepartmentId",
                table: "AppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountRequests_QMSDepartments_DepartmentId",
                table: "AccountRequests");

            // Step 4: Restore old relationships.
            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_Departments_DepartmentId",
                table: "AppUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRequests_Departments_DepartmentId",
                table: "AccountRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
