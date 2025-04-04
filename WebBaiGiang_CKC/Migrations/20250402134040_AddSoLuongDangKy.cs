using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class AddSoLuongDangKy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoLuongDangKy",
                table: "MonHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoLuongDangKy",
                table: "MonHoc");
        }
    }
}
