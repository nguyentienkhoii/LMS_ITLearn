using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class AddAnhDaiDienToMonHoc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDaiDien",
                table: "MonHoc",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhDaiDien",
                table: "MonHoc");
        }
    }
}
