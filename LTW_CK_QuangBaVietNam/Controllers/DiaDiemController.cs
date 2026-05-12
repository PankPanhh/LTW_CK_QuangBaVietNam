using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class DiaDiemController : Controller
    {
        private readonly DataClasses1DataContext db = new DataClasses1DataContext(GetConnectionString());

        private static string GetConnectionString()
        {
            var appConnection = ConfigurationManager.ConnectionStrings["QBConnectionString"];
            if (appConnection != null && !string.IsNullOrWhiteSpace(appConnection.ConnectionString))
            {
                return appConnection.ConnectionString;
            }

            var defaultConnection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (defaultConnection != null && !string.IsNullOrWhiteSpace(defaultConnection.ConnectionString))
            {
                return defaultConnection.ConnectionString;
            }

            throw new ConfigurationErrorsException("Missing connection string.");
        }

        /// <summary>
        /// Lấy danh sách tất cả địa điểm (cho trang AllPlaces)
        /// </summary>
        public ActionResult GetAllPlaces(string vungMien = "", int page = 1, int pageSize = 9)
        {
            try
            {
                var query = db.DiaDiems.Where(d => d.TrangThai == true && d.LaDiemChinh == true);
                // Filter theo vùng miền nếu có
                if (!string.IsNullOrEmpty(vungMien))
                {
                    query = query.Where(d => d.VungMien.Contains(vungMien));
                }

                // Pagination
                int totalCount = query.Count();
                var places = query
                    .OrderByDescending(d => d.NgayDang)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
                        d.MoTaNgan,
                        d.DiaChiChiTiet,
                        d.VungMien,
                        d.TinhThanh,
                        d.KinhDo,
                        d.ViDo,
                        d.GiaVe,
                        AnhChinh = db.AnhDiaDiems
                            .Where(a => a.MaDiaDiem == d.MaDiaDiem && a.LaAnhChinh == true)
                            .Select(a => a.DuongDanAnh)
                            .FirstOrDefault(),
                        DanhMuc = d.DanhMuc.TenDanhMuc,
                        d.DiemDanhGiaTB,
                        d.LuotXem
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = places,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    currentPage = page
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy chi tiết một địa điểm theo slug (cho trang DetailPlace)
        /// </summary>
        public ActionResult GetPlaceDetail(string slug)
        {
            try
            {
                var place = db.DiaDiems.FirstOrDefault(d => d.Slug == slug && d.TrangThai == true);

                if (place == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);
                }

                // Tăng lượt xem
                place.LuotXem = (place.LuotXem ?? 0) + 1;
                db.SubmitChanges();

                // Lấy ảnh
                var images = db.AnhDiaDiems
                    .Where(a => a.MaDiaDiem == place.MaDiaDiem)
                    .Select(a => a.DuongDanAnh)
                    .ToList();

                // Lấy giá vé chi tiết
                var prices = db.GiaVeChiTiets
                    .Where(g => g.MaDiaDiem == place.MaDiaDiem)
                    .Select(g => new
                    {
                        g.MaGiaVe,
                        g.LoaiKhach,
                        g.Gia
                    })
                    .ToList();

                // Lấy trải nghiệm
                var experiences = db.TraiNghiems
                    .Where(t => t.MaDiaDiem == place.MaDiaDiem)
                    .GroupBy(t => t.LoaiTraiNghiem)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => new
                        {
                            t.MaTraiNghiem,
                            t.TieuDe,
                            t.MoTa,
                            t.Icon
                        }).ToList()
                    );

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        place = new
                        {
                            place.MaDiaDiem,
                            place.TenDiaDiem,
                            place.Slug,
                            place.MoTaNgan,
                            place.MoTaChiTiet,
                            place.GiaVe,
                            place.GioMoCua,
                            place.DiaChiChiTiet,
                            place.SoDienThoai,
                            place.Email,
                            place.Website,
                            place.KinhDo,
                            place.ViDo,
                            place.TinhThanh,
                            place.VungMien,
                            place.DiemDanhGiaTB,
                            place.LuotXem,
                            DanhMuc = place.DanhMuc.TenDanhMuc
                        },
                        images = images,
                        prices = prices,
                        experiences = experiences
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy danh sách địa điểm gần đó
        /// </summary>
        public ActionResult GetNearbyPlaces(int? maDiaDiem, int soKetQua = 10)
        {
            try
            {
                // Validate parameter
                if (!maDiaDiem.HasValue || maDiaDiem.Value <= 0)
                {
                    return Json(new { success = false, message = "Địa điểm không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                var currentPlace = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == maDiaDiem.Value);

                if (currentPlace == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);
                }

                // Ưu tiên lấy các địa điểm CÙNG TỈNH THÀNH (trừ chính nó)
                var nearbyPlaces = db.DiaDiems
                    .Where(d => d.TinhThanh == currentPlace.TinhThanh && d.MaDiaDiem != maDiaDiem.Value && d.TrangThai == true)
                    .OrderByDescending(d => d.DiemDanhGiaTB) // Ưu tiên điểm đánh giá cao
                    .Take(soKetQua)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
                        d.TinhThanh, // ✅ Trả thêm thông tin tỉnh để hiển thị UI
                        d.VungMien,
                        AnhChinh = db.AnhDiaDiems
                            .Where(a => a.MaDiaDiem == d.MaDiaDiem && a.LaAnhChinh == true)
                            .Select(a => a.DuongDanAnh)
                            .FirstOrDefault(),
                        d.DiemDanhGiaTB
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = nearbyPlaces
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Tìm kiếm địa điểm
        /// </summary>
        public ActionResult SearchPlaces(string keyword, int soKetQua = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa" }, JsonRequestBehavior.AllowGet);
                }

                var places = db.DiaDiems
                    .Where(d => d.TrangThai == true && 
                           (d.TenDiaDiem.Contains(keyword) || 
                            d.MoTaNgan.Contains(keyword) ||
                            d.VungMien.Contains(keyword)))
                    .Take(soKetQua)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
                        d.MoTaNgan,
                        AnhChinh = db.AnhDiaDiems
                            .Where(a => a.MaDiaDiem == d.MaDiaDiem && a.LaAnhChinh == true)
                            .Select(a => a.DuongDanAnh)
                            .FirstOrDefault(),
                        d.DiemDanhGiaTB,
                        d.LuotXem
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = places,
                    count = places.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy danh sách địa điểm phổ biến (nhiều lượt xem)
        /// </summary>
        public ActionResult GetPopularPlaces(int soKetQua = 6)
        {
            try
            {
                var places = db.DiaDiems
                    .Where(d => d.TrangThai == true)
                    .OrderByDescending(d => d.LuotXem)
                    .Take(soKetQua)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
                        AnhChinh = db.AnhDiaDiems
                            .Where(a => a.MaDiaDiem == d.MaDiaDiem && a.LaAnhChinh == true)
                            .Select(a => a.DuongDanAnh)
                            .FirstOrDefault(),
                        d.DiemDanhGiaTB,
                        d.LuotXem
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = places
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        public ActionResult GetAllCategories()
        {
            try
            {
                var categories = db.DanhMucs
                    .Select(c => new
                    {
                        c.MaDanhMuc,
                        c.TenDanhMuc,
                        c.MoTa,
                        SoDiaDiem = db.DiaDiems.Count(d => d.MaDanhMuc == c.MaDanhMuc && d.TrangThai == true)
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = categories
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
