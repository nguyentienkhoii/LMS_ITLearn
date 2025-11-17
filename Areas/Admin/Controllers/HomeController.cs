using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public HomeController(WebBaiGiangContext context)
        {
            _context = context;
        }

        public IActionResult Index(int kyKiemTraId)
        {
            // Thống kê tổng quan
            ViewBag.MonHocCount = _context.LopHocs.Count();
            ViewBag.CauHoiCount = _context.CauHoi.Count();
            ViewBag.ChuongCount = _context.ChuongNews.Count();
            ViewBag.MucCount = _context.Muc.Count();
            ViewBag.KyKiemTraCount = _context.KyKiemTra.Count();

            // Lấy danh sách bài làm có liên quan đến kỳ kiểm tra
            var baiLamList = _context.BaiLam
                .Include(x => x.CauHoi_BaiLam)
                    .ThenInclude(cb => cb.CauHoi_De)
                        .ThenInclude(cd => cd.De)
                            .ThenInclude(d => d.KyKiemTra)
                .Where(x => x.CauHoi_BaiLam.Any(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId))
                .AsNoTracking()
                .ToList();

            // Tính toán thống kê điểm
            var stats = CalculateStatistics(baiLamList);
            ViewBag.Percent0to5 = stats.percent0to5;
            ViewBag.Percent5to7 = stats.percent5to7;
            ViewBag.Percent7to8 = stats.percent7to8;
            ViewBag.Percent8to10 = stats.percent8to10;
            ViewBag.TotalParticipants = stats.totalParticipants;
            ViewBag.Diem0to5 = stats.count0to5;
            ViewBag.Diem5to7 = stats.count5to7;
            ViewBag.Diem7to8 = stats.count7to8;
            ViewBag.Diem8to10 = stats.count8to10;
            ViewBag.Diem10 = stats.count10;

            // Tính tỷ lệ trả lời đúng từng câu hỏi
            var tiLeDung = TinhTiLeTraLoiDungTungCauHoi(kyKiemTraId);
            ViewBag.TiLeTraLoiDungTungCauHoi = tiLeDung;

            // Số lượng sinh viên chưa làm bài
            ViewBag.CountChuaLamBai = CountHocVienChuaLamBai(kyKiemTraId);

            // Danh sách kỳ kiểm tra
            ViewBag.KyKiemTraList = _context.KyKiemTra.AsNoTracking().ToList();

            var tenKy = _context.KyKiemTra.FirstOrDefault(k => k.KyKiemTraId == kyKiemTraId)?.TenKyKiemTra;
            ViewBag.KyKiemTraName = tenKy;
            ViewBag.KyKiemTraId = kyKiemTraId;

            return View(baiLamList);
        }

        private (double percent0to5, double percent5to7, double percent7to8, double percent8to10,
                 int totalParticipants, int count0to5, int count5to7, int count7to8, int count8to10, int count10)
            CalculateStatistics(List<BaiLam> baiLamList)
        {
            if (!baiLamList.Any())
                return (0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            var totalParticipants = baiLamList
                .Select(x => x.MaHocVien)
                .Distinct()
                .Count();

            int count0to5 = baiLamList.Count(s => s.Diem >= 0 && s.Diem < 5);
            int count5to7 = baiLamList.Count(s => s.Diem >= 5 && s.Diem < 7);
            int count7to8 = baiLamList.Count(s => s.Diem >= 7 && s.Diem < 8);
            int count8to10 = baiLamList.Count(s => s.Diem >= 8 && s.Diem < 10);
            int count10 = baiLamList.Count(s => s.Diem == 10);

            int total = baiLamList.Count;
            double percent0to5 = Math.Round((double)count0to5 / total * 100, 2);
            double percent5to7 = Math.Round((double)count5to7 / total * 100, 2);
            double percent7to8 = Math.Round((double)count7to8 / total * 100, 2);
            double percent8to10 = Math.Round((double)count8to10 / total * 100, 2);

            return (percent0to5, percent5to7, percent7to8, percent8to10,
                    totalParticipants, count0to5, count5to7, count7to8, count8to10, count10);
        }

        private Dictionary<int, double> TinhTiLeTraLoiDungTungCauHoi(int kyKiemTraId, int? deThiId = null)
        {
            var soHocVien = _context.BaiLam
                .Where(b => b.CauHoi_BaiLam.Any(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId))
                .Select(b => b.MaHocVien)
                .Distinct()
                .Count();

            var dapAnDungList = _context.CauHoi_De
                .Include(cd => cd.CauHoi)
                .Where(cd => cd.De.KyKiemTraId == kyKiemTraId)
                .Select(cd => new { cd.CauHoiId, cd.CauHoi.DapAnDung })
                .AsNoTracking()
                .ToList();

            var baiLamList = _context.CauHoi_BaiLam
                .Include(cb => cb.CauHoi_De).ThenInclude(cd => cd.De)
                .Where(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId)
                .Select(cb => new { cb.BaiLam.MaHocVien, cb.CauHoi_De.CauHoiId, cb.DapAnSVChon })
                .AsNoTracking()
                .ToList();

            var tiLeTraLoiDung = dapAnDungList
                .GroupJoin(
                    baiLamList,
                    dapAn => dapAn.CauHoiId,
                    baiLam => baiLam.CauHoiId,
                    (dapAn, baiLamGroup) => new
                    {
                        dapAn.CauHoiId,
                        dapAn.DapAnDung,
                        BaiLamGroup = baiLamGroup
                    })
                .Select(x => new
                {
                    x.CauHoiId,
                    TiLeDung = x.BaiLamGroup.Any()
                        ? (double)x.BaiLamGroup.Count(bl => bl.DapAnSVChon?.ToLower() == x.DapAnDung?.ToLower()) /
                          x.BaiLamGroup.Count() * 100
                        : 0
                })
                .ToDictionary(x => x.CauHoiId, x => Math.Round(x.TiLeDung, 2));

            return tiLeTraLoiDung;
        }

        private int CountHocVienChuaLamBai(int kyKiemTraId)
        {
            return _context.DanhSachThi
                .Count(x => x.KyKiemTraId == kyKiemTraId && !x.TrangThai);
        }
    }
}
