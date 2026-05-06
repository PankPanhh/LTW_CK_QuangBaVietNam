using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LTW_CK_QuangBaVietNam.Models
{
    public class FavoritePlaceVM
    {
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
        public string VungMien { get; set; }
        public string AnhChinh { get; set; }
        public DateTime NgayLuu { get; set; }
    }

    public class BoSuuTapListVM
    {
        public int MaBoSuuTap { get; set; }
        public string TenBoSuuTap { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoDiaDiem { get; set; }
    }

    public class BoSuuTapPlaceVM
    {
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
        public string VungMien { get; set; }
        public string AnhChinh { get; set; }
        public DateTime NgayThem { get; set; }
    }

    public class BoSuuTapDetailVM
    {
        public BoSuuTap BoSuuTap { get; set; }
        public List<BoSuuTapPlaceVM> Places { get; set; } = new List<BoSuuTapPlaceVM>();
    }
}