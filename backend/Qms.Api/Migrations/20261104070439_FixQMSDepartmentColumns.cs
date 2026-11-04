using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    public partial class FixQMSDepartmentColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QMSDepartments",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "QMSDepartments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "QMSDepartments",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QMSDepartments");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "QMSDepartments");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "QMSDepartments");
        }
    }
}