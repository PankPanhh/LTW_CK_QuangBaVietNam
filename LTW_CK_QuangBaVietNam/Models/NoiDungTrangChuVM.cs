using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LTW_CK_QuangBaVietNam.Models
{
    public class NoiDungTrangChuVM
    {
        public List<HomeBlogVM> Blogs { get; set; }
    }

    public class HomeBlogVM
    {
        public int MaBaiViet { get; set; }
        public string TieuDe { get; set; }
        public string NoiDungRutGon { get; set; }
        public string AnhBia { get; set; }
        public string TenTacGia { get; set; }
        public string ThoiGianDang { get; set; }
    }

   
}
