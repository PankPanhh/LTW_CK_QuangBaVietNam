using LTW_CK_QuangBaVietNam.Helpers;
using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = text.Trim();
            return text.Length > maxLength ? text.Substring(0, maxLength).TrimEnd() + "..." : text;
        }

        private static string GetRelativeTimeText(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return string.Empty;
            }

            var diff = DateTime.Now - dateTime.Value;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalHours < 1) return $"{Math.Max(1, (int)diff.TotalMinutes)} phút trước";
            if (diff.TotalDays < 1) return $"{Math.Max(1, (int)diff.TotalHours)} giờ trước";
            if (diff.TotalDays < 7) return $"{Math.Max(1, (int)diff.TotalDays)} ngày trước";
            return dateTime.Value.ToString("dd/MM/yyyy");
        }

        private static string GetBlogCoverImage(BaiViet blog)
        {
            var image = blog.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(image))
            {
                return image;
            }

            return "/Content/images/places/default-place.jpg";
        }

        private static string GetAuthorName(BaiViet blog)
        {
            return blog.NguoiDung != null && !string.IsNullOrWhiteSpace(blog.NguoiDung.HoTen)
                ? blog.NguoiDung.HoTen
                : "Cộng đồng";
        }

        private static HomeBlogVM MapBlog(BaiViet blog)
        {
            return new HomeBlogVM
            {
                MaBaiViet = blog.MaBaiViet,
                TieuDe = blog.TieuDe,
                NoiDungRutGon = Truncate(blog.NoiDung, 150),
                AnhBia = GetBlogCoverImage(blog),
                TenTacGia = GetAuthorName(blog),
                ThoiGianDang = GetRelativeTimeText(blog.NgayDang)
            };
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

            var blogs = db.BaiViets
                .Where(b => b.TrangThai == "approved")
                .OrderByDescending(b => b.NgayDang)
                .Take(4)
                .ToList()
                .Select(MapBlog)
                .ToList();

            return View(new NoiDungTrangChuVM
            {
                Blogs = blogs
            });
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
            ViewBag.SoHanhTrinh = db.LichTrinhs.Count(x => x.MaNguoiDung == user.MaNguoiDung);
            ViewBag.SoDiaDiemDaLuu = db.YeuThiches.Count(x => x.MaNguoiDung == user.MaNguoiDung);

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Hồ sơ cá nhân", "/Home/Profile", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View(user);
        }

        //public ActionResult LichSuBinhLuan() 
        //{
        //    var sessionUser = Session["nguoiDung"] as NguoiDung;
        //    if (sessionUser == null)
        //    {
        //        TempData["LoginError"] = "Vui lòng đăng nhập để xem lịch sử bình luận.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    var comments = db.BinhLuans
        //        .Where(x => x.MaNguoiDung == sessionUser.MaNguoiDung)
        //        .OrderByDescending(x => x.NgayDang)
        //        .ToList(); // Model

        //    // 2) Lấy các bài viết liên quan
        //    var postIds = comments.Select(x => x.MaBaiViet).Distinct().ToList();

        //    var posts = db.BaiViets
        //        .Where(p => postIds.Contains(p.MaBaiViet))
        //        .ToList();

        //    ViewBag.PostMap = posts.ToDictionary(p => p.MaBaiViet, p => p);

        //    // 3) Lấy ảnh đại diện bài viết (lấy ảnh có ThuTu nhỏ nhất)
        //    var postImgs = db.AnhBaiViets
        //        .Where(a => postIds.Contains(a.MaBaiViet))
        //        .ToList();

        //    ViewBag.PostThumbMap = postImgs
        //        .GroupBy(a => a.MaBaiViet)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.OrderBy(x => x.ThuTu).ThenBy(x => x.MaAnh).Select(x => x.DuongDanAnh).FirstOrDefault()
        //        );

        //    // 4) Lấy địa điểm liên quan (nếu bài viết có MaDiaDiem)
        //    var placeIds = posts
        //        .Where(p => p.MaDiaDiem.HasValue)
        //        .Select(p => p.MaDiaDiem.Value)
        //        .Distinct()
        //        .ToList();

        //    ViewBag.PlaceMap = db.DiaDiems
        //        .Where(d => placeIds.Contains(d.MaDiaDiem))
        //        .ToDictionary(d => d.MaDiaDiem, d => d);

        //    // 5) (Tuỳ chọn) Map bình luận cha để hiển thị "đang trả lời..."
        //    var parentIds = comments
        //        .Where(x => x.ParentId.HasValue)
        //        .Select(x => x.ParentId.Value)
        //        .Distinct()
        //        .ToList();

        //    ViewBag.ParentMap = db.BinhLuans
        //        .Where(x => parentIds.Contains(x.MaBinhLuan))
        //        .ToDictionary(x => x.MaBinhLuan, x => x);

        //    return View(comments);
        //}

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

            
            var list = db.LichTrinhs
                .Where(x => x.MaNguoiDung == sessionUser.MaNguoiDung)
                .OrderByDescending(x => x.NgayTao)
                .ToList(); 

            var ltIds = list.Select(x => x.MaLichTrinh).ToList();

            
            var soDiaDiemMap =
                (from day in db.NgayLichTrinhs
                 join ct in db.ChiTietLichTrinhs on day.MaNgay equals ct.MaNgay
                 where ltIds.Contains(day.MaLichTrinh)
                 group ct by day.MaLichTrinh into g
                 select new { MaLichTrinh = g.Key, So = g.Count() })
                .ToDictionary(x => x.MaLichTrinh, x => x.So);

         
            var tongChiPhiMap =
                (from day in db.NgayLichTrinhs
                 join ct in db.ChiTietLichTrinhs on day.MaNgay equals ct.MaNgay
                 join dd in db.DiaDiems on ct.MaDiaDiem equals dd.MaDiaDiem
                 where ltIds.Contains(day.MaLichTrinh)
                 group dd by day.MaLichTrinh into g
                 select new
                 {
                     MaLichTrinh = g.Key,
                     Tong = g.Sum(x => (decimal?)x.GiaVe) ?? 0m
                 })
                .ToDictionary(x => x.MaLichTrinh, x => x.Tong);

            ViewBag.SoDiaDiemMap = soDiaDiemMap;
            ViewBag.TongChiPhiMap = tongChiPhiMap;

            return View(list); 
        }

        public ActionResult ChiTietLichTrinh(int? id)
        {
            if (!id.HasValue) return RedirectToAction("LichTrinhDaTao");

            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để xem chi tiết lịch trình.";
                return RedirectToAction("Login", "Home");
            }

            int ma = id.Value;

            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ma && x.MaNguoiDung == sessionUser.MaNguoiDung);
            if (lt == null) return HttpNotFound();

 
            var days = db.NgayLichTrinhs
                .Where(x => x.MaLichTrinh == ma)
                .OrderBy(x => x.ThuTuNgay)
                .ToList();

            var dayIds = days.Select(x => x.MaNgay).ToList();

            var details = db.ChiTietLichTrinhs
                .Where(x => dayIds.Contains(x.MaNgay))
                .OrderBy(x => x.MaNgay)
                .ThenBy(x => x.ThuTu)
                .ToList();

            var ddIds = details.Select(x => x.MaDiaDiem).Distinct().ToList();

            var diaDiemMap = db.DiaDiems
                .Where(d => ddIds.Contains(d.MaDiaDiem))
                .ToDictionary(d => d.MaDiaDiem, d => d);

            decimal tongChiPhi = 0m;
            if (details.Any())
            {
                tongChiPhi = (
                    from ct in details
                    join dd in db.DiaDiems on ct.MaDiaDiem equals dd.MaDiaDiem
                    select (decimal?)dd.GiaVe
                ).Sum() ?? 0m;
            }

            var anhChinhMap = db.AnhDiaDiems
                .Where(a => ddIds.Contains(a.MaDiaDiem) && a.LaAnhChinh == true)
                .GroupBy(a => a.MaDiaDiem)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DuongDanAnh).FirstOrDefault());

            var itemsByDay = details
                .GroupBy(x => x.MaNgay)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.Days = days;
            ViewBag.ItemsByDay = itemsByDay;
            ViewBag.DiaDiemMap = diaDiemMap;
            ViewBag.AnhChinhMap = anhChinhMap;

            ViewBag.TongChiPhi = tongChiPhi;

            return View(lt); 
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

        public ActionResult DetailBlog(int? id)
        {
            // Set breadcrumbs for Blog page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Blog", "/Home/Blog"),
                new BreadcrumbItem("Chi tiết bài viết", id.HasValue ? $"/Home/DetailBlog?id={id.Value}" : "/Home/DetailBlog", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            ViewBag.BlogId = id;
            return View();
        }

        public ActionResult SaveBlog()
        {
            return View();
        }

        public ActionResult DetailSchedule(int? id)
        {
            if (!id.HasValue) return RedirectToAction("Schedule");

            ViewBag.ScheduleId = id.Value;

            // Set breadcrumbs for Schedule page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Schedule"),
                new BreadcrumbItem("Chi tiết lịch trình", $"/Home/DetailSchedule?id={id.Value}", isActive: true)
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

        
    }
}
