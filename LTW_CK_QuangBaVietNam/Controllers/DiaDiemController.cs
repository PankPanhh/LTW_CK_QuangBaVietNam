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
            var appConnection = ConfigurationManager.ConnectionStrings["QL_DL_LTWConnectionString"];
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
        /// L?y danh sách t?t c? ??a ?i?m (cho trang AllPlaces)
        /// </summary>
        public ActionResult GetAllPlaces(string vungMien = "", int page = 1, int pageSize = 9)
        {
            try
            {
                var query = db.DiaDiems.Where(d => d.TrangThai == true);

                // Filter theo vùng mi?n n?u có
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
        /// L?y chi ti?t m?t ??a ?i?m theo slug (cho trang DetailPlace)
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

                // T?ng l??t xem
                place.LuotXem = (place.LuotXem ?? 0) + 1;
                db.SubmitChanges();

                // L?y ?nh
                var images = db.AnhDiaDiems
                    .Where(a => a.MaDiaDiem == place.MaDiaDiem)
                    .Select(a => a.DuongDanAnh)
                    .ToList();

                // L?y giá vé chi ti?t
                var prices = db.GiaVeChiTiets
                    .Where(g => g.MaDiaDiem == place.MaDiaDiem)
                    .Select(g => new
                    {
                        g.MaGiaVe,
                        g.LoaiKhach,
                        g.Gia
                    })
                    .ToList();

                // L?y tr?i nghi?m
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
        /// L?y danh sách ??a ?i?m g?n ?ó
        /// </summary>
        public ActionResult GetNearbyPlaces(int maDiaDiem, int soKetQua = 4)
        {
            try
            {
                var currentPlace = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == maDiaDiem);

                if (currentPlace == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);
                }

                // L?y các ??a ?i?m cùng danh m?c
                var nearbyPlaces = db.DiaDiems
                    .Where(d => d.MaDanhMuc == currentPlace.MaDanhMuc && d.MaDiaDiem != maDiaDiem && d.TrangThai == true)
                    .Take(soKetQua)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
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
        /// L?y danh sách ??a ?i?m theo danh m?c
        /// </summary>
        public ActionResult GetPlacesByCategory(int maDanhMuc, int soKetQua = 9)
        {
            try
            {
                var places = db.DiaDiems
                    .Where(d => d.MaDanhMuc == maDanhMuc && d.TrangThai == true)
                    .OrderByDescending(d => d.DiemDanhGiaTB)
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
                    data = places
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Tìm ki?m ??a ?i?m
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
        /// L?y danh sách ??a ?i?m ph? bi?n (nhi?u l??t xem)
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
        /// L?y danh sách t?t c? danh m?c
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

        /// <summary>
        /// L?y danh sách ??a ?i?m theo vùng mi?n (B?c/Trung/Nam)
        /// </summary>
        public ActionResult GetPlacesByRegion(string vungMien, int soKetQua = 9)
        {
            try
            {
                if (string.IsNullOrEmpty(vungMien))
                {
                    return Json(new { success = false, message = "Vui lòng chọn vùng miền" }, JsonRequestBehavior.AllowGet);
                }

                var places = db.DiaDiems
                    .Where(d => d.VungMien.Contains(vungMien) && d.TrangThai == true)
                    .OrderByDescending(d => d.DiemDanhGiaTB)
                    .Take(soKetQua)
                    .Select(d => new
                    {
                        d.MaDiaDiem,
                        d.TenDiaDiem,
                        d.Slug,
                        d.MoTaNgan,
                        d.VungMien,
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
        /// C?p nh?t l??t xem cho m?t ??a ?i?m
        /// </summary>
        public ActionResult UpdateViews(int maDiaDiem)
        {
            try
            {
                var place = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == maDiaDiem);

                if (place == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);
                }

                place.LuotXem = (place.LuotXem ?? 0) + 1;
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công",
                    views = place.LuotXem
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
