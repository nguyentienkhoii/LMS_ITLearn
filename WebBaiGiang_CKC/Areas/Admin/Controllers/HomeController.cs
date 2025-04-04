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
            var monHocCount = GetMonHocCount(); // Đếm số lượng môn học
            ViewBag.MonHocCount = monHocCount;
            var cauHoiCount = GetCauHoiCount();
            ViewBag.CauHoiCount = cauHoiCount;
            var chuongCount = GetChuongCount();
            ViewBag.ChuongCount = chuongCount;
            var mucCount = GetMucCount();
            ViewBag.MucCount = mucCount;
            var kyKiemTraCount = GetKyKiemTraCount();
            ViewBag.KyKiemTraCount = kyKiemTraCount;
            var baiLamList = _context.BaiLam
               .Include(x => x.CauHoi_BaiLam).ThenInclude(x => x.CauHoi_De.De.KyKiemTra)
               .Where(x => x.CauHoi_BaiLam.Any() && x.CauHoi_BaiLam.First().CauHoi_De.De.KyKiemTraId == kyKiemTraId)
               .ToList();

            var (percent0to5, percent5to7, percent7to8, percent8to10, totalParticipants, count0to5, count5to7, count7to8, count8to10, count10)
                = CalculateStatistics(baiLamList);
            var tinhtiletraloidungtungcauhoi = TinhTiLeTraLoiDungTungCauHoi(kyKiemTraId);
            ViewBag.tinhtiletraloidungtungcauhoi = tinhtiletraloidungtungcauhoi;
            var countChuaLamBai = CountSinhVienChuaLamBai(kyKiemTraId);
            ViewBag.CountChuaLamBai = countChuaLamBai;
            ViewBag.Percent0to5 = percent0to5;
            ViewBag.Percent5to7 = percent5to7;
            ViewBag.Percent7to8 = percent7to8;
            ViewBag.Percent8to10 = percent8to10;
            ViewBag.TotalParticipants = totalParticipants;
            ViewBag.Diem0to5 = count0to5;
            ViewBag.Diem5to7 = count5to7;
            ViewBag.Diem7to8 = count7to8;
            ViewBag.Diem8to10 = count8to10;
            ViewBag.Diem10 = count10;
            var kyKiemTraList = _context.KyKiemTra.ToList();
            ViewBag.KyKiemTraList = kyKiemTraList;
            var tenkykiemtra = _context.KyKiemTra.FirstOrDefault(k => k.KyKiemTraId == kyKiemTraId)?.TenKyKiemTra;
            ViewBag.KyKiemTraName = tenkykiemtra;
            ViewBag.KyKiemTraId = kyKiemTraId;
            return View(baiLamList);
        }

        private (double, double, double, double, int, int, int, int, int,int) CalculateStatistics(List<BaiLam> baiLamList)
        {
            
            var totalParticipants = baiLamList
                .Select(x => x.MSSV)
                .Distinct()
                .Count();
            var count0to5 = baiLamList.Count(s => s.Diem >= 0 && s.Diem < 5);
            var count5to7 = baiLamList.Count(s => s.Diem >= 5 && s.Diem < 7);
            var count7to8 = baiLamList.Count(s => s.Diem >= 7 && s.Diem < 8);
            var count8to10 = baiLamList.Count(s => s.Diem >= 8 && s.Diem <= 10);
            var total = baiLamList.Count();
            var percent0to5 = Math.Round((double)count0to5 / total * 100, 2);
            var percent5to7 = Math.Round((double)count5to7 / total * 100, 2);
            var percent7to8 = Math.Round((double)count7to8 / total * 100, 2);
            var percent8to10 = Math.Round((double)count8to10 / total * 100, 2);
            var count10 = baiLamList.Count(s => s.Diem ==10);
            return (percent0to5, percent5to7, percent7to8, percent8to10, totalParticipants, count0to5, count5to7, count7to8, count8to10, count10);
        }
        private Dictionary<int, double> TinhTiLeTraLoiDungTungCauHoi(int kyKiemTraId, int? deThiId = null)
        {
            // Lấy số sinh viên từ database
            var soSinhVien = _context.BaiLam
                .Where(b => deThiId == null || b.CauHoi_BaiLam.Any(c => c.CauHoi_De.DeId == deThiId))
                .Select(b => b.MSSV)
                .Distinct()
                .Count();

            // Lấy các câu hỏi và đáp án đúng
            var dapAnDungList = _context.CauHoi_De
                  .Include(cd => cd.CauHoi)
                  .Where(cd => cd.De.KyKiemTraId == kyKiemTraId)
                  .Select(cd => new { cd.CauHoiId, cd.CauHoi.DapAnDung })
                  .ToList();

            // Lấy bài làm của sinh viên
            var baiLamList = _context.CauHoi_BaiLam
                .Include(cb => cb.CauHoi_De).ThenInclude(cd => cd.De)
                .Where(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId && (deThiId == null || cb.CauHoi_De.DeId == deThiId))
                .Select(cb => new { cb.BaiLam.MSSV, cb.CauHoi_De.CauHoiId, cb.DapAnSVChon })
                .ToList();

            // Tính tỷ lệ trả lời đúng của từng câu hỏi
            var tiLeTraLoiDungTungCauHoi = dapAnDungList
                .GroupJoin(
                    baiLamList,
                    dapAn => dapAn.CauHoiId,
                    baiLam => baiLam.CauHoiId,
                    (dapAn, baiLamGroup) => new { dapAn.CauHoiId, dapAn.DapAnDung, BaiLamGroup = baiLamGroup }
                )
                .SelectMany(
                    x => x.BaiLamGroup.DefaultIfEmpty(),
                    (x, baiLam) => new { x.CauHoiId, x.DapAnDung, DapAnSV = baiLam?.DapAnSVChon }
                )
                .GroupBy(x => x.CauHoiId)
                .ToDictionary(
                    g => g.Key,
                    g => (double)g.Count(x => x.DapAnDung == x.DapAnSV) / g.Count() * 100
                );

            return tiLeTraLoiDungTungCauHoi;
        }
        private int CountSinhVienChuaLamBai(int kykiemtraid)
        {
            var count = _context.DanhSachThi
                .Where(x => x.KyKiemTraId == kykiemtraid && x.TrangThai == false)
                .Count();
            return count;
        }
        private int GetCauHoiCount()
        {
            var cauhoi = _context.CauHoi.Count();
            return cauhoi;
        }
        private int GetMonHocCount()
        {
            var monhoc = _context.MonHoc.Count(); // Giả sử bạn có bảng MonHocs trong cơ sở dữ liệu
            return monhoc;
        }

        private int GetChuongCount()
        {
            var chuong = _context.Chuong.Count();
            return chuong;
        }
        private int GetMucCount()
        {
            var muc = _context.Muc.Count();
            return muc;
        }
        private int GetKyKiemTraCount()
        {
            var kykiemtra = _context.KyKiemTra.Count();
            return kykiemtra;
        }
    }
}
