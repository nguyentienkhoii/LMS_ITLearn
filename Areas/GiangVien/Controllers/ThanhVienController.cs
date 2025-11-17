using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class ThanhVienController : GiangVienBaseController
    {
        public ThanhVienController(WebBaiGiangContext context) : base(context)
        {
        }

        // ✅ DANH SÁCH HỌC VIÊN TRONG LỚP
        public async Task<IActionResult> Index(int id)
        {
            var lop = await _context.LopHocs
                .Include(l => l.HocVien_LopHocs)
                    .ThenInclude(hvl => hvl.HocVien)
                        .ThenInclude(hv => hv.TaiKhoan) // 🔹 thêm dòng này để lấy trạng thái TK
                .FirstOrDefaultAsync(l => l.MaLopHoc == id);

            if (lop == null)
                return NotFound("Không tìm thấy lớp.");

            ViewBag.MaLopHoc = id;
            ViewBag.TenLop = lop.TenLopHoc;

            return View(lop.HocVien_LopHocs.Select(hv => hv.HocVien).ToList());
        }


        // ✅ HIỂN THỊ MODAL THÊM HỌC VIÊN
        [HttpGet]
        public async Task<IActionResult> ThemHocVien_Partial(int maLop)
        {
            var lop = await _context.LopHocs
                .Include(l => l.HocVien_LopHocs)
                .FirstOrDefaultAsync(l => l.MaLopHoc == maLop);
            if (lop == null) return NotFound();

            var allHocViens = await _context.HocViens.OrderBy(h => h.HoTen).ToListAsync();

            var model = allHocViens.Select(hv => new HocVienCheckboxVM
            {
                MaHocVien = hv.MaHocVien,
                HoTen = hv.HoTen,
                Email = hv.Email,
                DaTrongLop = lop.HocVien_LopHocs.Any(x => x.MaHocVien == hv.MaHocVien)
            }).ToList();

            ViewBag.MaLopHoc = maLop;
            return PartialView("_ThemHocVienPartial", model);
        }

        // ✅ CẬP NHẬT DANH SÁCH HỌC VIÊN TRONG LỚP
        [HttpPost]
        public async Task<IActionResult> CapNhatHocVienLop([FromBody] CapNhatHocVienLopRequest req)
        {
            if (req == null) return Json(new { success = false });

            var lop = await _context.LopHocs
                .Include(l => l.HocVien_LopHocs)
                .FirstOrDefaultAsync(l => l.MaLopHoc == req.MaLopHoc);
            if (lop == null) return Json(new { success = false });

            var hienTai = lop.HocVien_LopHocs.Select(x => x.MaHocVien).ToList();
            var canThem = req.MaHocViens.Except(hienTai).ToList();
            var canXoa = hienTai.Except(req.MaHocViens).ToList();

            if (canThem.Any())
            {
                _context.HocVien_LopHoc.AddRange(canThem.Select(id => new HocVien_LopHoc
                {
                    MaHocVien = id,
                    MaLopHoc = req.MaLopHoc
                }));
            }

            if (canXoa.Any())
            {
                var removeList = lop.HocVien_LopHocs.Where(x => canXoa.Contains(x.MaHocVien)).ToList();
                _context.HocVien_LopHoc.RemoveRange(removeList);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // ViewModels
        public class HocVienCheckboxVM
        {
            public int MaHocVien { get; set; }
            public string HoTen { get; set; }
            public string Email { get; set; }
            public bool DaTrongLop { get; set; }
        }

        public class CapNhatHocVienLopRequest
        {
            public int MaLopHoc { get; set; }
            public List<int> MaHocViens { get; set; } = new();
        }
    }
}
