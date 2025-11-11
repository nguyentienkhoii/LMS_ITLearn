using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KHOAHOC",
                columns: table => new
                {
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHOAHOC", x => x.MaKhoaHoc);
                });

            migrationBuilder.CreateTable(
                name: "KyKiemTra",
                columns: table => new
                {
                    KyKiemTraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKyKiemTra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianLamBai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyKiemTra", x => x.KyKiemTraId);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoanNews",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNews", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "De",
                columns: table => new
                {
                    DeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KyKiemTraId = table.Column<int>(type: "int", nullable: false),
                    SoCauHoi = table.Column<int>(type: "int", nullable: false),
                    DoKhoDe = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_De", x => x.DeId);
                    table.ForeignKey(
                        name: "FK_De_KyKiemTra_KyKiemTraId",
                        column: x => x.KyKiemTraId,
                        principalTable: "KyKiemTra",
                        principalColumn: "KyKiemTraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiangViens",
                columns: table => new
                {
                    MaGiangVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangViens", x => x.MaGiangVien);
                    table.ForeignKey(
                        name: "FK_GiangViens_TaiKhoanNews_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoanNews",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocViens",
                columns: table => new
                {
                    MaHocVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocViens", x => x.MaHocVien);
                    table.ForeignKey(
                        name: "FK_HocViens_TaiKhoanNews_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoanNews",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LOPHOC",
                columns: table => new
                {
                    MaLopHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLopHoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    MaGiangVien = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOPHOC", x => x.MaLopHoc);
                    table.ForeignKey(
                        name: "FK_LOPHOC_GiangViens_MaGiangVien",
                        column: x => x.MaGiangVien,
                        principalTable: "GiangViens",
                        principalColumn: "MaGiangVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LOPHOC_KHOAHOC_MaKhoaHoc",
                        column: x => x.MaKhoaHoc,
                        principalTable: "KHOAHOC",
                        principalColumn: "MaKhoaHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BAILAM",
                columns: table => new
                {
                    BaiLamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHocVien = table.Column<int>(type: "int", nullable: false),
                    SoCauDung = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<float>(type: "real", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianDenHan = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAILAM", x => x.BaiLamId);
                    table.ForeignKey(
                        name: "FK_BAILAM_HocViens_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocViens",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DANHSACHTHI",
                columns: table => new
                {
                    DanhSachThiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHocVien = table.Column<int>(type: "int", nullable: false),
                    KyKiemTraId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DANHSACHTHI", x => x.DanhSachThiId);
                    table.ForeignKey(
                        name: "FK_DANHSACHTHI_HocViens_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocViens",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DANHSACHTHI_KyKiemTra_KyKiemTraId",
                        column: x => x.KyKiemTraId,
                        principalTable: "KyKiemTra",
                        principalColumn: "KyKiemTraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CHUONG_NEW",
                columns: table => new
                {
                    MaChuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaLopHoc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHUONG_NEW", x => x.MaChuong);
                    table.ForeignKey(
                        name: "FK_CHUONG_NEW_LOPHOC_MaLopHoc",
                        column: x => x.MaLopHoc,
                        principalTable: "LOPHOC",
                        principalColumn: "MaLopHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HOCVIEN_LOPHOC",
                columns: table => new
                {
                    MaHocVien = table.Column<int>(type: "int", nullable: false),
                    MaLopHoc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HOCVIEN_LOPHOC", x => new { x.MaHocVien, x.MaLopHoc });
                    table.ForeignKey(
                        name: "FK_HOCVIEN_LOPHOC_HocViens_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocViens",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HOCVIEN_LOPHOC_LOPHOC_MaLopHoc",
                        column: x => x.MaLopHoc,
                        principalTable: "LOPHOC",
                        principalColumn: "MaLopHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BAI",
                columns: table => new
                {
                    BaiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuong = table.Column<int>(type: "int", nullable: false),
                    TenBai = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoBai = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "ntext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAI", x => x.BaiId);
                    table.ForeignKey(
                        name: "FK_BAI_CHUONG_NEW_MaChuong",
                        column: x => x.MaChuong,
                        principalTable: "CHUONG_NEW",
                        principalColumn: "MaChuong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CAUHOI",
                columns: table => new
                {
                    CauHoiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuong = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DapAnA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DapAnB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DapAnC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DapAnD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DapAnDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoKho = table.Column<float>(type: "real", nullable: false),
                    SoLanLay = table.Column<int>(type: "int", nullable: true),
                    SoLanTraLoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAUHOI", x => x.CauHoiId);
                    table.ForeignKey(
                        name: "FK_CAUHOI_CHUONG_NEW_MaChuong",
                        column: x => x.MaChuong,
                        principalTable: "CHUONG_NEW",
                        principalColumn: "MaChuong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Muc",
                columns: table => new
                {
                    MucId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaiId = table.Column<int>(type: "int", nullable: false),
                    MucSo = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "ntext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muc", x => x.MucId);
                    table.ForeignKey(
                        name: "FK_Muc_BAI_BaiId",
                        column: x => x.BaiId,
                        principalTable: "BAI",
                        principalColumn: "BaiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHoi_De",
                columns: table => new
                {
                    CauHoi_DeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CauHoiId = table.Column<int>(type: "int", nullable: false),
                    DeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi_De", x => x.CauHoi_DeId);
                    table.ForeignKey(
                        name: "FK_CauHoi_De_CAUHOI_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CAUHOI",
                        principalColumn: "CauHoiId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CauHoi_De_De_DeId",
                        column: x => x.DeId,
                        principalTable: "De",
                        principalColumn: "DeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHoi_BaiLam",
                columns: table => new
                {
                    CauHoi_BaiLamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiLamId = table.Column<int>(type: "int", nullable: false),
                    CauHoi_DeId = table.Column<int>(type: "int", nullable: false),
                    DapAnSVChon = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi_BaiLam", x => x.CauHoi_BaiLamId);
                    table.ForeignKey(
                        name: "FK_CauHoi_BaiLam_BAILAM_BaiLamId",
                        column: x => x.BaiLamId,
                        principalTable: "BAILAM",
                        principalColumn: "BaiLamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CauHoi_BaiLam_CauHoi_De_CauHoi_DeId",
                        column: x => x.CauHoi_DeId,
                        principalTable: "CauHoi_De",
                        principalColumn: "CauHoi_DeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BAI_MaChuong",
                table: "BAI",
                column: "MaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_BAILAM_MaHocVien",
                table: "BAILAM",
                column: "MaHocVien");

            migrationBuilder.CreateIndex(
                name: "IX_CAUHOI_MaChuong",
                table: "CAUHOI",
                column: "MaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_BaiLam_BaiLamId",
                table: "CauHoi_BaiLam",
                column: "BaiLamId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_BaiLam_CauHoi_DeId",
                table: "CauHoi_BaiLam",
                column: "CauHoi_DeId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_De_CauHoiId",
                table: "CauHoi_De",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_De_DeId",
                table: "CauHoi_De",
                column: "DeId");

            migrationBuilder.CreateIndex(
                name: "IX_CHUONG_NEW_MaLopHoc",
                table: "CHUONG_NEW",
                column: "MaLopHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DANHSACHTHI_KyKiemTraId",
                table: "DANHSACHTHI",
                column: "KyKiemTraId");

            migrationBuilder.CreateIndex(
                name: "IX_DANHSACHTHI_MaHocVien",
                table: "DANHSACHTHI",
                column: "MaHocVien");

            migrationBuilder.CreateIndex(
                name: "IX_De_KyKiemTraId",
                table: "De",
                column: "KyKiemTraId");

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_MaTaiKhoan",
                table: "GiangViens",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HOCVIEN_LOPHOC_MaLopHoc",
                table: "HOCVIEN_LOPHOC",
                column: "MaLopHoc");

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_MaTaiKhoan",
                table: "HocViens",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LOPHOC_MaGiangVien",
                table: "LOPHOC",
                column: "MaGiangVien");

            migrationBuilder.CreateIndex(
                name: "IX_LOPHOC_MaKhoaHoc",
                table: "LOPHOC",
                column: "MaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_Muc_BaiId",
                table: "Muc",
                column: "BaiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHoi_BaiLam");

            migrationBuilder.DropTable(
                name: "DANHSACHTHI");

            migrationBuilder.DropTable(
                name: "HOCVIEN_LOPHOC");

            migrationBuilder.DropTable(
                name: "Muc");

            migrationBuilder.DropTable(
                name: "BAILAM");

            migrationBuilder.DropTable(
                name: "CauHoi_De");

            migrationBuilder.DropTable(
                name: "BAI");

            migrationBuilder.DropTable(
                name: "HocViens");

            migrationBuilder.DropTable(
                name: "CAUHOI");

            migrationBuilder.DropTable(
                name: "De");

            migrationBuilder.DropTable(
                name: "CHUONG_NEW");

            migrationBuilder.DropTable(
                name: "KyKiemTra");

            migrationBuilder.DropTable(
                name: "LOPHOC");

            migrationBuilder.DropTable(
                name: "GiangViens");

            migrationBuilder.DropTable(
                name: "KHOAHOC");

            migrationBuilder.DropTable(
                name: "TaiKhoanNews");
        }
    }
}
