using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDocumentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentFileName",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentStoredName",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentFileName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DocumentStoredName",
                table: "Projects");
        }
    }
}
