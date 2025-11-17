using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class BaiController : GiangVienBaseController
    {
        public BaiController(WebBaiGiangContext context) : base(context)
        {
        }

        [HttpPost]
        public async Task<IActionResult> ThemAjax([FromForm] Bai model)
        {
            if (string.IsNullOrWhiteSpace(model.TenBai))
                return Json(new { success = false, message = "Tên bài không được để trống." });

            if (model.SoBai <= 0)
                return Json(new { success = false, message = "Số bài phải lớn hơn 0." });

            if (string.IsNullOrWhiteSpace(model.MoTa))
                return Json(new { success = false, message = "Mô tả không được để trống." });

            _context.Bai.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    id = model.BaiId,
                    tenBai = model.TenBai,
                    soBai = model.SoBai,
                    moTa = model.MoTa
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var bai = await _context.Bai.FindAsync(id);
            if (bai == null)
                return Json(new { success = false, message = "Không tìm thấy bài." });

            return Json(new
            {
                success = true,
                data = new
                {
                    baiId = bai.BaiId,
                    tenBai = bai.TenBai,
                    moTa = bai.MoTa
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> SuaAjax(int BaiId, string TenBai, string MoTa)
        {
            var bai = await _context.Bai.FindAsync(BaiId);
            if (bai == null)
                return Json(new { success = false, message = "Không tìm thấy bài." });

            bai.TenBai = TenBai;
            bai.MoTa = MoTa;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> XoaAjax(int id)
        {
            var bai = await _context.Bai.FindAsync(id);
            if (bai == null)
                return Json(new { success = false, message = "Không tìm thấy bài." });

            _context.Bai.Remove(bai);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}
