using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QFileServer.Migrations
{
    public partial class rimossadescrizionedaqfilenentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "FilesRepo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FilesRepo",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
