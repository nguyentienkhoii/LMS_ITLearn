using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class ChuongController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public ChuongController(WebBaiGiangContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ThemAjax([FromForm] ChuongNew model)
        {
            if (string.IsNullOrWhiteSpace(model.TenChuong))
                return Json(new { success = false, message = "Tên chương không được để trống." });

            _context.ChuongNews.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    id = model.MaChuong,
                    tenChuong = model.TenChuong
                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> SuaAjax([FromForm] ChuongNew model)
        {
            if (string.IsNullOrWhiteSpace(model.TenChuong))
                return Json(new { success = false, message = "Tên chương không được để trống." });

            var chuong = await _context.ChuongNews.FindAsync(model.MaChuong);
            if (chuong == null)
                return Json(new { success = false, message = "Không tìm thấy chương." });

            chuong.TenChuong = model.TenChuong;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XoaAjax(int id)
        {
            var chuong = await _context.ChuongNews.FindAsync(id);
            if (chuong == null)
                return Json(new { success = false, message = "Không tìm thấy chương." });

            _context.ChuongNews.Remove(chuong);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }



    }
}
