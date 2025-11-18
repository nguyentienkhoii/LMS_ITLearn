using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Globalization;

namespace WebBaiGiang_CKC.Controllers
{
    public class BaiTapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService _noti;
        private readonly ILogger<BaiTapController> _logger;


        public BaiTapController(WebBaiGiangContext context, INotyfService notyf, IWebHostEnvironment env, NotificationService noti,  ILogger<BaiTapController> logger)
        {
            _context = context;
            _notyf = notyf;
            _env = env;
            _noti = noti;
            _logger = logger;

        }

        // ✅ Xem chi tiết bài tập
        public async Task<IActionResult> ChiTiet(int baiTapId)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Warning("Bạn cần đăng nhập để xem chi tiết bài tập!");
                return RedirectToAction("Index", "Home");
            }

            var baiTap = await _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(b => b.MaBaiTap == baiTapId);

            if (baiTap == null)
            {
                _notyf.Error("Không tìm thấy bài tập.");
                return RedirectToAction("Index", "Home");
            }

            // 🧩 Load danh sách lớp học học viên đã đăng ký (để hiển thị sidebar)
            var lopHocDangKy = await _context.HocVien_LopHoc
                .Include(x => x.LopHoc)
                .Where(x => x.MaHocVien == maHocVien)
                .Select(x => x.LopHoc)
                .ToListAsync();

            ViewBag.LopHocDangKy = lopHocDangKy;
            ViewBag.ActiveMenu = "LopHoc";
            ViewBag.CurrentLopHocId = baiTap.Bai?.Chuong?.MaLopHoc;

            // 🧾 Lấy lần nộp gần nhất (nếu có)
            var baiTapNop = await _context.BaiTapNops
                .Where(x => x.MaBaiTap == baiTapId && x.MaHocVien == maHocVien)
                .OrderByDescending(x => x.NgayNop)
                .FirstOrDefaultAsync();

            ViewBag.TrangThaiNop = baiTapNop != null ? "Đã nộp" : "Chưa nộp";
            if (baiTapNop != null)
            {
                if (baiTapNop.Diem != null)
                {
                    var trangThai = "<div class='text-success fw-semibold'>Đã chấm</div>";
                    trangThai += $"<div>Điểm: <strong>{baiTapNop.Diem:0.0}</strong></div>";

                    if (!string.IsNullOrEmpty(baiTapNop.NhanXet))
                        trangThai += $"<div>Nhận xét: <em>{baiTapNop.NhanXet}</em></div>";

                    ViewBag.TrangThaiCham = trangThai;
                }
                else
                {
                    ViewBag.TrangThaiCham = "<span class='text-warning fw-semibold'>Chưa chấm</span>";
                }
            }
            else
            {
                ViewBag.TrangThaiCham = "<span class='text-muted'>Chưa có bài nộp</span>";
            }


            ViewBag.HanChot = baiTap.HanNop?.ToString("dddd, dd 'Tháng' MM yyyy, h:mm tt", new CultureInfo("vi-VN")) ?? "Không có hạn chót";
            ViewBag.BaiTapNop = baiTapNop;


            // ========== Tính toán thời gian ==========
            DateTime now = DateTime.Now;

            DateTime? deadline = baiTap.HanNop;
            DateTime? lateDeadline = baiTap.LateSubmission;

            if (lateDeadline.HasValue && deadline.HasValue && lateDeadline < deadline)
            {
                lateDeadline = null;
            }
            string trangThaiThoiGian = "";   // text hiển thị
            //bool allowSubmit = true;   // điều khiển enable/disable nút nộp bài (CŨ)

            bool allowSubmit;
            if (!deadline.HasValue && !lateDeadline.HasValue)
            {
                // Không cấu hình hạn -> cho nộp
                allowSubmit = true;
            }
            else if (lateDeadline.HasValue)
            {
                // Có hạn nộp muộn -> cho đến lateDeadline
                allowSubmit = now <= lateDeadline.Value;
            }
            else
            {
                // Không có nộp muộn -> cho đến deadline
                allowSubmit = now <= deadline.Value;
            }
            
            string FormatDuration(TimeSpan t) {
                t = t.Duration();
                var parts = new List<string>();
                if (t.Days > 0) parts.Add($"{t.Days} ngày");
                if (t.Hours > 0) parts.Add($"{t.Hours} giờ");
                if (t.Minutes > 0) parts.Add($"{t.Minutes} phút");
                if (parts.Count == 0) parts.Add($"{t.Seconds} giây");
                return string.Join(" ", parts);
            }

            
            if (deadline == null)
            {
                trangThaiThoiGian = "Không có hạn chót";
            }
            else
            {
                // Trường hợp chưa nộp
                if (baiTapNop == null)
                {
                    // CHƯA NỘP + còn hạn
                    if (now < deadline.Value)
                    {
                        var remaining = deadline.Value - now;
                        trangThaiThoiGian = $"Còn lại: <strong>{FormatDuration(remaining)}</strong>";
                    }
                    // Hết hạn chính nhưng có hạn nộp muộn
                    else if (lateDeadline != null && now <= lateDeadline.Value)
                    {
                        var remainingLate = now - deadline.Value;
                         trangThaiThoiGian = $"<span class='text-danger'>Bài tập bị quá hạn: {FormatDuration(remainingLate)}</span>";
                    }
                    // Quá hạn hoàn toàn
                    else
                    {
                        var over = now - (lateDeadline ?? deadline).Value;
                        trangThaiThoiGian = $"<span class='text-danger'>Bài tập bị quá hạn: {FormatDuration(over)} - (Không thể nộp nữa)</span>";
                       // allowSubmit = false;
                    }
                }
                else
                {
                    DateTime submitTime = baiTapNop.NgayNop ?? now;

                    // -----------------------------------------
                    // CASE 1: KHÔNG CÓ HẠN NỘP MUỘN
                    // -----------------------------------------
                    if (lateDeadline == null)
                    {
                        if (submitTime < deadline.Value)
                        {
                            var early = deadline.Value - submitTime;
                            trangThaiThoiGian = $"<span class='text-success fw-semibold'>Bài tập đã nộp sớm: {FormatDuration(early)}</span>";
                        }
                        else
                        {
                            var over = submitTime - deadline.Value;
                            trangThaiThoiGian = $"<span class='text-danger fw-semibold'>Nộp sau hạn {FormatDuration(over)} (không cho phép nộp muộn)</span>";
                            //allowSubmit = false; // ❗ KHÔNG CÓ NỘP MUỘN → KHÓA SỬA LUÔN
                        }
                    }
                    else
                    {
                        // -----------------------------------------
                        // CASE 2: CÓ HẠN NỘP MUỘN
                        // -----------------------------------------
                        if (submitTime < deadline.Value)
                        {
                            var early = deadline.Value - submitTime;
                             trangThaiThoiGian = $"<span class='text-success fw-semibold'>Bài tập đã nộp sớm: {FormatDuration(early)}</span>";
                        }
                        else if (submitTime <= lateDeadline.Value)
                        {
                            var late = submitTime - deadline.Value;
                            trangThaiThoiGian = $"<span class='text-warning fw-semibold'>Bài tập đã nộp muộn: {FormatDuration(late)}</span>";
                        }
                        else
                        {
                            var overLate = submitTime - lateDeadline.Value;
                            trangThaiThoiGian = $"<span class='text-danger fw-semibold'>Nộp sau thời gian cho phép: {FormatDuration(overLate)}</span>";
                             //allowSubmit = false;
                        }
                    }
                }

            }

            ViewBag.TrangThaiThoiGian = trangThaiThoiGian;
            ViewBag.AllowSubmit = allowSubmit;

            return View(baiTap);
        }

        // ✅ Trang nộp bài
        public async Task<IActionResult> NopBai(int baiTapId)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Warning("Bạn cần đăng nhập để nộp bài!");
                return RedirectToAction("Index", "Home");
            }

            var baiTap = await _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(x => x.MaBaiTap == baiTapId);

            if (baiTap == null)
            {
                _notyf.Error("Không tìm thấy bài tập.");
                return RedirectToAction("Index", "Home");
            }

            // 🧩 Load danh sách lớp học học viên đã đăng ký (để hiển thị sidebar)
            var lopHocDangKy = await _context.HocVien_LopHoc
                .Include(x => x.LopHoc)
                .Where(x => x.MaHocVien == maHocVien)
                .Select(x => x.LopHoc)
                .ToListAsync();

            ViewBag.LopHocDangKy = lopHocDangKy;
            ViewBag.ActiveMenu = "LopHoc";
            ViewBag.CurrentLopHocId = baiTap.Bai?.Chuong?.MaLopHoc;

            ViewBag.TenBaiTap = baiTap.TenBaiTap;
            ViewBag.BaiTapId = baiTapId;
            ViewBag.MoTa = baiTap.MoTa;
            
            return View();

        }


        // ✅ Xử lý POST khi nộp bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopBai(int baiTapId, List<IFormFile> files)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Error("Không xác thực được học viên!");
                return RedirectToAction("Index", "Home");
            }

            // 🔍 Load bài tập kèm GiangVien
            var baiTap = await _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                            .ThenInclude(l => l.GiangVien)   // ⬅️ BẮT BUỘC PHẢI CÓ
                .FirstOrDefaultAsync(x => x.MaBaiTap == baiTapId);

            if (baiTap == null)
            {
                _notyf.Error("Không tìm thấy bài tập.");
                return RedirectToAction("Index", "Home");
            }

            if (files == null || !files.Any())
            {
                _notyf.Warning("Vui lòng chọn ít nhất một tệp để nộp.");
                return RedirectToAction("NopBai", new { baiTapId });
            }

                // ========== KIỂM TRA HẠN NỘP ==========
            DateTime now = DateTime.Now;
            DateTime? hanChot = baiTap.HanNop;           // hạn đúng giờ
            DateTime? hanMuon = baiTap.LateSubmission;   // hạn muộn

            if (hanMuon.HasValue && hanChot.HasValue && hanMuon < hanChot)
            {
                hanMuon = null;
            }    

            bool isLate = false; // gắn nhãn cho trạng thái khi được phép nộp
            // Trường hợp KHÔNG có LateSubmission
            if (!hanMuon.HasValue)
            {
                if (hanChot.HasValue && now > hanChot.Value)
                {
                    _notyf.Error("Đã quá hạn nộp, hệ thống không chấp nhận bài nộp sau hạn chót.");
                    return RedirectToAction("ChiTiet", new { baiTapId });
                }
                // else: now <= hanChot (đúng hạn) hoặc không có hanChot (không giới hạn) → cho nộp, isLate = false
            }
            else
            {
                // Có LateSubmission
                if (now > hanMuon.Value)
                {
                    _notyf.Error("Đã quá hạn nộp muộn, hệ thống không chấp nhận bài nộp.");
                    return RedirectToAction("ChiTiet", new { baiTapId });
                }

                // Nếu có HanNop: now > HanNop và ≤ LateSubmission thì coi là nộp muộn
                if (hanChot.HasValue && now > hanChot.Value)
                {
                    isLate = true; // nộp muộn nhưng vẫn cho nộp vì chưa quá LateSubmission
                }
            }

            string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "bainop");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            int lastNopBaiId = 0; // ⭐ biến lưu ID bài nộp mới nhất

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = $"{maHocVien}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var lanNop = await _context.BaiTapNops
                        .CountAsync(x => x.MaBaiTap == baiTapId && x.MaHocVien == maHocVien) + 1;

                    var baiTapNop = new BaiTapNop
                    {
                        MaBaiTap = baiTapId,
                        MaHocVien = maHocVien,
                        FileNop = $"/uploads/bainop/{fileName}",
                        NgayNop = now,
                        LanNop = lanNop,
                        TrangThai = "Đã nộp"
                    };  
                    _context.BaiTapNops.Add(baiTapNop);
                
                    // ⭐ SAU SAVECHANGE() mới lấy được ID
                    await _context.SaveChangesAsync();
                    lastNopBaiId = baiTapNop.MaBaiTapNop; // ⭐ LƯU ID bài nộp
                }
            }

            // =============================
            // 🛎 GỬI THÔNG BÁO — NOTI2
            // =============================

            // 1) Lấy thông tin giảng viên từ bài tập → chương → lớp học
            var lop = baiTap.Bai?.Chuong?.LopHoc;

            if (lop == null)
            {
                Console.WriteLine("Lỗi: LopHoc NULL — kiểm tra mapping Bai → Chuong → LopHoc");
            }
            else if (lop.GiangVien == null)
            {
                Console.WriteLine("Lỗi: LopHoc.GiangVien NULL — kiểm tra MaGiangVien & navigation GiangVien");
            }
            else
            {
                var giangVien = lop.GiangVien;

                //  Dùng MÃ TÀI KHOẢN để gửi SignalR (UserId login)
                int? teacherAccountId = giangVien.MaTaiKhoan;

                if (teacherAccountId == null || teacherAccountId == 0)
                {
                    Console.WriteLine("❌ Lỗi: MaTaiKhoan của giảng viên NULL hoặc 0 — giảng viên chưa có tài khoản?");
                }
                else
                {
                    // 2) Lấy thông tin học viên nộp bài
                    var hocVienInfo = await _context.HocViens
                        .FirstOrDefaultAsync(h => h.MaHocVien == maHocVien);

                    string tenHocVien = hocVienInfo?.HoTen ?? "Học viên";

                    // 3) Gửi thông báo
                    await _noti.SendToTeacher(
                        teacherAccountId.Value,
                        "Học viên nộp bài",
                        $"{tenHocVien} đã nộp bài: {baiTap.TenBaiTap}",
                        2,
                        $"/GiangVien/BaiTap/ChamDiem/{lastNopBaiId}"
                   );

                }
            }
            if (isLate){
                _notyf.Warning("Bạn đã nộp muộn so với hạn chót, bài vẫn được ghi nhận.");
            }
            else{
                _notyf.Success("Nộp bài thành công!"); 
            }
           //_notyf.Success("Nộp bài thành công!");
            return RedirectToAction("ChiTiet", new { baiTapId });
        }

    }
}
