using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
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

        private NguoiDung GetCurrentUser()
        {
            return Session["nguoiDung"] as NguoiDung;
        }

        private JsonResult UnauthorizedJson(string message)
        {
            Response.StatusCode = 401;
            return Json(new { success = false, message = message });
        }

        private JsonResult ForbiddenJson(string message)
        {
            Response.StatusCode = 403;
            return Json(new { success = false, message = message });
        }

        private static string BuildAvatarUrl(string fullName)
        {
            var name = string.IsNullOrWhiteSpace(fullName) ? "User" : fullName;
            return "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(name) + "&background=0EA5E9&color=fff";
        }

        private static string FormatDate(DateTime? date)
        {
            if (!date.HasValue) return "";
            return date.Value.ToString("dd/MM/yyyy");
        }

        private bool IsAdmin()
        {
            var user = GetCurrentUser();
            return user != null && user.VaiTro == 1;
        }

        // GET: /Blog/
        public ActionResult Index()
        {
            return View();
        }

        #region PUBLIC APIs
        /// <summary>
        /// API: Lấy danh sách Blog công khai (Chỉ những bài có TrangThai = 'approved')
        /// </summary>
        [HttpGet]
        public JsonResult GetPublicBlogs(string search = "", string category = "all")
        {
            try
            {
                var currentUser = GetCurrentUser();
                var currentUserId = currentUser != null ? currentUser.MaNguoiDung : 0;

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
                        totalComments = b.BinhLuans.Count(c => c.TrangThai == "visible"),
                        author = b.NguoiDung.HoTen,
                        status = b.TrangThai,
                        maDiaDiem = b.MaDiaDiem,
                        isLiked = currentUserId > 0 && b.LikeBaiViets.Any(l => l.MaNguoiDung == currentUserId),
                        isSaved = currentUserId > 0 && b.DiaDiem != null && db.YeuThiches.Any(y => y.MaNguoiDung == currentUserId && y.MaDiaDiem == b.MaDiaDiem)
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
                        isLiked = b.isLiked,
                        isSaved = b.isSaved
                    })
                    .ToList();

                return Json(new { success = true, data = blogs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region USER APIs
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
                        hideReason = b.LyDoAn,
                        dateHidden = b.NgayAn,
                        totalLikes = b.LuotLike,
                        totalComments = b.BinhLuans.Count(c => c.TrangThai == "visible"),
                        author = b.NguoiDung.HoTen,
                        maDiaDiem = b.MaDiaDiem,
                        isLiked = b.LikeBaiViets.Any(l => l.MaNguoiDung == currentUserId),
                        isSaved = b.DiaDiem != null && db.YeuThiches.Any(y => y.MaNguoiDung == currentUserId && y.MaDiaDiem == b.MaDiaDiem)
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
                        hideReason = b.hideReason,
                        dateHidden = b.dateHidden.HasValue ? b.dateHidden.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        totalLikes = b.totalLikes,
                        totalComments = b.totalComments,
                        author = b.author,
                        avatar = BuildAvatarUrl(b.author),
                        isLiked = b.isLiked,
                        isSaved = b.isSaved
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
        public JsonResult CreateBlog(string title, string content, int? locationId)
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

                var files = Request.Files;
                var coverImageIndex = Request.Form["coverImageIndex"];
                int.TryParse(coverImageIndex, out int coverIdx);

                const int maxImages = 10;
                if (files.Count > maxImages)
                {
                    return Json(new { success = false, message = $"Bài viết chỉ được đính kèm tối đa {maxImages} ảnh." });
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

                // Process uploaded images
                if (files.Count > 0)
                {
                    try
                    {
                        string uploadsFolder = System.IO.Path.Combine(HttpContext.Server.MapPath("~/Content/images/blogs"));

                        if (!System.IO.Directory.Exists(uploadsFolder))
                        {
                            System.IO.Directory.CreateDirectory(uploadsFolder);
                        }

                        int thuTu = 1;
                        for (int i = 0; i < files.Count; i++)
                        {
                            var file = files[i];

                            if (file == null || file.ContentLength == 0)
                                continue;

                            // Validate file
                            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

                            if (!validExtensions.Contains(fileExtension))
                            {
                                continue;
                            }

                            // Generate unique filename
                            string fileName = $"{newBlog.MaBaiViet}_{DateTime.Now.Ticks}{fileExtension}";
                            string filePath = System.IO.Path.Combine(uploadsFolder, fileName);

                            // Save file
                            file.SaveAs(filePath);

                            // Generate web URL
                            string relativeUrl = $"/Content/images/blogs/{fileName}";

                            var img = new AnhBaiViet
                            {
                                MaBaiViet = newBlog.MaBaiViet,
                                DuongDanAnh = relativeUrl,
                                ThuTu = thuTu
                            };

                            db.AnhBaiViets.InsertOnSubmit(img);
                            thuTu++;
                        }

                        db.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        // If image processing fails, continue anyway
                        System.Diagnostics.Debug.WriteLine($"Error processing images: {ex.Message}");
                    }
                }

                return Json(new { success = true, message = "Bài viết đã được gửi và đang chờ duyệt!", data = new { blogId = newBlog.MaBaiViet } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Chỉnh sửa bài viết chưa duyệt, bị từ chối hoặc bị ẩn
        /// </summary>
        [HttpPost]
        public JsonResult EditBlog(int id, string title, string content, int? locationId, List<string> imageUrls)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập để sử dụng chức năng này.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                if (blog.MaNguoiDung != currentUser.MaNguoiDung)
                {
                    return ForbiddenJson("Bạn không có quyền chỉnh sửa bài viết này.");
                }

                // Cho phép edit bài pending, rejected, hoặc hidden. Không cho edit bài approved
                if (blog.TrangThai == "approved")
                {
                    return Json(new { success = false, message = "Bài viết đã duyệt không thể chỉnh sửa." });
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Tiêu đề và nội dung không được để trống!" });
                }

                blog.TieuDe = title.Trim();
                blog.NoiDung = content.Trim();
                blog.MaDiaDiem = locationId.HasValue && locationId.Value > 0 ? locationId : null;
                blog.TrangThai = "pending";
                blog.LyDoTuChoi = null;
                blog.NgayDuyet = null;
                blog.NguoiDuyet = null;
                // Xóa trạng thái ẩn nếu bài bị ẩn
                blog.LyDoAn = null;
                blog.NgayAn = null;
                blog.NguoiAn = null;

                db.SubmitChanges();

                var validImageUrls = (imageUrls ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();

                const int maxImages = 5;
                if (validImageUrls.Count > maxImages)
                {
                    return Json(new { success = false, message = "Bài viết chỉ được đính kèm tối đa 5 ảnh." });
                }

                if (validImageUrls.Any())
                {
                    db.AnhBaiViets.DeleteAllOnSubmit(db.AnhBaiViets.Where(a => a.MaBaiViet == id));
                    db.SubmitChanges();

                    int thuTu = 1;
                    foreach (var url in validImageUrls)
                    {
                        var img = new AnhBaiViet
                        {
                            MaBaiViet = id,
                            DuongDanAnh = url,
                            ThuTu = thuTu++
                        };
                        db.AnhBaiViets.InsertOnSubmit(img);
                    }
                    db.SubmitChanges();
                }

                return Json(new { success = true, message = "Bài viết đã được cập nhật và đang chờ duyệt lại!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Lấy danh sách địa điểm (Dùng cho dropdown)
        /// </summary>
        [HttpGet]
        public JsonResult GetLocationsForDropdown()
        {
            var locs = db.DiaDiems.Where(d => d.TrangThai == true)
                         .Select(d => new { id = d.MaDiaDiem, name = d.TenDiaDiem }).ToList();
            return Json(new { success = true, data = locs }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ADMIN APIs
        /// <summary>
        /// API: Lấy danh sách bài chờ duyệt (Admin only)
        /// </summary>
        [HttpGet]
        public JsonResult GetPendingBlogs(int page = 1, int pageSize = 10)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var skip = (page - 1) * pageSize;
                var blogs = db.BaiViets
                    .Where(b => b.TrangThai == "pending")
                    .OrderByDescending(b => b.NgayDang)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        images = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).ToList(),
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Chung",
                        author = b.NguoiDung.HoTen,
                        authorId = b.MaNguoiDung,
                        dateSubmitted = b.NgayDang,
                        status = b.TrangThai
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 200 ? b.content.Substring(0, 200) + "..." : b.content,
                        content = b.content,
                        images = b.images,
                        imageCount = b.images.Count,
                        locationName = b.locationName,
                        author = b.author,
                        authorId = b.authorId,
                        avatar = BuildAvatarUrl(b.author),
                        dateSubmitted = b.dateSubmitted.HasValue ? b.dateSubmitted.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        status = b.status
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var totalCount = db.BaiViets.Count(b => b.TrangThai == "pending");

                return Json(new
                {
                    success = true,
                    data = blogs,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        total = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy danh sách bài đã duyệt (Admin only)
        /// </summary>
        [HttpGet]
        public JsonResult GetApprovedBlogs(int page = 1, int pageSize = 10)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var skip = (page - 1) * pageSize;
                var blogs = db.BaiViets
                    .Where(b => b.TrangThai == "approved")
                    .OrderByDescending(b => b.NgayDang)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        images = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).ToList(),
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Chung",
                        author = b.NguoiDung.HoTen,
                        authorId = b.MaNguoiDung,
                        dateSubmitted = b.NgayDang,
                        status = b.TrangThai
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 200 ? b.content.Substring(0, 200) + "..." : b.content,
                        content = b.content,
                        images = b.images,
                        imageCount = b.images.Count,
                        locationName = b.locationName,
                        author = b.author,
                        authorId = b.authorId,
                        avatar = BuildAvatarUrl(b.author),
                        dateSubmitted = b.dateSubmitted.HasValue ? b.dateSubmitted.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        status = b.status
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var totalCount = db.BaiViets.Count(b => b.TrangThai == "approved");

                return Json(new
                {
                    success = true,
                    data = blogs,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        total = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy danh sách bài bị từ chối (Admin only) - Chờ user chỉnh sửa
        /// </summary>
        [HttpGet]
        public JsonResult GetRejectedBlogs(int page = 1, int pageSize = 10)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var skip = (page - 1) * pageSize;
                var blogs = db.BaiViets
                    .Where(b => b.TrangThai == "rejected")
                    .OrderByDescending(b => b.NgayDuyet)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        images = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).ToList(),
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Chung",
                        author = b.NguoiDung.HoTen,
                        authorId = b.MaNguoiDung,
                        dateSubmitted = b.NgayDuyet,
                        rejectReason = b.LyDoTuChoi,
                        status = b.TrangThai
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 200 ? b.content.Substring(0, 200) + "..." : b.content,
                        content = b.content,
                        images = b.images,
                        imageCount = b.images.Count,
                        locationName = b.locationName,
                        author = b.author,
                        authorId = b.authorId,
                        avatar = BuildAvatarUrl(b.author),
                        dateSubmitted = b.dateSubmitted.HasValue ? b.dateSubmitted.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        rejectReason = b.rejectReason,
                        status = b.status
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var totalCount = db.BaiViets.Count(b => b.TrangThai == "rejected");

                return Json(new
                {
                    success = true,
                    data = blogs,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        total = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Duyệt bài viết (Admin only)
        /// </summary>
        [HttpPost]
        public JsonResult ApproveBlog(int id)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền thực hiện hành động này.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                var adminUser = GetCurrentUser();
                blog.TrangThai = "approved";
                blog.NgayDuyet = DateTime.Now;
                blog.NguoiDuyet = adminUser.MaNguoiDung;
                blog.LyDoTuChoi = null;

                db.SubmitChanges();

                return Json(new { success = true, message = "Bài viết đã được duyệt!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Từ chối bài viết (Admin only)
        /// </summary>
        [HttpPost]
        public JsonResult RejectBlog(int id, string reason)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền thực hiện hành động này.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });
                }

                blog.TrangThai = "rejected";
                blog.LyDoTuChoi = reason.Trim();
                blog.NgayDuyet = DateTime.Now;
                blog.NguoiDuyet = GetCurrentUser().MaNguoiDung;

                db.SubmitChanges();

                return Json(new { success = true, message = "Bài viết đã bị từ chối!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Ẩn bài viết (Admin only) - Soft Hide
        /// Lưu lý do ẩn, người ẩn, và thời gian ẩn để theo dõi
        /// </summary>
        [HttpPost]
        public JsonResult HideBlog(int id, string reason = "")
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền thực hiện hành động này.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                if (blog.TrangThai == "hidden")
                {
                    return Json(new { success = false, message = "Bài viết đã bị ẩn rồi." });
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Vui lòng cung cấp lý do ẩn bài viết." });
                }

                var adminUser = GetCurrentUser();
                blog.TrangThai = "hidden";
                blog.LyDoAn = reason.Trim();
                blog.NgayAn = DateTime.Now;
                blog.NguoiAn = adminUser.MaNguoiDung;

                db.SubmitChanges();

                return Json(new { success = true, message = "Bài viết đã bị ẩn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Khôi phục bài viết đã ẩn (Admin only)
        /// Chuyển trạng thái từ hidden về approved
        /// </summary>
        [HttpPost]
        public JsonResult RestoreBlog(int id)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền thực hiện hành động này.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                if (blog.TrangThai != "hidden")
                {
                    return Json(new { success = false, message = "Chỉ có thể khôi phục bài viết đã bị ẩn." });
                }

                blog.TrangThai = "approved";
                blog.LyDoAn = null;
                blog.NgayAn = null;
                blog.NguoiAn = null;

                db.SubmitChanges();

                return Json(new { success = true, message = "Bài viết đã được khôi phục!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Lấy danh sách bài viết đã ẩn (Admin only)
        /// </summary>
        [HttpGet]
        public JsonResult GetHiddenBlogs(int page = 1, int pageSize = 10)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var skip = (page - 1) * pageSize;
                var blogs = db.BaiViets
                    .Where(b => b.TrangThai == "hidden")
                    .OrderByDescending(b => b.NgayAn)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        images = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).ToList(),
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Chung",
                        author = b.NguoiDung.HoTen,
                        authorId = b.MaNguoiDung,
                        dateSubmitted = b.NgayDang,
                        dateHidden = b.NgayAn,
                        hiddenReason = b.LyDoAn,
                        hiddenBy = b.NguoiAn,
                        status = b.TrangThai
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        excerpt = b.content.Length > 200 ? b.content.Substring(0, 200) + "..." : b.content,
                        content = b.content,
                        images = b.images,
                        imageCount = b.images.Count,
                        locationName = b.locationName,
                        author = b.author,
                        authorId = b.authorId,
                        avatar = BuildAvatarUrl(b.author),
                        dateSubmitted = b.dateSubmitted.HasValue ? b.dateSubmitted.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        dateHidden = b.dateHidden.HasValue ? b.dateHidden.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        hiddenReason = b.hiddenReason,
                        status = b.status
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var totalCount = db.BaiViets.Count(b => b.TrangThai == "hidden");

                return Json(new
                {
                    success = true,
                    data = blogs,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        total = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy thống kê blog (Admin only)
        /// </summary>
        [HttpGet]
        public JsonResult GetBlogStats()
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var totalBlogs = db.BaiViets.Count();
                var pendingCount = db.BaiViets.Count(b => b.TrangThai == "pending");
                var approvedCount = db.BaiViets.Count(b => b.TrangThai == "approved");
                var rejectedCount = db.BaiViets.Count(b => b.TrangThai == "rejected");
                var hiddenCount = db.BaiViets.Count(b => b.TrangThai == "hidden");

                var today = DateTime.Now.Date;
                var todayCount = db.BaiViets
                    .Where(b => b.NgayDang.HasValue && b.NgayDang.Value >= today && b.NgayDang.Value < today.AddDays(1))
                    .Count();

                var topAuthors = db.BaiViets
                    .Where(b => b.TrangThai == "approved")
                    .GroupBy(b => b.NguoiDung)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new
                    {
                        name = g.Key.HoTen,
                        posts = g.Count(),
                        avatar = BuildAvatarUrl(g.Key.HoTen)
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        total = totalBlogs,
                        pending = pendingCount,
                        approved = approvedCount,
                        rejected = rejectedCount,
                        hidden = hiddenCount,
                        today = todayCount,
                        topAuthors = topAuthors
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy chi tiết bài viết cho admin moderation (Admin only)
        /// </summary>
        [HttpGet]
        public JsonResult GetBlogForDetail(int id)
        {
            try
            {
                if (!IsAdmin())
                {
                    return ForbiddenJson("Bạn không có quyền truy cập.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                var images = db.AnhBaiViets
                    .Where(a => a.MaBaiViet == id)
                    .OrderBy(a => a.ThuTu)
                    .Select(a => a.DuongDanAnh)
                    .ToList();

                var rejectionHistory = new List<object>();
                if (!string.IsNullOrWhiteSpace(blog.LyDoTuChoi) && blog.TrangThai == "rejected")
                {
                    rejectionHistory.Add(new
                    {
                        attemptNumber = 1,
                        date = blog.NgayDuyet.HasValue ? blog.NgayDuyet.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        reason = blog.LyDoTuChoi
                    });
                }

                var resubmitCount = 0; // Có thể tính từ history nếu cần

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = blog.MaBaiViet,
                        title = blog.TieuDe,
                        content = blog.NoiDung,
                        images = images,
                        locationName = blog.DiaDiem != null ? blog.DiaDiem.TenDiaDiem : "Chưa gắn thẻ",
                        author = blog.NguoiDung.HoTen,
                        authorEmail = blog.NguoiDung.Email,
                        avatar = BuildAvatarUrl(blog.NguoiDung.HoTen),
                        status = blog.TrangThai,
                        dateSubmitted = blog.NgayDang.HasValue ? blog.NgayDang.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        likes = blog.LuotLike ?? 0,
                        comments = blog.BinhLuans.Count(c => c.TrangThai == "visible"),
                        resubmitCount = resubmitCount,
                        rejectionHistory = rejectionHistory,
                        // Thông tin ẩn bài
                        hideReason = blog.LyDoAn,
                        dateHidden = blog.NgayAn.HasValue ? blog.NgayAn.Value.ToString("dd/MM/yyyy HH:mm") : ""
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region SOCIAL INTERACTION APIs
        /// <summary>
        /// API: Lấy dữ liệu bài viết để chỉnh sửa
        /// </summary>
        [HttpGet]
        public JsonResult GetBlogForEdit(int id)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                if (blog.MaNguoiDung != currentUser.MaNguoiDung)
                {
                    return ForbiddenJson("Bạn không có quyền.");
                }

                var images = db.AnhBaiViets
                    .Where(a => a.MaBaiViet == id)
                    .OrderBy(a => a.ThuTu)
                    .Select(a => a.DuongDanAnh)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = blog.MaBaiViet,
                        title = blog.TieuDe,
                        content = blog.NoiDung,
                        locationId = blog.MaDiaDiem,
                        images = images
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Like/Unlike bài viết
        /// </summary>
        [HttpPost]
        public JsonResult ToggleBlogLike(int blogId)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập để like bài viết.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == blogId);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                var existingLike = db.LikeBaiViets.FirstOrDefault(l => l.MaBaiViet == blogId && l.MaNguoiDung == currentUser.MaNguoiDung);

                if (existingLike != null)
                {
                    db.LikeBaiViets.DeleteOnSubmit(existingLike);
                    blog.LuotLike = (blog.LuotLike ?? 1) - 1;
                }
                else
                {
                    var newLike = new LikeBaiViet
                    {
                        MaBaiViet = blogId,
                        MaNguoiDung = currentUser.MaNguoiDung,
                        NgayLike = DateTime.Now
                    };
                    db.LikeBaiViets.InsertOnSubmit(newLike);
                    blog.LuotLike = (blog.LuotLike ?? 0) + 1;
                }

                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    likes = blog.LuotLike ?? 0,
                    message = existingLike != null ? "Đã bỏ thích" : "Đã thích bài viết"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Kiểm tra user đã like bài viết chưa
        /// </summary>
        [HttpGet]
        public JsonResult CheckBlogLike(int blogId)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = true, isLiked = false }, JsonRequestBehavior.AllowGet);
                }

                var isLiked = db.LikeBaiViets.Any(l => l.MaBaiViet == blogId && l.MaNguoiDung == currentUser.MaNguoiDung);
                return Json(new { success = true, isLiked = isLiked }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Thêm bình luận bài viết
        /// </summary>
        [HttpPost]
        public JsonResult AddBlogComment(int blogId, string content)
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập để bình luận.");
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Nội dung không được để trống." });
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == blogId);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                var comment = new BinhLuan
                {
                    MaBaiViet = blogId,
                    MaNguoiDung = user.MaNguoiDung,
                    NoiDung = content,
                    NgayDang = DateTime.Now
                };

                db.BinhLuans.InsertOnSubmit(comment);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Bình luận đã được thêm.",
                    comment = new
                    {
                        id = comment.MaBinhLuan,
                        author = user.HoTen,
                        avatar = BuildAvatarUrl(user.HoTen),
                        content = content,
                        date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        likes = 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Toggle save/bookmark bài viết
        /// </summary>
        [HttpPost]
        public JsonResult ToggleSaveBlog(int blogId)
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập.");
                }

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == blogId);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại." });
                }

                var existingSave = db.LikeBaiViets.FirstOrDefault(l =>
                    l.MaBaiViet == blogId && l.MaNguoiDung == user.MaNguoiDung);

                if (existingSave != null)
                {
                    db.LikeBaiViets.DeleteOnSubmit(existingSave);
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Đã bỏ lưu bài viết" });
                }
                else
                {
                    var newSave = new LikeBaiViet
                    {
                        MaBaiViet = blogId,
                        MaNguoiDung = user.MaNguoiDung,
                        NgayLike = DateTime.Now
                    };
                    db.LikeBaiViets.InsertOnSubmit(newSave);
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Đã lưu bài viết" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Saved Blogs (User Collections)

        /// <summary>
        /// API: Lấy danh sách bài viết đã lưu của user hiện tại
        /// </summary>
        [HttpGet]
        public JsonResult GetSavedBlogs(int page = 1, int pageSize = 12, string sort = "newest", int? locationId = null, string search = "")
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập.");
                }

                // Get saved blog IDs for current user
                var savedBlogIds = db.LikeBaiViets
                    .Where(l => l.MaNguoiDung == user.MaNguoiDung)
                    .Select(l => l.MaBaiViet)
                    .ToList();

                // Build query
                var query = db.BaiViets
                    .Where(b => savedBlogIds.Contains(b.MaBaiViet) && b.TrangThai == "approved");

                // Apply location filter
                if (locationId.HasValue && locationId.Value > 0)
                {
                    query = query.Where(b => b.MaDiaDiem == locationId.Value);
                }

                // Apply search
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(b => b.TieuDe.ToLower().Contains(searchLower) || b.NoiDung.ToLower().Contains(searchLower));
                }

                // Apply sorting
                switch (sort)
                {
                    case "oldest":
                        query = query.OrderBy(b => b.NgayDang);
                        break;
                    case "popular":
                        query = query.OrderByDescending(b => b.LuotLike);
                        break;
                    case "newest":
                    default:
                        query = query.OrderByDescending(b => b.NgayDang);
                        break;
                }

                var totalCount = query.Count();
                var skip = (page - 1) * pageSize;

                var blogs = query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        content = b.NoiDung,
                        excerpt = b.NoiDung.Length > 100 ? b.NoiDung.Substring(0, 100) + "..." : b.NoiDung,
                        image = b.AnhBaiViets.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        locationName = b.DiaDiem != null ? b.DiaDiem.TenDiaDiem : "Khám phá",
                        locationId = b.MaDiaDiem,
                        dateSubmitted = b.NgayDang ?? DateTime.Now,
                        likes = b.LuotLike ?? 0,
                        author = b.NguoiDung.HoTen,
                        avatar = BuildAvatarUrl(b.NguoiDung.HoTen)
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        b.id,
                        b.title,
                        b.content,
                        b.excerpt,
                        b.image,
                        b.locationName,
                        b.locationId,
                        dateSubmitted = b.dateSubmitted.ToString("dd/MM/yyyy"),
                        b.likes,
                        b.author,
                        b.avatar
                    })
                    .ToList();

                var totalPages = (totalCount + pageSize - 1) / pageSize;

                return Json(new
                {
                    success = true,
                    data = blogs,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API: Lấy danh sách địa điểm từ saved blogs
        /// </summary>
        [HttpGet]
        public JsonResult GetSavedLocations()
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return UnauthorizedJson("Vui lòng đăng nhập.");
                }

                var savedBlogIds = db.LikeBaiViets
                    .Where(l => l.MaNguoiDung == user.MaNguoiDung)
                    .Select(l => l.MaBaiViet)
                    .ToList();

                var locations = db.DiaDiems
                    .Where(d => db.BaiViets.Where(b => savedBlogIds.Contains(b.MaBaiViet)).Select(b => b.MaDiaDiem).Contains(d.MaDiaDiem))
                    .Distinct()
                    .Select(d => new
                    {
                        id = d.MaDiaDiem,
                        name = d.TenDiaDiem
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = locations
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region USER DETAIL PAGE API
        /// <summary>
        /// API: Lấy chi tiết blog của user để hiển thị
        /// </summary>
        [HttpGet]
        public JsonResult GetBlogDetail(int id)
        {
            try
            {
                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id && b.TrangThai == "approved");
                if (blog == null)
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại hoặc chưa được duyệt." }, JsonRequestBehavior.AllowGet);
                }

                var currentUser = GetCurrentUser();
                var isLiked = currentUser != null && db.LikeBaiViets.Any(l => l.MaBaiViet == id && l.MaNguoiDung == currentUser.MaNguoiDung);
                var isSaved = currentUser != null && db.LikeBaiViets.Any(l => l.MaBaiViet == id && l.MaNguoiDung == currentUser.MaNguoiDung);

                var images = db.AnhBaiViets
                    .Where(a => a.MaBaiViet == id)
                    .OrderBy(a => a.ThuTu)
                    .Select(a => a.DuongDanAnh)
                    .ToList();

                var relatedBlogs = db.BaiViets
                    .Where(b => b.MaDiaDiem == blog.MaDiaDiem && b.MaBaiViet != id && b.TrangThai == "approved")
                    .OrderByDescending(b => b.LuotLike)
                    .Take(4)
                    .Select(b => new
                    {
                        id = b.MaBaiViet,
                        title = b.TieuDe,
                        author = b.NguoiDung.HoTen,
                        // Chỉ lấy đường dẫn ảnh, chưa gọi BuildAvatarUrl ở đây
                        image = b.AnhBaiViets.OrderBy(a => a.ThuTu).FirstOrDefault().DuongDanAnh,
                        likes = b.LuotLike ?? 0
                    })
                    .AsEnumerable() // <--- Thêm dòng này để xử lý trên RAM
                    .Select(b => new
                    {
                        id = b.id,
                        title = b.title,
                        author = b.author,
                        avatar = BuildAvatarUrl(b.author), // Gọi an toàn sau AsEnumerable
                        image = b.image,
                        likes = b.likes
                    })
                    .ToList();

                var comments = db.BinhLuans
                    .Where(c => c.MaBaiViet == id && !c.ParentId.HasValue)
                    .OrderByDescending(c => c.NgayDang)
                    .Take(5)
                    .Select(c => new
                    {
                        id = c.MaBinhLuan,
                        author = c.NguoiDung.HoTen,
                        content = c.NoiDung,
                        dateValue = c.NgayDang // Chỉ lấy giá trị DateTime thô
                    })
                    .AsEnumerable() // <--- Thêm dòng này để xử lý trên RAM
                    .Select(c => new
                    {
                        id = c.id,
                        author = c.author,
                        avatar = BuildAvatarUrl(c.author), // Gọi an toàn sau AsEnumerable
                        content = c.content,
                        // Định dạng chuỗi ToString() an toàn tại đây
                        date = c.dateValue.HasValue ? c.dateValue.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        likes = 0
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = blog.MaBaiViet,
                        title = blog.TieuDe,
                        content = blog.NoiDung,
                        images = images,
                        locationId = blog.MaDiaDiem,
                        locationSlug = blog.DiaDiem != null ? blog.DiaDiem.Slug : "",
                        locationName = blog.DiaDiem != null ? blog.DiaDiem.TenDiaDiem : "Chưa gắn thẻ",
                        author = blog.NguoiDung.HoTen,
                        authorId = blog.MaNguoiDung,
                        avatar = BuildAvatarUrl(blog.NguoiDung.HoTen),
                        dateSubmitted = blog.NgayDang.HasValue ? blog.NgayDang.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        likes = blog.LuotLike ?? 0,
                        comments = blog.BinhLuans.Count(c => c.TrangThai == "visible"),
                        isLiked = isLiked,
                        isSaved = isSaved,
                        relatedBlogs = relatedBlogs,
                        recentComments = comments
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region LOCATION APIS
        /// <summary>
        /// API: Lấy địa điểm liên quan cho sidebar detail blog
        /// </summary>
        [HttpGet]
        public JsonResult GetRelatedLocations(int locationId, int take = 3)
        {
            try
            {
                var locations = db.DiaDiems
                    .Where(d => d.MaDiaDiem != locationId)
                    .OrderByDescending(d => d.BaiViets.Count())
                    .Take(take)
                    .Select(d => new
                    {
                        id = d.MaDiaDiem,
                        slug = d.Slug,
                        name = d.TenDiaDiem,
                        region = d.VungMien,
                        image = d.AnhDiaDiems.FirstOrDefault(a => a.LaAnhChinh.GetValueOrDefault()).DuongDanAnh,
                        blogs = d.BaiViets.Count()
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = locations
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region COMMENT APIs
        /// <summary>
        /// API: Lấy danh sách bình luận của bài viết theo cấu trúc cây
        /// </summary>
        [HttpGet]
        public JsonResult GetComments(int id)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var currentUserId = currentUser != null ? currentUser.MaNguoiDung : 0;

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == id);
                if (blog == null) return Json(new { success = false, message = "Bài viết không tồn tại" }, JsonRequestBehavior.AllowGet);

                var allComments = db.BinhLuans
                    .Where(c => c.MaBaiViet == id)
                    .OrderBy(c => c.NgayDang)
                    .Select(c => new
                    {
                        id = c.MaBinhLuan,
                        content = c.NoiDung,
                        dateValue = c.NgayDang,
                        authorId = c.MaNguoiDung,
                        authorName = c.NguoiDung.HoTen,
                        parentId = c.ParentId,
                        status = c.TrangThai,
                        reason = c.LyDoAn,
                        ngayXuLy = c.NgayXuLy
                    })
                    .ToList();

                var processedComments = allComments
                    .Where(c => c.status == "visible" || c.authorId == currentUserId || currentUserId == 1)
                    .Select(c => new
                    {
                        id = c.id,
                        content = c.content,
                        date = c.dateValue.HasValue ? GetRelativeTime(c.dateValue.Value) : "",
                        author = c.authorName,
                        authorId = c.authorId,
                        avatar = BuildAvatarUrl(c.authorName),
                        isAuthor = (c.authorId == blog.MaNguoiDung),
                        status = c.status,
                        parentId = c.parentId,
                        reason = c.reason,
                        dateProcessed = c.ngayXuLy.HasValue ? c.ngayXuLy.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        likes = 0,
                        isOwner = (c.authorId == currentUserId)
                    }).ToList();

                // Tạo cấu trúc cây (2 level)
                var rootComments = processedComments.Where(c => c.parentId == null).Select(c => new
                {
                    id = c.id,
                    content = c.content,
                    date = c.date,
                    author = c.author,
                    authorId = c.authorId,
                    avatar = c.avatar,
                    isAuthor = c.isAuthor,
                    likes = c.likes,
                    status = c.status,
                    reason = c.reason,
                    dateProcessed = c.dateProcessed,
                    isOwner = c.isOwner,
                    replies = processedComments.Where(r => r.parentId == c.id).ToList()
                }).ToList();

                return Json(new { success = true, data = rootComments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddComment(int blogId, string content, int? parentId)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null) return UnauthorizedJson("Bạn cần đăng nhập để bình luận.");

                if (string.IsNullOrWhiteSpace(content)) return Json(new { success = false, message = "Nội dung không được để trống." });

                if (content.Length > 1000) return Json(new { success = false, message = "Bình luận quá dài (tối đa 1000 ký tự)." });

                // Encode HTML để chống XSS
                content = System.Web.HttpUtility.HtmlEncode(content.Trim());

                var blog = db.BaiViets.FirstOrDefault(b => b.MaBaiViet == blogId);
                if (blog == null) return Json(new { success = false, message = "Bài viết không tồn tại." });

                var comment = new BinhLuan
                {
                    MaBaiViet = blogId,
                    MaNguoiDung = currentUser.MaNguoiDung,
                    NoiDung = content,
                    ParentId = parentId,
                    NgayDang = DateTime.Now,
                    TrangThai = "visible"
                };

                db.BinhLuans.InsertOnSubmit(comment);
                db.SubmitChanges();

                var newComment = new
                {
                    id = comment.MaBinhLuan,
                    content = comment.NoiDung,
                    date = "Vừa xong",
                    author = currentUser.HoTen,
                    authorId = currentUser.MaNguoiDung,
                    avatar = BuildAvatarUrl(currentUser.HoTen),
                    isAuthor = (currentUser.MaNguoiDung == blog.MaNguoiDung),
                    likes = 0,
                    parentId = comment.ParentId,
                    replies = new List<object>()
                };

                return Json(new { success = true, data = newComment, message = "Đã đăng bình luận thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi đăng bình luận: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteComment(int id)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null) return UnauthorizedJson("Bạn cần đăng nhập để thực hiện chức năng này.");

                var comment = db.BinhLuans.FirstOrDefault(c => c.MaBinhLuan == id);
                if (comment == null) return Json(new { success = false, message = "Bình luận không tồn tại." });

                if (comment.MaNguoiDung != currentUser.MaNguoiDung && currentUser.VaiTro != 1)
                {
                    return ForbiddenJson("Bạn không có quyền xóa bình luận này.");
                }

                // Xoá mềm (soft delete) cho chính chủ
                comment.TrangThai = "deleted";
                comment.LyDoAn = "Người dùng tự xoá";
                comment.NgayXuLy = DateTime.Now;
                comment.NguoiXuLy = currentUser.MaNguoiDung;

                db.SubmitChanges();

                return Json(new { success = true, message = "Đã xoá bình luận" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xoá bình luận: " + ex.Message });
            }
        }

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            if (timeSpan <= TimeSpan.FromSeconds(60)) return "Vừa xong";
            if (timeSpan <= TimeSpan.FromMinutes(60)) return timeSpan.Minutes + " phút trước";
            if (timeSpan <= TimeSpan.FromHours(24)) return timeSpan.Hours + " giờ trước";
            if (timeSpan <= TimeSpan.FromDays(30)) return timeSpan.Days + " ngày trước";
            return dateTime.ToString("dd/MM/yyyy");
        }
        #endregion
    }
}