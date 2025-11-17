using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Controllers
{
    public class BaiGiangsController : HomeController
    {
        public BaiGiangsController(WebBaiGiangContext context, IWebHostEnvironment environment, INotyfService notyfService)
            : base(context, environment, notyfService)
        {
        }

        public IActionResult NoiDungChinh() => View();

        [Route("DanhSachGiangVien")]
        public IActionResult GiaoVien()
        {
            var giaovien = _context.GiangViens.AsNoTracking().ToList();
            ViewBag.Giaovien = giaovien;
            return View();
        }


        [Route("LopHoc/{lopHocId}/Chuong/{chuongId}/Bai/{baiId}")]
        public IActionResult Bai(int lopHocId, int chuongId, int baiId)
        {
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem bài học!");
                return RedirectToAction("Index", "Home");
            }

            //active menu
            ViewBag.ActiveMenu = "LopHoc";        // xác định menu chính là lớp học
            ViewBag.CurrentLopHocId = lopHocId;         // xác định lớp học đang mở



            var lopHoc = _context.LopHocs
                .Include(m => m.Chuongs)
                .ThenInclude(c => c.Bais)
                .FirstOrDefault(m => m.MaLopHoc == lopHocId);

            if (lopHoc == null)
            {
                _notyfService.Error("Lớp học không tồn tại!");
                return RedirectToAction("Index", "Home");
            }

            bool isRegistered = _context.HocVien_LopHoc
                .Any(dkm => dkm.MaHocVien == hocVienId && dkm.MaLopHoc == lopHocId);

            if (!isRegistered)
            {
                _notyfService.Warning("Bạn chưa đăng ký lớp học này!");
                return RedirectToAction("Index", "Home");
            }

            // 🩵 Load bài và mục, kèm danh sách tài liệu (TaiLieu)
            var lstBai = _context.Bai
                .Where(x => x.BaiId == baiId && x.MaChuong == chuongId && x.Chuong.LopHoc.MaLopHoc == lopHocId)
                .Include(a => a.Mucs)
                    .ThenInclude(m => m.TaiLieus)
                .Include(a => a.BaiTaps)
                .AsNoTracking()
                .ToList();


            if (lstBai.Any() && lstBai.First().Mucs?.Any() == true)
            {
                lstBai = lstBai.OrderByDescending(x => x.Mucs.Min(y => y.MucSo)).ToList();
            }

            ViewBag.SelectedLopHoc = lopHoc;
            return View(lstBai);
        }

        public IActionResult Lich() => View();

        [Route("/HoSo")]
        public IActionResult HoSo()
        {
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem hồ sơ!");
                return RedirectToAction("Index", "Home");
            }

            var danhSachThi = _context.DanhSachThi
                .Where(d => d.MaHocVien == hocVienId)
                .Include(d => d.HocVien)
                    .ThenInclude(h => h.TaiKhoan)
                .Include(d => d.KyKiemTra)
                    .ThenInclude(k => k.De)
                        .ThenInclude(de => de.CauHoi_DeThi)
                            .ThenInclude(ch => ch.CauHoi_BaiLam)
                                .ThenInclude(cb => cb.BaiLam)
                .AsNoTracking()
                .ToList();

            ViewBag.kiemtra = danhSachThi;
            return View();
        }

        public IActionResult NopBai() => View();

        public IActionResult KyThi()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _notyfService.Warning("Vui lòng đăng nhập để tham gia kỳ thi!");
                return RedirectToAction("Index", "Home");
            }

            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Error("Lỗi xác thực học viên!");
                return RedirectToAction("Index", "Home");
            }


            ViewBag.ActiveMenu = "KyThi";         // đánh dấu tab "Kỳ thi" đang mở
            ViewBag.CurrentLopHocId = null;


            var danhSachThi = _context.DanhSachThi
                .Where(d => d.MaHocVien == hocVienId)
                .Include(d => d.HocVien)
                    .ThenInclude(h => h.TaiKhoan)
                .Include(d => d.KyKiemTra)
                    .ThenInclude(k => k.De)
                        .ThenInclude(de => de.CauHoi_DeThi)
                            .ThenInclude(ch => ch.CauHoi_BaiLam)
                                .ThenInclude(cb => cb.BaiLam)
                .AsNoTracking()
                .ToList();

            ViewBag.kiemtra = danhSachThi;
            return View();
        }

        [Route("/BaiGiangs/KyThi/BaiKiemTra")]
        [HttpPost]
        public async Task<IActionResult> BaiKiemTra(int id)
        {
            var hocVienIdClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Error("Không xác thực được học viên!");
                return RedirectToAction("Index", "Home");
            }

            var dsThi = await _context.DanhSachThi
                .Include(d => d.KyKiemTra)
                .FirstOrDefaultAsync(x => x.MaHocVien == hocVienId && x.KyKiemTraId == id);

            if (dsThi == null)
            {
                _notyfService.Error("Bạn không có bài cho kỳ kiểm tra này!");
                return RedirectToAction("KyThi");
            }

            Random random = new Random();
            var dsCauHoi = _context.CauHoi_De
                .Where(x => x.De.KyKiemTraId == id)
                .Include(t => t.CauHoi)
                .Include(t => t.De)
                    .ThenInclude(t => t.KyKiemTra)
                .AsEnumerable()
                .OrderBy(_ => random.Next())
                .ToList();

            var baiLamCu = await _context.CauHoi_BaiLam
                .Include(x => x.BaiLam)
                .FirstOrDefaultAsync(x => x.BaiLam.MaHocVien == hocVienId && x.CauHoi_De.De.KyKiemTraId == id);

            var deThi = await _context.CauHoi_De
                .Include(x => x.De)
                    .ThenInclude(d => d.KyKiemTra)
                .FirstOrDefaultAsync(x => x.De.KyKiemTraId == id);

            string tenKy = deThi?.De?.KyKiemTra?.TenKyKiemTra ?? "Kỳ thi";

            if (baiLamCu == null)
            {
                var now = DateTime.UtcNow.AddHours(7);
                var endTime = now.AddMinutes(deThi.De.KyKiemTra.ThoiGianLamBai);

                if (endTime > deThi.De.KyKiemTra.ThoiGianKetThuc)
                    endTime = deThi.De.KyKiemTra.ThoiGianKetThuc;

                var newBaiLam = new BaiLam
                {
                    MaHocVien = hocVienId,
                    ThoiGianBatDau = now,
                    ThoiGianDenHan = endTime
                };
                _context.BaiLam.Add(newBaiLam);
                await _context.SaveChangesAsync();

                var pairs = dsCauHoi.Select(x => new CauHoi_BaiLam
                {
                    BaiLamId = newBaiLam.BaiLamId,
                    CauHoi_DeId = x.CauHoi_DeId
                }).ToList();

                _context.CauHoi_BaiLam.AddRange(pairs);
                await _context.SaveChangesAsync();

                baiLamCu = await _context.CauHoi_BaiLam
                    .Include(x => x.BaiLam)
                    .FirstOrDefaultAsync(x => x.BaiLam.MaHocVien == hocVienId && x.CauHoi_De.De.KyKiemTraId == id);
            }

            ViewBag.kiemtrasv_id = hocVienId;
            ViewBag.TenKiKiemTra = tenKy;
            ViewBag.IdKiKiemTra = id;

            var tgKetThuc = baiLamCu.BaiLam.ThoiGianDenHan ?? DateTime.UtcNow.AddHours(7);
            ViewBag.TimThoigian = tgKetThuc - DateTime.UtcNow.AddHours(7);

            var cauHoi_HocVien = await _context.CauHoi_BaiLam
                .Where(x => x.BaiLam.MaHocVien == hocVienId && x.CauHoi_De.De.KyKiemTraId == id)
                .Include(x => x.CauHoi_De)
                    .ThenInclude(cd => cd.CauHoi)
                .Include(x => x.BaiLam)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.cauhoi_de_mssv = cauHoi_HocVien;
            ViewBag.tg_batdau = baiLamCu.BaiLam.ThoiGianBatDau;
            ViewBag.tg_ketthuc = baiLamCu.BaiLam.ThoiGianDenHan;
            ViewBag.tg_lambai = baiLamCu.BaiLam.ThoiGianDenHan - baiLamCu.BaiLam.ThoiGianBatDau;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(IFormCollection form, int id)
        {
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int maHocVien))
            {
                _notyfService.Error("Không xác thực được học viên!");
                return RedirectToAction("Index", "Home");
            }

            // 🩵 Include sâu để có DapAnDung
            var ds_cauhoi = _context.CauHoi_BaiLam
                .Where(x => x.CauHoi_De.De.KyKiemTraId == id && x.BaiLam.MaHocVien == maHocVien)
                .Include(x => x.BaiLam)
                .Include(x => x.CauHoi_De)
                    .ThenInclude(x => x.CauHoi)
                .Include(x => x.CauHoi_De)
                    .ThenInclude(x => x.De)
                .ToList();

            var trangthai = _context.DanhSachThi
                .FirstOrDefault(x => x.MaHocVien == maHocVien && x.KyKiemTraId == id);

            if (trangthai == null)
            {
                _notyfService.Error("Không tìm thấy thông tin kỳ thi!");
                return RedirectToAction("Index", "Home");
            }

            if (!trangthai.TrangThai)
            {
                foreach (var cauhoi in ds_cauhoi)
                {
                    var dapAnSinhVien = form[cauhoi.CauHoi_DeId.ToString()];
                    cauhoi.DapAnSVChon = string.IsNullOrEmpty(dapAnSinhVien) ? "X" : dapAnSinhVien;

                    // ✅ So sánh trực tiếp với đáp án đúng đã Include
                    bool dung = cauhoi.CauHoi_De.CauHoi?.DapAnDung?.Trim().ToLower() ==
                                cauhoi.DapAnSVChon?.Trim().ToLower();

                    if (dung)
                        cauhoi.BaiLam.SoCauDung++;

                    int tongCauHoi = cauhoi.CauHoi_De.De.SoCauHoi;
                    cauhoi.BaiLam.Diem = (float)cauhoi.BaiLam.SoCauDung / tongCauHoi * 10;
                }

                trangthai.TrangThai = true;
                await _context.SaveChangesAsync();
                _notyfService.Success("Chúc mừng bạn đã nộp bài thành công!");
            }
            else
            {
                _notyfService.Warning("Bài này đã được nộp trước đó!");
            }

            return RedirectToAction("Index", "Home");
        }


        [Route("/HoSo/XemLaiBaiThi")]
        [HttpPost]
        public IActionResult XemLaiBaiThi(int id)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem bài thi!");
                return RedirectToAction("Index", "Home");
            }

            var exBaiLam = _context.BaiLam
                .Include(x => x.CauHoi_BaiLam)
                    .ThenInclude(x => x.CauHoi_De)
                .FirstOrDefault(x => x.MaHocVien == maHocVien &&
                    x.CauHoi_BaiLam.FirstOrDefault().CauHoi_De.De.KyKiemTraId == id);

            if (exBaiLam == null)
            {
                _notyfService.Warning("Không tìm thấy bài làm!");
                return RedirectToAction("HoSo");
            }

            var kyThi = _context.KyKiemTra.FirstOrDefault(x => x.KyKiemTraId == id);
            ViewBag.TenKiKiemTra = kyThi?.TenKyKiemTra ?? "Kỳ kiểm tra";
            ViewBag.kiemtrasv_id = exBaiLam.MaHocVien;

            var cauhoi_de_hocvien = _context.CauHoi_BaiLam
                .Where(x => x.BaiLam.MaHocVien == maHocVien && x.CauHoi_De.De.KyKiemTraId == id)
                .Include(x => x.CauHoi_De)
                    .ThenInclude(x => x.CauHoi)
                .Include(x => x.CauHoi_De)
                    .ThenInclude(x => x.De)
                .ToList();

            ViewBag.cauhoi_de_mssv = cauhoi_de_hocvien;
            ViewBag.tg_batdau = exBaiLam.ThoiGianBatDau;
            ViewBag.tg_ketthuc = exBaiLam.ThoiGianDenHan;

            return View();
        }

    }
}
