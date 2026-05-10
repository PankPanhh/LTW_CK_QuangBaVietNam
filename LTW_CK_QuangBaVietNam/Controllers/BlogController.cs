using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class BlogController : Controller
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

        private NguoiDung GetCurrentUser()
        {
            return Session["nguoiDung"] as NguoiDung;
        }

        private JsonResult UnauthorizedJson(string message)
        {
            Response.StatusCode = 401;
            return Json(new { success = false, message = message });
        }

        private static string BuildAvatarUrl(string fullName)
        {
            var name = string.IsNullOrWhiteSpace(fullName) ? "User" : fullName;
            return "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(name) + "&background=0EA5E9&color=fff";
        }

        // GET: /Blog/
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// API: Lấy danh sách Blog công khai (Chỉ những bài có TrangThai = 'approved')
        /// </summary>
        [HttpGet]
        public JsonResult GetPublicBlogs(string search = "", string category = "all")
        {
            try
            {
                var query = db.BaiViets.Where(b => b.TrangThai == "approved");

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(b => b.TieuDe.Contains(search) || b.NoiDung.Contains(search));
                }

                var blogs = query
                    .OrderByDescending(b => b.NgayDang)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        image = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Khám phá",
                        dateValue = b.NgayDang,
                        likes = b.LuotLike,
                        totalComments = b.BinhLuans.Count(),
                        author = b.NguoiDung.HoTen,
                        status = b.TrangThai,
                        isLiked = false
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 150 ? b.content.Substring(0, 150) + "..." : b.content,
                        image = b.image,
                        locationName = b.locationName,
                        date = b.dateValue.HasValue ? b.dateValue.Value.ToString("dd/MM/yyyy") : "",
                        likes = b.likes,
                        totalComments = b.totalComments,
                        author = b.author,
                        avatar = BuildAvatarUrl(b.author),
                        status = b.status,
                        isLiked = b.isLiked
                    })
                    .ToList();

                return Json(new { success = true, data = blogs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy danh sách Blog của user đang đăng nhập (Lấy tất cả trạng thái)
        /// </summary>
        [HttpGet]
        public JsonResult GetMyBlogs()
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập để sử dụng chức năng này.");
                }

                var currentUserId = currentUser.MaNguoiDung;

                var myBlogs = db.BaiViets
                    .Where(b => b.MaNguoiDung == currentUserId)
                    .OrderByDescending(b => b.NgayDang)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        image = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Chung",
                        dateValue = b.NgayDang,
                        status = b.TrangThai,
                        rejectReason = b.LyDoTuChoi,
                        totalLikes = b.LuotLike,
                        totalComments = b.BinhLuans.Count(),
                        author = b.NguoiDung.HoTen
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 100 ? b.content.Substring(0, 100) + "..." : b.content,
                        image = b.image,
                        locationName = b.locationName,
                        date = b.dateValue.HasValue ? b.dateValue.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        status = b.status,
                        rejectReason = b.rejectReason,
                        totalLikes = b.totalLikes,
                        totalComments = b.totalComments,
                        author = b.author,
                        avatar = BuildAvatarUrl(b.author)
                    })
                    .ToList();

                return Json(new { success = true, data = myBlogs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Tạo bài viết mới
        /// </summary>
        [HttpPost]
        public JsonResult CreateBlog(string title, string content, int? locationId, List<string> imageUrls)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập để sử dụng chức năng này.");
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Tiêu đề và nội dung không được để trống!" });
                }

                var validImageUrls = (imageUrls ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();

                const int maxImages = 5;
                if (validImageUrls.Count > maxImages)
                {
                    return Json(new { success = false, message = "Bài viết chỉ được đính kèm tối đa 5 ảnh." });
                }

                var newBlog = new BaiViet
                {
                    TieuDe = title.Trim(),
                    NoiDung = content.Trim(),
                    MaDiaDiem = locationId.HasValue && locationId.Value > 0 ? locationId : null,
                    MaNguoiDung = currentUser.MaNguoiDung,
                    TrangThai = "pending",
                    NgayDang = DateTime.Now,
                    LuotLike = 0
                };

                db.BaiViets.InsertOnSubmit(newBlog);
                db.SubmitChanges();

                if (validImageUrls.Any())
                {
                    int thuTu = 1;
                    foreach (var url in validImageUrls)
                    {
                        var img = new AnhBaiViet
                        {
                            MaBaiViet = newBlog.MaBaiViet,
                            DuongDanAnh = url,
                            ThuTu = thuTu++
                        };
                        db.AnhBaiViets.InsertOnSubmit(img);
                    }
                    db.SubmitChanges();
                }

                return Json(new { success = true, message = "Bài viết đã được gửi và đang chờ duyệt!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Lấy danh sách địa điểm (Dùng cho dropdown lúc chọn địa điểm check-in)
        /// </summary>
        [HttpGet]
        public JsonResult GetLocationsForDropdown()
        {
            var locs = db.DiaDiems.Where(d => d.TrangThai == true)
                         .Select(d => new { id = d.MaDiaDiem, name = d.TenDiaDiem }).ToList();
            return Json(new { success = true, data = locs }, JsonRequestBehavior.AllowGet);
        }
    }
}