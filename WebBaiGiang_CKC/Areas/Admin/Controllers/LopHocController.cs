using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LopHocController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public LopHocController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // 📜 Danh sách lớp học
        public async Task<IActionResult> Index()
        {
            var list = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .OrderByDescending(l => l.MaLopHoc)
                .ToListAsync();

            return View(list);
        }

        // ➕ GET: Thêm lớp học
        public IActionResult Create()
        {
            ViewBag.KhoaHocList = _context.KhoaHocs.ToList();
            ViewBag.GiangVienList = _context.GiangViens.ToList();
            return View();
        }

        // ➕ POST: Thêm lớp học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LopHoc model)
        {
            if (ModelState.IsValid)
            {
                _context.LopHocs.Add(model);
                await _context.SaveChangesAsync();
                _notyf.Success("Thêm lớp học thành công!");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhoaHocList = _context.KhoaHocs.ToList();
            ViewBag.GiangVienList = _context.GiangViens.ToList();
            return View(model);
        }

        // ✏️ GET: Sửa
        public async Task<IActionResult> Edit(int id)
        {
            var lop = await _context.LopHocs.FindAsync(id);
            if (lop == null) return NotFound();

            ViewBag.KhoaHocList = _context.KhoaHocs.ToList();
            ViewBag.GiangVienList = _context.GiangViens.ToList();

            return View(lop);
        }

        // ✏️ POST: Sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LopHoc model)
        {
            if (id != model.MaLopHoc) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật lớp học thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LopHocs.Any(e => e.MaLopHoc == id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhoaHocList = _context.KhoaHocs.ToList();
            ViewBag.GiangVienList = _context.GiangViens.ToList();
            return View(model);
        }

        // ❌ GET: Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var lop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .FirstOrDefaultAsync(l => l.MaLopHoc == id);

            if (lop == null) return NotFound();
            return View(lop);
        }

        // ❌ POST: Xác nhận xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lop = await _context.LopHocs.FindAsync(id);
            if (lop != null)
            {
                _context.LopHocs.Remove(lop);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa lớp học thành công!");
            }
            return RedirectToAction(nameof(Index));
        }

        // =============================
        // 📘 CHI TIẾT LỚP HỌC
        // =============================
        public async Task<IActionResult> ChiTiet(int id)
        {
            var lop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Include(l => l.HocVien_LopHocs)
                    .ThenInclude(hv => hv.HocVien)
                .FirstOrDefaultAsync(l => l.MaLopHoc == id);

            if (lop == null)
                return NotFound();

            return View(lop);
        }

        // =============================
        // 🎓 HIỂN THỊ MODAL THÊM HỌC VIÊN
        // =============================
        [HttpGet]
        public async Task<IActionResult> ThemHocVien_Partial(int maLop)
        {
            var lop = await _context.LopHocs
                .Include(l => l.HocVien_LopHocs)
                .FirstOrDefaultAsync(l => l.MaLopHoc == maLop);

            if (lop == null) return NotFound();

            // Lấy danh sách tất cả học viên
            var allHocViens = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .OrderBy(h => h.HoTen)
                .ToListAsync();

            // Tạo danh sách view model: mỗi học viên kèm flag đã trong lớp
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

        [HttpPost]
        public async Task<IActionResult> CapNhatHocVienLop([FromBody] CapNhatHocVienLopRequest req)
        {
            if (req == null)
                return Json(new { success = false, message = "Yêu cầu không hợp lệ" });

            var lop = await _context.LopHocs
                .Include(l => l.HocVien_LopHocs)
                .FirstOrDefaultAsync(l => l.MaLopHoc == req.MaLopHoc);

            if (lop == null)
                return Json(new { success = false, message = "Không tìm thấy lớp" });

            // Lấy danh sách hiện tại
            var hienTai = lop.HocVien_LopHocs.Select(x => x.MaHocVien).ToList();

            // Ai mới tick (chưa có) -> thêm
            var canThem = req.MaHocViens.Except(hienTai).ToList();
            // Ai bỏ tick (đang có) -> xóa
            var canXoa = hienTai.Except(req.MaHocViens).ToList();

            if (canThem.Any())
            {
                var addList = canThem.Select(id => new HocVien_LopHoc { MaHocVien = id, MaLopHoc = req.MaLopHoc });
                _context.HocVien_LopHoc.AddRange(addList);
            }

            if (canXoa.Any())
            {
                var removeList = lop.HocVien_LopHocs.Where(x => canXoa.Contains(x.MaHocVien)).ToList();
                _context.HocVien_LopHoc.RemoveRange(removeList);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã lưu thay đổi danh sách học viên!" });
        }

        // ViewModel phụ trợ
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


        // ➕ THÊM HỌC VIÊN VÀO LỚP (AJAX)
        [HttpPost]
        public async Task<IActionResult> ThemHocVien_Ajax([FromBody] HocVien_LopHoc model)
        {
            if (await _context.HocVien_LopHoc.AnyAsync(x => x.MaHocVien == model.MaHocVien && x.MaLopHoc == model.MaLopHoc))
            {
                return Json(new { success = false, message = "Học viên đã có trong lớp này!" });
            }

            _context.HocVien_LopHoc.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm học viên vào lớp thành công!" });
        }

        // ❌ XOÁ HỌC VIÊN KHỎI LỚP (AJAX)
        [HttpPost]
        public async Task<IActionResult> XoaHocVien_Ajax([FromBody] HocVien_LopHoc model)
        {
            var hvLop = await _context.HocVien_LopHoc
                .FirstOrDefaultAsync(x => x.MaHocVien == model.MaHocVien && x.MaLopHoc == model.MaLopHoc);

            if (hvLop == null)
                return Json(new { success = false, message = "Không tìm thấy học viên trong lớp." });

            _context.HocVien_LopHoc.Remove(hvLop);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa học viên khỏi lớp." });
        }
    }
}
