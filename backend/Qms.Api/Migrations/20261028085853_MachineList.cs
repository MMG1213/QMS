using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class MachineList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentTypeId",
                table: "MachinesLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MachinesLists_DocumentTypeId",
                table: "MachinesLists",
                column: "DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachinesLists_DocumentTypes_DocumentTypeId",
                table: "MachinesLists",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachinesLists_DocumentTypes_DocumentTypeId",
                table: "MachinesLists");

            migrationBuilder.DropIndex(
                name: "IX_MachinesLists_DocumentTypeId",
                table: "MachinesLists");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "MachinesLists");
        }
    }
}
