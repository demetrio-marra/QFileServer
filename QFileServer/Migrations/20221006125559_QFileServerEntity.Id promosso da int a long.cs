using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QFileServer.Migrations
{
    public partial class QFileServerEntityIdpromossodaintalong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_FilesRepo", "FilesRepo");
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "FilesRepo",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_FilesRepo", "FilesRepo");
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FilesRepo",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
