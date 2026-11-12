
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    public partial class FixQMSDepartmentColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These columns already exist in the database.
            // No schema changes are required.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty.
            // Do not drop existing database columns.
        }
    }
}
