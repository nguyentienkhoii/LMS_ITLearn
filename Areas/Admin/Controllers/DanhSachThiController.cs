
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using WebBaiGiang_CKC.Models.ViewModels;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DanhSachThiController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyfService;

        public DanhSachThiController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // ============================
        // 📋 Danh sách thí sinh theo kỳ thi
        // ============================
        [HttpGet]
        public async Task<IActionResult> ChonKy(string q)
        {
            var kys = await _context.KyKiemTra
                .AsNoTracking()
                .Where(k => string.IsNullOrEmpty(q) || k.TenKyKiemTra.Contains(q))
                .OrderByDescending(k => k.KyKiemTraId) // hoặc OrderByDescending(k => k.NgayBatDau) nếu có
                .ToListAsync();

            ViewBag.Query = q;
            return View(kys); // View: Areas/Admin/Views/DanhSachThi/ChonKy.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> DanhSachTheoKyThi(int kyKiemTraId)
        {
            var danhSach = await _context.DanhSachThi
                .Include(d => d.HocVien)
                .Include(d => d.KyKiemTra)
                .Where(d => d.KyKiemTraId == kyKiemTraId)
                .ToListAsync();

            // Lấy danh sách bài làm tương ứng
            var baiLams = await _context.BaiLam
                .Include(b => b.CauHoi_BaiLam)
                    .ThenInclude(cb => cb.CauHoi_De)
                        .ThenInclude(cd => cd.De)
                .Where(b => b.CauHoi_BaiLam
                    .Any(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId))
                .ToListAsync();

            // Map lại: mỗi học viên lấy số câu đúng và điểm
            foreach (var ds in danhSach)
            {
                var baiLam = baiLams.FirstOrDefault(b => b.MaHocVien == ds.MaHocVien);
                ds.SoCauDung = baiLam?.SoCauDung ?? 0;
                ds.Diem = baiLam?.Diem ?? 0;
            }

            var ky = await _context.KyKiemTra.FindAsync(kyKiemTraId);
            ViewBag.KyKiemTraId = kyKiemTraId;
            ViewBag.TenKy = ky?.TenKyKiemTra ?? "Không có kỳ kiểm tra";

             // Danh sách kỳ để “chuyển kỳ nhanh” trên view
            ViewBag.KyList = await _context.KyKiemTra.AsNoTracking()
            .OrderByDescending(x => x.KyKiemTraId)
            .Select(x => new SelectListItem { Value = x.KyKiemTraId.ToString(), Text = x.TenKyKiemTra })
            .ToListAsync();

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> DanhSachTheoKyThi_Partial(int kyKiemTraId)
        {
            var danhSach = await _context.DanhSachThi
                .AsNoTracking()
                .Include(d => d.HocVien)
                .Include(d => d.KyKiemTra)
                .Where(d => d.KyKiemTraId == kyKiemTraId)
                .ToListAsync();

            var baiLams = await _context.BaiLam
                .AsNoTracking()
                .Include(b => b.CauHoi_BaiLam)
                    .ThenInclude(cb => cb.CauHoi_De)
                        .ThenInclude(cd => cd.De)
                .Where(b => b.CauHoi_BaiLam.Any(cb => cb.CauHoi_De.De.KyKiemTraId == kyKiemTraId))
                .ToListAsync();

            foreach (var ds in danhSach)
            {
                var b = baiLams.FirstOrDefault(x => x.MaHocVien == ds.MaHocVien);
                ds.SoCauDung = b?.SoCauDung ?? 0;
                ds.Diem = b?.Diem ?? 0;
            }

            return PartialView("_DanhSachThiTable", danhSach);
        }


        // ============================
        // Partial quản lý danh sách học viên (checkbox)
        // ============================
        [HttpGet]
        public async Task<IActionResult> QuanLyThiSinh_Partial(int kyKiemTraId)
        {
            var kyThi = await _context.KyKiemTra
                .Include(k => k.DanhSachThi).ThenInclude(ds => ds.HocVien)
                .FirstOrDefaultAsync(k => k.KyKiemTraId == kyKiemTraId);

            if (kyThi == null) return NotFound("Không tìm thấy kỳ kiểm tra");

            var allHocViens = await _context.HocViens
                .OrderBy(h => h.HoTen)
                .ToListAsync();

            var model = allHocViens.Select(hv => new HocVienThiCheckboxVM
            {
                MaHocVien = hv.MaHocVien,
                HoTen = hv.HoTen,
                Email = hv.Email,
                DaTrongDanhSach = kyThi.DanhSachThi.Any(x => x.MaHocVien == hv.MaHocVien)
            }).ToList();

            ViewBag.KyKiemTraId = kyKiemTraId;
            return PartialView("_QuanLyThiSinhPartial", model);
        }


        // ============================
        // Cập nhật danh sách thi (tick/untick)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatDanhSachThi([FromBody] CapNhatDanhSachThiRequest req)
        {
            if (req == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var kyThi = await _context.KyKiemTra
                .Include(k => k.DanhSachThi)
                .FirstOrDefaultAsync(k => k.KyKiemTraId == req.KyKiemTraId);

            if (kyThi == null)
                return Json(new { success = false, message = "Không tìm thấy kỳ thi!" });

            var hienTai = kyThi.DanhSachThi.Select(x => x.MaHocVien).ToList();
            var canThem = req.MaHocViens.Except(hienTai).ToList();
            var canXoa = hienTai.Except(req.MaHocViens).ToList();

            if (canThem.Any())
            {
                var addList = canThem.Select(id => new DanhSachThi
                {
                    KyKiemTraId = req.KyKiemTraId,
                    MaHocVien = id,
                    TrangThai = false
                });
                _context.DanhSachThi.AddRange(addList);
            }

            if (canXoa.Any())
            {
                var removeList = kyThi.DanhSachThi.Where(x => canXoa.Contains(x.MaHocVien)).ToList();
                _context.DanhSachThi.RemoveRange(removeList);
            }

            await _context.SaveChangesAsync();
            _notyfService.Success("Đã cập nhật danh sách thi thành công!");

            return Json(new { success = true, message = "Cập nhật danh sách thi thành công!" });
        }


        // ============================
        // ⚙️ Kiểm tra tồn tại
        // ============================
        private bool DanhSachThiExists(int id)
        {
            return _context.DanhSachThi.Any(e => e.DanhSachThiId == id);
        }
    }
}
