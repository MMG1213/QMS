using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedMachinesList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeptId",
                table: "Departments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "MachinesLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MachineCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineCodeNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MachineSlNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CapacityOrSpecification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CommissionedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    LoadInHP = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachinesLists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptId",
                table: "Departments",
                column: "DeptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachinesLists");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DeptId",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "DeptId",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
