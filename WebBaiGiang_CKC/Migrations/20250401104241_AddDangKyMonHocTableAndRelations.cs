using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class AddDangKyMonHocTableAndRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
      
            migrationBuilder.CreateTable(
                name: "DangKyMonHoc",
                columns: table => new
                {
                    DangKyMonHocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyMonHoc", x => x.DangKyMonHocId);
                    table.ForeignKey(
                        name: "FK_DangKyMonHoc_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "MonHocId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyMonHoc_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "TaiKhoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            
            

            migrationBuilder.CreateIndex(
                name: "IX_DangKyMonHoc_MonHocId",
                table: "DangKyMonHoc",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyMonHoc_TaiKhoanId_MonHocId",
                table: "DangKyMonHoc",
                columns: new[] { "TaiKhoanId", "MonHocId" },
                unique: true);


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiTap");

            migrationBuilder.DropTable(
                name: "CauHoi_BaiLam");

            migrationBuilder.DropTable(
                name: "DangKyMonHoc");

            migrationBuilder.DropTable(
                name: "DanhSachThi");

            migrationBuilder.DropTable(
                name: "DeCuong");

            migrationBuilder.DropTable(
                name: "GiaoVien");

            migrationBuilder.DropTable(
                name: "Muc");

            migrationBuilder.DropTable(
                name: "BaiLam");

            migrationBuilder.DropTable(
                name: "CauHoi_De");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "Bai");

            migrationBuilder.DropTable(
                name: "CauHoi");

            migrationBuilder.DropTable(
                name: "De");

            migrationBuilder.DropTable(
                name: "Chuong");

            migrationBuilder.DropTable(
                name: "KyKiemTra");

            migrationBuilder.DropTable(
                name: "MonHoc");
        }
    }
}
