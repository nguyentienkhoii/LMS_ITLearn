using AspNetCoreHero.ToastNotification.Abstractions;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Hubs;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien,Admin")]
    public class BaiTapController : GiangVienBaseController
    {
        private readonly INotyfService _notyf;
        private readonly IHubContext<NotificationsHub> _hub;
        private readonly ILogger<BaiTapController> _logger;

        public BaiTapController(WebBaiGiangContext context, INotyfService notyf, IHubContext<NotificationsHub> hub, ILogger<BaiTapController> logger) : base(context)
        {
            _notyf = notyf;
            _hub = hub;
            _logger = logger;
        }

        // GET: GiangVien/BaiTap/Them?baiId=5&maLopHoc=3
        public async Task<IActionResult> Them(int baiId, int maLopHoc)
        {
            var bai = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(b => b.BaiId == baiId);

            if (bai == null)
                return NotFound("Không tìm thấy bài học.");

            ViewBag.Bai = bai;
            ViewBag.MaLopHoc = maLopHoc;

            return View(new BaiTap { BaiId = baiId });
        }

        // POST: GiangVien/BaiTap/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(BaiTap baiTap, int maLopHoc, IFormFile? FileUpload)
        {
            if (!ModelState.IsValid)
            {
                var bai = await _context.Bai.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.BaiId == baiTap.BaiId);
                ViewBag.Bai = bai;
                ViewBag.MaLopHoc = maLopHoc;
                _notyf.Error("⚠️ Vui lòng nhập đầy đủ thông tin hợp lệ!");
                return View(baiTap);
            }

            // ======================
            // 📌 1. Upload file
            // ======================
            if (FileUpload != null && FileUpload.Length > 0)
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "baitap");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }

                baiTap.FileDinhKem = "/uploads/baitap/" + uniqueFileName;
            }

            // ======================
            // 📌 2. Xử lý nhắc chấm bài (Noti1)
            // ======================

            // Nếu GV không chọn → mặc định nhắc đúng hạn nộp
            if (!baiTap.RemindToGrade.HasValue)
                baiTap.RemindToGrade = baiTap.HanNop;

            baiTap.ReminderSent = false;   // Quan trọng!!!

            // ======================
            // 📌 3. Lưu DB
            // ======================
            _context.BaiTaps.Add(baiTap);
            await _context.SaveChangesAsync();
            // Tạo job nhắc chấm bài
            // Nếu GV không chọn → mặc định nhắc đúng hạn nộp
            if (!baiTap.RemindToGrade.HasValue)
                baiTap.RemindToGrade = baiTap.HanNop;

            // Quan trọng để background service biết chưa gửi
            baiTap.ReminderSent = false;

            _notyf.Success("✅ Đã thêm bài tập mới thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
        }


        // =========================
        // 🔹 SỬA BÀI TẬP
        // =========================

        // GET: GiangVien/BaiTap/Sua?id=10&maLopHoc=3
        public async Task<IActionResult> Sua(int id, int maLopHoc)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập cần sửa.");
                return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
            }

            ViewBag.Bai = baiTap.Bai;
            ViewBag.MaLopHoc = maLopHoc;
            return View(baiTap);
        }

        // POST: GiangVien/BaiTap/Sua
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(BaiTap baiTap, int maLopHoc, IFormFile? FileUpload)
        {
            if (!ModelState.IsValid)
            {
                var bai = await _context.Bai.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.BaiId == baiTap.BaiId);
                ViewBag.Bai = bai;
                ViewBag.MaLopHoc = maLopHoc;
                _notyf.Warning("⚠️ Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
                return View(baiTap);
            }

            var existingBaiTap = await _context.BaiTaps.FindAsync(baiTap.MaBaiTap);
            if (existingBaiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập cần sửa!");
                return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
            }

            // Cập nhật thông tin
            existingBaiTap.TenBaiTap = baiTap.TenBaiTap;
            existingBaiTap.MoTa = baiTap.MoTa;
            existingBaiTap.HanNop = baiTap.HanNop;

            // Nếu có file upload mới
            if (FileUpload != null && FileUpload.Length > 0)
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "baitap");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }

                existingBaiTap.FileDinhKem = "/uploads/baitap/" + uniqueFileName;
            }

            _context.Update(existingBaiTap);
            await _context.SaveChangesAsync();

            _notyf.Success("✅ Đã cập nhật bài tập thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
        }

        // ======================
        // 🔹 XÓA BÀI TẬP
        // ======================
        [HttpGet]
        public async Task<IActionResult> Xoa(int id, int? maLopHoc)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập để xóa!");
                return RedirectToAction("Index", "LopHoc");
            }

            // ✅ Lấy mã lớp học từ bài tập nếu không có trong URL
            int lopHocId = maLopHoc ?? baiTap.Bai.Chuong.MaLopHoc;

            try
            {
                _context.BaiTaps.Remove(baiTap);
                await _context.SaveChangesAsync();
                _notyf.Success("🗑️ Đã xóa bài tập thành công!");
            }
            catch (Exception)
            {
                _notyf.Error("⚠️ Không thể xóa bài tập (có thể đang được liên kết)!");
            }

            // ✅ Quay về đúng lớp học
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = lopHocId });
        }


        // 🔹 DANH SÁCH BÀI NỘP
        // ===========================
        public async Task<IActionResult> DanhSachBaiNop(int id)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập!");
                return RedirectToAction("Index", "LopHoc", new { area = "GiangVien" });
            }

            // ⭐ CHỈ LẤY BÀI NỘP MỚI NHẤT CỦA MỖI HỌC VIÊN
            // Lấy tất cả bài đã nộp của bài tập
            var tatCaNop = await _context.BaiTapNops
                .Include(n => n.HocVien)
                .Where(n => n.MaBaiTap == id)
                .OrderByDescending(n => n.NgayNop)
                .ToListAsync();

            // Chọn bài mới nhất của mỗi học viên
            var danhSachNop = tatCaNop
                .GroupBy(n => n.MaHocVien)
                .Select(g => g.First())     // bài mới nhất do đã OrderBy trước
                .OrderByDescending(n => n.NgayNop)
                .ToList();


            ViewBag.BaiTap = baiTap;
            ViewBag.LopHoc = baiTap.Bai?.Chuong?.LopHoc;
            ViewBag.MaLopHoc = baiTap.Bai?.Chuong?.MaLopHoc;

            return View(danhSachNop);
        }


        // Model/BaiTapNop/SubmissionStatus.cs
        // (Để đây cho tiện xem)
        // public static class SubmissionStatus
        // {
        //     public const string MoiNop = "MOI_NOP";
        //    // public const string DaChamNhap = "DA_CHAM_NHAP"; // nháp
        //     public const string DaChamChot = "DA_CHOT";      // đã công bố & khóa

        //     public static readonly TimeSpan Grace = TimeSpan.FromHours(1);
        //     public static bool IsWithinGrace(string status, DateTime? ngayCham)
        //     {
        //         if (!string.Equals(status, DaChamChot, StringComparison.OrdinalIgnoreCase)) return false;
        //         if (ngayCham == null) return false;
        //         return DateTime.Now < ngayCham.Value.Add(Grace);

        //     }
        //     public static bool IsLocked(string s, DateTime? ngayCham)
        //     {
        //         if (!string.Equals(s, DaChamChot, StringComparison.OrdinalIgnoreCase)) return false;
        //         if (ngayCham == null) return false;
        //         return DateTime.Now >= ngayCham.Value.Add(Grace);
        //     } 
        // }
        // Trang chấm điểm
        [HttpGet]
        public async Task<IActionResult> ChamDiem(int id)
        {
            var baiNop = await _context.BaiTapNops
                .Include(x => x.HocVien)
                .Include(x => x.BaiTap)
                    .ThenInclude(bt => bt.Bai)
                        .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(x => x.MaBaiTapNop == id);

            if (baiNop == null)
            {
                TempData["Error"] = "Không tìm thấy bài nộp!";
                return RedirectToAction("Index");
            }

            if (baiNop.TrangThai == SubmissionStatus.DaChotSoft &&
                baiNop.NgayCham != null &&
                DateTime.Now >= baiNop.NgayCham.Value.Add(SubmissionStatus.Grace))
            {
                baiNop.TrangThai = SubmissionStatus.DaChamChot;
                await _context.SaveChangesAsync();
            }

            ViewBag.MaLopHoc = baiNop.BaiTap?.Bai?.Chuong?.MaLopHoc;
            return View(baiNop);
        }



        //Cập nhật thêm thông báo và chốt điểm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiem(int id, double? diem, string nhanXet)
        {
            var baiNop = await _context.BaiTapNops
                .Include(x => x.BaiTap)
                .FirstOrDefaultAsync(x => x.MaBaiTapNop == id);

            if (baiNop == null)
            {
                TempData["Error"] = "Bài nộp không tồn tại!";
                return RedirectToAction("Index");
            }

            var softLocked = SubmissionStatus.IsSoftLocked(baiNop.TrangThai);
            var withinGrace = SubmissionStatus.IsWithinGrace(baiNop.TrangThai, baiNop.NgayCham);
            var lockedHard = SubmissionStatus.IsLocked(baiNop.TrangThai, baiNop.NgayCham);
            var isReopen    = SubmissionStatus.IsReopened(baiNop.TrangThai);

            // ĐÃ Chốt điểm → chặn
            if (lockedHard)
            {
                _notyf.Warning("Bài nộp đã được chốt điểm. Vui lòng gửi yêu cầu mở khóa cho quản trị viên.");
                return RedirectToAction(nameof(ChamDiem), new { id });
            }

            var hocVienUserId = await _context.HocViens
                .Where(h => h.MaHocVien == baiNop.MaHocVien)
                .Select(h => h.MaTaiKhoan) // string Id của AspNetUsers
                .FirstOrDefaultAsync();

            // var link = Url.Action("ChiTiet","BaiTap", new { area = "", baiTapId = baiNop.MaBaiTap }, Request.Scheme);
            // _logger?.LogInformation("Notify link: {Link}", link);

            object BuildPayload(string title, string msg) => new
            {
                title,
                message = msg,
                link = Url.Action("ChiTiet", "BaiTap", new { area = "", baiTapId = baiNop.MaBaiTap }, Request.Scheme),
                createdAt = DateTime.UtcNow
            };

            if (!softLocked && !isReopen && !withinGrace)
            {
                baiNop.Diem = diem;
                baiNop.NhanXet = (nhanXet ?? "").Trim();
                baiNop.TrangThai = SubmissionStatus.DaChotSoft;
                baiNop.NgayCham = DateTime.Now;

                _context.Update(baiNop);
                await _context.SaveChangesAsync();

                if (hocVienUserId > 0)
                {
                    await _hub.Clients.User(hocVienUserId.ToString())
                        .SendAsync("ReceiveNotification",
                          BuildPayload("Điểm bài tập đã được công bố",
                                       $"{baiNop.BaiTap?.TenBaiTap}: {baiNop.Diem: 0.##}/10"));
                }
                _notyf.Success("Chấm điểm thành công. Giảng viên có thể chỉnh sửa trong thời gian cho phép !");
                return RedirectToAction("DanhSachBaiNop", new { id = baiNop.MaBaiTap });
            }

            if (withinGrace)
            {
                // CẬP NHẬT TRONG 1H (KHÔNG đổi NgayCham)
                baiNop.Diem = diem;
                baiNop.NhanXet = (nhanXet ?? "").Trim();

                _context.Update(baiNop);
                await _context.SaveChangesAsync();

                if (hocVienUserId > 0)
                {
                    await _hub.Clients.User(hocVienUserId.ToString())
                            .SendAsync("ReceiveNotification",
                                BuildPayload($"Điểm bài tập đã được cập nhật",
                                            $"{baiNop.BaiTap?.TenBaiTap}: {baiNop.Diem:0.##}/10 (đã cập nhật)"));

                }

                _notyf.Success("Đã cập nhật trong thời gian cho phép.");
                return RedirectToAction(nameof(ChamDiem), new { id });
            }

            if (isReopen)
            {
                baiNop.Diem     = diem;
                baiNop.NhanXet  = (nhanXet ?? "").Trim();
                baiNop.TrangThai = SubmissionStatus.DaChamChot;

                // Mẹo để khóa ngay: set NgayCham lùi quá khoảng ân hạn
                baiNop.NgayCham  = DateTime.Now.Subtract(SubmissionStatus.Grace);

                _context.Update(baiNop);
                await _context.SaveChangesAsync();

                if (hocVienUserId > 0)
                {
                    await _hub.Clients.User(hocVienUserId.ToString())
                        .SendAsync("ReceiveNotification",
                            BuildPayload("Điểm bài tập đã được phúc khảo lại",
                                        $"{baiNop.BaiTap?.TenBaiTap}: {baiNop.Diem:0.##}/10 "));
                }

                _notyf.Success("Đã chốt lại điểm sau khi mở khóa.");
                return RedirectToAction(nameof(ChamDiem), new { id });
            }

                // Phòng hờ (không rơi vào nhánh nào)
            _notyf.Warning("Không thể cập nhật nữa");
            return RedirectToAction(nameof(ChamDiem), new { id });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult YeuCauMoKhoa(int id)
        {
            // TODO: Ghi log/ gửi Noti/ Email cho Admin nếu có hệ thống thông báo
            _notyf.Success("Đã gửi yêu cầu mở khóa tới quản trị viên.");
            return RedirectToAction(nameof(ChamDiem), new { id });
    
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoaBaiNop(int id)
        {
            var b = await _context.BaiTapNops.FindAsync(id);
            if (b == null) return NotFound();

            b.TrangThai = SubmissionStatus.ReOpened; // mở để chấm lại
            b.NgayCham = null;                                               
            await _context.SaveChangesAsync();

            _notyf.Success("Đã mở khóa bài nộp, giảng viên có thể chấm lại.");
            return RedirectToAction(nameof(ChamDiem), new { id });
        }

        public async Task<IActionResult> LichSuNop(int hocVienId, int baiTapId)
        {
            var hocVien = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .FirstOrDefaultAsync(h => h.MaHocVien == hocVienId);

            if (hocVien == null)
                return NotFound();

            // Lấy lịch sử nộp (tất cả các bản ghi)
            var lichSu = await _context.BaiTapNops
                .Where(n => n.MaHocVien == hocVienId && n.MaBaiTap == baiTapId)
                .OrderByDescending(n => n.NgayNop)
                .ToListAsync();

            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == baiTapId);

            ViewBag.HocVien = hocVien;
            ViewBag.BaiTap = baiTap;

            // ⭐ lấy bài nộp mới nhất để quay lại đúng trang chấm điểm
            ViewBag.MaBaiTapNopMoiNhat = lichSu.FirstOrDefault()?.MaBaiTapNop;

            return View(lichSu);
        }


    }
}
