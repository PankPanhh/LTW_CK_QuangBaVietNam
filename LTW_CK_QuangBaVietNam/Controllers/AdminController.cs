using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class AdminController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext(
            ConfigurationManager.ConnectionStrings["QBConnectionString"].ConnectionString
        );

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["nguoiDung"] == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập tài khoản Admin.";
                filterContext.Result = RedirectToAction("Login", "Home");
                return;
            }

            var user = Session["nguoiDung"] as LTW_CK_QuangBaVietNam.Models.NguoiDung;
            if (user == null || user.VaiTro != 1)
            {
                Session["nguoiDung"] = null;
                Session["khach"] = null;
                TempData["LoginError"] = "Tài khoản không có quyền Admin. Vui lòng đăng nhập lại.";
                filterContext.Result = RedirectToAction("Login", "Home");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private void LoadDanhMuc()
        {
            ViewBag.DanhMucList = db.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList();
        }

        public ActionResult Index()
        {
           
            var now = DateTime.Now;
            var from30 = now.AddDays(-30);
            var from60 = now.AddDays(-60);
            var from7 = now.AddDays(-7);

            int totalPlaces = db.DiaDiems.Count();
            int places30 = db.DiaDiems.Count(x => x.NgayDang >= from30);
            int placesPrev30 = db.DiaDiems.Count(x => x.NgayDang < from30 && x.NgayDang >= from60);
            double placesGrowth = (placesPrev30 <= 0) ? (places30 > 0 ? 100 : 0) : ((places30 - placesPrev30) * 100.0 / placesPrev30);

            int totalUsers = db.NguoiDungs.Count(x => x.VaiTro == 2);
            int users30 = db.NguoiDungs.Count(x => x.VaiTro == 2 && x.NgayTao >= from30);
            int usersPrev30 = db.NguoiDungs.Count(x => x.VaiTro == 2 && x.NgayTao < from30 && x.NgayTao >= from60);
            double usersGrowth = (usersPrev30 <= 0) ? (users30 > 0 ? 100 : 0) : ((users30 - usersPrev30) * 100.0 / usersPrev30);

            int newReviews7 = db.BinhLuans.Count(x => x.NgayDang >= from7 && x.TrangThai != "deleted");
            int needModeration = db.BinhLuans.Count(x => x.TrangThai == "hidden");
            int pendingPosts = db.BaiViets.Count(x => x.TrangThai == "pending");

            var topPlaces = db.DiaDiems
                .OrderByDescending(x => x.LuotXem ?? 0)
                .Take(10)
                .ToList();

            ViewBag.TotalPlaces = totalPlaces;
            ViewBag.PlacesGrowth = placesGrowth;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersGrowth = usersGrowth;
            ViewBag.NewUsers30 = users30;

            ViewBag.NewReviews7 = newReviews7;
            ViewBag.NeedModeration = needModeration;
            ViewBag.PendingPosts = pendingPosts;
            ViewBag.TopPlaces = topPlaces;

            return View();
        }

        public ActionResult DiaDiem(string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            filter = (filter ?? "all").ToLower().Trim();
            q = (q ?? "").Trim();
            vung = (vung ?? "all").Trim();

            IQueryable<DiaDiem> query = db.DiaDiems;

            if (filter == "showing")
            {
                query = query.Where(x => (x.TrangThai ?? true) == true);
            }
            else if (filter == "hidden")
            {
                query = query.Where(x => (x.TrangThai ?? true) == false);
            }

            if (danhMuc.HasValue)
            {
                query = query.Where(x => x.MaDanhMuc == danhMuc.Value);
            }

            var list = query.OrderBy(x => x.MaDiaDiem).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qNorm = NormalizeText(q);
                list = list.Where(x => NormalizeText(x.TenDiaDiem).Contains(qNorm)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(vung) && vung.ToLower() != "all")
            {
                var vNorm = NormalizeText(vung);
                list = list.Where(x => NormalizeText(x.VungMien).Contains(vNorm)).ToList();
            }

            ViewBag.DanhMucMap = db.DanhMucs.ToDictionary(x => x.MaDanhMuc, x => x.TenDanhMuc);

            var ddIds = list.Select(x => x.MaDiaDiem).ToList();
            ViewBag.AnhChinhMap = db.AnhDiaDiems
                .Where(a => ddIds.Contains(a.MaDiaDiem) && a.LaAnhChinh == true)
                .GroupBy(a => a.MaDiaDiem)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DuongDanAnh).FirstOrDefault());

            ViewBag.Filter = filter;
            ViewBag.Q = q;
            ViewBag.Vung = vung;
            ViewBag.DanhMuc = danhMuc;
            ViewBag.DanhMucList = db.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList();

            return View(list);
        }

        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            input = input.Trim().ToLowerInvariant();
            var formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int i = 0; i < formD.Length; i++)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(formD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(formD[i]);
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("đ", "d");
        }

        private string SaveUploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
                throw new Exception("Chỉ cho phép ảnh: jpg, jpeg, png, webp, gif.");

            var folder = Server.MapPath("~/Content/uploads/diadiem");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Content/uploads/diadiem/" + fileName;
        }

        [HttpGet]
        public ActionResult TaoDiaDiem()
        {
            ViewBag.Title = "Thêm địa điểm";
            LoadDanhMuc();

            var dd = new DiaDiem
            {
                GiaVe = 0,
                TrangThai = true,
                LaDiemChinh = false,
                NgayDang = DateTime.Now
            };

            return View(dd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoDiaDiem(DiaDiem form, HttpPostedFileBase AnhChinhFile, HttpPostedFileBase[] AnhPhuFiles)
        {
            ViewBag.Title = "Thêm địa điểm";
            LoadDanhMuc();

            form.TenDiaDiem = (form.TenDiaDiem ?? "").Trim();
            form.Slug = (form.Slug ?? "").Trim();

            if (string.IsNullOrWhiteSpace(form.TenDiaDiem))
                ModelState.AddModelError("TenDiaDiem", "Vui lòng nhập tên địa điểm.");

            if (string.IsNullOrWhiteSpace(form.Slug))
                ModelState.AddModelError("Slug", "Vui lòng nhập slug.");

            if (form.MaDanhMuc <= 0)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            if (!ModelState.IsValid) return View(form);

            var dd = new DiaDiem
            {
                TenDiaDiem = form.TenDiaDiem,
                Slug = form.Slug,
                MoTaNgan = string.IsNullOrWhiteSpace(form.MoTaNgan) ? null : form.MoTaNgan.Trim(),
                MoTaChiTiet = string.IsNullOrWhiteSpace(form.MoTaChiTiet) ? null : form.MoTaChiTiet.Trim(),
                MaDanhMuc = form.MaDanhMuc,
                TinhThanh = string.IsNullOrWhiteSpace(form.TinhThanh) ? null : form.TinhThanh.Trim(),
                GiaVe = form.GiaVe ?? 0,
                GioMoCua = string.IsNullOrWhiteSpace(form.GioMoCua) ? null : form.GioMoCua.Trim(),
                VungMien = string.IsNullOrWhiteSpace(form.VungMien) ? null : form.VungMien.Trim(),
                KinhDo = form.KinhDo,
                ViDo = form.ViDo,
                DiaChiChiTiet = string.IsNullOrWhiteSpace(form.DiaChiChiTiet) ? null : form.DiaChiChiTiet.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(form.SoDienThoai) ? null : form.SoDienThoai.Trim(),
                Email = string.IsNullOrWhiteSpace(form.Email) ? null : form.Email.Trim(),
                Website = string.IsNullOrWhiteSpace(form.Website) ? null : form.Website.Trim(),
                LaDiemChinh = form.LaDiemChinh ?? false,
                TrangThai = form.TrangThai ?? true,
                LuotXem = 0,
                DiemDanhGiaTB = 0,
                NgayDang = DateTime.Now
            };

            db.DiaDiems.InsertOnSubmit(dd);
            db.SubmitChanges();

            if (AnhChinhFile != null && AnhChinhFile.ContentLength > 0)
            {
                var url = SaveUploadImage(AnhChinhFile);
                db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                {
                    MaDiaDiem = dd.MaDiaDiem,
                    DuongDanAnh = url,
                    LaAnhChinh = true
                });
            }

            if (AnhPhuFiles != null)
            {
                foreach (var f in AnhPhuFiles)
                {
                    if (f == null || f.ContentLength == 0) continue;

                    var url = SaveUploadImage(f);
                    db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                    {
                        MaDiaDiem = dd.MaDiaDiem,
                        DuongDanAnh = url,
                        LaAnhChinh = false
                    });
                }
            }

            db.SubmitChanges();
            TempData["Success"] = "Đã thêm địa điểm.";
            return RedirectToAction("DiaDiem");
        }

        [HttpGet]
        public ActionResult SuaDiaDiem(int id)
        {
            ViewBag.Title = "Sửa địa điểm";
            LoadDanhMuc();

            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd == null) return HttpNotFound();

            ViewBag.Images = db.AnhDiaDiems
                .Where(x => x.MaDiaDiem == id)
                .OrderByDescending(x => x.LaAnhChinh)
                .ToList();

            return View(dd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDiaDiem(DiaDiem form, HttpPostedFileBase AnhChinhFile, HttpPostedFileBase[] AnhPhuFiles)
        {
            ViewBag.Title = "Sửa địa điểm";
            LoadDanhMuc();

            form.TenDiaDiem = (form.TenDiaDiem ?? "").Trim();
            form.Slug = (form.Slug ?? "").Trim();

            if (string.IsNullOrWhiteSpace(form.TenDiaDiem))
                ModelState.AddModelError("TenDiaDiem", "Vui lòng nhập tên địa điểm.");

            if (string.IsNullOrWhiteSpace(form.Slug))
                ModelState.AddModelError("Slug", "Vui lòng nhập slug.");

            if (form.MaDanhMuc <= 0)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            if (!ModelState.IsValid)
            {
                ViewBag.Images = db.AnhDiaDiems
                    .Where(x => x.MaDiaDiem == form.MaDiaDiem)
                    .OrderByDescending(x => x.LaAnhChinh)
                    .ToList();

                return View(form);
            }

            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == form.MaDiaDiem);
            if (dd == null) return HttpNotFound();

            dd.TenDiaDiem = form.TenDiaDiem;
            dd.Slug = form.Slug;
            dd.MoTaNgan = string.IsNullOrWhiteSpace(form.MoTaNgan) ? null : form.MoTaNgan.Trim();
            dd.MoTaChiTiet = string.IsNullOrWhiteSpace(form.MoTaChiTiet) ? null : form.MoTaChiTiet.Trim();
            dd.MaDanhMuc = form.MaDanhMuc;
            dd.GiaVe = form.GiaVe ?? 0;
            dd.GioMoCua = string.IsNullOrWhiteSpace(form.GioMoCua) ? null : form.GioMoCua.Trim();
            dd.VungMien = string.IsNullOrWhiteSpace(form.VungMien) ? null : form.VungMien.Trim();
            dd.TinhThanh = string.IsNullOrWhiteSpace(form.TinhThanh) ? null : form.TinhThanh.Trim();
            dd.KinhDo = form.KinhDo;
            dd.ViDo = form.ViDo;
            dd.DiaChiChiTiet = string.IsNullOrWhiteSpace(form.DiaChiChiTiet) ? null : form.DiaChiChiTiet.Trim();
            dd.SoDienThoai = string.IsNullOrWhiteSpace(form.SoDienThoai) ? null : form.SoDienThoai.Trim();
            dd.Email = string.IsNullOrWhiteSpace(form.Email) ? null : form.Email.Trim();
            dd.Website = string.IsNullOrWhiteSpace(form.Website) ? null : form.Website.Trim();
            dd.TrangThai = form.TrangThai ?? true;
            dd.LaDiemChinh = form.LaDiemChinh ?? false;

            if (AnhChinhFile != null && AnhChinhFile.ContentLength > 0)
            {
                db.ExecuteCommand("UPDATE AnhDiaDiem SET LaAnhChinh = 0 WHERE MaDiaDiem = {0}", dd.MaDiaDiem);

                var url = SaveUploadImage(AnhChinhFile);
                db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                {
                    MaDiaDiem = dd.MaDiaDiem,
                    DuongDanAnh = url,
                    LaAnhChinh = true
                });
            }

            if (AnhPhuFiles != null)
            {
                foreach (var f in AnhPhuFiles)
                {
                    if (f == null || f.ContentLength == 0) continue;

                    var url = SaveUploadImage(f);
                    db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                    {
                        MaDiaDiem = dd.MaDiaDiem,
                        DuongDanAnh = url,
                        LaAnhChinh = false
                    });
                }
            }

            db.SubmitChanges();
            TempData["Success"] = "Đã cập nhật địa điểm.";
            return RedirectToAction("DiaDiem");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThaiDiaDiem(int id, string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd != null)
            {
                var current = dd.TrangThai ?? true;
                dd.TrangThai = !current;
                db.SubmitChanges();
            }

            return RedirectToAction("DiaDiem", new { filter = filter, q = q, vung = vung, danhMuc = danhMuc });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaDiaDiem(int id, string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd == null)
            {
                TempData["Error"] = "Không tìm thấy địa điểm.";
                return RedirectToAction("DiaDiem", new { filter, q, vung, danhMuc });
            }

            dd.TrangThai = false;
            db.SubmitChanges();

            TempData["Success"] = "Đã ẩn địa điểm (không xoá vĩnh viễn).";
            return RedirectToAction("DiaDiem", new { filter, q, vung, danhMuc });
        }

        public ActionResult DanhMuc()
        {
            ViewBag.Title = "Quản lý danh mục";
            var list = db.DanhMucs.OrderBy(x => x.MaDanhMuc).ToList();
            return View(list);
        }

        [HttpPost]
        public ActionResult ThemDanhMuc(string tenDanhMuc, string moTa)
        {
            tenDanhMuc = (tenDanhMuc ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            if (string.IsNullOrEmpty(tenDanhMuc))
                return RedirectToAction("DanhMuc");

            db.DanhMucs.InsertOnSubmit(new DanhMuc
            {
                TenDanhMuc = tenDanhMuc,
                MoTa = moTa
            });
            db.SubmitChanges();

            return RedirectToAction("DanhMuc");
        }

        [HttpPost]
        public ActionResult SuaDanhMuc(int id, string tenDanhMuc, string moTa)
        {
            tenDanhMuc = (tenDanhMuc ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            var dm = db.DanhMucs.SingleOrDefault(x => x.MaDanhMuc == id);
            if (dm == null) return HttpNotFound();

            dm.TenDanhMuc = tenDanhMuc;
            dm.MoTa = moTa;
            db.SubmitChanges();

            return RedirectToAction("DanhMuc");
        }

        [HttpPost]
        public ActionResult XoaDanhMuc(int id)
        {
            var dm = db.DanhMucs.SingleOrDefault(x => x.MaDanhMuc == id);
            if (dm != null)
            {
                db.DanhMucs.DeleteOnSubmit(dm);
                db.SubmitChanges();
            }

            return RedirectToAction("DanhMuc");
        }

        public ActionResult NguoiDung(string filter = "all")
        {
            ViewBag.Title = "Quản lý người dùng";
            ViewBag.Filter = filter;

            var ds = db.NguoiDungs.AsQueryable();

            if (filter == "active")
            {
                ds = ds.Where(x => x.TrangThai == true);
            }
            else if (filter == "locked")
            {
                ds = ds.Where(x => x.TrangThai == false);
            }

            return View(ds.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiQuyenNguoiDung(int userId, int vaiTro, string filter = "all")
        {
            var u = db.NguoiDungs.SingleOrDefault(x => x.MaNguoiDung == userId);
            if (u == null) return HttpNotFound();

            u.VaiTro = vaiTro;
            db.SubmitChanges();

            TempData["Success"] = "Đổi quyền thành công.";
            return RedirectToAction("NguoiDung", new { filter = filter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KhoaNguoiDung(int userId, string filter = "all")
        {
            // Đã lược bỏ đoạn check Admin thủ công tại đây
            var u = db.NguoiDungs.SingleOrDefault(x => x.MaNguoiDung == userId);
            if (u == null) return HttpNotFound();

            u.TrangThai = false;
            db.SubmitChanges();

            TempData["Success"] = "Đã khoá người dùng.";
            return RedirectToAction("NguoiDung", new { filter = filter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoKhoaNguoiDung(int userId, string filter = "all")
        {
            var u = db.NguoiDungs.SingleOrDefault(x => x.MaNguoiDung == userId);
            if (u == null) return HttpNotFound();

            u.TrangThai = true;
            db.SubmitChanges();

            TempData["Success"] = "Đã mở khoá người dùng.";
            return RedirectToAction("NguoiDung", new { filter = filter });
        }

        public ActionResult ThongKeBaoCao()
        {
            var topViewed = db.DiaDiems
                .OrderByDescending(x => x.LuotXem ?? 0)
                .Take(10)
                .ToList();

            var mostViewed = topViewed.FirstOrDefault();

            var topFavCounts = db.YeuThiches
                .GroupBy(x => x.MaDiaDiem)
                .Select(g => new { MaDiaDiem = g.Key, So = g.Count() })
                .OrderByDescending(x => x.So)
                .Take(10)
                .ToList();

            var topFavIds = topFavCounts.Select(x => x.MaDiaDiem).ToList();
            var topFavCountMap = topFavCounts.ToDictionary(x => x.MaDiaDiem, x => x.So);

            var topFavPlaces = db.DiaDiems
                .Where(d => topFavIds.Contains(d.MaDiaDiem))
                .ToList();
            var topFavPlaceMap = topFavPlaces.ToDictionary(d => d.MaDiaDiem, d => d);

            DiaDiem topFavPlace = null;
            int topFavCount = 0;
            if (topFavCounts.Count > 0)
            {
                var row0 = topFavCounts[0];
                topFavCount = row0.So;
                topFavPlaceMap.TryGetValue(row0.MaDiaDiem, out topFavPlace);
            }

            var topUserRow = db.BinhLuans
                .Where(x => x.TrangThai != "deleted")
                .GroupBy(x => x.MaNguoiDung)
                .Select(g => new { MaNguoiDung = g.Key, So = g.Count() })
                .OrderByDescending(x => x.So)
                .FirstOrDefault();

            NguoiDung topUser = null;
            int topUserCount = 0;
            if (topUserRow != null)
            {
                topUserCount = topUserRow.So;
                topUser = db.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == topUserRow.MaNguoiDung);
            }

            var from30 = DateTime.Now.AddDays(-30);
            int commentCount30 = db.BinhLuans
                .Count(x => x.NgayDang.HasValue && x.NgayDang.Value >= from30 && x.TrangThai != "deleted");

            ViewBag.MostViewed = mostViewed;
            ViewBag.TopViewed = topViewed;
            ViewBag.TopFavPlace = topFavPlace;
            ViewBag.TopFavCount = topFavCount;
            ViewBag.TopUser = topUser;
            ViewBag.TopUserCount = topUserCount;
            ViewBag.CommentCount30 = commentCount30;
            ViewBag.TopFavIds = topFavIds;
            ViewBag.TopFavCountMap = topFavCountMap;
            ViewBag.TopFavPlaceMap = topFavPlaceMap;

            return View();
        }

        public ActionResult Blog()
        {
            if (Session["nguoiDung"] == null) return RedirectToAction("Index", "Home");
            var user = Session["nguoiDung"] as NguoiDung;
            if (user.VaiTro != 1) return RedirectToAction("Index", "Home");

            ViewBag.Title = "Quản lý bài viết blog";
            return View();
        }

        public ActionResult BlogDetail(int id)
        {
            if (Session["nguoiDung"] == null) return RedirectToAction("Index", "Home");
            var user = Session["nguoiDung"] as NguoiDung;
            if (user.VaiTro != 1) return RedirectToAction("Index", "Home");

            ViewBag.Title = "Chi tiết bài viết - Kiểm duyệt";
            ViewBag.BlogId = id;
            return View();
        }

       
        public ActionResult Comments()
        {
            // Đã lược bỏ đoạn check Admin thủ công tại đây
            ViewBag.Title = "Quản lý bình luận";
            return View();
        }

        [HttpGet]
        public JsonResult GetCommentsAdmin()
        {
            try
            {
                var dbComments = db.BinhLuans.OrderByDescending(c => c.NgayDang).Select(c => new
                {
                    id = c.MaBinhLuan,
                    blogId = c.MaBaiViet,
                    blogTitle = c.BaiViet.TieuDe,
                    content = c.NoiDung,
                    author = c.NguoiDung.HoTen,
                    date = c.NgayDang,
                    status = c.TrangThai,
                    reason = c.LyDoAn
                }).ToList();

                var comments = dbComments.Select(c => new
                {
                    id = c.id,
                    blogId = c.blogId,
                    blogTitle = c.blogTitle,
                    content = c.content,
                    author = c.author,
                    authorAvatar = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(c.author ?? "User") + "&background=0EA5E9&color=fff",
                    date = c.date.HasValue ? c.date.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    status = c.status,
                    reason = c.reason
                });

                return Json(new { success = true, data = comments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult HideComment(int id, string reason)
        {
            try
            {
                var user = Session["nguoiDung"] as NguoiDung;
                var c = db.BinhLuans.FirstOrDefault(x => x.MaBinhLuan == id);
                if (c == null) return Json(new { success = false, message = "Không tìm thấy bình luận" });

                c.TrangThai = "hidden";
                c.LyDoAn = reason;
                c.NgayXuLy = DateTime.Now;
                c.NguoiXuLy = user.MaNguoiDung;

                db.SubmitChanges();
                return Json(new { success = true, message = "Đã ẩn bình luận" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public JsonResult DeleteCommentAdmin(int id)
        {
            try
            {
                var user = Session["nguoiDung"] as NguoiDung;
                var c = db.BinhLuans.FirstOrDefault(x => x.MaBinhLuan == id);
                if (c == null) return Json(new { success = false, message = "Không tìm thấy bình luận" });

                c.TrangThai = "deleted";
                c.LyDoAn = "Bị admin xoá";
                c.NgayXuLy = DateTime.Now;
                c.NguoiXuLy = user.MaNguoiDung;

                db.SubmitChanges();
                return Json(new { success = true, message = "Đã xoá mềm bình luận" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}