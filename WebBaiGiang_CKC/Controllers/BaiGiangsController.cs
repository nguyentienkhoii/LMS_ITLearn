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
        public IActionResult DeCuong(int monHocId)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            var taiKhoanIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId");
            if (taiKhoanIdClaim == null)
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem đề cương!");
                return RedirectToAction("Index");
            }

            // Chuyển đổi TaiKhoanId từ string sang int
            if (!int.TryParse(taiKhoanIdClaim.Value, out int userTaiKhoanId))
            {
                _notyfService.Error("Lỗi xác thực tài khoản!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem môn học có tồn tại không
            var monHoc = _context.MonHoc.AsNoTracking().FirstOrDefault(m => m.MonHocId == monHocId);
            if (monHoc == null)
            {
                _notyfService.Error("Môn học không tồn tại!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem người dùng đã đăng ký môn học này chưa
            bool isAlreadyRegistered = _context.DangKyMonHoc
                .Any(dkm => dkm.TaiKhoanId == userTaiKhoanId && dkm.MonHocId == monHocId);

            if (!isAlreadyRegistered)
            {
                _notyfService.Warning("Bạn chưa đăng ký môn học này!");
                return RedirectToAction("Index");
            }

            var deCuong = _context.DeCuong
                .Where(dc => dc.MonHocId == monHocId)
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.SelectedMonHoc = monHoc;
            ViewBag.MonHocId = monHocId;

            return View(deCuong);
        }



        public IActionResult NoiDungChinh()
        {
            return View();
        }
        public IActionResult GiaoVien()
        {
            var giaovien = _context.GiaoVien.ToList();
            ViewBag.Giaovien = giaovien;
            return View();
        }
        [Route("MonHoc/{monHocId}/Chuong/{chuongId}/Bai/{baiId}")]
        public IActionResult Bai(int monHocId, int chuongId, int baiId)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            var taiKhoanIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId");
            if (taiKhoanIdClaim == null)
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem bài học!");
                return RedirectToAction("Index", "Home");
            }

            // Chuyển đổi TaiKhoanId từ string sang int
            if (!int.TryParse(taiKhoanIdClaim.Value, out int userTaiKhoanId))
            {
                _notyfService.Error("Lỗi xác thực tài khoản!");
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem môn học có tồn tại không
            var monHoc = _context.MonHoc
                .Include(m => m.Chuongs)
                .ThenInclude(c => c.Bais)
                .FirstOrDefault(m => m.MonHocId == monHocId);

            if (monHoc == null)
            {
                _notyfService.Error("Môn học không tồn tại!");
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem người dùng đã đăng ký môn học này chưa
            bool isAlreadyRegistered = _context.DangKyMonHoc
                .Any(dkm => dkm.TaiKhoanId == userTaiKhoanId && dkm.MonHocId == monHocId);

            if (!isAlreadyRegistered)
            {
                _notyfService.Warning("Bạn chưa đăng ký môn học này!");
                return RedirectToAction("Index", "Home");
            }

            var lstBai = _context.Bai
                .Where(x => x.BaiId == baiId && x.ChuongId == chuongId && x.Chuong.MonHocId == monHocId)
                .Include(a => a.Mucs)
                .AsNoTracking()
                .ToList();

            // Tránh lỗi nếu danh sách bài giảng không có mục nào
            if (lstBai.Any() && lstBai.First().Mucs.Any())
            {
                lstBai = lstBai.OrderByDescending(x => x.Mucs.Min(y => y.MucSo)).ToList();
            }

            if (monHoc != null)
            {
                ViewBag.SelectedMonHoc = monHoc; // Gán môn học để menu hiển thị
            }

            return View(lstBai);
        }


        public IActionResult Lich()
        {

            return View();
        }
        [Route("MonHoc/{monHocId}/BaiTap")]
        public IActionResult BaiTap(int monHocId)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            var taiKhoanIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId");
            if (taiKhoanIdClaim == null)
            {
                _notyfService.Warning("Bạn cần đăng nhập để xem bài tập!");
                return RedirectToAction("Index");
            }

            // Chuyển đổi TaiKhoanId từ string sang int
            if (!int.TryParse(taiKhoanIdClaim.Value, out int userTaiKhoanId))
            {
                _notyfService.Error("Lỗi xác thực tài khoản!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem môn học có tồn tại không
            var monHoc = _context.MonHoc.AsNoTracking().FirstOrDefault(m => m.MonHocId == monHocId);
            if (monHoc == null)
            {
                _notyfService.Error("Môn học không tồn tại!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem người dùng đã đăng ký môn học này chưa
            bool isAlreadyRegistered = _context.DangKyMonHoc
                .Any(dkm => dkm.TaiKhoanId == userTaiKhoanId && dkm.MonHocId == monHocId);

            if (!isAlreadyRegistered)
            {
                _notyfService.Warning("Bạn chưa đăng ký môn học này!");
                return RedirectToAction("Index");
            }

            var baiTaps = _context.BaiTap
                .Where(bt => bt.MonHocId == monHocId)
                .AsNoTracking()
                .ToList();

            ViewBag.SelectedMonHoc = monHoc;
            ViewBag.MonHocId = monHocId;

            return View(baiTaps);
        }

        [Route("BaiGiangs/DetailBaiTap/{baiTapId}")]
        public IActionResult DetailBaiTap(int baiTapId)
        {
            var baiTap = _context.BaiTap
                .Include(bt => bt.MonHoc) // Bao gồm môn học liên quan
                .FirstOrDefault(bt => bt.BaiTapId == baiTapId); // Tìm bài tập theo id

            if (baiTap == null)
            {
                return NotFound(); // Nếu không tìm thấy bài tập, trả về trang lỗi 404
            }

            ViewBag.SelectedMonHoc = baiTap.MonHoc; // Truyền môn học vào ViewBag
            return View(baiTap); // Trả về View cùng với dữ liệu bài tập
        }




        [Route("/HoSo")]
        public IActionResult HoSo()
        {
         
            var kikiemtra = _context.DanhSachThi.Include(t => t.TaiKhoan).Include(x => x.KyKiemTra).ThenInclude(x => x.De).ThenInclude(x => x.CauHoi_DeThi).ThenInclude(x => x.CauHoi_BaiLam).ThenInclude(x => x.BaiLam).ToList();
            ViewBag.kiemtra = kikiemtra;
            return View();
        }
        public IActionResult NopBai()
        {

            return View();
        }
        public IActionResult KyThi()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _notyfService.Warning("Vui lòng đăng nhập để tham gia kỳ thi.");
                return RedirectToAction("Index", "Home");
            }

            var kikiemtra = _context.DanhSachThi
                .Include(t => t.TaiKhoan)
                .Include(x => x.KyKiemTra)
                    .ThenInclude(x => x.De)
                    .ThenInclude(x => x.CauHoi_DeThi)
                    .ThenInclude(x => x.CauHoi_BaiLam)
                    .ThenInclude(x => x.BaiLam)
                .ToList();

            ViewBag.kiemtra = kikiemtra;
            return View();
        }

        [Route("/BaiGiangs/KyThi/BaiKiemTra")]
        [HttpPost]
        public async Task<IActionResult> BaiKiemTra(int id)
        {
            var mssvclaim = User.Claims.FirstOrDefault(c => c.Type == "MSSV");
            var mssv_ =mssvclaim.Value;
            var kt_kikiemtra = _context.DanhSachThi.FirstOrDefault(x=>x.TaiKhoan.MSSV == mssv_ && x.KyKiemTraId == id);
            if(kt_kikiemtra !=null)
            { 
                // lấu câu hỏi đã chọn và ramdom
                Random random = new Random(DateTime.Now.Millisecond);
                var ds_cauhoi = _context.CauHoi_De.Where(x => x.De.KyKiemTra.KyKiemTraId == id).Include(t => t.CauHoi).Include(t => t.De).ThenInclude(t => t.KyKiemTra).AsEnumerable().OrderBy(x => random.Next()).ToList();
                // trung vấn bài làm & kiểm tra điều kiệm mssv , 
                var kiemtrabailam = await _context.CauHoi_BaiLam.Include(x => x.BaiLam).FirstOrDefaultAsync(x => x.BaiLam.MSSV == mssv_ && x.CauHoi_De.De.KyKiemTraId == id);
                //tìm kiếm kiều kiện để add dữ liệu tại câu trên truy vấn bảng chưa có dữ liệu
                var thoigiandenhan = await _context.CauHoi_De.FirstOrDefaultAsync(x => x.De.KyKiemTraId == id);
                var tenkikiem = "";
                tenkikiem = thoigiandenhan.De.KyKiemTra.TenKyKiemTra;
                if (kiemtrabailam == null)
                {
                    // Kiểm tra xem đã thêm 'BaiLam' cho 'De' này chưa
                    DateTime currentTime = DateTime.UtcNow.AddHours(7);
                    DateTime startDateTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
                    DateTime updatedStartDateTime = startDateTime.AddMinutes(thoigiandenhan.De.KyKiemTra.ThoiGianLamBai);
                    var newbai = new BaiLam
                    {
                        MSSV = mssv_,
                        HoTen = User.Identity.Name,
                        ThoiGianBatDau = startDateTime,
                        ThoiGianDenHan = (updatedStartDateTime < thoigiandenhan.De.KyKiemTra.ThoiGianKetThuc) ? updatedStartDateTime : thoigiandenhan.De.KyKiemTra.ThoiGianKetThuc,
                    };
                    _context.BaiLam.Add(newbai);
                    await _context.SaveChangesAsync();
                    // 
                    var baiLamId = newbai.BaiLamId;
                    // add bang cauhoi_bailam
                    var cauHoiBaiLamListb = ds_cauhoi.Select(x => new CauHoi_BaiLam
                    {
                        BaiLamId = baiLamId,
                        CauHoi_DeId = x.CauHoi_DeId
                    }).ToList();
                    _context.CauHoi_BaiLam.AddRange(cauHoiBaiLamListb);
                    await _context.SaveChangesAsync();

                }
                var newexistingBaiLam = await _context.CauHoi_BaiLam.FirstOrDefaultAsync(x => x.BaiLam.MSSV == mssv_ && x.CauHoi_De.De.KyKiemTraId == id);
                ViewBag.kiemtrasv_id = newexistingBaiLam.BaiLam.MSSV;
                ViewBag.TenKiKiemTra = tenkikiem;
                ViewBag.IdKiKiemTra = id;
                // thời gian đếm ngược khi làm bài tự nộp 
                var tgbd = DateTime.Now;
                var tg_kt = newexistingBaiLam.BaiLam.ThoiGianDenHan;
                TimeSpan timeSpan = (DateTime)tg_kt - (DateTime)tgbd;
                ViewBag.TimThoigian = timeSpan;
                ///thoi gian lam bai cua sv truy vấn ở đây chờ ở trên add dữ liệu mới có mà sài 
                var cauhoi_de_mssv = await _context.CauHoi_BaiLam.Where(x => x.BaiLam.MSSV == mssv_ && x.CauHoi_De.De.KyKiemTraId == id).Include(x => x.BaiLam).Include(x => x.CauHoi_De).ToListAsync();
                ViewBag.cauhoi_de_mssv = cauhoi_de_mssv;
                DateTime? tg_batdau = newexistingBaiLam.BaiLam.ThoiGianBatDau;
                DateTime? tg_ketthuc = newexistingBaiLam.BaiLam.ThoiGianDenHan;
                ViewBag.tg_batdau = tg_batdau;
                ViewBag.tg_ketthuc = tg_ketthuc;

            }   
            else {
                _notyfService.Error("Sinh viên không có bài cho kỳ kiểm tra này");
                return RedirectToAction("KyThi", "BaiGiangs");
            }
           
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> NopBai(IFormCollection form, int id)
        {
            var mssvClaim = User.Claims.SingleOrDefault(c => c.Type == "MSSV");
            var mssv = mssvClaim?.Value;

            var ds_cauhoi = _context.CauHoi_BaiLam.Where(x => x.CauHoi_De.De.KyKiemTraId == id && x.BaiLam.MSSV == mssv).Include(t => t.BaiLam).Include(t => t.CauHoi_De).ThenInclude(x => x.De).ToList();

            // lấy danh sách câu hỏi trong đề kiểm tra
            var trangthai = _context.DanhSachThi.FirstOrDefault(x => x.TaiKhoan.MSSV == mssv && x.KyKiemTraId == id);

            if (trangthai.TrangThai == false)
            {
                foreach (var cauhoi in ds_cauhoi)
                {
                    var dapAnSinhVien = form[cauhoi.CauHoi_DeId.ToString()];
                    var baiLam = await _context.CauHoi_BaiLam.Include(x => x.BaiLam).Include(x => x.CauHoi_De).ThenInclude(x => x.CauHoi)
                        .FirstOrDefaultAsync(y => y.CauHoi_De.De.KyKiemTraId == id && y.BaiLam.MSSV == mssv && y.CauHoi_DeId == cauhoi.CauHoi_DeId);
                    cauhoi.DapAnSVChon = string.IsNullOrEmpty(dapAnSinhVien) ? "X" : dapAnSinhVien;
                    int socaudung = cauhoi.BaiLam.CauHoi_BaiLam
                  .Count(cd => cd.CauHoi_De.CauHoi?.DapAnDung?.ToLower() == cd.DapAnSVChon?.ToLower());
                    cauhoi.BaiLam.SoCauDung = socaudung;
                    cauhoi.BaiLam.Diem = socaudung > 0 ? (float)socaudung / cauhoi.CauHoi_De.De.SoCauHoi * 10 : 0;
                }
                trangthai.TrangThai = true;
                await _context.SaveChangesAsync();
                _notyfService.Success("Chúc mừng bạn nộp bài thành công!!!");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _notyfService.Warning("Chúc mừng bạn bài này đã được nộp trước đó !!!");
                return RedirectToAction("Index", "Home");
            }

        }
        [Route("/HoSo/XemLaiBaiThi")]
        [HttpPost]
        public IActionResult XemLaiBaiThi(int id)
        {
            var mssvclaim = User.Claims.FirstOrDefault(c => c.Type == "MSSV");
            var mssv_ = "";
            if (mssvclaim != null)
            {
                mssv_ = mssvclaim.Value;
            }
            var exBaiLam = _context.BaiLam.Include(x => x.CauHoi_BaiLam).ThenInclude(x => x.CauHoi_De).FirstOrDefault(x => x.MSSV == mssv_ && x.CauHoi_BaiLam.FirstOrDefault().CauHoi_De.De.KyKiemTraId == id);
            //tìm kiếm kiều kiện để add dữ liệu 
            var thoigiandenhan = _context.KyKiemTra.FirstOrDefault(x => x.KyKiemTraId == id);
            var tenkikiem = "";
            tenkikiem = thoigiandenhan.TenKyKiemTra;
            @ViewBag.TenKiKiemTra = tenkikiem;
            ///thoi gian lam bai cua sv 
            @ViewBag.kiemtrasv_id = exBaiLam.MSSV;
            var cauhoi_de_mssv = _context.CauHoi_BaiLam.Where(x => x.BaiLam.MSSV == mssv_ && x.CauHoi_De.De.KyKiemTraId == id).Include(x => x.CauHoi_De).ThenInclude(x => x.CauHoi).ThenInclude(x => x.CauHoi_De).ThenInclude(x => x.De).ToList();
            ViewBag.cauhoi_de_mssv = cauhoi_de_mssv;
            DateTime? tg_batdau = exBaiLam.ThoiGianBatDau;
            DateTime? tg_ketthuc = exBaiLam.ThoiGianDenHan;
            ViewBag.tg_batdau = tg_batdau;
            ViewBag.tg_ketthuc = tg_ketthuc;
            return View();
        }
    }
}
