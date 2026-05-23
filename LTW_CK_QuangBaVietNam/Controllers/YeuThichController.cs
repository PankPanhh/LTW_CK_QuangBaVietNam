using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
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
        public JsonResult DoiTrangThaiYeuThich(int maDiaDiem)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var existed = db.YeuThiches.SingleOrDefault(x =>
                x.MaNguoiDung == user.MaNguoiDung && x.MaDiaDiem == maDiaDiem);

            if (existed != null)
            {
                db.YeuThiches.DeleteOnSubmit(existed);
                db.SubmitChanges();
                return Json(new { success = true, isFavorite = false });
            }

            db.YeuThiches.InsertOnSubmit(new YeuThich
            {
                MaNguoiDung = user.MaNguoiDung,
                MaDiaDiem = maDiaDiem,
                NgayLuu = DateTime.Now
            });
            db.SubmitChanges();

            return Json(new { success = true, isFavorite = true });
        }

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
        public ActionResult TaoBoSuuTap(string tenBoSuuTap, string moTa)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            tenBoSuuTap = (tenBoSuuTap ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            if (string.IsNullOrWhiteSpace(tenBoSuuTap))
            {
                TempData["Error"] = "Tên bộ sưu tập không được để trống.";
                return RedirectToAction("BoSuuTap");
            }

            var bst = new BoSuuTap
            {
                MaNguoiDung = user.MaNguoiDung,
                TenBoSuuTap = tenBoSuuTap,
                MoTa = moTa,
                NgayTao = DateTime.Now
            };

            db.BoSuuTaps.InsertOnSubmit(bst);
            db.SubmitChanges();

            TempData["Message"] = "Tạo bộ sưu tập thành công.";
            return RedirectToAction("ChiTietBoSuuTap", new { id = bst.MaBoSuuTap });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemVaoBoSuuTap(int? maBoSuuTap, int? maDiaDiem, string returnUrl = null)
        {
            var user = Session["nguoiDung"] as LTW_CK_QuangBaVietNam.Models.NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            if (!maBoSuuTap.HasValue || maBoSuuTap.Value <= 0 || !maDiaDiem.HasValue || maDiaDiem.Value <= 0)
            {
                TempData["Error"] = "Vui lòng chọn bộ sưu tập và địa điểm.";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("BoSuuTap");
            }

            int bstId = maBoSuuTap.Value;
            int ddId = maDiaDiem.Value;

            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == bstId && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null)
            {
                TempData["Error"] = "Bộ sưu tập không hợp lệ.";
                return RedirectToAction("BoSuuTap");
            }

            var existed = db.BoSuuTapDiaDiems.FirstOrDefault(x => x.MaBoSuuTap == bstId && x.MaDiaDiem == ddId);
            if (existed != null)
            {
                TempData["Message"] = "Địa điểm đã có trong bộ sưu tập.";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("ChiTietBoSuuTap", new { id = bstId });
            }

            db.BoSuuTapDiaDiems.InsertOnSubmit(new BoSuuTapDiaDiem
            {
                MaBoSuuTap = bstId,
                MaDiaDiem = ddId,
                NgayThem = DateTime.Now
            });
            db.SubmitChanges();

            TempData["Message"] = "Đã thêm vào bộ sưu tập.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("ChiTietBoSuuTap", new { id = bstId });
        }

        public ActionResult YeuThich()
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var opt = new DataLoadOptions();
            opt.LoadWith<YeuThich>(x => x.DiaDiem);
            opt.LoadWith<DiaDiem>(x => x.AnhDiaDiems);
            db.LoadOptions = opt;

            var list = db.YeuThiches
                         .Where(x => x.MaNguoiDung == user.MaNguoiDung)
                         .OrderByDescending(x => x.NgayLuu)
                         .ToList();

            ViewBag.Collections = db.BoSuuTaps
                .Where(x => x.MaNguoiDung == user.MaNguoiDung)
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            var ids = list.Select(x => x.MaDiaDiem).Distinct().ToList();

            ViewBag.AnhChinhMap = db.AnhDiaDiems
                .Where(a => a.LaAnhChinh == true && ids.Contains(a.MaDiaDiem))
                .GroupBy(a => a.MaDiaDiem)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DuongDanAnh).FirstOrDefault());
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BoLuuForm(int maDiaDiem, string returnUrl = null)
        {
            
            var user = Session["nguoiDung"] as LTW_CK_QuangBaVietNam.Models.NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var item = db.YeuThiches.FirstOrDefault(x => x.MaDiaDiem == maDiaDiem && x.MaNguoiDung == user.MaNguoiDung);
            if (item != null)
            {
                db.YeuThiches.DeleteOnSubmit(item);
                db.SubmitChanges();
                TempData["Message"] = "Đã xóa địa điểm khỏi danh sách yêu thích.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "YeuThich");
        }
        public ActionResult BoSuuTap()
        {
            var user = Session["nguoiDung"] as LTW_CK_QuangBaVietNam.Models.NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var list = db.BoSuuTaps
                .Where(x => x.MaNguoiDung == user.MaNguoiDung)
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            var bstIds = list.Select(x => x.MaBoSuuTap).ToList();

            ViewBag.Counts = db.BoSuuTapDiaDiems
                .Where(x => bstIds.Contains(x.MaBoSuuTap))
                .GroupBy(x => x.MaBoSuuTap)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.AnhMap = (from ct in db.BoSuuTapDiaDiems
                              join a in db.AnhDiaDiems on ct.MaDiaDiem equals a.MaDiaDiem
                              where bstIds.Contains(ct.MaBoSuuTap) && a.LaAnhChinh == true
                              group a.DuongDanAnh by ct.MaBoSuuTap into g
                              select new
                              {
                                  MaBoSuuTap = g.Key,
                               
                                  DuongDanAnh = g.FirstOrDefault()
                              })
                              .ToDictionary(x => x.MaBoSuuTap, x => x.DuongDanAnh);

            return View(list);
        }

        public ActionResult ChiTietBoSuuTap(int id)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

            var bst = db.BoSuuTaps.FirstOrDefault(x => x.MaBoSuuTap == id && x.MaNguoiDung == user.MaNguoiDung);
            if (bst == null) return HttpNotFound();

            var rows = db.BoSuuTapDiaDiems
                .Where(x => x.MaBoSuuTap == id)
                .OrderByDescending(x => x.NgayThem)
                .ToList(); 

            var ddIds = rows.Select(x => x.MaDiaDiem).Distinct().ToList();

            ViewBag.Rows = rows;
            ViewBag.DiaDiemMap = db.DiaDiems
                .Where(d => ddIds.Contains(d.MaDiaDiem))
                .ToDictionary(d => d.MaDiaDiem, d => d);

            ViewBag.AnhChinhMap = db.AnhDiaDiems
                .Where(a => ddIds.Contains(a.MaDiaDiem) && a.LaAnhChinh == true)
                .GroupBy(a => a.MaDiaDiem)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DuongDanAnh).FirstOrDefault());

            return View(bst); 
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaKhoiBoSuuTap(int maBoSuuTap, int maDiaDiem)
        {
            var user = Session["nguoiDung"] as NguoiDung;
            if (user == null) return RedirectToAction("Login", "Home");

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

            var rows = db.BoSuuTapDiaDiems.Where(x => x.MaBoSuuTap == id).ToList();
            db.BoSuuTapDiaDiems.DeleteAllOnSubmit(rows);

            db.BoSuuTaps.DeleteOnSubmit(bst);
            db.SubmitChanges();

            return RedirectToAction("BoSuuTap");
        }
    }
}