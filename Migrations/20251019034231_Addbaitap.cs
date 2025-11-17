using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class Addbaitap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BAITAP",
                columns: table => new
                {
                    MaBaiTap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBaiTap = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileDinhKem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HanNop = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAITAP", x => x.MaBaiTap);
                    table.ForeignKey(
                        name: "FK_BAITAP_BAI_BaiId",
                        column: x => x.BaiId,
                        principalTable: "BAI",
                        principalColumn: "BaiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BAITAPNOP",
                columns: table => new
                {
                    MaBaiTapNop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileNop = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LanNop = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<double>(type: "float", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayCham = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaBaiTap = table.Column<int>(type: "int", nullable: false),
                    MaHocVien = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAITAPNOP", x => x.MaBaiTapNop);
                    table.ForeignKey(
                        name: "FK_BAITAPNOP_BAITAP_MaBaiTap",
                        column: x => x.MaBaiTap,
                        principalTable: "BAITAP",
                        principalColumn: "MaBaiTap",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BAITAPNOP_HocViens_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocViens",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BAITAP_BaiId",
                table: "BAITAP",
                column: "BaiId");

            migrationBuilder.CreateIndex(
                name: "IX_BAITAPNOP_MaBaiTap",
                table: "BAITAPNOP",
                column: "MaBaiTap");

            migrationBuilder.CreateIndex(
                name: "IX_BAITAPNOP_MaHocVien",
                table: "BAITAPNOP",
                column: "MaHocVien");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BAITAPNOP");

            migrationBuilder.DropTable(
                name: "BAITAP");
        }
    }
}
