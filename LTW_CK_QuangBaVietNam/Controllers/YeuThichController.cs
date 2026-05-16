using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class YeuThichController : Controller
    {
        private readonly DataClasses1DataContext db =
             new DataClasses1DataContext(
                 ConfigurationManager.ConnectionStrings["QBConnectionString"].ConnectionString
             );

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Toggle(int maDiaDiem)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            // LƯU Ý: tên bảng trong DataContext có thể là YeuThiches hoặc YeuThichs tùy bạn tạo LINQ to SQL
            var existed = db.YeuThiches.SingleOrDefault(x =>
                x.MaNguoiDung == user.MaNguoiDung && x.MaDiaDiem == maDiaDiem);

            if (existed != null)
            {
                db.YeuThiches.DeleteOnSubmit(existed);
                db.SubmitChanges();
                return Json(new { success = true, isFavorite = false });
            }

            var fav = new YeuThich
            {
                MaNguoiDung = user.MaNguoiDung,
                MaDiaDiem = maDiaDiem,
                NgayLuu = DateTime.Now
            };

            db.YeuThiches.InsertOnSubmit(fav);
            db.SubmitChanges();

            return Json(new { success = true, isFavorite = true });
        }

        // (Tuỳ chọn) Lấy danh sách ID yêu thích để render tim đúng theo DB
        [HttpGet]
        public JsonResult MyIds()
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, ids = new int[0] }, JsonRequestBehavior.AllowGet);
            }

            var ids = db.YeuThiches
                .Where(x => x.MaNguoiDung == user.MaNguoiDung)
                .Select(x => x.MaDiaDiem)
                .ToList();

            return Json(new { success = true, ids = ids }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TaoBoSuuTap(string tenBoSuuTap, string moTa)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            tenBoSuuTap = (tenBoSuuTap ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            if (string.IsNullOrWhiteSpace(tenBoSuuTap))
                return Json(new { success = false, message = "Tên bộ sưu tập không được để trống." });

            var bst = new BoSuuTap
            {
                MaNguoiDung = user.MaNguoiDung,
                TenBoSuuTap = tenBoSuuTap,
                MoTa = moTa,
                NgayTao = DateTime.Now
            };

            db.BoSuuTaps.InsertOnSubmit(bst);
            db.SubmitChanges();

            return Json(new
            {
                success = true,
                data = new { maBoSuuTap = bst.MaBoSuuTap, tenBoSuuTap = bst.TenBoSuuTap }
            });
        }

        // ========= 5) Bộ sưu tập: Thêm địa điểm vào BST =========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemVaoBoSuuTap(int maBoSuuTap, int maDiaDiem)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            // kiểm tra bst thuộc về user
            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == maBoSuuTap && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null) return Json(new { success = false, message = "Bộ sưu tập không hợp lệ." });

            var existed = db.BoSuuTapDiaDiems.FirstOrDefault(x => x.MaBoSuuTap == maBoSuuTap && x.MaDiaDiem == maDiaDiem);
            if (existed != null)
                return Json(new { success = true, message = "Địa điểm đã có trong bộ sưu tập." });

            db.BoSuuTapDiaDiems.InsertOnSubmit(new BoSuuTapDiaDiem
            {
                MaBoSuuTap = maBoSuuTap,
                MaDiaDiem = maDiaDiem,
                NgayThem = DateTime.Now
            });
            db.SubmitChanges();

            return Json(new { success = true, message = "Đã thêm vào bộ sưu tập." });
        }

        // GET: /YeuThich/BoSuuTap
        public ActionResult BoSuuTap()
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var list = (from bst in db.BoSuuTaps
                        where bst.MaNguoiDung == user.MaNguoiDung
                        orderby bst.NgayTao descending
                        select new BoSuuTapListVM
                        {
                            MaBoSuuTap = bst.MaBoSuuTap,
                            TenBoSuuTap = bst.TenBoSuuTap,
                            MoTa = bst.MoTa,
                            NgayTao = bst.NgayTao,
                            SoDiaDiem = db.BoSuuTapDiaDiems.Count(x => x.MaBoSuuTap == bst.MaBoSuuTap)
                        }).ToList();

            return View(list); // Views/YeuThich/BoSuuTap.cshtml
        }

        // GET: /YeuThich/ChiTietBoSuuTap/5
        public ActionResult ChiTietBoSuuTap(int id)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == id && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null) return HttpNotFound();

            var places = (from x in db.BoSuuTapDiaDiems
                          join dd in db.DiaDiems on x.MaDiaDiem equals dd.MaDiaDiem
                          join a in db.AnhDiaDiems.Where(x => x.LaAnhChinh == true)
on dd.MaDiaDiem equals a.MaDiaDiem into ga
                          from a in ga.DefaultIfEmpty()
                          where x.MaBoSuuTap == id
                          orderby x.NgayThem descending
                          select new BoSuuTapPlaceVM
                          {
                              MaDiaDiem = dd.MaDiaDiem,
                              TenDiaDiem = dd.TenDiaDiem,
                              VungMien = dd.VungMien,
                              AnhChinh = (a != null ? a.DuongDanAnh : null),
                              NgayThem = x.NgayThem
                          }).ToList();

            var vm = new BoSuuTapDetailVM
            {
                BoSuuTap = bst,
                Places = places
            };

            return View(vm); // Views/YeuThich/ChiTietBoSuuTap.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaKhoiBoSuuTap(int maBoSuuTap, int maDiaDiem)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            // đảm bảo BST thuộc user
            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == maBoSuuTap && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null) return HttpNotFound();

            var row = db.BoSuuTapDiaDiems.FirstOrDefault(x => x.MaBoSuuTap == maBoSuuTap && x.MaDiaDiem == maDiaDiem);
            if (row != null)
            {
                db.BoSuuTapDiaDiems.DeleteOnSubmit(row);
                db.SubmitChanges();
            }

            return RedirectToAction("ChiTietBoSuuTap", new { id = maBoSuuTap });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaBoSuuTap(int id)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == id && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null) return HttpNotFound();

            // xoá chi tiết trước
            var rows = db.BoSuuTapDiaDiems.Where(x => x.MaBoSuuTap == id).ToList();
            db.BoSuuTapDiaDiems.DeleteAllOnSubmit(rows);

            db.BoSuuTaps.DeleteOnSubmit(bst);
            db.SubmitChanges();

            return RedirectToAction("BoSuuTap");
        }
    }
}
