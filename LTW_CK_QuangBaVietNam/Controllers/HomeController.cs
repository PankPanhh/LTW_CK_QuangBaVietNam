using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Helpers;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class HomeController : Controller
    {
        private class UserProfileExtraData
        {
            public DateTime? NgaySinh { get; set; }
            public string TieuSu { get; set; }
            public string ThanhPho { get; set; }
            public string QuocGia { get; set; }
        }

        private readonly DataClasses1DataContext db = new DataClasses1DataContext(GetConnectionString());

        private static string GetConnectionString()
        {
            var appConnection = ConfigurationManager.ConnectionStrings["CK_QBVNConnectionString"];
            if (appConnection != null && !string.IsNullOrWhiteSpace(appConnection.ConnectionString))
            {
                return appConnection.ConnectionString;
            }

            var defaultConnection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (defaultConnection != null && !string.IsNullOrWhiteSpace(defaultConnection.ConnectionString))
            {
                return defaultConnection.ConnectionString;
            }

            throw new ConfigurationErrorsException("Missing connection string. Please add 'CK_QBVNConnectionString' (or 'DefaultConnection') in Web.config.");
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            // Set breadcrumbs for Index page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Index", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult AllPlaces()
        {
            // Set breadcrumbs for All Places page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Index"),
                new BreadcrumbItem("Tất cả địa điểm", "/Home/AllPlaces", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Intro()
        {
            // Intro page doesn't need breadcrumbs
            return View();
        }

        public ActionResult Createschedule()
        {
            // Set breadcrumbs for Create Schedule page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Index"),
                new BreadcrumbItem("Tạo lịch trình", "/Home/Createschedule", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Login()
        {
            // Set breadcrumbs for Login page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Đăng nhập", "/Home/Login", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Register()
        {
            // Set breadcrumbs for Register page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Đăng ký", "/Home/Register", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Profile()
        {
            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để xem hồ sơ.";
                return RedirectToAction("Login", "Home");
            }

            var user = db.NguoiDungs.FirstOrDefault(x => x.MaNguoiDung == sessionUser.MaNguoiDung && x.TrangThai);
            if (user == null)
            {
                Session["nguoiDung"] = null;
                Session["khach"] = null;
                TempData["LoginError"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Home");
            }

            Session["nguoiDung"] = user;
            Session["khach"] = user;

            var extra = db.ExecuteQuery<UserProfileExtraData>(
                "SELECT NgaySinh, TieuSu, ThanhPho, QuocGia FROM NguoiDung WHERE MaNguoiDung = {0}",
                user.MaNguoiDung).FirstOrDefault();

            ViewBag.BirthDate = (extra != null && extra.NgaySinh.HasValue)
                ? extra.NgaySinh.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            ViewBag.Bio = extra != null ? (extra.TieuSu ?? string.Empty) : string.Empty;
            ViewBag.City = extra != null ? (extra.ThanhPho ?? string.Empty) : string.Empty;
            ViewBag.Country = extra != null ? (extra.QuocGia ?? string.Empty) : string.Empty;

            var reviewList = (from dg in db.DanhGias
                              join dd in db.DiaDiems on dg.MaDiaDiem equals dd.MaDiaDiem
                              where dg.MaNguoiDung == user.MaNguoiDung
                              orderby dg.NgayGui descending
                              select new ReviewHistoryVM
                              {
                                  MaDanhGia = dg.MaDanhGia,
                                  MaDiaDiem = dd.MaDiaDiem,
                                  TenDiaDiem = dd.TenDiaDiem,
                                  SoSao = dg.SoSao,
                                  NoiDung = dg.NoiDung,
                                  TrangThaiKiemDuyet = dg.TrangThaiKiemDuyet,
                                  NgayGui = dg.NgayGui
                              }).ToList();

            ViewData["Reviews"] = reviewList;

            // Set breadcrumbs for Profile page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Hồ sơ cá nhân", "/Home/Profile", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View(user);
        }

        public ActionResult LichSuDanhGia()
        {
            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để xem lịch sử đánh giá.";
                return RedirectToAction("Login", "Home");
            }

            var breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Hồ sơ cá nhân", "/Home/Profile"),
        new BreadcrumbItem("Lịch sử đánh giá", "/Home/LichSuDanhGia", isActive: true)
    };
            this.SetBreadcrumbs(breadcrumbs);

            var list = (from dg in db.DanhGias
                        join dd in db.DiaDiems on dg.MaDiaDiem equals dd.MaDiaDiem
                        where dg.MaNguoiDung == sessionUser.MaNguoiDung
                        orderby dg.NgayGui descending
                        select new ReviewHistoryVM
                        {
                            MaDanhGia = dg.MaDanhGia,
                            MaDiaDiem = dd.MaDiaDiem,
                            TenDiaDiem = dd.TenDiaDiem,
                            SoSao = dg.SoSao,
                            NoiDung = dg.NoiDung,
                            TrangThaiKiemDuyet = dg.TrangThaiKiemDuyet,
                            NgayGui = dg.NgayGui
                        }).ToList();

            return View(list); 
        }

        // /Home/LichTrinhDaTao
        public ActionResult LichTrinhDaTao()
        {
            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để xem lịch trình.";
                return RedirectToAction("Login", "Home");
            }

            var breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Hồ sơ cá nhân", "/Home/Profile"),
        new BreadcrumbItem("Lịch trình đã tạo", "/Home/LichTrinhDaTao", isActive: true)
    };
            this.SetBreadcrumbs(breadcrumbs);

            // Lấy danh sách lịch trình + đếm số địa điểm
            var list = (from lt in db.LichTrinhs
                        where lt.MaNguoiDung == sessionUser.MaNguoiDung
                        join ct in db.ChiTietLichTrinhs on lt.MaLichTrinh equals ct.MaLichTrinh into g
                        orderby lt.NgayTao descending
                        select new ItineraryListVM
                        {
                            MaLichTrinh = lt.MaLichTrinh,
                            TenLichTrinh = lt.TenLichTrinh,
                            TongChiPhiDuKien = lt.TongChiPhiDuKien,
                            NgayTao = lt.NgayTao,
                            SoDiaDiem = g.Count()
                        }).ToList();

            return View(list); 
        }

        public ActionResult ChiTietLichTrinh(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("LichTrinhDaTao");

            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để xem chi tiết lịch trình.";
                return RedirectToAction("Login", "Home");
            }

            int ma = id.Value;

            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ma && x.MaNguoiDung == sessionUser.MaNguoiDung);
            if (lt == null) return HttpNotFound();

            var items = (from ct in db.ChiTietLichTrinhs
                         join dd in db.DiaDiems on ct.MaDiaDiem equals dd.MaDiaDiem
                         where ct.MaLichTrinh == ma
                         orderby ct.NgayThamQuan, ct.ThuTuUuTien
                         select new ItineraryItemVM
                         {
                             NgayThamQuan = ct.NgayThamQuan,
                             ThuTuUuTien = ct.ThuTuUuTien,
                             MaDiaDiem = dd.MaDiaDiem,
                             TenDiaDiem = dd.TenDiaDiem,
                             VungMien = dd.VungMien
                         }).ToList();

            var vm = new ItineraryDetailVM
            {
                MaLichTrinh = lt.MaLichTrinh,
                TenLichTrinh = lt.TenLichTrinh,
                TongChiPhiDuKien = lt.TongChiPhiDuKien,
                NgayTao = lt.NgayTao,
                Items = items
            };

            return View(vm);
        }

        public ActionResult Map()
        {
            // Set breadcrumbs for Map page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Bản đồ", "/Home/Map", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Schedule()
        {
            // Set breadcrumbs for Schedule page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Lịch trình du lịch", "/Home/Schedule", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Blog()
        {
            // Set breadcrumbs for Blog page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Blog", "/Home/Blog", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult DetailBlog()
        {
            // Set breadcrumbs for Blog page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Blog", "/Home/DetailBlog", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult DetailSchedule()
        {
            // Set breadcrumbs for Schedule page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Lịch trình du lịch", "/Home/DetailSchedule", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult DetailPlace()
        {
            // Set breadcrumbs for Place detail page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Bản đồ", "/Home/Map"),
                new BreadcrumbItem("Chi tiết địa điểm", "/Home/DetailPlace", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult YeuThich()
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            // Lấy danh sách yêu thích + ảnh chính (nếu có)
            var list = (from yt in db.YeuThiches
                        join dd in db.DiaDiems on yt.MaDiaDiem equals dd.MaDiaDiem
                        join a in db.AnhDiaDiems.Where(x => x.LaAnhChinh == true)
 on dd.MaDiaDiem equals a.MaDiaDiem into ga
                        from a in ga.DefaultIfEmpty()
                        where yt.MaNguoiDung == user.MaNguoiDung
                        orderby yt.NgayLuu descending
                        select new FavoritePlaceVM
                        {
                            MaDiaDiem = dd.MaDiaDiem,
                            TenDiaDiem = dd.TenDiaDiem,
                            VungMien = dd.VungMien,
                            AnhChinh = (a != null ? a.DuongDanAnh : null),
                            NgayLuu = yt.NgayLuu
                        }).ToList();

            ViewBag.Collections = db.BoSuuTaps
        .Where(x => x.MaNguoiDung == user.MaNguoiDung)
        .OrderByDescending(x => x.NgayTao)
        .ToList();

            return View(list); 
        }

    }
}
