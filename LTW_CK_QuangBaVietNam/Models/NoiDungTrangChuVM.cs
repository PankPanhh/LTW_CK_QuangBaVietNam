 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LTW_CK_QuangBaVietNam.Models
{
    public class NoiDungTrangChuVM
    {
        public List<DiaDiem> DiaDiems { get; set; }
        public List<Banner> Banners { get; set; }
    }

    public class ReviewHistoryVM
    {
        public int MaDanhGia { get; set; }
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
        public int SoSao { get; set; }
        public string NoiDung { get; set; }
        public bool TrangThaiKiemDuyet { get; set; }
        public DateTime NgayGui { get; set; }
    }

    public class ItineraryListVM
    {
        public int MaLichTrinh { get; set; }
        public string TenLichTrinh { get; set; }
        public decimal? TongChiPhiDuKien { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoDiaDiem { get; set; }
    }

    public class ItineraryItemVM
    {
        public DateTime? NgayThamQuan { get; set; }
        public int? ThuTuUuTien { get; set; }
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
        public string VungMien { get; set; }
    }

    public class ItineraryDetailVM
    {
        public int MaLichTrinh { get; set; }
        public string TenLichTrinh { get; set; }
        public decimal? TongChiPhiDuKien { get; set; }
        public DateTime NgayTao { get; set; }
        public List<ItineraryItemVM> Items { get; set; } = new List<ItineraryItemVM>();
    }

    public class DiaDiemRowVM
    {
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
        public string GioMoCua { get; set; }

        public int? MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }

        public string VungMien { get; set; }
        public decimal? GiaVe { get; set; }
        public bool TrangThai { get; set; }
        public DateTime NgayDang { get; set; }
    }
}
