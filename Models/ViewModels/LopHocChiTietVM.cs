using System.Collections.Generic;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Models.ViewModels
{
    public class LopHocChiTietVM
    {
        public LopHoc LopHoc { get; set; }
        public List<ChuongNew> Chuongs { get; set; }
    }
}
