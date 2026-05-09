 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LTW_CK_QuangBaVietNam.Models
{

    public class NoiDungTrangChuVM
    {
        public List<DiaDiem> DiaDiems { get; set; }
        
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
        public HttpPostedFileBase AnhChinhFile { get; set; }     
        public HttpPostedFileBase[] AnhPhuFiles { get; set; }
        public string AnhChinh { get; set; }

        public int MaDiaDiem { get; set; }

        public string TenDiaDiem { get; set; }
        public string Slug { get; set; }

        public string MoTaNgan { get; set; }
        public string MoTaChiTiet { get; set; }

        public int? MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }

        public decimal? GiaVe { get; set; }
        public string GioMoCua { get; set; }

        public string VungMien { get; set; }
        public string TinhThanh { get; set; }

        public decimal? KinhDo { get; set; }
        public decimal? ViDo { get; set; }

        public string DiaChiChiTiet { get; set; }

        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }

        public bool TrangThai { get; set; }
        public int? LuotXem { get; set; }
        public double? DiemDanhGiaTB { get; set; }
        public DateTime? NgayDang { get; set; }

        public bool LaDiemChinh { get; set; }
    }
}
